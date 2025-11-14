# Scene/Situation Architecture Vision

## Overview

This document describes the INTENDED architecture for Scene/Situation spawning, placement, progression, and location accessibility. This is the target design we are implementing, NOT the current codebase state.

**Core Design Principles:**
1. **Query-Based Accessibility**: Location accessibility derived from active situations, not flag modification
2. **Index-Based Progression**: Simple integer index for situation sequencing
3. **Derived Projection**: Perfect information computed at display time from next situation
4. **Specification-Only System 3**: Package generation writes categorical specs, never resolves entities
5. **Categorical Entity Resolution**: System 4 resolves ALL entities from categories (no concrete IDs)

---

## 1. Five-System Scene Spawning Architecture

### System 1: Selection Decision (When)

**Purpose**: Determines WHEN a scene template becomes eligible to spawn.

**Mechanism**:
- SpawnConditions declarative specification
- Evaluated at query time by GameFacade
- Boolean gates based on player state (knowledge, achievements, story progress)
- Sentinel pattern: `SpawnConditions.AlwaysEligible` for starter content

**No Changes from Current**: This system already works correctly.

### System 2: Categorical Specification (Where)

**Purpose**: Specifies WHERE a scene should appear using ONLY categorical filters.

**Mechanism**:
- PlacementFilter with categorical dimensions ONLY
- NO concrete ID properties (no NpcId, no LocationId, no RouteId)
- CSS-style inheritance: SituationTemplate.LocationFilter overrides SceneTemplate.BaseLocationFilter
- Hierarchical placement: Each situation specifies its own categorical filter

**Key Design**: CATEGORICAL ONLY. Authored scenes specify categories that match specific entities, not concrete IDs.

**Example - Secure Lodging Tutorial**:
```json
SceneTemplate: {
  "baseLocationFilter": {
    "locationTypes": ["Inn"]
  },
  "situations": [
    {
      "locationFilter": {
        "locationTypes": ["CommonRoom"],
        "locationTags": ["main_hall"]
      }
    },
    {
      "locationFilter": {
        "locationTypes": ["GuestRoom"],
        "privacyLevels": ["Private"],
        "purposes": ["Dwelling"]
      }
    }
  ]
}
```

**Result**: Situation 1 matches common_room (categorical), Situation 2 matches private_room (categorical). NO concrete IDs.

### System 3: Package Generation (What Specifications)

**Purpose**: Writes PlacementFilterDTO categorical specifications for self-contained scenes.

**Component**: SceneInstantiator

**Mechanism**:
- Self-contained scenes generate PlacementFilterDTO specifications
- Writes categorical dimensions: "GuestRoom + Private + Dwelling"
- Does NOT resolve entities (that's System 4's job)
- Does NOT create Location objects (that's System 4's job)
- Pure specification writing

**Key Design Change**: System 3 ONLY writes categorical specs. It never calls EntityResolver, never creates entities, never generates content.

**Output**: SceneDTO with PlacementFilterDTO specifications for each situation.

### System 4: Entity Resolution (Which Entities)

**Purpose**: Resolves categorical specifications to actual entity objects OR creates new entities if no match.

**Component**: EntityResolver

**Mechanism**:
- Receives PlacementFilterDTO from System 3
- FindOrCreateLocation(filter) - searches GameWorld for categorical match
- If found: Return existing entity
- If not found: CreateLocationFromCategories(filter) - generates new entity
- Selection strategies: Random, First, Closest, HighestBond, LeastRecent

**Key Design**: 100% categorical. NO concrete ID lookup logic. Every entity resolution uses categorical matching.

**Example Flow**:
```
PlacementFilterDTO: { LocationTypes: ["GuestRoom"], PrivacyLevels: ["Private"] }
  ↓
EntityResolver.FindMatchingLocation()
  ↓
Search GameWorld.Locations for match
  ↓
Found: Return existing private_room
Not Found: CreateLocationFromCategories() → Generate new private_room
  ↓
Return Location object
```

### System 5: Scene Instantiation (Materialize)

**Purpose**: Creates Scene/Situation instances with resolved entity references.

**Component**: SceneParser, SituationParser

**Mechanism**:
- Receives SceneDTO with PlacementFilterDTO specifications
- For each SituationDTO: Call EntityResolver to get Location/NPC/Route objects
- Assign resolved objects to Situation.Location/Npc/Route properties
- Create Scene with Situations list and CurrentSituationIndex = 0

**Output**: Scene with embedded Situations, each owning Location/NPC/Route object references.

---

## 2. Situation Spatial Placement (Entity Ownership)

**Architectural Truth**: Situations own Location/NPC/Route references. Scenes are containers.

