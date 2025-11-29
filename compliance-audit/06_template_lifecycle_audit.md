# Template/Instance Lifecycle Compliance Audit

## Status: ✅ COMPLETE - FULLY COMPLIANT

## Summary

The Wayfarer codebase demonstrates **FULL COMPLIANCE** with the Three-Tier Timing Model and Template/Instance Lifecycle architecture described in arc42/08 §8.4, §8.13, and ADR-008/011.

**Key findings:**
- ✅ Templates are truly immutable (all properties use `init`)
- ✅ Instances are mutable with proper state tracking (SceneState, InstantiationState)
- ✅ Deferred scenes have NO situation instances until activation
- ✅ Situations created at activation time (Tier 2) from SituationTemplates
- ✅ Actions created at query-time (Tier 3) and are ephemeral
- ✅ Entity resolution happens at activation, not parse-time
- ✅ Package-round entity tracking implemented (no GameWorld scanning)

**NO VIOLATIONS FOUND**

## Principles Being Checked

### From arc42/08 §8.4 Three-Tier Timing Model
| Tier | When | Content Type | Mutability |
|------|------|--------------|------------|
| Templates | Parse-time | Archetypes/patterns | Immutable |
| Instances | Spawn-time | Active game entities | Mutable |
| Actions | Query-time | Player options | Ephemeral |

### From arc42/08 §8.13 Template vs Instance Lifecycle
- Deferred scenes have NO situation instances
- Situations created at ACTIVATION when entity references can be resolved
- Actions materialize only at query-time

### From ADR-008 and ADR-011
- InstantiationState tracking on scenes/situations
- Package-round entity tracking
- Explicit entity lists, no GameWorld scanning during init

## Methodology
1. Search for all Template classes and verify immutability
2. Search for Instance classes and verify mutable state + InstantiationState
3. Trace action creation to verify query-time materialization
4. Examine package loading for explicit entity tracking
5. Verify deferred scene patterns and activation lifecycle
6. Check entity resolution timing

## Findings

### ✅ COMPLIANT: Template Classes Are Immutable

**Files examined:**
- `/home/user/Wayfarer/src/GameState/SceneTemplate.cs` (157 lines)
- `/home/user/Wayfarer/src/GameState/SituationTemplate.cs` (99 lines)
- `/home/user/Wayfarer/src/GameState/ChoiceTemplate.cs` (113 lines)
- `/home/user/Wayfarer/src/GameState/VenueTemplate.cs` (exists)

**Evidence:**
- **ALL properties use `{ get; init; }` accessors** - immutable after construction
- Templates explicitly documented as "immutable archetypes" in XML comments
- Templates stored in GameWorld collections and never modified after parse-time

**Example from SceneTemplate.cs:**
```csharp
public string Id { get; init; }
public SpawnPattern Archetype { get; init; }
public List<SituationTemplate> SituationTemplates { get; init; } = new List<SituationTemplate>();
```

### ✅ COMPLIANT: Instance Classes Are Mutable

**Files examined:**
- `/home/user/Wayfarer/src/GameState/Scene.cs` (588 lines)
- `/home/user/Wayfarer/src/GameState/Situation.cs` (407 lines)

**Evidence:**
- **ALL properties use `{ get; set; }` accessors** - mutable state
- Instances track lifecycle state (SceneState, InstantiationState)
- Instances accumulate runtime data (LastChoice, LastChallengeSucceeded, etc.)

**Example from Scene.cs:**
```csharp
public SceneState State { get; set; } = SceneState.Deferred;
public List<Situation> Situations { get; set; } = new List<Situation>();
public int CurrentSituationIndex { get; set; } = 0;
```

**Example from Situation.cs (line 31):**
```csharp
public InstantiationState InstantiationState { get; set; } = InstantiationState.Deferred;
```

### ✅ COMPLIANT: InstantiationState Tracking Exists

**File:** `/home/user/Wayfarer/src/GameState/Enums/InstantiationState.cs`

**Evidence:**
```csharp
public enum InstantiationState
{
    /// Situation exists but ChoiceTemplates NOT instantiated into actions
    /// NO actions exist in GameWorld.LocationActions/NPCActions/PathCards
    Deferred,

    /// Player entered context, ChoiceTemplates instantiated into action entities
    /// Actions exist in GameWorld.LocationActions/NPCActions/PathCards collections
    Instantiated
}
```

