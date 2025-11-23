# THREE-TIER TIMING MODEL IMPLEMENTATION PLAN

**Date:** 2025-01-23
**Purpose:** Holistic refactoring to align codebase with Three-Tier Timing Model architecture
**Bug:** Scene activation creates dependent entities but never assigns them to Situations, causing "A2 doesn't spawn" crash

---

## ARCHITECTURAL GROUND TRUTH

From `architecture/08_crosscutting_concepts.md` § 8.2.4:

**Tier 1 (Parse Time):**
- Templates created from JSON
- NO entity resolution
- NO entity creation
- Stored in GameWorld.SceneTemplates

**Tier 2 (Spawn Time → Activation):**
- **Phase 1 (Deferred):** Scene + Situations created, PlacementFilters stored, InstantiationState=Deferred, NO entity resolution
- **Phase 2 (Active):** Player enters location → CheckAndActivateDeferredScenes() → Dependent resources created → **Entity resolution happens HERE** → State=Active

**Tier 3 (Query Time):**
- Actions instantiated from ChoiceTemplates
- Added to GameWorld collections

---

## CURRENT VIOLATIONS

### Violation #1: SceneParser Resolves During Parse
**File:** `src/Content/SceneParser.cs` (lines 137-162)

**Current Code:**
```csharp
PlacementFilterDTO effectiveLocationFilter = situationDto.LocationFilter ?? dto.LocationFilter;
if (effectiveLocationFilter != null)
{
    PlacementFilter locationFilter = SceneTemplateParser.ParsePlacementFilter(effectiveLocationFilter, locationContext);
    resolvedLocation = entityResolver.FindOrCreateLocation(locationFilter);  // ❌ TIER VIOLATION
}
```

**Problem:**
- For deferred scenes, dependent entities DON'T EXIST yet during parse
- EntityResolver creates placeholder "Unknown Location" with null HexPosition
- Crash occurs later in spatial validation

**What Should Happen:**
- Store PlacementFilter on Situation
- Leave Situation.Location = null
- Resolution happens at activation time (Tier 2 Phase 2)

### Violation #2: No Post-Activation Entity Assignment
**File:** `src/Subsystems/Location/LocationFacade.cs` (lines 424-468)

**Current Flow:**
```
CheckAndActivateDeferredScenes()
→ SceneInstantiator.ActivateScene() generates dependent resource JSON
→ PackageLoader.LoadDynamicPackage() creates entities
→ Entities stored in GameWorld
→ scene.State = SceneState.Active
→ DONE (NO entity assignment!)
```

**Problem:**
- Dependent entities exist in GameWorld
- But Situation.Location/Npc/Route stay NULL
- Situations can't find their placement during gameplay

**What Should Happen:**
- After PackageLoader, call entity resolution
- Use stored PlacementFilters to query GameWorld
- Assign resolved entities to Situation.Location/Npc/Route

---

## COMPLETE REFACTORING PLAN

### Component 1: Remove Entity Resolution from SceneParser

**File:** `src/Content/SceneParser.cs`

**Lines to modify:** 127-201

**Changes:**

1. **Remove entity resolution calls** (lines 137-162):
   - Delete `entityResolver.FindOrCreateLocation(locationFilter)`
   - Delete `entityResolver.FindOrCreateNPC(npcFilter)`
   - Delete `entityResolver.FindOrCreateRoute(routeFilter)`

2. **Store PlacementFilters on Situation**:
   - Parse PlacementFilterDTO → PlacementFilter (keep this)
   - Store on Situation.LocationFilter/NpcFilter/RouteFilter (NEW)

3. **Change SituationParser.ConvertDTOToSituation() call**:
   - Remove pre-resolved entity parameters
   - Pass NULL for all entity references

**Before:**
```csharp
PlacementFilter locationFilter = SceneTemplateParser.ParsePlacementFilter(...);
resolvedLocation = entityResolver.FindOrCreateLocation(locationFilter);

Situation situation = SituationParser.ConvertDTOToSituation(
    situationDto, gameWorld, resolvedLocation, resolvedNpc, resolvedRoute);
```

