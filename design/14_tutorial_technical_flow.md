# Section 11: Tutorial Technical Flow (For Playtesters and Debuggers)

## 11.1 Overview

This document explains the technical implementation of the tutorial system for playtesters and debuggers who need to understand WHY scenes activate, HOW entities get created, and WHERE to look when something breaks.

**Key Concepts:**
- **Two-Phase Spawning**: Scenes created in Deferred state, activated on location entry
- **Categorical Triggers**: Scenes activate based on location properties, not hardcoded IDs
- **Deferred Entity Resolution**: Entity references assigned AFTER dependent resources created
- **Reward-Triggered Chaining**: A1 → A2 → A3 linked via ChoiceReward.ScenesToSpawn
- **HIGHLANDER Pipeline**: All content flows JSON → Parser → Entity (single path)

## 11.2 Complete Flow Diagram

```
GAME INITIALIZATION
│
├── Phase 1: Create Deferred Scenes
│   └── GameFacade.SpawnStarterScenes()
│       └── For each template with IsStarter=true:
│           └── CreateDeferredSceneWithDynamicContent()
│               └── SceneInstanceFacade.CreateDeferredScenePackage()
│                   └── SceneInstantiator.CreateDeferredScene()
│                       └── Generates JSON with State="Deferred"
│               └── ContentGenerationFacade.CreateDynamicPackageFile()
│               └── PackageLoaderFacade.LoadDynamicPackage()
│                   └── Scene added to GameWorld.Scenes (Deferred state)
│
├── Phase 2: Player Moves to Starting Location
│   └── LocationFacade.MoveToSpot(startingSpot)
│       └── CheckAndActivateDeferredScenes(location)
│           └── For each Deferred scene:
│               └── LocationMatchesActivationFilter()
│                   └── Checks: Privacy, Safety, Activity, Purpose, Capabilities
│               └── If match:
│                   └── SceneInstantiator.ActivateScene()
│                       └── Generates dependent resources JSON
│                   └── PackageLoaderFacade.LoadDynamicPackage()
│                       └── Creates Location, NPC, Item entities
│                   └── SceneInstantiator.ResolveSceneEntityReferences()
│                       └── Assigns entity references to situations
│                   └── Scene.State = Active
│
├── Phase 3: Player Interacts with Scene
│   └── Player enters NPC conversation
│       └── SceneFacade.GetActionsForNPC(npc, player)
│           └── Finds active scenes where CurrentSituation.Npc == npc
│           └── Creates ephemeral actions from ChoiceTemplates
│       └── Player selects choice
│           └── GameFacade.ExecuteNPCAction()
│               └── SituationChoiceExecutor.Execute()
│               └── RewardApplicationService.Apply()
│               └── SituationCompletionHandler.Advance()
│
└── Phase 4: Scene Chaining (A1 → A2)
    └── Final situation choice has RewardTemplate.ScenesToSpawn
        └── RewardApplicationService.FinalizeSceneSpawns()
            └── SceneInstanceFacade.SpawnScene() for A2
                └── A2 created in Deferred state
                └── Activates when player enters matching location
```

## 11.3 Activation Trigger Details

### 11.3.1 What Triggers Scene Activation (NOT isStarter)

**CRITICAL**: `isStarter=true` only determines which templates spawn at game start. The actual ACTIVATION happens via categorical triggers.

**LocationActivationFilter** (from SceneTemplate):
```json
{
  "locationActivationFilter": {
    "placementType": "Location",
    "capabilities": ["Commercial", "Restful"]
  }
}
```

**Activation Check** (in `LocationFacade.CheckAndActivateDeferredScenes`):
```csharp
List<Scene> deferredScenes = _gameWorld.Scenes
    .Where(s => s.State == SceneState.Deferred)
    .Where(s => s.LocationActivationFilter != null &&
               LocationMatchesActivationFilter(location, s.LocationActivationFilter, player))
    .ToList();
```

**Categorical Matching** (in `LocationFacade.LocationMatchesActivationFilter`):
```csharp
// Checks: PrivacyLevels, SafetyLevels, ActivityLevels, Purposes, Capabilities
// Returns true only if ALL specified filters match
if (filter.Capabilities != null && filter.Capabilities.Count > 0)
{
    if (!filter.Capabilities.All(cap => location.Capabilities.HasFlag(cap)))
        return false;
}
return true;
```

### 11.3.2 NPC Activation (Parallel Path)

Scenes can also activate when player interacts with matching NPC:

```csharp
// LocationFacade.CheckAndActivateDeferredScenesForNPC()
List<Scene> deferredScenes = _gameWorld.Scenes
    .Where(s => s.State == SceneState.Deferred && s.NpcActivationFilter != null)
    .ToList();

foreach (Scene scene in deferredScenes)
{
    if (NPCMatchesActivationFilter(npc, scene.NpcActivationFilter, player))
    {
        // Activate scene...
    }
}
```

