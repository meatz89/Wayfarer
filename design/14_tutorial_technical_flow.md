# Section 14: Technical Tutorial Flow (For Playtesters and Debuggers)

## 14.1 Overview

This document explains the complete technical implementation of the scene system for playtesters and debuggers who need to understand WHY scenes activate, HOW entities get created, and WHERE to look when something breaks.

**NOTE**: `IsStarter` is LEGACY and should be removed. The target architecture: ALL scenes load as Deferred at game initialization, then activate via categorical trigger when player enters matching location.

## 14.2 Complete System Architecture

### 14.2.1 The HIGHLANDER Pipeline

ALL content flows through a single path - no exceptions:

```
JSON --> DTO --> Parser --> Entity
```

**Layer Responsibilities:**
- **JSON**: Content authored in files or generated dynamically
- **DTO**: Data Transfer Objects (Package, LocationDTO, SceneTemplateDTO) - JSON deserialized to typed objects
- **Parser**: Converts DTOs to domain entities, resolves references, validates
- **Entity**: Domain objects stored in GameWorld

- **Authored content**: Written in `Content/Core/*.json`
- **Dynamic content**: Generated to `Content/Dynamic/*.json` at runtime
- **Both use identical loading**: `PackageLoader.LoadPackageContent()`

Key files:
- `src/Content/PackageLoader.cs:47-132` - Static package loading with dependency order
- `src/Content/PackageLoader.cs:284-340` - Dynamic package loading at runtime

### 14.2.2 Three-Tier Timing Model

| Tier | When | What's Created | Example |
|------|------|----------------|---------|
| **Parse-Time** (Tier 1) | Package loading | Templates and atmospheric actions | SceneTemplate, ChoiceTemplate, LocationAction (atmospheric) |
| **Spawn-Time** (Tier 2) | Scene activation | Instances with resolved entity references | Scene, Situation with Location/NPC objects |
| **Query-Time** (Tier 3) | UI request | Ephemeral scene-based actions | LocationAction/NPCAction from ChoiceTemplates (fresh on every query) |

**DUAL-TIER ACTION ARCHITECTURE (see arc42/08 §8.8):**
- **Atmospheric actions ARE stored** in `GameWorld.LocationActions` (permanent, parse-time from LocationActionCatalog)
- **Scene-based actions are ephemeral** (rebuilt fresh from ChoiceTemplates every query)

This is intentional: atmospheric actions (Travel, Work, Rest, Move) prevent soft-locks by always being available, while scene-based actions layer dynamic narrative content on top.

## 14.3 Complete Game Initialization Flow

### 14.3.1 Startup Sequence

```
Program.cs
  GameWorldInitializer.CreateGameWorld()
    1. ClearDynamicContentFolder() - Remove stale runtime JSON
    2. PackageLoader.LoadPackagesFromDirectory()
        LoadStaticPackages() in alphabetical order:
          LoadHexMap() - Spatial scaffolding first
          LoadRegions/Districts/Venues - Spatial hierarchy
          LoadLocations() - Places within venues
          PlaceVenues() - Procedural hex placement
          PlaceLocations() - Procedural hex assignment
          SyncLocationHexPositions() - Bind locations to hex grid
          LoadNPCs() - Characters at locations
          LoadSceneTemplates() - Authored scene definitions
          GenerateLocationActionsFromCatalogue() - Atmospheric actions
          ApplyInitialPlayerConfiguration() - Starting resources
    3. SpawnInitialScenes() - Log starter templates (actual spawn in GameFacade)
```

Key file: `src/Content/GameWorldInitializer.cs:14-46`

### 14.3.2 Game Start Sequence

