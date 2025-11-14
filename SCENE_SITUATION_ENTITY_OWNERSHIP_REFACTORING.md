# Scene-Situation Entity Ownership Refactoring

## Root Cause Analysis

**Bug:** a1_arrival scene spawns but fails to activate at Elena's NPC context.

**Server Logs Evidence:**
```
[Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' checking activation at location 'common_room', npc ''
```

Scene receives EMPTY npc context (`''`) instead of `'elena'`.

**Architectural Root Cause:** ENTITY OWNERSHIP VIOLATION

Scene.cs owns Location/NPC/Route properties (lines 42, 51, 60), but Situation.cs stores STRING IDs (ResolvedRequiredLocationId, ResolvedRequiredNpcId) instead of entity references.

This creates:
1. **State Duplication**: Scene.Location entity vs Situation.ResolvedRequiredLocationId string
2. **Manual ID Copying**: SceneParser.cs lines 134-135 manually extract IDs from entities
3. **Template Overwriting**: SceneParser blindly copies Scene placement to ALL situations, ignoring per-situation Template requirements
4. **Coupling**: Situation.GetPlacementId() queries ParentScene.Location instead of own property

## Architectural Truth (from User)

> "scene should not have location and npc. situation should have it. one scene can have multiple situations at different locations or npc."

**Why this is correct:**
- Scene is narrative CONTAINER (owns Situations collection)
- Each Situation can require DIFFERENT Location/NPC context
- Scene.CompareContexts (lines 387-393) already compares consecutive situation contexts
- Scene.ShouldActivateAtContext (line 350) falls back to SituationTemplate.RequiredLocationId
- Multi-situation scenes progress across locations: Situation 1 at common_room → Situation 2 at upper_floor

**Current Code Violates This:**
- Scene owns Location/NPC/Route (WRONG - Scene is container, not context owner)
- Situation stores STRING IDs (WRONG - should be entity references)
- SceneParser overwrites all situation contexts with Scene placement (WRONG - ignores Template requirements)

## SceneParser Architectural Cancer (lines 134-135)

```csharp
// CRITICAL: Populate resolved context IDs from Scene placement for activation matching
// Scene.ShouldActivateAtContext checks ResolvedRequiredLocationId/NpcId to determine if scene should resume
// Flow: Scene.Npc object (from PlacementFilter resolution) → situation.ResolvedRequiredNpcId (for activation check)
// This bridges System 5 (Scene placement) to Scene.ShouldActivateAtContext runtime checks
situation.ResolvedRequiredLocationId = scene.Location?.Id;  // OVERWRITES template requirement!
situation.ResolvedRequiredNpcId = scene.Npc?.ID;  // OVERWRITES template requirement!
```

**Problems:**
1. Blindly copies Scene placement to ALL situations
2. Overwrites each Situation's Template-defined RequiredLocationId/NpcId
3. Makes all situations activate at same context (violates multi-location progression)
4. Manually extracts STRING ID from entity reference (state duplication)
5. Comment claims this is "critical" but it's actually architectural violation

**Evidence of Overwriting:**
- SituationTemplate.RequiredLocationId = "upper_floor" (from JSON)
- SceneParser overwrites: situation.ResolvedRequiredLocationId = "common_room" (from Scene.Location)
- Result: Situation 2 never activates at upper_floor (context mismatch)

## Correct Architecture

### Entity Ownership

**Scene:**
- Owns Situations collection (composition)
- Owns SpawnRules (transition logic)
- Owns CurrentSituation reference (progression tracking)
- Does NOT own Location/NPC/Route (these belong to Situations)