### Ownership Pattern

**Scene Entity**:
- NO Location property
- NO Npc property
- NO Route property
- Situations property (List<Situation>) - ordered array
- CurrentSituationIndex property (int) - points to active situation

**Situation Entity**:
- Location property (object reference) - resolved by System 4
- Npc property (object reference) - resolved by System 4
- Route property (object reference) - resolved by System 4
- Template property (object reference) - for accessing template properties

**Key Design**: No CurrentSituation object pointer. Use index arithmetic instead: `Situations[CurrentSituationIndex]`

### Hierarchical Placement Filters

**CSS-Style Inheritance**:
- SceneTemplate.BaseLocationFilter (shared baseline categorical spec)
- SituationTemplate.LocationFilter (override categorical spec)
- Parser applies: Situation filter ?: Scene base filter

**Example - Secure Lodging Tutorial**:
```
SceneTemplate: BaseLocationFilter = { LocationTypes: ["Inn"] }
  Situation 1: LocationFilter = { LocationTypes: ["CommonRoom"] }
  Situation 2: LocationFilter = { LocationTypes: ["GuestRoom"], PrivacyLevels: ["Private"] }
  Situation 3: Inherits BaseLocationFilter
```

Result: Each situation has categorical filter → System 4 resolves to objects → Situations own object references.

---

## 3. Location Visibility & Accessibility

**Core Pattern**: Query-based accessibility from active situations. NO flag modification.

### Designed Architecture

**Location Entity**:
- NO IsLocked property
- NO IsVisible property
- NO IsAccessible property
- Pure data entity (categorical dimensions only)

**SituationTemplate Entity**:
- GrantsLocationAccess property (bool) - "This situation grants access to its Location"
- When situation active + GrantsLocationAccess true + player at situation.Location → Location accessible

**Accessibility Query Logic**:
```csharp
public bool IsLocationAccessible(Location location)
{
    // Check active scenes for situations granting access to this location
    foreach (Scene scene in GameWorld.Scenes.Where(s => s.State == SceneState.Active))
    {
        Situation currentSituation = scene.Situations[scene.CurrentSituationIndex];

        // Active situation grants access to its location
        if (currentSituation.Template.GrantsLocationAccess &&
            currentSituation.Location?.Id == location.Id)
        {
            return true;
        }
    }

    // Location always accessible if it has no situations requiring it
    return !LocationRequiredByAnySituation(location);
}
```

**Key Design**: Pure query, no state modification, derived accessibility.

### Secure Lodging Pattern

**How Temporary Accessibility Works**:

1. **Scene Spawn**: System 4 creates private_room (or finds existing)
2. **Situation 1 Active**: At common_room, GrantsLocationAccess = false
3. **Player Selects Choice**: CurrentSituationIndex++ (advance to Situation 2)
4. **Situation 2 Active**: At private_room, GrantsLocationAccess = true
5. **Query Returns True**: IsLocationAccessible(private_room) → Active situation grants access
6. **Navigation Enabled**: MovementValidator allows movement to private_room
7. **Situation 3 Completes**: CurrentSituationIndex++ (no more situations)
8. **Scene State = Complete**: Removed from active scenes
9. **Query Returns False**: No active situation grants access to private_room
10. **Navigation Disabled**: MovementValidator blocks movement
11. **Persistence**: Location remains in GameWorld.Locations forever but inaccessible

**Example Template**:
```json
{
  "situations": [
    {
      "locationFilter": { "locationTypes": ["CommonRoom"] },
      "grantsLocationAccess": false
    },
    {
      "locationFilter": { "locationTypes": ["GuestRoom"], "privacyLevels": ["Private"] },
      "grantsLocationAccess": true
    },
    {
      "locationFilter": { "locationTypes": ["GuestRoom"] },
      "grantsLocationAccess": true
    }
  ]
}
```

**Why This Works**:
- Situation 2 grants access to private_room while active
- When Situation 2/3 complete and scene removed, query returns false
- No flag modification, pure query-based accessibility
- Location persists but unreachable (unless future scene activates at that location)

---

## 4. Sequential Situation Progression

**State Machine Pattern**: Scene.CurrentSituationIndex integer advances through Situations array.

### Progression Mechanism

**Scene Entity**:
- Situations property (List<Situation>) - ordered array
- CurrentSituationIndex property (int) - index into Situations array
- AdvanceToNextSituation() method - simple index++ logic

**Situation Entity**:
- Template property (object reference) - for accessing template properties
- Location/Npc/Route properties (object references) - resolved placement
- No lifecycle status properties (derived from index position)