**Usage:**
- Situation.InstantiationState tracks action materialization state
- Orthogonal to LifecycleStatus (progression tracking)
- Part of three-tier timing model documentation

### ✅ COMPLIANT: Actions Created at Query-Time (Tier 3)

**File:** `/home/user/Wayfarer/src/Subsystems/Scene/SceneFacade.cs`

**Evidence (lines 84-104):**
```csharp
/// THREE-TIER TIMING MODEL: Creates ephemeral actions (Tier 3) fresh on every query
/// Actions never stored, always rebuilt from ChoiceTemplates
public List<LocationAction> GetActionsAtLocation(Location location, Player player)
{
    // ...
    foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
    {
        LocationAction action = new LocationAction
        {
            Name = choiceTemplate.ActionTextTemplate,
            ChoiceTemplate = choiceTemplate,
            Situation = situation,
            // ...
        };

        allActions.Add(action); // Add to return list only (NOT stored in GameWorld)
    }
    return allActions;
}
```

**Pattern repeated for:**
- `GetActionsForNPC()` - creates NPCAction instances
- `GetPathCardsForRoute()` - creates PathCard instances
- `GetPathCardsForRouteSegment()` - creates PathCard instances with segment filtering

**Key compliance points:**
- Actions created FRESH on every query
- Actions NEVER stored in GameWorld collections
- Actions are EPHEMERAL - exist only for single UI render
- Actions built from ChoiceTemplates (immutable source)

### ✅ COMPLIANT: Deferred Scenes Have NO Situation Instances

**File:** `/home/user/Wayfarer/src/Content/SceneInstantiator.cs` (line 80)

**Evidence:**
```csharp
/// PHASE 1: Create deferred scene WITHOUT Situations or dependent resources
public string CreateDeferredScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
{
    SceneDTO sceneDto = GenerateSceneDTO(template, spawnReward, context, isDeferredState: true);

    // CORRECT ARCHITECTURE: Deferred scenes have NO Situation instances
    // Situations created at activation time from Template.SituationTemplates
    sceneDto.Situations = new List<SituationDTO>();

    string packageJson = BuildScenePackage(sceneDto, new List<LocationDTO>(), new List<ItemDTO>());

    Console.WriteLine($"[SceneInstantiator] Generated DEFERRED scene package '{sceneDto.Id}' (State=Deferred, NO situations - created at activation)");

    return packageJson;
}
```

**Documentation compliance:**
- Deferred scenes explicitly set to empty Situations list
- Comment references "CORRECT ARCHITECTURE"
- Console log confirms "NO situations - created at activation"

### ✅ COMPLIANT: Situations Created at Activation (Tier 2)

**File:** `/home/user/Wayfarer/src/Content/SceneInstantiator.cs` (lines 100-284)

**Evidence:**
```csharp
/// PHASE 2: Activate scene - INTEGRATED PROCESS
/// Creates Situation instances from SituationTemplates AND resolves entities in one operation.
public void ActivateScene(Scene scene, SceneSpawnContext context)
{
    if (scene.State != SceneState.Deferred)
        throw new InvalidOperationException($"Scene cannot be activated - State is {scene.State}, expected Deferred");

    if (scene.Template.SituationTemplates == null || scene.Template.SituationTemplates.Count == 0)
        throw new InvalidOperationException($"Scene has no SituationTemplates. SceneTemplates must have ALL SituationTemplates embedded.");

    Console.WriteLine($"[SceneInstantiator] Activating scene '{scene.DisplayName}' - creating {scene.Template.SituationTemplates.Count} Situation instances from SituationTemplates");

    // INTEGRATED PROCESS: Create Situation instances AND resolve entities in one loop
    foreach (SituationTemplate sitTemplate in scene.Template.SituationTemplates)
    {
        // Step 1: Create Situation instance from template
        Situation situation = CreateSituationFromTemplate(sitTemplate, scene);

        // Step 2: Resolve entities (find-or-create) - INTEGRATED, NOT SEPARATE
        // ... entity resolution code ...

        // Step 3: Add to Scene.Situations
        scene.Situations.Add(situation);
    }

    // Transition state: Deferred → Active
    scene.State = SceneState.Active;
}
```