**Situation:**
- Owns Location/NPC/Route entity references (context ownership)
- Each Situation can have DIFFERENT context from other Situations in same Scene
- Activation checks own Location/NPC (not parent Scene's)

### Data Flow

**Spawn Time (PackageLoader → SceneParser):**
1. PackageLoader resolves Scene PlacementFilter → resolvedLocation, resolvedNpc, resolvedRoute
2. SceneParser creates Scene with these entities (initial placement)
3. For each SituationDTO:
   - Check SituationTemplate.RequiredLocationId/NpcId
   - If present: Resolve from GameWorld (query by ID)
   - If absent: Use Scene's placement entities (inherit from parent)
   - Set situation.Location/Npc entity references

**Activation Time (Scene.ShouldActivateAtContext):**
1. Player enters context (location + optional NPC)
2. Scene checks: CurrentSituation.Location?.Id == locationId
3. Scene checks: CurrentSituation.Npc?.ID == npcId
4. Both match → Situation activates

**Progression Time (Scene.CompareContexts):**
1. Situation completes, Scene advances to next Situation
2. Compare contexts: previousSituation.Location?.Id vs nextSituation.Location?.Id
3. Same context → ContinueInScene (seamless cascade)
4. Different context → ExitToWorld (player must navigate)

## Refactoring Plan

### 1. Add Entity References to Situation.cs

**AFTER line 144 (after ResolvedRequiredNpcId), ADD:**
```csharp
/// <summary>
/// Location where this situation activates
/// Each situation can require different location from other situations in same scene
/// Used for context matching: player must be at this location for situation to activate
/// null = situation has no location requirement (activates anywhere)
/// </summary>
public Location Location { get; set; }

/// <summary>
/// NPC associated with this situation (interaction partner)
/// Each situation can require different NPC from other situations in same scene
/// Used for context matching: player must be with this NPC for situation to activate
/// null = situation has no NPC requirement (location-only situation)
/// </summary>
public NPC Npc { get; set; }

/// <summary>
/// Route associated with this situation (travel context)
/// Each situation can require different route from other situations in same scene
/// Used for context matching in route-specific situations
/// null = situation has no route requirement
/// </summary>
public RouteOption Route { get; set; }
```

### 2. Delete STRING ID Properties from Situation.cs

**DELETE lines 127-145 (ResolvedRequiredLocationId and ResolvedRequiredNpcId):**
```csharp
// DELETE THIS ARCHITECTURAL CANCER:
/// <summary>
/// Resolved location ID for self-contained scenes (marker resolution at finalization)
/// Template.RequiredLocationId may contain marker ("generated:private_room")
/// This property contains actual resolved ID ("scene_abc123_private_room")
/// null = use Template.RequiredLocationId directly (no marker resolution needed)
/// Populated during SceneInstantiator.FinalizeScene from marker resolution
/// Context matching uses this property, NOT template property
/// </summary>
public string ResolvedRequiredLocationId { get; set; }

/// <summary>
/// Resolved NPC ID for self-contained scenes (marker resolution at finalization)
/// Template.RequiredNpcId may contain marker (if NPCs were dynamically created)
/// This property contains actual resolved ID after finalization
/// null = use Template.RequiredNpcId directly (no marker resolution needed)
/// Populated during SceneInstantiator.FinalizeScene from marker resolution
/// Context matching uses this property, NOT template property
/// </summary>
public string ResolvedRequiredNpcId { get; set; }
```

### 3. Update Situation.GetPlacementId() (lines 242-254)

**FROM (coupling to ParentScene):**
```csharp
public string GetPlacementId(PlacementType placementType)
{
    if (ParentScene == null)
        return null;

    return placementType switch
    {
        PlacementType.Location => ParentScene.Location?.Id,
        PlacementType.NPC => ParentScene.Npc?.ID,
        PlacementType.Route => ParentScene.Route?.Id,
        _ => null
    };
}
```

**TO (own properties):**
```csharp
public string GetPlacementId(PlacementType placementType)
{
    return placementType switch
    {
        PlacementType.Location => Location?.Id,
        PlacementType.NPC => Npc?.ID,
        PlacementType.Route => Route?.Id,
        _ => null
    };
}
```

### 4. Update Scene.ShouldActivateAtContext (lines 350-351)

**FROM (STRING IDs):**
```csharp
string requiredLocationId = CurrentSituation.ResolvedRequiredLocationId ?? CurrentSituation.Template.RequiredLocationId;
string requiredNpcId = CurrentSituation.ResolvedRequiredNpcId ?? CurrentSituation.Template.RequiredNpcId;
```

**TO (entity references with template fallback):**
```csharp
// Use situation's resolved entity reference, fall back to template ID string if entity not resolved
string requiredLocationId = CurrentSituation.Location?.Id ?? CurrentSituation.Template?.RequiredLocationId;
string requiredNpcId = CurrentSituation.Npc?.ID ?? CurrentSituation.Template?.RequiredNpcId;
```

### 5. Update Scene.CompareContexts (lines 387-393)

**FROM (STRING IDs):**
```csharp
string prevLocationId = previousSituation.ResolvedRequiredLocationId ?? previousSituation.Template.RequiredLocationId;
string nextLocationId = nextSituation.ResolvedRequiredLocationId ?? nextSituation.Template.RequiredLocationId;

string prevNpcId = previousSituation.ResolvedRequiredNpcId ?? previousSituation.Template.RequiredNpcId;
string nextNpcId = nextSituation.ResolvedRequiredNpcId ?? nextSituation.Template.RequiredNpcId;
```

**TO (entity references with template fallback):**
```csharp
string prevLocationId = previousSituation.Location?.Id ?? previousSituation.Template?.RequiredLocationId;
string nextLocationId = nextSituation.Location?.Id ?? nextSituation.Template?.RequiredLocationId;

string prevNpcId = previousSituation.Npc?.ID ?? previousSituation.Template?.RequiredNpcId;
string nextNpcId = nextSituation.Npc?.ID ?? nextSituation.Template?.RequiredNpcId;
```

### 6. Update SceneParser.cs (lines 124-136)

**FROM (manual ID copying, template overwriting):**
```csharp
Situation situation = SituationParser.ConvertDTOToSituation(situationDto, gameWorld);

// CRITICAL: Set composition relationship (Situation → ParentScene)
// Required for GetPlacementId() which queries ParentScene.Location/Npc/Route objects
situation.ParentScene = scene;

// CRITICAL: Populate resolved context IDs from Scene placement for activation matching
// Scene.ShouldActivateAtContext checks ResolvedRequiredLocationId/NpcId to determine if scene should resume
// Flow: Scene.Npc object (from PlacementFilter resolution) → situation.ResolvedRequiredNpcId (for activation check)
// This bridges System 5 (Scene placement) to Scene.ShouldActivateAtContext runtime checks
situation.ResolvedRequiredLocationId = scene.Location?.Id;
situation.ResolvedRequiredNpcId = scene.Npc?.ID;
```

**TO (per-situation entity resolution):**
```csharp
Situation situation = SituationParser.ConvertDTOToSituation(situationDto, gameWorld);

// Set composition relationship (Situation → ParentScene)
situation.ParentScene = scene;

// Resolve situation-specific context from template requirements
// Each situation can require different Location/NPC from other situations in same scene
// If template specifies RequiredLocationId/NpcId, resolve from GameWorld
// Otherwise inherit Scene's placement entities (default to parent context)
if (situation.Template != null)
{
    // Location resolution
    if (!string.IsNullOrEmpty(situation.Template.RequiredLocationId))
    {
        // Situation has specific location requirement - resolve from GameWorld
        situation.Location = gameWorld.Locations.FirstOrDefault(loc => loc.Id == situation.Template.RequiredLocationId);
        if (situation.Location == null)
        {
            Console.WriteLine($"[SceneParser] WARNING: Situation '{situation.Id}' template requires LocationId '{situation.Template.RequiredLocationId}' " +
                $"but no such location found in GameWorld");
        }
    }
    else
    {
        // Situation has no specific requirement - inherit from Scene placement
        situation.Location = scene.Location;
    }

    // NPC resolution
    if (!string.IsNullOrEmpty(situation.Template.RequiredNpcId))
    {
        // Situation has specific NPC requirement - resolve from GameWorld
        situation.Npc = gameWorld.NPCs.FirstOrDefault(npc => npc.ID == situation.Template.RequiredNpcId);
        if (situation.Npc == null)
        {
            Console.WriteLine($"[SceneParser] WARNING: Situation '{situation.Id}' template requires NpcId '{situation.Template.RequiredNpcId}' " +
                $"but no such NPC found in GameWorld");
        }
    }
    else
    {
        // Situation has no specific requirement - inherit from Scene placement
        situation.Npc = scene.Npc;
    }

    // Route resolution (rare - most situations don't require specific routes)
    if (!string.IsNullOrEmpty(situation.Template.RequiredRouteId))
    {
        situation.Route = gameWorld.RouteOptions.FirstOrDefault(route => route.Id == situation.Template.RequiredRouteId);
        if (situation.Route == null)
        {
            Console.WriteLine($"[SceneParser] WARNING: Situation '{situation.Id}' template requires RouteId '{situation.Template.RequiredRouteId}' " +
                $"but no such route found in GameWorld");
        }
    }
    else
    {
        situation.Route = scene.Route;
    }
}
```

### 7. Update SceneFacade.BuildContextFromParentScene (lines 299-307)

**FROM (Scene.Location/Npc/Route):**
```csharp
private SceneSpawnContext BuildContextFromParentScene(Scene parentScene, Player player)
{
    return new SceneSpawnContext
    {
        Player = player,
        CurrentLocation = parentScene.Location,
        CurrentNPC = parentScene.Npc,
        CurrentRoute = parentScene.Route,
        CurrentSituation = null
    };
}
```

**TO (CurrentSituation.Location/Npc/Route if available, else Scene's):**
```csharp
private SceneSpawnContext BuildContextFromParentScene(Scene parentScene, Player player)
{
    // Use CurrentSituation's context if present (per-situation context)
    // Fall back to Scene's context if no CurrentSituation (initial placement)
    Situation currentSituation = parentScene.CurrentSituation;

    return new SceneSpawnContext
    {
        Player = player,
        CurrentLocation = currentSituation?.Location ?? parentScene.Location,
        CurrentNPC = currentSituation?.Npc ?? parentScene.Npc,
        CurrentRoute = currentSituation?.Route ?? parentScene.Route,
        CurrentSituation = null
    };
}
```

### 8. Update SceneContent.razor.cs (line 461)

**FROM (Scene.Npc):**
```csharp
NPC npc = Scene.Npc;
```

**TO (CurrentSituation.Npc with fallback):**
```csharp
NPC npc = Scene.CurrentSituation?.Npc ?? Scene.Npc;
```

### 9. Update Documentation

**Files to update:**
- `08_crosscutting_concepts.md` (System 5 flow)
- `design/09_design_patterns.md` (Entity ownership examples)
- `design/11_design_decisions.md` (Direct object references section)

**Changes:**
- Scene owns Situations collection (composition)
- Situation owns Location/NPC/Route (context ownership)
- Each Situation can have different context from other Situations in same Scene
- SceneParser resolves per-situation context from Template requirements

## Impact Analysis

### Files Modified
1. `src/GameState/Situation.cs` - Add entity properties, delete STRING ID properties, update GetPlacementId()
2. `src/GameState/Scene.cs` - Update ShouldActivateAtContext and CompareContexts
3. `src/Content/SceneParser.cs` - Update situation parsing to resolve per-situation context
4. `src/Subsystems/Scene/SceneFacade.cs` - Update BuildContextFromParentScene
5. `src/Pages/Components/SceneContent.razor.cs` - Update NPC retrieval
6. Documentation files (3 files)

### Breaking Changes
- **Save/Load**: ResolvedRequiredLocationId/NpcId no longer exist in Situation
  - **Fix**: SituationDTO must have locationId/npcId fields to persist entity references
  - **Alternative**: Rely on Template.RequiredLocationId/NpcId (reconstruction at load time)

### Tests Required
- Scene activation at correct location/NPC context
- Multi-situation scene progression across different contexts
- Situation inheriting Scene placement when no template requirement
- Situation with specific template requirement overriding Scene placement
- SceneParser correctly resolves per-situation entities from GameWorld

## Expected Outcome

**Before Refactoring:**
```
[Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' checking activation at location 'common_room', npc ''
```
Activation fails - npc context is empty string.

**After Refactoring:**
```
[Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' checking activation at location 'common_room', npc 'elena'
```
Activation succeeds - npc context correctly resolved from Situation.Npc entity reference.

## Implementation Order

1. Add entity references to Situation.cs (non-breaking addition)
2. Update SceneParser to populate new properties (parallel with STRING IDs)
3. Update Scene.cs to use entity references with STRING ID fallback (backward compatible)
4. Verify tests pass with both paths working
5. Delete STRING ID properties from Situation.cs (breaking change)
6. Update all usage sites to use entity references only
7. Run full test suite
8. Test tutorial scene activation with Playwright
9. Update documentation

## Verification Commands

```bash
# Build
cd /c/Git/Wayfarer/src && dotnet build

# Test
cd /c/Git/Wayfarer/src && dotnet test

# Run server
ASPNETCORE_URLS="http://localhost:8100" timeout 120 dotnet run --no-build

# Playwright test (see AGENT_OPERATIONS_GUIDE.md)
```