**Transition Flow**:
1. Player selects choice from Situations[CurrentSituationIndex]
2. ChoiceConsequenceHandler executes rewards (grants knowledge, items, etc.)
3. SituationCompletionHandler calls Scene.AdvanceToNextSituation()
4. Scene.CurrentSituationIndex++
5. UI determines routing based on placement comparison

**Routing Logic**:
```csharp
public SceneRoutingDecision AdvanceToNextSituation()
{
    Situation current = Situations[CurrentSituationIndex];
    CurrentSituationIndex++;

    // Check if scene complete
    if (CurrentSituationIndex >= Situations.Count)
    {
        State = SceneState.Complete;
        return SceneRoutingDecision.SceneComplete;
    }

    Situation next = Situations[CurrentSituationIndex];

    // Same context = cascade in modal
    if (next.Location?.Id == current.Location?.Id &&
        next.Npc?.ID == current.Npc?.ID)
    {
        return SceneRoutingDecision.ContinueInScene;
    }

    // Different context = player must navigate
    return SceneRoutingDecision.ExitToWorld;
}
```

**Example - Secure Lodging**:
- Situation 0 at common_room → Situation 1 at private_room: DIFFERENT LOCATION → ExitToWorld
- Player navigates to private_room (accessible because Situation 1 active + GrantsLocationAccess)
- Situation 1 at private_room → Situation 2 at private_room: SAME LOCATION → ContinueInScene

### Query-Time Action Instantiation

**Deferred Creation Pattern** (unchanged from current):
- Scene spawns with Situations in Deferred state
- NO actions in GameWorld collections yet
- When player enters context, SceneFacade instantiates actions from templates
- Actions appear in GameWorld collections for UI query

**This part already works correctly.**

---

## 5. Perfect Information Projection

**Design Principle**: All consequences visible before choice at strategic layer. Information DERIVED at display time, not stored.

### Live Query Pattern

**NO Stored Projection Properties**:
- ~~Situation.ProjectedBondChanges~~ (DELETE)
- ~~Situation.ProjectedScaleShifts~~ (DELETE)
- ~~Situation.ProjectedStates~~ (DELETE)

**YES Live Query Logic**:
```csharp
public SituationProjection GetNextSituationProjection(Scene scene)
{
    int nextIndex = scene.CurrentSituationIndex + 1;
    if (nextIndex >= scene.Situations.Count)
        return null; // Scene will complete

    Situation nextSituation = scene.Situations[nextIndex];

    return new SituationProjection
    {
        Name = nextSituation.Template.Name,
        Description = nextSituation.Template.Description,
        Location = nextSituation.Location?.Name,
        GrantsAccess = nextSituation.Template.GrantsLocationAccess,
        RequiredKnowledge = nextSituation.Template.RequiredKnowledge,
        // Derive all information from template + resolved objects
    };
}
```

**UI Presentation**:
- Strategic layer queries next situation at display time
- Shows: "Next: Private Room Investigation (unlocks access to private quarters)"
- Player sees progression path before selecting choice
- Information always current (no stale cached projections)

**Key Design**: Derive, don't store. Query next situation from array, read template properties, display.

---

## 6. Temporary Situation-Driven Accessibility (Secure Lodging Pattern)

**Complete Pattern Documentation**:

### Narrative Flow

**Scene**: Secure Lodging Tutorial (tutorial_secure_lodging)
**Goal**: Teach player multi-situation scene progression with temporary location accessibility