```
GameFacade.StartGameAsync()
  PHASE 1: Initialize systems
    TimeFacade.SetInitialTimeState()
    ExchangeFacade.InitializeNPCExchanges()

  PHASE 2: SpawnStarterScenes() - Creates DEFERRED scenes
    For each template with IsStarter=true:  // LEGACY - should be ALL templates
      CreateDeferredSceneWithDynamicContent()
        SceneInstantiator.CreateDeferredScene()
          Generate JSON with State="Deferred"
          NO dependent resources yet
        PackageLoaderFacade.LoadDynamicPackage()
          Scene added to GameWorld.Scenes (State=Deferred)

  PHASE 3: LocationFacade.MoveToSpot(startingLocation)
    CheckAndActivateDeferredScenes(location)
      For each Deferred scene matching location's categorical properties:
        SceneInstantiator.ActivateScene() - Generate dependent resources
        PackageLoaderFacade.LoadDynamicPackage() - Load resources
        SceneInstantiator.ResolveSceneEntityReferences() - Bind entities
        Scene.State = Active

  PHASE 4: GameWorld.IsGameStarted = true
```

Key file: `src/Services/GameFacade.cs:706-783`

## 14.4 Categorical Activation Triggers (The Core Mechanism)

### 14.4.1 How Scenes Activate (NOT via IsStarter)

**CRITICAL**: `IsStarter=true` only determines which templates spawn at game start as Deferred. The actual ACTIVATION happens via categorical property matching.

### 14.4.2 Strongly-Typed Categorical Properties

**All categorical properties are strongly-typed enums with specific domain meaning.** JSON strings are parsed to enums at startup with fail-fast validation—invalid values throw immediately.

**Two Distinct Concepts:**
- **Identity Dimensions** (what location IS): Privacy, Safety, Activity, Purpose — list matching (any-of)
- **Capabilities** (what location CAN DO): Crossroads, Commercial, SleepingSpace — flags enum (all-of)

**LocationActivationFilter** (from SceneTemplate JSON):
```json
{
  "locationActivationFilter": {
    "placementType": "Location",
    "capabilities": ["Commercial", "Restful"],
    "privacyLevels": ["Public"]
  }
}
```

**Property Type Mapping:**
| JSON Field | Enum Type | Values | Matching Rule |
|------------|-----------|--------|---------------|
| `capabilities` | `LocationCapability` (Flags) | Commercial, Restful, Crossroads, SleepingSpace, Indoor, Outdoor, Market, etc. | Location must have ALL |
| `privacyLevels` | `LocationPrivacy` | Public, SemiPublic, Private | Location must have ONE OF |
| `safetyLevels` | `LocationSafety` | Dangerous, Neutral, Safe | Location must have ONE OF |
| `activityLevels` | `LocationActivity` | Quiet, Moderate, Busy | Location must have ONE OF |
| `purposes` | `LocationPurpose` | Transit, Dwelling, Commerce, Civic, Defense, Governance, Worship, Learning, Entertainment, Generic | Location must have ONE OF |

**Game Mechanic Effects:**
- `Commercial` → Enables Work action (earn coins)
- `SleepingSpace` → Enables Rest action (restore health/stamina)
- `Crossroads` → Enables Travel action (route selection)
- `Restful` → Enhanced restoration quality

### 14.4.3 Activation Check Flow

When player moves to a location (`LocationFacade.MoveToSpot`):

```csharp
// src/Subsystems/Location/LocationFacade.cs:424-483
private async Task CheckAndActivateDeferredScenes(Location location)
{
    // Find all deferred scenes with matching activation filters
    List<Scene> deferredScenes = _gameWorld.Scenes
        .Where(s => s.State == SceneState.Deferred)
        .Where(s => s.LocationActivationFilter != null &&
                   LocationMatchesActivationFilter(location, s.LocationActivationFilter, player))
        .ToList();

    foreach (Scene scene in deferredScenes)
    {
        // PHASE 2: Generate dependent resources
        string resourceJson = _sceneInstantiator.ActivateScene(scene, activationContext);
        await _packageLoaderFacade.LoadDynamicPackage(resourceJson, packagePath);

        // PHASE 2.5: Resolve entity references
        _sceneInstantiator.ResolveSceneEntityReferences(scene, activationContext);

        // Transition state
        scene.State = SceneState.Active;
    }
}
```