**After:**
```csharp
PlacementFilter locationFilter = SceneTemplateParser.ParsePlacementFilter(...);
// NO resolution - store filter for activation-time resolution

Situation situation = SituationParser.ConvertDTOToSituation(
    situationDto, gameWorld);

// Store filters for later resolution
situation.LocationFilter = locationFilter;
situation.NpcFilter = npcFilter;
situation.RouteFilter = routeFilter;
```

### Component 2: Modify SituationParser Signature

**File:** `src/Content/SituationParser.cs`

**Lines to modify:** 14-19, 55-57

**Changes:**

1. **Remove pre-resolved entity parameters from method signature**:
   ```csharp
   // OLD
   public static Situation ConvertDTOToSituation(
       SituationDTO dto,
       GameWorld gameWorld,
       Location resolvedLocation,
       NPC resolvedNpc,
       RouteOption resolvedRoute)

   // NEW
   public static Situation ConvertDTOToSituation(
       SituationDTO dto,
       GameWorld gameWorld)
   ```

2. **Initialize entity properties to NULL**:
   ```csharp
   Situation situation = new Situation
   {
       Name = dto.Name,
       Description = dto.Description,
       Location = null,  // Will be resolved at activation
       Npc = null,       // Will be resolved at activation
       Route = null,     // Will be resolved at activation
       // ... rest of properties
   };
   ```

### Component 3: Add Entity Resolution to SceneInstantiator

**File:** `src/Services/SceneInstantiator.cs`

**New method to add:**

```csharp
/// <summary>
/// Resolve entity references from filters AFTER dependent resources created
/// THREE-TIER TIMING: Filters stored at parse (Tier 1), resolved here at activation (Tier 2 Phase 2)
/// Called by LocationFacade.CheckAndActivateDeferredScenes() after PackageLoader completes
/// </summary>
/// <param name="scene">Scene with situations containing filters but null entity references</param>
/// <param name="context">Spawn context with player and location for categorical matching</param>
public void ResolveSceneEntityReferences(Scene scene, SceneSpawnContext context)
{
    Console.WriteLine($"[SceneInstantiator] Resolving entity references for scene '{scene.DisplayName}' with {scene.Situations.Count} situations");

    // Create EntityResolver for categorical matching
    SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
    EntityResolver entityResolver = new EntityResolver(_gameWorld, context.Player, narrativeService);

    // For each situation, resolve entities from stored filters
    foreach (Situation situation in scene.Situations)
    {
        // Resolve Location from LocationFilter
        if (situation.LocationFilter != null && situation.Location == null)
        {
            situation.Location = entityResolver.FindOrCreateLocation(situation.LocationFilter);
            Console.WriteLine($"[SceneInstantiator]   ✅ Resolved Location '{situation.Location?.Name ?? "NULL"}' for situation '{situation.Name}'");
        }

        // Resolve NPC from NpcFilter
        if (situation.NpcFilter != null && situation.Npc == null)
        {
            situation.Npc = entityResolver.FindOrCreateNPC(situation.NpcFilter);
            Console.WriteLine($"[SceneInstantiator]   ✅ Resolved NPC '{situation.Npc?.Name ?? "NULL"}' for situation '{situation.Name}'");
        }

        // Resolve Route from RouteFilter
        if (situation.RouteFilter != null && situation.Route == null)
        {
            situation.Route = entityResolver.FindOrCreateRoute(situation.RouteFilter);
            Console.WriteLine($"[SceneInstantiator]   ✅ Resolved Route '{situation.Route?.Name ?? "NULL"}' for situation '{situation.Name}'");
        }
    }

    Console.WriteLine($"[SceneInstantiator] ✅ Entity resolution complete for scene '{scene.DisplayName}'");
}
```

**Dependencies:**
- SceneInstantiator constructor must inject `GameWorld _gameWorld`
- Verify `SceneNarrativeService` is accessible