**NPC Matching** checks: Professions, SocialStandings, StoryRoles, KnowledgeLevels, PersonalityTypes

## 11.4 Tutorial Scene Definitions (A1, A2, A3)

### 11.4.1 A1: Secure Lodging

**Source File**: `src/Content/Core/22_a_story_tutorial.json`

```json
{
  "id": "a1_secure_lodging",
  "category": "MainStory",
  "mainStorySequence": 1,
  "archetype": "Linear",
  "sceneArchetypeId": "inn_lodging",
  "isStarter": true,
  "locationActivationFilter": {
    "placementType": "Location",
    "capabilities": ["Commercial", "Restful"]
  },
  "npcActivationFilter": {
    "placementType": "NPC",
    "professions": ["Innkeeper"]
  }
}
```

**Archetype Generation**: `SceneArchetypeCatalog.GenerateInnLodging()` generates:
- Situation 1: `inn_lodging_negotiate` (identity formation choices)
- Situation 2: `inn_lodging_rest` (private room choices)
- Situation 3: `inn_lodging_depart` (departure choices with A2 spawn)

**Dependent Resources**:
- `private_room` Location (created at activation)
- `room_key` Item (added to inventory)

### 11.4.2 A2: First Delivery

```json
{
  "id": "a2_morning",
  "category": "MainStory",
  "mainStorySequence": 2,
  "sceneArchetypeId": "delivery_contract",
  "locationActivationFilter": {
    "placementType": "Location",
    "privacyLevels": ["Public"],
    "capabilities": ["Commercial"]
  },
  "npcActivationFilter": {
    "placementType": "NPC",
    "professions": ["Merchant"]
  }
}
```

### 11.4.3 A3: Route Travel

```json
{
  "id": "a3_route_travel",
  "category": "MainStory",
  "mainStorySequence": 3,
  "sceneArchetypeId": "route_segment_travel",
  "locationActivationFilter": {
    "placementType": "Location",
    "activityLevels": ["Quiet"],
    "capabilities": ["Outdoor"]
  }
}
```

## 11.5 Scene Chaining Mechanism

### 11.5.1 How A1 Completion Triggers A2

**In SceneArchetypeCatalog.GenerateInnLodging()** (lines 359-370):

```csharp
// A1 tutorial: ALL departure choices spawn A2
if (isA1Tutorial)
{
    earlyDepartureReward.ScenesToSpawn = new List<SceneSpawnReward>
    {
        new SceneSpawnReward { SceneTemplateId = "a2_morning" }
    };
    socializeReward.ScenesToSpawn = new List<SceneSpawnReward>
    {
        new SceneSpawnReward { SceneTemplateId = "a2_morning" }
    };
}
```

**Execution Flow**:
1. Player completes final situation choice
2. `SituationChoiceExecutor` applies reward
3. `RewardApplicationService.FinalizeSceneSpawns()` spawns A2:
   ```csharp
   foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
   {
       // Find template, create deferred scene
       await _sceneInstanceFacade.SpawnScene(template, spawnReward, context);
   }
   ```

### 11.5.2 Procedural Chaining (A4+)

**In AStorySceneArchetypeCatalog.EnrichFinalSituationWithNextASceneSpawn()** (line 1282):

```csharp
// Next A-scene ID (generic - no special cases)
string nextASceneId = $"a_story_{context.AStorySequence.Value + 1}";

// Enrich ALL choices with SceneSpawnReward
reward.ScenesToSpawn = new List<SceneSpawnReward>
{
    new SceneSpawnReward { SceneTemplateId = nextASceneId }
};
```

This ensures infinite progression: A11 → A12 → A13 → ... ad infinitum.

## 11.6 Entity Resolution Flow

### 11.6.1 Why Deferred Resolution?

Entities (Location, NPC, Item) cannot be resolved at scene creation because:
1. Dependent resources don't exist yet
2. Categorical filters need entity pool to search
3. Object references must point to actual GameWorld entities

### 11.6.2 Resolution Sequence

**Phase 1: Deferred Creation**
- Scene created with PlacementFilters (categorical specifications)
- Situation.Location = null, Situation.Npc = null (not resolved)
- State = Deferred

**Phase 2: Activation (on location entry)**
```csharp
// Generate dependent resources
string resourceJson = _sceneInstantiator.ActivateScene(scene, activationContext);
await _packageLoaderFacade.LoadDynamicPackage(resourceJson, packagePath);

// NOW resolve entity references
_sceneInstantiator.ResolveSceneEntityReferences(scene, activationContext);
```

**Phase 3: Resolution**
```csharp
// EntityResolver.FindOrCreate searches GameWorld
Location location = entityResolver.FindLocationByCategoricalFilter(
    situation.Template.LocationFilter,
    context.CurrentVenue
);
situation.Location = location;  // Object reference assigned
```

## 11.7 Debugging Guide

### 11.7.1 Scene Not Activating

**Check these in order:**