### 14.4.4 Categorical Matching Logic

```csharp
// src/Subsystems/Location/LocationFacade.cs:490-521
private bool LocationMatchesActivationFilter(Location location, PlacementFilter filter, Player player)
{
    // Each categorical dimension checked independently
    // Empty list = don't filter, Non-empty = must match ONE OF values (identity dimensions)
    // Capabilities use flags enum with bitwise AND (all-of matching)

    if (filter.PrivacyLevels != null && filter.PrivacyLevels.Count > 0)
        if (!filter.PrivacyLevels.Contains(location.Privacy))
            return false;

    if (filter.SafetyLevels != null && filter.SafetyLevels.Count > 0)
        if (!filter.SafetyLevels.Contains(location.Safety))
            return false;

    if (filter.ActivityLevels != null && filter.ActivityLevels.Count > 0)
        if (!filter.ActivityLevels.Contains(location.Activity))
            return false;

    if (filter.Purposes != null && filter.Purposes.Count > 0)
        if (!filter.Purposes.Contains(location.Purpose))
            return false;

    return true; // All checks passed
}
```

### 14.4.5 NPC Activation (Parallel Path)

Scenes can also activate when player interacts with matching NPC:

```csharp
// src/Subsystems/Location/LocationFacade.cs:528-574
public async Task CheckAndActivateDeferredScenesForNPC(NPC npc, Player player)
{
    // Similar logic but checks NpcActivationFilter against NPC categorical properties:
    // - Professions
    // - SocialStandings
    // - StoryRoles
    // - KnowledgeLevels
    // - PersonalityTypes
}
```

## 14.5 Entity Resolution Flow (Venue-Scoped)

### 14.5.1 Why Deferred Resolution?

Entities cannot be resolved at scene creation because:
1. Dependent resources (private_room, room_key) don't exist yet
2. Categorical filters need the entity pool to search
3. Object references must point to actual GameWorld entities

### 14.5.2 The EntityResolver

```csharp
// src/Content/EntityResolver.cs:31-45
public Location FindOrCreateLocation(PlacementFilter filter)
{
    // Try to find existing location matching categories
    Location existingLocation = FindMatchingLocation(filter);
    if (existingLocation != null)
        return existingLocation;

    // No match found - generate new location from categories
    Location newLocation = CreateLocationFromCategories(filter);
    _gameWorld.Locations.Add(newLocation);
    return newLocation;
}
```

### 14.5.3 Venue-Scoped Search

**CRITICAL**: Entity resolution is VENUE-SCOPED to prevent cross-venue teleportation:

```csharp
// src/Content/EntityResolver.cs:101-127
private Location FindMatchingLocation(PlacementFilter filter)
{
    if (_currentVenue != null)
    {
        // Search ONLY within venue
        matchingLocations = _gameWorld.Locations
            .Where(loc => loc.Venue == _currentVenue && LocationMatchesFilter(loc, filter))
            .ToList();
    }
    else
    {
        // Global search (parse-time only)
        matchingLocations = _gameWorld.Locations
            .Where(loc => LocationMatchesFilter(loc, filter))
            .ToList();
    }
}
```

## 14.6 Scene-Situation-Choice Execution Pipeline

### 14.6.1 Getting Available Actions (Query-Time)

When UI requests actions for an NPC:

```csharp
// src/Subsystems/Scene/SceneFacade.cs:119-166
public List<NPCAction> GetActionsForNPC(NPC npc, Player player)
{
    // Find active Scenes at this NPC (object comparison, not ID)
    List<Scene> scenes = _gameWorld.Scenes
        .Where(s => s.State == SceneState.Active &&
                   s.CurrentSituation?.Npc == npc)
        .ToList();

    List<NPCAction> allActions = new List<NPCAction>();

    foreach (Scene scene in scenes)
    {
        Situation situation = scene.CurrentSituation;

        // Create actions FRESH from ChoiceTemplates (ephemeral, never stored)
        foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
        {
            NPCAction action = new NPCAction
            {
                Name = choiceTemplate.ActionTextTemplate,
                NPC = npc,
                ChoiceTemplate = choiceTemplate,
                Situation = situation
            };
            allActions.Add(action);
        }
    }

    return allActions;
}
```