**Situation 0: Negotiate Lodging**
- **Context**: common_room (Location), Elena (NPC)
- **Template.GrantsLocationAccess**: false (doesn't grant access to common_room)
- **Choices**: Pay coins, negotiate, alternative approaches
- **Outcome**: CurrentSituationIndex++ (advance to Situation 1)
- **Routing Decision**: ExitToWorld (next situation at different location)

**Situation 1: Settle Into Room**
- **Context**: private_room (Location), no NPC
- **Template.GrantsLocationAccess**: true (grants access to private_room)
- **Query Result**: IsLocationAccessible(private_room) → true (active situation grants access)
- **Player Navigation**: Can now move to private_room
- **Choices**: Explore room, rest, prepare
- **Outcome**: CurrentSituationIndex++ (advance to Situation 2)
- **Routing Decision**: ContinueInScene (same location)

**Situation 2: Prepare for Departure**
- **Context**: private_room (Location), no NPC
- **Template.GrantsLocationAccess**: true (still grants access)
- **Choices**: Final preparations, gather belongings
- **Outcome**: CurrentSituationIndex++ (index now >= Situations.Count)
- **Routing Decision**: SceneComplete (no more situations)

**Post-Scene State**:
- Scene.State = SceneState.Complete
- Scene removed from GameWorld.Scenes (or filtered from active queries)
- private_room remains in GameWorld.Locations (persistence)
- IsLocationAccessible(private_room) → false (no active situation grants access)
- MovementValidator blocks navigation to private_room
- Location persists forever but unreachable

### Technical Implementation

**Self-Contained Scene Generation**:
- System 3 writes PlacementFilterDTO for private_room (categorical spec)
- System 4 resolves: FindOrCreateLocation() → creates new Location or finds existing
- Provenance marks room as generated by this scene template

**Query-Based Accessibility**:
- NO Location.IsLocked flag
- NO ChoiceReward.LocationsToUnlock property
- NO RewardApplicationService state modification
- Pure query: Active situation + GrantsLocationAccess + Location match = accessible

**Context-Based Activation**:
- Situation 0 requires common_room context (categorical match)
- Situation 1 requires private_room context (categorical match)
- Situation 2 requires private_room context (same as Situation 1)

**Sequential Progression**:
- Scene.CurrentSituationIndex advances: 0 → 1 → 2 → Complete
- Simple index arithmetic, no state machine complexity
- Routing derived from placement comparison

---

## 7. Implementation Delta (Current vs Vision)

### What Needs to Change

**DELETE from Current Architecture**:
1. Location.IsLocked property (flag-based accessibility)
2. ChoiceReward.LocationsToUnlock/LocationsToLock properties
3. RewardApplicationService lock/unlock logic
4. Scene.CurrentSituation object pointer
5. Scene.AdvanceToNextSituation() complex state machine
6. Scene.CompareContexts() method
7. Situation.ProjectedBondChanges/ProjectedScaleShifts/ProjectedStates stored properties
8. Situation.ResolvedRequiredLocationId/NpcId legacy string properties
9. PlacementFilterDTO.NpcId/LocationId/RouteId concrete ID properties
10. SceneInstantiator entity generation logic (moves to EntityResolver)

**ADD to New Architecture**:
1. SituationTemplate.GrantsLocationAccess property
2. Scene.CurrentSituationIndex integer property
3. MovementValidator.IsLocationAccessible() query method
4. SceneFacade.GetNextSituationProjection() query method
5. Refactor SceneInstantiator to write PlacementFilterDTO only
6. Refactor EntityResolver to handle FindOrCreate for generated content

**KEEP Unchanged**:
1. Five-system pipeline structure
2. Situation ownership of Location/NPC/Route
3. Hierarchical placement filters
4. Query-time action instantiation
5. SpawnConditions evaluation
6. Categorical entity resolution

### Migration Strategy

**Phase 1: Add New Properties** (non-breaking)
- Add SituationTemplate.GrantsLocationAccess to DTO/Template/Parser
- Add Scene.CurrentSituationIndex to Scene entity
- Add IsLocationAccessible() query method to MovementValidator

**Phase 2: Parallel Implementation** (coexistence)
- New query logic checks GrantsLocationAccess
- Old flag logic still works
- Both paths functional

**Phase 3: Delete Legacy** (breaking changes)
- Remove Location.IsLocked property
- Remove ChoiceReward lock/unlock properties
- Remove Scene.CurrentSituation pointer
- Remove stored projection properties
- Remove concrete ID properties

**Phase 4: Refactor System 3** (cleanup)
- SceneInstantiator writes specs only
- EntityResolver owns FindOrCreate entirely

---

## Summary

**Wayfarer's Scene/Situation architecture implements query-based accessibility from active situations, index-based progression through situation arrays, and derived perfect information projection.**

The secure lodging tutorial pattern demonstrates the complete flow:
1. Scene spawns with Situations[0, 1, 2] and CurrentSituationIndex = 0
2. System 4 resolves categorical filters to Location objects (common_room, private_room)
3. Situation 0 active at common_room, GrantsLocationAccess = false
4. Player selects choice, CurrentSituationIndex++ (now 1)
5. Situation 1 active at private_room, GrantsLocationAccess = true
6. IsLocationAccessible(private_room) queries active situations, returns true
7. Player navigates to private_room, completes Situation 1
8. CurrentSituationIndex++ (now 2), Situation 2 active
9. Situation 2 completes, CurrentSituationIndex++ (now 3, out of bounds)
10. Scene.State = Complete, removed from active scenes
11. IsLocationAccessible(private_room) returns false (no active situation)
12. Location persists but inaccessible

**Core Design Differences from Current**:
- Query-based accessibility (not flag-based)
- Integer index progression (not object pointer)
- Derived projection (not stored properties)
- System 3 writes specs (doesn't generate entities)
- 100% categorical resolution (no concrete IDs)

**This is the architecture we are building.**