### Component 4: Call Resolution in LocationFacade

**File:** `src/Subsystems/Location/LocationFacade.cs`

**Lines to modify:** 461 (insert after PackageLoader, before state transition)

**Current code (lines 453-467):**
```csharp
string resourceJson = _sceneInstantiator.ActivateScene(scene, activationContext);

if (!string.IsNullOrEmpty(resourceJson))
{
    string packagePath = $"scene_activation_{scene.TemplateId}_{Guid.NewGuid().ToString("N")}";
    await _contentGenerationFacade.CreateDynamicPackageFile(resourceJson, packagePath);
    await _packageLoaderFacade.LoadDynamicPackage(resourceJson, packagePath);

    Console.WriteLine($"[LocationFacade] Loaded dependent resources for scene '{scene.DisplayName}'");
}

// Transition scene state: Deferred → Active
scene.State = SceneState.Active;
Console.WriteLine($"[LocationFacade] Scene '{scene.DisplayName}' activated successfully (State: Deferred → Active)");
```

**Modified code:**
```csharp
string resourceJson = _sceneInstantiator.ActivateScene(scene, activationContext);

if (!string.IsNullOrEmpty(resourceJson))
{
    string packagePath = $"scene_activation_{scene.TemplateId}_{Guid.NewGuid().ToString("N")}";
    await _contentGenerationFacade.CreateDynamicPackageFile(resourceJson, packagePath);
    await _packageLoaderFacade.LoadDynamicPackage(resourceJson, packagePath);

    Console.WriteLine($"[LocationFacade] Loaded dependent resources for scene '{scene.DisplayName}'");
}

// *** NEW PHASE 2.5: Resolve entity references now that dependent resources exist ***
_sceneInstantiator.ResolveSceneEntityReferences(scene, activationContext);
Console.WriteLine($"[LocationFacade] ✅ Resolved entity references for scene '{scene.DisplayName}'");

// Transition scene state: Deferred → Active
scene.State = SceneState.Active;
Console.WriteLine($"[LocationFacade] Scene '{scene.DisplayName}' activated successfully (State: Deferred → Active)");
```

---

## DATA FLOW TRANSFORMATION

### BEFORE (BROKEN):

```
Parse Time (Game Initialization):
  PackageLoader → SceneParser
               → EntityResolver.FindOrCreateLocation()
               → Dependent entity doesn't exist
               → Creates "Unknown Location" with HexPosition=null
               → Situation.Location = Unknown Location

Activation Time (Player enters Inn):
  CheckAndActivateDeferredScenes()
  → ActivateScene() generates dependent location JSON
  → PackageLoader creates "Elena's Lodging Room" entity
  → Adds to GameWorld.Locations
  → scene.State = Active
  → STOPS (no entity assignment)

Result:
  Situation.Location = Unknown Location (wrong entity!)
  Spatial validation crashes on Unknown Location with null HexPosition
```

### AFTER (FIXED):

```
Parse Time (Game Initialization):
  PackageLoader → SceneParser
               → Parse PlacementFilter from DTO
               → Store on Situation.LocationFilter
               → Situation.Location = NULL (correct, deferred)

Activation Time (Player enters Inn):
  CheckAndActivateDeferredScenes()
  → ActivateScene() generates dependent location JSON
  → PackageLoader creates "Elena's Lodging Room" entity
  → Adds to GameWorld.Locations
  → *** NEW *** ResolveSceneEntityReferences() called
  → EntityResolver.FindOrCreateLocation(situation.LocationFilter)
  → Finds "Elena's Lodging Room" by LocationTags match
  → Situation.Location = "Elena's Lodging Room" (correct entity!)
  → scene.State = Active

Result:
  Situation.Location = "Elena's Lodging Room" (correct)
  A2 spawns successfully
  Tutorial completes end-to-end
```

---

## IMPLEMENTATION ORDER