### 14.6.2 Executing an Action

```
Player selects choice in UI
  GameFacade.ExecuteNPCAction(action)
    SituationChoiceExecutor.ValidateAndExtract()
      Check CompoundRequirements
      Check Resolve/Coins/Health/Stamina/Focus costs
      Return ActionExecutionPlan

    Apply costs (player.Resolve -= plan.ResolveCost, etc.)

    RewardApplicationService.ApplyRewards()
      Apply stat gains, coins, items
      FinalizeSceneSpawns() - Chain to next scene

    SituationCompletionHandler.CompleteSituation()
      situation.Complete()
      ApplySituationCardRewards()
      scene.AdvanceToNextSituation()
```

Key files:
- `src/Services/SituationChoiceExecutor.cs:1-80` - Validation
- `src/Subsystems/Consequence/RewardApplicationService.cs:201-271` - Scene spawning
- `src/Services/SituationCompletionHandler.cs:30-101` - Situation lifecycle

### 14.6.3 Scene State Advancement

```csharp
// Scene.AdvanceToNextSituation() determines routing:
// - Linear: Just advance to next situation index
// - Conditional: Check OnSuccess/OnFailure based on LastChallengeSucceeded
// - Branching: Follow specific transition rules

// When all situations complete:
scene.State = SceneState.Completed
```

## 14.7 Scene Chaining (A1 -> A2 -> A3 -> ...)

### 14.7.1 How A1 Completion Triggers A2

In SceneArchetypeCatalog.GenerateInnLodging() (lines 359-370):

```csharp
// A1 tutorial: ALL departure choices spawn A2
if (isA1Tutorial)
{
    earlyDepartureReward.ScenesToSpawn = new List<SceneSpawnReward>
    {
        new SceneSpawnReward { SceneTemplateId = "a2_morning" }
    };
}
```

### 14.7.2 Reward-Triggered Scene Spawning

```csharp
// src/Subsystems/Consequence/RewardApplicationService.cs:201-271
private async Task FinalizeSceneSpawns(ChoiceReward reward, Situation currentSituation)
{
    foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
    {
        // For A-story scenes (a_story_N pattern):
        if (sceneSpawn.SceneTemplateId.StartsWith("a_story_"))
        {
            // Extract sequence number
            int sequence = int.Parse(sceneSpawn.SceneTemplateId.Replace("a_story_", ""));

            // Find or generate template
            template = _gameWorld.SceneTemplates
                .FirstOrDefault(t => t.MainStorySequence == sequence);

            if (template == null)
            {
                // Generate procedurally (on-demand)
                await _proceduralAStoryService.GenerateNextATemplate(sequence, aStoryContext);
            }
        }

        // Spawn scene (creates as Deferred, activates on location entry)
        await _sceneInstanceFacade.SpawnScene(template, sceneSpawn, context);
    }
}
```

## 14.8 Procedural A-Story Generation (A4+)

### 14.8.1 When Procedural Generation Triggers

After A3 (or whatever the last authored tutorial scene is), completion spawns `a_story_4`. If template doesn't exist, it's generated procedurally.

### 14.8.2 Generation Flow

```csharp
// src/Subsystems/ProceduralContent/ProceduralAStoryService.cs:55-77
public async Task<string> GenerateNextATemplate(int sequence, AStoryContext context)
{
    // 1. Select archetype (rotation: Investigation -> Social -> Confrontation -> Crisis)
    string archetypeId = SelectArchetype(sequence, context);

    // 2. Calculate tier (1-30=Personal, 31-50=Local, 51+=Regional)
    int tier = CalculateTier(sequence);

    // 3. Build SceneTemplateDTO with categorical properties
    SceneTemplateDTO dto = BuildSceneTemplateDTO(sequence, archetypeId, tier, context);

    // 4. Serialize to JSON package
    string packageJson = SerializeTemplatePackage(dto);

    // 5. Write to Content/Dynamic/
    await _contentFacade.CreateDynamicPackageFile(packageJson, packageId);

    // 6. Load through HIGHLANDER pipeline
    await _packageLoaderFacade.LoadDynamicPackage(packageJson, packageId);

    return dto.Id;
}
```