1. **Is scene in GameWorld.Scenes?**
   ```csharp
   var scenes = _gameWorld.Scenes.Where(s => s.TemplateId == "a1_secure_lodging");
   // Should find 1 scene
   ```

2. **Is scene State == Deferred?**
   ```csharp
   scene.State == SceneState.Deferred  // Must be Deferred to activate
   ```

3. **Does LocationActivationFilter match current location?**
   - Check location's Capabilities, Privacy, Safety, Activity, Purpose
   - Compare against scene's LocationActivationFilter

4. **Is CheckAndActivateDeferredScenes being called?**
   - Called from `LocationFacade.MoveToSpot()`
   - Look for console log: `[LocationFacade] Activating deferred scene...`

### 11.7.2 Scene Activated But No NPC Interaction

1. **Is scene State == Active?**

2. **Does CurrentSituation.Npc match the NPC you're talking to?**
   ```csharp
   scene.CurrentSituation?.Npc == npc  // Object equality check
   ```

3. **Was ResolveSceneEntityReferences called?**
   - Look for: `[LocationFacade] ✅ Resolved entity references for scene`

4. **Does NPC match the situation's NpcFilter?**
   - Check NPC's Profession, SocialStanding, etc.

### 11.7.3 A2 Not Spawning After A1 Completion

1. **Did the final choice have ScenesToSpawn?**
   - Check `inn_lodging_depart` choice templates
   - Should have `RewardTemplate.ScenesToSpawn = [{ SceneTemplateId: "a2_morning" }]`

2. **Was FinalizeSceneSpawns called?**
   - Look for: `[RewardApplicationService] Spawning scene...`

3. **Does a2_morning template exist?**
   ```csharp
   var template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == "a2_morning");
   ```

4. **Is A2 in Deferred state waiting for activation?**
   - Check `GameWorld.Scenes` for State == Deferred

### 11.7.4 Console Log Markers to Watch

| Log Pattern | Meaning |
|-------------|---------|
| `[GameFacade] Created deferred starter scene` | Phase 1 complete |
| `[LocationFacade] Activating deferred scene` | Phase 2 starting |
| `[SceneActivation] Loaded dependent resources` | Resources created |
| `[LocationFacade] ✅ Resolved entity references` | References assigned |
| `Scene activated successfully (State: Deferred → Active)` | Phase 2 complete |

## 11.8 Key File Locations

| Component | File Path |
|-----------|-----------|
| Tutorial JSON | `src/Content/Core/22_a_story_tutorial.json` |
| Scene archetype generation | `src/Content/Catalogs/SceneArchetypeCatalog.cs` |
| A-story archetype generation | `src/Content/Catalogs/AStorySceneArchetypeCatalog.cs` |
| Deferred scene creation | `src/Subsystems/ProceduralContent/SceneInstanceFacade.cs` |
| Activation trigger check | `src/Subsystems/Location/LocationFacade.cs:423-483` |
| Categorical matching | `src/Subsystems/Location/LocationFacade.cs:490-521` |
| Entity resolution | `src/Content/SceneInstantiator.cs` |
| Scene chaining | `src/Subsystems/Consequence/RewardApplicationService.cs:201-250` |
| Game initialization | `src/Services/GameFacade.cs:759-778` |

## 11.9 Architecture Principles

### 11.9.1 HIGHLANDER Pipeline

All content (static and dynamic) flows through the same path:
```
JSON → Parser → Entity
```

Dynamic scenes generate JSON at runtime, written to disk, loaded via PackageLoader. No direct entity creation.

### 11.9.2 Categorical Over ID-Based

Scenes use categorical filters, not hardcoded IDs:
- **Correct**: `capabilities: ["Commercial", "Restful"]`
- **Incorrect**: `locationId: "common_room"`

This enables procedural generation and hex-map independence.

### 11.9.3 Situation Owns Placement

Each situation has its own entity references:
- `Situation.Location` (not Scene.Location)
- `Situation.Npc` (not Scene.Npc)

This enables multi-location scenes (negotiate at common_room, rest at private_room).

### 11.9.4 Three-Tier Timing Model

1. **Parse-Time**: Templates created (SceneTemplate, SituationTemplate, ChoiceTemplate)
2. **Spawn-Time**: Instances created (Scene, Situation with resolved entities)
3. **Query-Time**: Actions created fresh (ephemeral LocationAction, NPCAction)

Actions never stored, always rebuilt from templates on each query.

## 11.10 Summary

The tutorial system relies on categorical triggers, not hardcoded IDs. Scenes activate when player enters locations matching their activation filters. Entity references resolve AFTER dependent resources are created. Scene chaining happens via ChoiceReward.ScenesToSpawn. All content flows through the HIGHLANDER pipeline (JSON → Parser → Entity).

When debugging:
1. Check scene State (Deferred vs Active)
2. Verify categorical filter matches
3. Confirm entity resolution completed
4. Look for console log markers

The system is complex but intentionally so - it enables procedural generation, hex-map independence, and infinite A-story continuation.