### Phase 1: Domain Model (Already Correct)
✅ Situation.LocationFilter/NpcFilter/RouteFilter properties exist (lines 133-185 of Situation.cs)
✅ No changes needed

### Phase 2: Parser Layer
1. Modify SceneParser.ConvertDTOToScene() to store filters, not resolve entities
2. Modify SituationParser.ConvertDTOToSituation() signature to remove entity parameters
3. Verify no other code calls SituationParser with old signature

### Phase 3: Activation Layer
4. Add ResolveSceneEntityReferences() method to SceneInstantiator
5. Update LocationFacade.CheckAndActivateDeferredScenes() to call resolution
6. Verify SceneInstantiator has GameWorld dependency

### Phase 4: Build and Test
7. Run `dotnet build` and fix compilation errors
8. Run `dotnet test` and verify unit tests pass
9. Start game and navigate to Inn
10. Verify console shows "Resolved Location" messages
11. Verify A2 spawns successfully
12. Complete tutorial playthrough

---

## VERIFICATION CRITERIA

### Build Verification
- [ ] `dotnet build` compiles with zero errors
- [ ] No warnings about missing entity references
- [ ] SituationParser signature change doesn't break other code

### Runtime Verification
- [ ] Game starts without crash
- [ ] Console shows: `[SceneParser] Scene loaded with PlacementFilters, entities NULL`
- [ ] Player arrives at Inn location
- [ ] Console shows: `[LocationFacade] Activating deferred scene 'Secure Lodging'`
- [ ] Console shows: `[SceneInstantiator] Resolving entity references for scene 'Secure Lodging'`
- [ ] Console shows: `[SceneInstantiator] ✅ Resolved Location 'Elena's Lodging Room' for situation 'Negotiate for Room'`
- [ ] Console shows: `[LocationFacade] ✅ Resolved entity references for scene 'Secure Lodging'`
- [ ] A2 spawns in Common Room (NPC conversation options appear)
- [ ] Clicking A2 opens situation with choices
- [ ] Completing A2 progresses tutorial

### Architecture Verification
- [ ] SceneParser NEVER calls EntityResolver
- [ ] Entity resolution happens ONLY in ResolveSceneEntityReferences()
- [ ] PlacementFilters stored at parse time
- [ ] Entity objects assigned at activation time
- [ ] Three-Tier Timing Model fully respected

---

## ROLLBACK STRATEGY

If implementation fails:
1. Git stash all changes
2. Identify which component caused failure
3. Fix component in isolation
4. Re-test incrementally

Critical files for backup:
- `src/Content/SceneParser.cs`
- `src/Content/SituationParser.cs`
- `src/Services/SceneInstantiator.cs`
- `src/Subsystems/Location/LocationFacade.cs`

---

## SUCCESS METRICS

**✅ Complete Success:**
- Build compiles
- Game starts
- Tutorial completes end-to-end
- A2 spawns correctly
- Entity references assigned correctly

**⚠️ Partial Success:**
- Build compiles
- Game starts
- Situations have NULL entities (filters not resolved)

**❌ Failure:**
- Build doesn't compile
- Game crashes on start
- Spatial validation crashes

---

## ARCHITECTURAL ALIGNMENT

This refactoring aligns the codebase with documented architecture:

**From § 8.2.4 Three-Tier Timing Model:**
> "Tier 2: Scenes/Situations (Spawn Time)
> **When**: Scene spawns from Obligation or trigger
> **What**: Runtime instances with lifecycle and mutable state
> **Characteristics**:
> - Scene instance created with embedded Situations
> - Situation.Template reference stored (ChoiceTemplates NOT instantiated yet)
> - InstantiationState = Deferred
> - NO actions created in GameWorld collections yet"

**From § 8.4.3 Scene Lifecycle States:**
> "**Deferred**: Scene and Situations created, dependent resources NOT spawned yet
> **Active**: Dependent resources spawned (locations placed, npcs created), scene fully playable"

After this refactoring, implementation will PERFECTLY MATCH documented architecture.