### 14.8.3 Archetype Rotation

```csharp
// Sequence determines category via modulo:
int cyclePosition = (sequence - 1) % 4;
string category = cyclePosition switch
{
    0 => "Investigation",  // seek_audience, investigate_location, gather_testimony
    1 => "Social",         // meet_order_member
    2 => "Confrontation",  // confront_antagonist
    3 => "Crisis"          // urgent_decision, moral_crossroads
};
```

### 14.8.4 Anti-Repetition

```csharp
// Recent archetypes, regions, and personality types are tracked
// and filtered out to ensure variety
List<string> availableArchetypes = candidateArchetypes
    .Where(a => !context.IsArchetypeRecent(a))  // 5-scene window
    .ToList();
```

## 14.9 Hex Map and Spatial System

### 14.9.1 Spatial Hierarchy

```
Region (e.g., "Northern Highlands")
  District (e.g., "Riverside")
    Venue (e.g., "Crossroads Inn")
      Location (e.g., "Common Room")
        HexPosition (q, r coordinates)
```

### 14.9.2 Procedural Placement

Locations don't have hardcoded hex positions. They're placed procedurally:

```csharp
// src/Content/PackageLoader.cs:96-100
PlaceVenues(allVenues);      // Assign CenterHex to venues
PlaceLocations(allLocations); // Assign HexPosition to locations within venues
```

### 14.9.3 Hex Grid Integration

```csharp
// src/GameState/HexMap.cs:160-163
public Hex GetHexForLocation(Location location)
{
    return Hexes.FirstOrDefault(h => h.Location == location);
}
```

## 14.10 Debugging Guide

### 14.10.1 Scene Not Activating

Check in order:

1. **Is scene in GameWorld.Scenes?**
   - Should be added during SpawnStarterScenes or FinalizeSceneSpawns