**Key compliance points:**
- Situations created ONLY at activation time (spawn-time/Tier 2)
- Entity references resolved at activation via EntityResolver.FindOrCreate
- State transition: Deferred → Active
- Template.SituationTemplates used as blueprints (immutable source)

### ✅ COMPLIANT: Entity Resolution at Activation (Not Parse-Time)

**File:** `/home/user/Wayfarer/src/Content/SceneInstantiator.cs` (lines 140-256)

**Evidence:**
```csharp
// Step 2: Resolve entities (find-or-create) - INTEGRATED, NOT SEPARATE
// LOCATION: Find or create (with RouteDestination proximity support)
if (situation.LocationFilter != null)
{
    Location location;

    // Standard categorical resolution
    location = finder.FindLocation(situation.LocationFilter, context.CurrentLocation);

    if (location == null)
    {
        // Not found - create via PackageLoader (HIGHLANDER single creation path)
        LocationDTO dto = BuildLocationDTOFromFilter(situation.LocationFilter);
        location = _packageLoader.CreateSingleLocation(dto, context.CurrentVenue);

        // Mark as SceneCreated for dual-model accessibility
        location.Origin = LocationOrigin.SceneCreated;
        Console.WriteLine($"[SceneInstantiator]     ✅ CREATED Location '{location.Name}' (SceneCreated)");
    }

    situation.Location = location;
}
```

**Entity resolution pattern:**
1. **Parse-time (Tier 1):** Store PlacementFilter (categorical specification)
2. **Activation-time (Tier 2):** Resolve Filter → Entity via EntityResolver.FindOrCreate
3. **Query-time (Tier 3):** Use resolved entity references to create actions

**Entities resolved:**
- Location: FindLocation() or CreateSingleLocation()
- NPC: FindNPC() or CreateSingleNpc()
- Route: FindRoute() (no creation, fail-fast if missing)

### ✅ COMPLIANT: Package-Round Entity Tracking (ADR-011/014)

**File:** `/home/user/Wayfarer/src/Content/PackageLoadResult.cs`

**Evidence:**
```csharp
/// PackageLoadResult: Entity tracking structure for package-round isolation
/// Track entities added during a single package load round to enforce
/// package-round principle (initialize ONLY entities from THIS round, never re-process)
public class PackageLoadResult
{
    public List<Venue> VenuesAdded { get; set; } = new List<Venue>();
    public List<Location> LocationsAdded { get; set; } = new List<Location>();
    public List<NPC> NPCsAdded { get; set; } = new List<NPC>();
    public List<SceneTemplate> SceneTemplatesAdded { get; set; } = new List<SceneTemplate>();
    public List<Scene> ScenesAdded { get; set; } = new List<Scene>();
    // ... etc
    public string PackageId { get; set; } = "unknown";
}
```

**Usage in PackageLoader.cs (line 220):**
```csharp
private PackageLoadResult LoadPackageContent(Package package, bool allowSkeletons)
{
    // Create result to track entities from THIS round
    PackageLoadResult result = new PackageLoadResult { PackageId = package.PackageId ?? "unknown" };

    // ... load entities, adding to result ...

    return result;
}
```

**Static loading pattern (lines 129-214):**
```csharp
public void LoadStaticPackages(List<string> packageFilePaths)
{
    // Accumulate results from all packages
    List<PackageLoadResult> allResults = new List<PackageLoadResult>();

    foreach (string packagePath in sortedPackages)
    {
        PackageLoadResult result = LoadPackageContent(package, allowSkeletons: false);
        allResults.Add(result);
    }

    // Aggregate all entities across all packages
    List<Venue> allVenues = allResults.SelectMany(r => r.VenuesAdded).ToList();
    List<Location> allLocations = allResults.SelectMany(r => r.LocationsAdded).ToList();

    // Initialize ONCE with explicit entity lists (no GameWorld scanning)
    PlaceVenues(allVenues);
    PlaceLocations(allLocations);
}
```

**Compliance with ADR-011:**
- ✅ PackageLoadResult tracks entities added per package
- ✅ Initialization methods accept explicit entity lists
- ✅ NO GameWorld collection scanning during initialization
- ✅ Package-round isolation enforced by construction

## Remaining TODOs
None - audit complete.