2. **Is scene State == Deferred?**
   - Must be Deferred to activate (already Active = won't trigger)

3. **Does LocationActivationFilter match?**
   - Check location's categorical properties: Privacy, Safety, Activity, Purpose, Capabilities
   - Compare against scene's LocationActivationFilter

4. **Is CheckAndActivateDeferredScenes being called?**
   - Called from LocationFacade.MoveToSpot()
   - Look for: `[LocationFacade] Activating deferred scene...`

### 14.10.2 Scene Activated But No Choices Showing

1. **Is scene State == Active?**

2. **Does CurrentSituation.Npc match the NPC you're talking to?**
   - Object equality check: `scene.CurrentSituation?.Npc == npc`

3. **Was ResolveSceneEntityReferences called?**
   - Look for: `[SceneInstantiator] Resolved entity references`

4. **Are there ChoiceTemplates in the SituationTemplate?**
   - Empty ChoiceTemplates = no actions to display

### 14.10.3 A2 Not Spawning After A1 Completion

1. **Did the final choice have ScenesToSpawn?**
   - Check inn_lodging_depart ChoiceTemplates
   - Should have RewardTemplate.ScenesToSpawn = [{ SceneTemplateId: "a2_morning" }]

2. **Was FinalizeSceneSpawns called?**
   - Look for: `[RewardApplicationService] Spawning scene...`

3. **Does a2_morning template exist?**
   - Check GameWorld.SceneTemplates

### 14.10.4 Key Console Log Markers

| Log Pattern | Meaning | File:Line |
|-------------|---------|-----------|
| `[PackageLoader] Loading package:` | Static content loading | PackageLoader.cs:61 |
| `[GameFacade] Created deferred starter scene` | Phase 1 complete | GameFacade.cs:1720 |
| `[LocationFacade] Activating deferred scene` | Phase 2 starting | LocationFacade.cs:441 |
| `[SceneActivation] Loaded dependent resources` | Resources created | LocationFacade.cs:468 |
| `[SceneInstantiator] Resolved entity references` | References assigned | SceneInstantiator.cs:200 |
| `Scene activated successfully` | Phase 2 complete | LocationFacade.cs:481 |
| `[SituationCompletionHandler] Challenge succeeded` | Challenge result | SituationCompletionHandler.cs:51 |

## 14.11 Key File Locations

| Component | File Path | Key Lines |
|-----------|-----------|-----------|
| Game initialization | `src/Content/GameWorldInitializer.cs` | 14-46 |
| Package loading | `src/Content/PackageLoader.cs` | 47-132, 284-340 |
| Game start flow | `src/Services/GameFacade.cs` | 706-783, 1683-1722 |
| Deferred scene creation | `src/Content/SceneInstantiator.cs` | 49-78 |
| Scene activation | `src/Content/SceneInstantiator.cs` | 87-133 |
| Entity resolution | `src/Content/SceneInstantiator.cs` | 156-201 |
| Activation trigger check | `src/Subsystems/Location/LocationFacade.cs` | 424-483 |
| Categorical matching | `src/Subsystems/Location/LocationFacade.cs` | 490-521 |
| Entity resolver | `src/Content/EntityResolver.cs` | 31-85, 101-127 |
| Action query (Tier 3) | `src/Subsystems/Scene/SceneFacade.cs` | 58-108, 119-166 |
| Choice validation | `src/Services/SituationChoiceExecutor.cs` | 13-79 |
| Situation completion | `src/Services/SituationCompletionHandler.cs` | 30-101 |
| Scene chaining | `src/Subsystems/Consequence/RewardApplicationService.cs` | 201-271 |
| Procedural A-story | `src/Subsystems/ProceduralContent/ProceduralAStoryService.cs` | 55-77, 91-138 |
| Hex map | `src/GameState/HexMap.cs` | 43-68, 160-163 |

## 14.12 Architecture Principles

### 14.12.1 HIGHLANDER Pipeline

All content flows through the same path:
```
JSON --> DTO --> Parser --> Entity
```

- **DTO Layer**: JSON deserializes to typed DTOs (Package, LocationDTO, SceneTemplateDTO)
- **Parser Layer**: Converts DTOs to domain entities, resolves references, validates enums

Dynamic scenes generate JSON at runtime, written to Content/Dynamic/, loaded via PackageLoader. NO direct entity creation allowed.

### 14.12.2 Strongly-Typed Categorical Properties

Scenes use categorical filters with strongly-typed enums, not hardcoded IDs or generic strings.

**Two Property Types:**
- **Identity Dimensions** (what entity IS): `LocationPrivacy`, `LocationSafety`, `LocationActivity`, `LocationPurpose`
- **Capabilities** (what entity CAN DO): `LocationCapability` flags enum

**Correct:**
```json
{
  "capabilities": ["Commercial", "Restful"],
  "privacyLevels": ["SemiPublic"],
  "purposes": ["Commerce", "Dwelling"]
}
```

**Forbidden:**
- `locationId: "common_room"` — hardcoded IDs
- Generic strings with no enum backing

All JSON strings are parsed to enums at startup. Invalid values fail immediately (fail-fast validation).

This enables procedural generation and hex-map independence.

### 14.12.3 Situation Owns Placement

Each situation has its own entity references:
- `Situation.Location` (not Scene.Location)
- `Situation.Npc` (not Scene.Npc)
- `Situation.Route` (not Scene.Route)

This enables multi-location scenes (negotiate at common_room, rest at private_room).

### 14.12.4 Three-Tier Timing

1. **Parse-Time**: Templates created (immutable archetypes)
2. **Spawn-Time**: Instances created with resolved entities
3. **Query-Time**: Actions created fresh (ephemeral, never stored)

### 14.12.5 Venue-Scoped Resolution

Entity resolution searches ONLY within the activation venue to prevent teleportation across the map when resolving categorical filters.

## 14.13 Tutorial Scene Definitions

### 14.13.1 A1: Secure Lodging

**Source**: `src/Content/Core/22_a_story_tutorial.json`

```json
{
  "id": "a1_secure_lodging",
  "category": "MainStory",
  "mainStorySequence": 1,
  "sceneArchetypeId": "inn_lodging",
  "isStarter": true,
  "locationActivationFilter": {
    "capabilities": ["Commercial", "Restful"]
  },
  "npcActivationFilter": {
    "professions": ["Innkeeper"]
  }
}
```

**Categorical Property Types:**
- `capabilities` → `LocationCapability` flags: `Commercial` (enables Work), `Restful` (enhanced rest)
- `professions` → `Professions` enum: `Innkeeper` (occupational role)

**NOTE**: `isStarter` is LEGACY. All scenes should work via categorical trigger only.

**Archetype Generation**: `SceneArchetypeCatalog.GenerateInnLodging()` creates:
- Situation 1: `inn_lodging_negotiate` (identity formation)
- Situation 2: `inn_lodging_rest` (private room)
- Situation 3: `inn_lodging_depart` (chains to A2 via ScenesToSpawn)

**Dependent Resources**:
- `private_room` Location (created at activation)
- `room_key` Item (added to inventory)

### 14.13.2 A2: First Delivery

```json
{
  "id": "a2_morning",
  "mainStorySequence": 2,
  "sceneArchetypeId": "delivery_contract",
  "locationActivationFilter": {
    "privacyLevels": ["Public"],
    "capabilities": ["Commercial"]
  },
  "npcActivationFilter": {
    "professions": ["Merchant"]
  }
}
```

**Categorical Property Types:**
- `privacyLevels` → `LocationPrivacy` enum: `Public` (many witnesses, high social stakes)
- `capabilities` → `LocationCapability` flags: `Commercial` (enables Work action)
- `professions` → `Professions` enum: `Merchant` (occupational role)

### 14.13.3 A3: Route Travel

```json
{
  "id": "a3_route_travel",
  "mainStorySequence": 3,
  "sceneArchetypeId": "route_segment_travel",
  "locationActivationFilter": {
    "activityLevels": ["Quiet"],
    "capabilities": ["Outdoor"]
  }
}
```

**Categorical Property Types:**
- `activityLevels` → `LocationActivity` enum: `Quiet` (isolated, few people)
- `capabilities` → `LocationCapability` flags: `Outdoor` (exposed to weather)

## 14.14 Summary

The tutorial system uses **strongly-typed categorical triggers** for scene activation. All categorical properties are enums with specific domain meaning—never generic strings.

**Two Property Types:**
- **Identity Dimensions** (what location IS): `LocationPrivacy`, `LocationSafety`, `LocationActivity`, `LocationPurpose`
- **Capabilities** (what location CAN DO): `LocationCapability` flags enum enabling game mechanics

**Activation Flow:**

1. `LocationFacade.CheckAndActivateDeferredScenes()` finds all Deferred scenes
2. Compares location's categorical properties against each scene's `LocationActivationFilter`
3. For matches: generates dependent resources, resolves entity references, transitions to Active
4. `SceneFacade.GetActionsForNPC()` queries active scenes and creates ephemeral actions
5. Player choice execution applies rewards and may chain to next scene via `ScenesToSpawn`
6. Procedural generation (A4+) follows identical flow, generating JSON on-demand

**Key Files for Debugging**:
- Activation: `LocationFacade.cs:424-521`
- Resolution: `SceneInstantiator.cs:156-201`
- Actions: `SceneFacade.cs:119-166`
- Chaining: `RewardApplicationService.cs:201-271`
- Enum definitions: `src/GameState/LocationPrivacy.cs`, `LocationCapability.cs`, etc.
