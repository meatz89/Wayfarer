# ID ELIMINATION EXECUTION PLAN

## OVERVIEW

**Objective:** Complete elimination of ALL ID properties and ID-based lookups across the entire codebase. HIGHLANDER enforcement: Object references ONLY.

**Strategy:** Clean break refactoring with NO compatibility layers. Delete all IDs simultaneously, fix compilation errors in dependency order.

**Scope Statistics:**
- Total C# files: 655
- Files with Id properties: 74
- Files with `.Id ==` comparisons: 70 (214 occurrences)
- Files with `GetLocation/GetNPC/GetRoute/GetVenue` calls: 20 (57 occurrences)
- Facade methods with `string Id` parameters: 31 files (138 methods)
- UI event handlers with `string` parameters: 5 files (11 handlers)
- GetById methods to delete: 5

**Estimated files to modify:** 100-120 files

**Build verification:** Run `cd src && dotnet build` after each phase to verify progress

---

## PHASE 1: DOMAIN ENTITY CLEANUP (Breaks Compilation)

**Goal:** Delete ALL ID properties from domain entities. This intentionally breaks compilation - fixes come in later phases.

**Exception:** Template IDs remain (SceneTemplate.Id, SituationTemplate.Id, Achievement.Id, State.Id, etc.) because templates are catalogue entities referenced by configuration.

### 1.1: Core Entity ID Deletion

**Files to modify (11 files):**

1. `/home/user/Wayfarer/src/Content/Location.cs`
   - DELETE: `public string Id { get; set; }` (line 3)
   - DELETE: `public string VenueId { get; set; }` (line 5)
   - KEEP: `public Venue Venue { get; internal set; }` (line 6) - object reference stays
   - DELETE: `public List<string> ActiveSituationIds` (line 31) - query Scenes instead
   - DELETE: Constructor parameter `string id` (line 91)
   - UPDATE: Constructor to `public Location(string name)` with only Name parameter

2. `/home/user/Wayfarer/src/GameState/RouteOption.cs`
   - DELETE: `public string Id { get; set; }` (line 55)
   - DELETE: `public string OriginLocationId { get; set; }` (line 60)
   - DELETE: `public string DestinationLocationId { get; set; }` (line 62)
   - KEEP: `public Location OriginLocation { get; set; }` (line 61)
   - KEEP: `public Location DestinationLocation { get; set; }` (line 63)
   - DELETE: `public List<string> EncounterDeckIds` (line 86) - replace with object list or inline

3. `/home/user/Wayfarer/src/GameState/Venue.cs`
   - DELETE: `public string Id { get; set; }`
   - DELETE: Any Location ID list properties
   - KEEP: Object reference properties

4. `/home/user/Wayfarer/src/GameState/Item.cs`
   - DELETE: `public string Id { get; set; }`
   - Use `Name` as natural key (like NPC)

5. `/home/user/Wayfarer/src/GameState/Scene.cs`
   - DELETE: `public string Id { get; set; }` (if exists - runtime scenes only, templates keep Id)
   - VERIFY: Distinguish SceneTemplate.Id (keep) vs Scene instance Id (delete)

6. `/home/user/Wayfarer/src/GameState/Situation.cs`
   - DELETE: `public string Id { get; set; }` (if exists - runtime instances only)

7. `/home/user/Wayfarer/src/GameState/DeliveryJob.cs`
   - DELETE: `public string Id { get; set; }`
   - DELETE: Any NPC/Location ID properties
   - KEEP: Object references

8. `/home/user/Wayfarer/src/GameState/ObservationScene.cs`
   - DELETE: `public string Id { get; set; }`

9. `/home/user/Wayfarer/src/GameState/ConversationTree.cs`
   - DELETE: `public string Id { get; set; }`

10. `/home/user/Wayfarer/src/GameState/EmergencySituation.cs`
    - DELETE: `public string Id { get; set; }`

11. `/home/user/Wayfarer/src/GameState/Obligation.cs`
    - DELETE: `public string Id { get; set; }`
    - DELETE: Any NPC/Location ID list properties
    - KEEP: Object references

**Verification:**
```bash
cd /home/user/Wayfarer/src
grep -r "public string Id { get; set; }" GameState/ Content/ --include="*.cs" | grep -v Template | grep -v DTO
```

Should return ONLY template classes (SceneTemplate, SituationTemplate, Achievement, State, etc.)

### 1.2: Delete ID Properties from Supporting Entities

**Files to check and modify (~15 files):**

Search pattern: All files in GameState/ and Content/ directories with Id properties

```bash
grep -l "public string Id { get; set; }" src/GameState/*.cs src/Content/*.cs
```

For each file:
- If it's a runtime entity (NPC, Location, Route, etc.) → DELETE Id
- If it's a template/configuration (SceneTemplate, Achievement, etc.) → KEEP Id
- Delete all `List<string> EntityIds` properties → replace with `List<Entity>` or query pattern

**Expected compilation errors after Phase 1:** 500+ errors
- Missing Id property references
- Constructor signature mismatches
- LINQ queries using .Id
- Dictionary keys using Id

**DO NOT FIX ERRORS YET - proceed to Phase 2**

---

## PHASE 2: VIEWMODEL REFACTORING (Fixes UI Layer)

**Goal:** Replace all ViewModel Id properties with object properties. Update facade methods that build ViewModels.

### 2.1: Core ViewModels - Travel System

**File:** `/home/user/Wayfarer/src/ViewModels/GameFacadeViewModels.cs`

**Changes (lines 1-100):**

1. `TravelDestinationViewModel`:
   - DELETE: `public string LocationId { get; set; }` (line 5)
   - ADD: `public Location Location { get; set; }`
   - UPDATE: All usages to access `Location.Name` instead of `LocationName`

2. `TravelRouteViewModel`:
   - DELETE: `public string RouteId { get; set; }` (line 47)
   - ADD: `public RouteOption Route { get; set; }`

3. `RouteTokenRequirementViewModel`:
   - KEEP AS IS (this is display data, not entity reference)

4. `RouteDiscoveryOptionViewModel`:
   - DELETE: `public string TeachingNPCId { get; set; }` (line 85)
   - DELETE: `public string TeachingNPCName { get; set; }` (line 86)
   - ADD: `public NPC TeachingNPC { get; set; }`
   - Frontend accesses `TeachingNPC.Name` directly

### 2.2: Location ViewModels

**Continue in same file:** `/home/user/Wayfarer/src/ViewModels/GameFacadeViewModels.cs`

Search for all ViewModel classes containing:
- `LocationId`
- `NPCId` / `NpcId`
- `RouteId`
- `VenueId`
- `ItemId`
- `SceneId`
- `SituationId`

**Pattern for each:**
```csharp
// BEFORE
public class SomeViewModel
{
    public string EntityId { get; set; }
    public string EntityName { get; set; }
}

// AFTER
public class SomeViewModel
{
    public EntityType Entity { get; set; }
}
```

Frontend accesses `Entity.Name` directly from object.

### 2.3: Additional ViewModel Files

**Files to modify (~5-10 files):**

```bash
grep -l "Id { get; set; }" src/ViewModels/*.cs
```

Apply same pattern: Delete `EntityId`, add `Entity` object property.

**Estimated changes:** 30-50 ViewModel classes

**Verification after Phase 2:**
```bash
grep -r "public string.*Id { get; set; }" src/ViewModels/ --include="*.cs"
```

Should return ZERO results (or only display-only fields like `RequirementKey`)

---

## PHASE 3: UI EVENT HANDLER REFACTORING (Fixes User Interactions)

**Goal:** Update all UI event handlers to receive objects instead of ID strings. Update @onclick calls to pass objects from ViewModels.

### 3.1: LocationContent Component

**File:** `/home/user/Wayfarer/src/Pages/Components/LocationContent.razor.cs`

**Changes:**

1. Delete `string locationId` usage (line 55):
   ```csharp
   // BEFORE
   Location currentLocation = GameWorld.GetPlayerCurrentLocation();
   string locationId = currentLocation?.Id;
   if (locationId != null)
   {
       AvailableConversationTrees = GameFacade.GetAvailableConversationTreesAtLocation(locationId);
   }

   // AFTER
   Location currentLocation = GameWorld.GetPlayerCurrentLocation();
   if (currentLocation != null)
   {
       AvailableConversationTrees = GameFacade.GetAvailableConversationTreesAtLocation(currentLocation);
   }
   ```

2. Event handler signatures:
   ```csharp
   // BEFORE
   protected async Task HandleTalkToNPC(string npcId) { ... }
   protected async Task HandleMoveToLocation(string locationId) { ... }

   // AFTER
   protected async Task HandleTalkToNPC(NPC npc) { ... }
   protected async Task HandleMoveToLocation(Location location) { ... }
   ```

**File:** `/home/user/Wayfarer/src/Pages/Components/LocationContent.razor`

**Changes:**

Update all @onclick calls:
```razor
<!-- BEFORE -->
<button @onclick="() => HandleTalkToNPC(npc.NpcId)">Talk</button>

<!-- AFTER -->
<button @onclick="() => HandleTalkToNPC(npc.Npc)">Talk</button>
```

### 3.2: Other UI Components

**Files to modify (10 files):**

1. `/home/user/Wayfarer/src/Pages/Components/TravelContent.razor.cs`
2. `/home/user/Wayfarer/src/Pages/Components/TravelContent.razor`
3. `/home/user/Wayfarer/src/Pages/Components/ConversationContent.razor.cs`
4. `/home/user/Wayfarer/src/Pages/Components/ExchangeContent.razor.cs`
5. `/home/user/Wayfarer/src/Pages/Components/SceneContent.razor.cs`
6. `/home/user/Wayfarer/src/Pages/Components/MentalContent.razor.cs`
7. `/home/user/Wayfarer/src/Pages/Components/PhysicalContent.razor.cs`
8. `/home/user/Wayfarer/src/Pages/Components/ObservationContent.razor.cs`
9. `/home/user/Wayfarer/src/Pages/Components/EmergencyContent.razor.cs`
10. `/home/user/Wayfarer/src/Pages/GameScreen.razor.cs`

**Pattern for each:**
1. Find all event handlers: `protected.*Task Handle.*\(string.*Id\)`
2. Change signature to receive object: `HandleTravelTo(Location destination)`
3. Update Razor file @onclick calls to pass objects from ViewModel
4. Update facade calls inside handlers to pass objects (not IDs)

**Verification:**
```bash
grep -r "Handle.*\(string.*Id\)" src/Pages/ --include="*.cs"
```

Should return ZERO results.

---

## PHASE 4: FACADE METHOD REFACTORING (Fixes Business Logic)

**Goal:** Update ALL facade method signatures to receive objects instead of IDs. Delete ALL GetById methods.

### 4.1: Delete GetById Methods

**Files to modify (5 files):**

1. `/home/user/Wayfarer/src/Services/GameFacade.cs`
   - DELETE: `public NPC GetNPCById(string npcId)` (line 209)
   - DELETE: `public RouteOption GetRouteById(string routeId)` (line 1477)

2. `/home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs`
   - DELETE: `public NPC GetNPCById(string npcId)` (line 219)

3. `/home/user/Wayfarer/src/Content/ItemRepository.cs`
   - DELETE: `public Item GetItemById(string id)` (line 20)
   - REPLACE with: `public Item GetItemByName(string name)` (Name is natural key)

4. `/home/user/Wayfarer/src/GameState/RouteRepository.cs`
   - DELETE: `public RouteOption GetRouteById(string routeId)` (line 39)
   - KEEP: `public RouteOption GetRouteBetweenLocations(Location origin, Location destination)`

5. `/home/user/Wayfarer/src/Content/NPCRepository.cs`
   - Verify no GetById methods (NPC already uses Name as key)

### 4.2: TravelFacade Refactoring

**File:** `/home/user/Wayfarer/src/Subsystems/Travel/TravelFacade.cs`

**Method signature changes:**

```csharp
// BEFORE
public RouteOption GetRouteBetweenLocations(string fromLocationId, string toLocationId)
{
    return _routeManager.GetRouteBetweenLocations(fromLocationId, toLocationId);
}

public bool CanTravelTo(string locationId) { ... }
public TravelResult TravelTo(string locationId, TravelMethods transportMethod) { ... }

// AFTER
public RouteOption GetRouteBetweenLocations(Location origin, Location destination)
{
    return _routeManager.GetRouteBetweenLocations(origin, destination);
}

public bool CanTravelTo(Location destination) { ... }
public TravelResult TravelTo(Location destination, TravelMethods transportMethod) { ... }
```

**Internal implementation changes:**

Delete all `GetLocation(id)` calls:
```csharp
// BEFORE (line 51)
Location destination = _gameWorld.GetLocation(route.DestinationLocationId);

// AFTER
Location destination = route.DestinationLocation;
```

### 4.3: LocationFacade Refactoring

**File:** `/home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs`

Update all methods taking `string locationId` or `string npcId`:
```csharp
// BEFORE
public LocationActionsViewModel GetLocationActionsViewModel(string locationId) { ... }
public void ProcessLocationAction(string locationId, string actionId) { ... }

// AFTER
public LocationActionsViewModel GetLocationActionsViewModel(Location location) { ... }
public void ProcessLocationAction(Location location, string actionId) { ... }
```

### 4.4: All Other Facades

**Files to modify (22 facade files):**

Apply same pattern to ALL facades:

1. ConversationTreeFacade.cs
2. EmergencyFacade.cs
3. ExchangeFacade.cs
4. MentalFacade.cs
5. PhysicalFacade.cs
6. ObservationFacade.cs
7. SceneFacade.cs
8. SituationFacade.cs
9. SocialFacade.cs
10. MarketFacade.cs
11. ResourceFacade.cs
12. TokenFacade.cs
13. CubeFacade.cs
14. SpawnFacade.cs
15. TimeFacade.cs
16. NarrativeFacade.cs
17. ConsequenceFacade.cs
18. ContentGenerationFacade.cs
19. SceneInstanceFacade.cs
20. SceneGenerationFacade.cs
21. PackageLoaderFacade.cs
22. ObligationFacades (MeetingManager, etc.)

**Search command:**
```bash
grep -n "public.*\(string.*Id\)" src/Subsystems/**/*Facade.cs
```

For each match:
1. Change parameter from `string entityId` to `EntityType entity`
2. Remove `GetEntityById(entityId)` lookup inside method
3. Use `entity` object directly

**Estimated changes:** 138 method signatures

**Verification:**
```bash
# Should return minimal results (only legitimate uses like actionId, templateId)
grep -r "public.*\(string.*Id\)" src/Subsystems/ --include="*Facade.cs"
```

---

## PHASE 5: PARSER/SERVICE UPDATES (Fixes Data Loading)

**Goal:** Update parsers to store object references instead of IDs. Fix all services that create/manipulate entities.

### 5.1: Core Parsers

**Files to modify (~15 files):**

1. `/home/user/Wayfarer/src/Content/LocationParser.cs`
   - When creating Location: Pass only Name to constructor
   - When setting Venue reference: Lookup Venue object, assign to `location.Venue`
   - Delete all `locationId` variables

2. `/home/user/Wayfarer/src/Content/NPCParser.cs`
   - Already object-reference based (NPC has no Id)
   - Fix line with `route.Id` comparison (line ~159 in NPC.cs)

3. `/home/user/Wayfarer/src/Content/RouteParser.cs`
   - When creating RouteOption: Don't set Id, OriginLocationId, DestinationLocationId
   - Lookup Location objects by Name: `gameWorld.Locations.FirstOrDefault(l => l.Name == originName)`
   - Assign objects: `route.OriginLocation = origin; route.DestinationLocation = destination;`

4. `/home/user/Wayfarer/src/Content/SceneParser.cs`
   - Scene instances don't need Id (templates keep Id)
   - Update all entity references to use objects

5. `/home/user/Wayfarer/src/Content/SituationParser.cs`
   - Situation instances don't need Id
   - Update all references

6. Other parsers:
   - ObservationSceneParser.cs
   - ConversationTreeParser.cs
   - EmergencyParser.cs
   - HexParser.cs
   - ObligationParser.cs
   - StrangerParser.cs

**Pattern for all parsers:**
```csharp
// BEFORE
var entity = new Entity
{
    Id = dto.Id,
    RelatedEntityId = dto.RelatedEntityId
};

// AFTER
var entity = new Entity
{
    // No Id property
    RelatedEntity = gameWorld.Entities.FirstOrDefault(e => e.Name == dto.RelatedEntityName)
};
```

### 5.2: SkeletonGenerator

**File:** `/home/user/Wayfarer/src/Content/SkeletonGenerator.cs`

**Problem:** Uses ID comparisons to check if skeleton exists (line references from grep)

**Fix:**
```csharp
// BEFORE
if (gameWorld.NPCs.Any(n => n.Id == skeletonId))

// AFTER
if (gameWorld.NPCs.Any(n => n.Name == skeletonName))
```

### 5.3: PackageLoader

**File:** `/home/user/Wayfarer/src/Content/PackageLoader.cs`

Update all entity creation and linking:
- Remove Id assignments
- Use object references for relationships
- Query by Name instead of Id

**Lines to fix:** 15 occurrences of `.Id ==` comparisons

### 5.4: Service Layer

**Files to modify (~20 files):**

Services that create/manipulate entities:

1. DependentResourceOrchestrationService.cs
2. ObligationActivity.cs
3. ObligationDiscoveryEvaluator.cs
4. SituationCompletionHandler.cs
5. VenueGeneratorService.cs
6. RewardApplicationService.cs
7. RouteManager.cs
8. LocationManager.cs
9. NPCLocationTracker.cs
10. TravelManager.cs
11. PermitValidator.cs
12. MarketSubsystemManager.cs
13. MarketStateTracker.cs
14. PriceManager.cs
15. ArbitrageCalculator.cs
16. RelationshipTracker.cs
17. ConnectionTokenManager.cs
18. TokenUnlockManager.cs
19. MeetingManager.cs
20. ExchangeOrchestrator.cs

**Pattern:**
```csharp
// BEFORE
Location location = gameWorld.GetLocation(locationId);

// AFTER
// Location object already available from caller (passed as parameter)
// No lookup needed
```

**Verification:**
```bash
# Should return ZERO results
grep -r "GetLocation\(.*Id\)" src/Services/ src/Subsystems/ --include="*.cs"
grep -r "GetNPC\(.*Id\)" src/Services/ src/Subsystems/ --include="*.cs"
grep -r "GetRoute\(.*Id\)" src/Services/ src/Subsystems/ --include="*.cs"
```

---

## PHASE 6: COMPILATION FIX SWEEP (Final Cleanup)

**Goal:** Fix all remaining compilation errors. Search systematically for broken patterns.

### 6.1: GameWorld Method Cleanup

**File:** `/home/user/Wayfarer/src/GameState/GameWorld.cs`

**Delete methods:**
```csharp
// DELETE these methods (currently at lines unknown, search for them)
public Location GetLocation(string locationId)
{
    return Locations.FirstOrDefault(l => l.Id == locationId);
}

public NPC GetNPC(string npcId)
{
    return NPCs.FirstOrDefault(n => n.Id == npcId);
}

public RouteOption GetRoute(string routeId)
{
    return Routes.FirstOrDefault(r => r.Id == routeId);
}

public Venue GetVenue(string venueId)
{
    return Venues.FirstOrDefault(v => v.Id == venueId);
}
```

**Replace with Name-based lookups (ONLY if needed):**
```csharp
public Location GetLocationByName(string name)
{
    return Locations.FirstOrDefault(l => l.Name == name);
}

public NPC GetNPCByName(string name)
{
    return NPCs.FirstOrDefault(n => n.Name == name);
}
```

**Note:** These are ONLY for initial loading/parsing. Runtime code should NEVER look up by name - should already have object references.

### 6.2: Fix .Id == Comparisons

**Search pattern:**
```bash
grep -rn "\.Id\s*==" src/ --include="*.cs" | grep -v Template | grep -v DTO
```

**214 occurrences across 70 files to fix**

**For each occurrence:**

1. **If comparing entity identity:**
   ```csharp
   // BEFORE
   if (npc.Location.Id == currentLocation.Id)

   // AFTER
   if (npc.Location == currentLocation)  // Object reference equality
   // OR
   if (npc.Location.Name == currentLocation.Name)  // Name comparison if different instances
   ```

2. **If checking for specific entity:**
   ```csharp
   // BEFORE
   var route = routes.FirstOrDefault(r => r.Id == routeId);

   // AFTER
   var route = routes.FirstOrDefault(r => r == targetRoute);  // Object comparison
   // OR caller already has route object, no lookup needed
   ```

3. **If checking PendingForcedSceneId:**
   ```csharp
   // GameWorld.PendingForcedSceneId is TEMPLATE ID (legitimate use)
   // This stays as string comparison because it references SceneTemplate.Id
   ```

### 6.3: Fix LINQ Queries

**Common patterns to fix:**

1. **Location filtering:**
   ```csharp
   // BEFORE
   var npcs = gameWorld.NPCs.Where(n => n.Location.Id == locationId).ToList();

   // AFTER
   var npcs = gameWorld.NPCs.Where(n => n.Location == location).ToList();
   ```

2. **Route filtering:**
   ```csharp
   // BEFORE
   var routes = gameWorld.Routes.Where(r => r.OriginLocationId == currentLocationId).ToList();

   // AFTER
   var routes = gameWorld.Routes.Where(r => r.OriginLocation == currentLocation).ToList();
   ```

3. **Scene filtering by placement:**
   ```csharp
   // BEFORE
   var scenes = gameWorld.Scenes.Where(s => s.PlacementId == locationId).ToList();

   // AFTER
   // If PlacementId is STILL a string (because it could be Location OR NPC OR Route):
   // Use Location.Name as PlacementId during scene creation
   var scenes = gameWorld.Scenes.Where(s => s.PlacementId == location.Name).ToList();
   ```

### 6.4: Fix Debug/Logging Code

**File:** `/home/user/Wayfarer/src/GameState/DebugLogger.cs`

Replace all `entity.Id` references in logging:
```csharp
// BEFORE
Console.WriteLine($"Location: {location.Id}");

// AFTER
Console.WriteLine($"Location: {location.Name}");
```

### 6.5: Fix Validation Code

**File:** `/home/user/Wayfarer/src/Content/Validation/SpawnedScenePlayabilityValidator.cs`

Update entity references in validation logic.

### 6.6: Build and Test

**After all fixes:**
```bash
cd /home/user/Wayfarer/src
dotnet build
```

**Expected result:** ZERO compilation errors

**If errors remain:**
1. Read error message carefully
2. Identify file and line number
3. Apply appropriate pattern from above
4. Rebuild

---

## PHASE 7: SPECIAL CASES AND EDGE CASES

**Goal:** Handle edge cases that don't fit standard patterns.

### 7.1: PlacementId System (Scene-Situation Architecture)

**Problem:** Scenes use `PlacementId` string to reference Location, NPC, or Route

**Current pattern:**
```csharp
public class Scene
{
    public PlacementType PlacementType { get; set; }  // Location, NPC, Route
    public string PlacementId { get; set; }  // Could be any entity type
}
```

**Solution options:**

**OPTION A: Keep PlacementId as Name string**
```csharp
// Scene creation
scene.PlacementType = PlacementType.Location;
scene.PlacementId = location.Name;  // Use Name as ID

// Scene lookup
var scenes = gameWorld.Scenes.Where(s =>
    s.PlacementType == PlacementType.Location &&
    s.PlacementId == location.Name).ToList();
```

**OPTION B: Object reference with type union**
```csharp
public class Scene
{
    public PlacementType PlacementType { get; set; }

    // Only ONE of these is set, based on PlacementType
    public Location PlacementLocation { get; set; }
    public NPC PlacementNPC { get; set; }
    public RouteOption PlacementRoute { get; set; }
}
```

**RECOMMENDED: Option A** (simpler, PlacementId is effectively a Name lookup)

### 7.2: Dictionary Keys

**Search for dictionaries using Id as key:**
```bash
grep -r "Dictionary<string," src/ --include="*.cs" | grep -i "id"
```

**Pattern:**
```csharp
// BEFORE
Dictionary<string, Location> locationLookup = new();
locationLookup[location.Id] = location;

// AFTER
// Use List instead of Dictionary (per DICTIONARY ANTIPATTERN)
List<Location> locations = new();
var location = locations.FirstOrDefault(l => l.Name == targetName);
```

### 7.3: JSON Serialization

**Files:** All DTO classes keep Id properties (they're data transfer objects, not domain entities)

**Pattern:**
```csharp
// DTO (stays as-is)
public class LocationDTO
{
    public string Id { get; set; }  // KEEP - this is JSON structure
    public string Name { get; set; }
}

// Domain entity (NO Id)
public class Location
{
    public string Name { get; set; }  // DELETE Id property
}

// Parser (maps DTO to domain)
Location location = new Location(dto.Name);  // Use Name, not Id
```

### 7.4: Active Session IDs

**GameWorld properties that legitimately use IDs:**

```csharp
// These reference TEMPLATE IDs (legitimate use - templates keep Ids)
public string PendingForcedSceneId { get; set; }  // SceneTemplate.Id
public string CurrentMentalSituationId { get; set; }  // SituationTemplate.Id
public string CurrentMentalObligationId { get; set; }  // Obligation template Id
public string CurrentPhysicalSituationId { get; set; }
public string CurrentPhysicalObligationId { get; set; }
```

**These stay as string Ids because they reference catalogue templates, not runtime instances.**

**IF they reference runtime instances:** Change to object references
```csharp
public Scene PendingForcedScene { get; set; }
public Situation CurrentMentalSituation { get; set; }
```

### 7.5: SkeletonSource Tracking

**Pattern in entities:**
```csharp
public string SkeletonSource { get; set; }  // "letter_template_elena_refusal"
```

This is a DESCRIPTION string, not an entity ID. KEEP AS IS.

---

## PHASE 8: VERIFICATION AND TESTING

**Goal:** Verify refactoring completeness and ensure game still runs.

### 8.1: Compilation Verification

```bash
cd /home/user/Wayfarer/src
dotnet build

# Should output: Build succeeded. 0 Error(s)
```

### 8.2: Pattern Verification

**1. No Id properties on domain entities:**
```bash
grep -r "public string Id { get; set; }" src/GameState/ src/Content/ --include="*.cs" \
  | grep -v Template | grep -v DTO | grep -v Achievement | grep -v State

# Expected: ZERO results (or only legitimate templates)
```

**2. No Id properties in ViewModels:**
```bash
grep -r "public string.*Id { get; set; }" src/ViewModels/ --include="*.cs"

# Expected: ZERO results (or only display-only fields)
```

**3. No GetById methods:**
```bash
grep -r "GetLocationById\|GetNPCById\|GetRouteById\|GetVenueById" src/ --include="*.cs"

# Expected: ZERO results
```

**4. No GetLocation(id) calls:**
```bash
grep -r "GetLocation\(" src/ --include="*.cs" | grep -v "GetPlayerCurrentLocation\|GetLocationByName"

# Expected: Minimal results (only GetLocationByName for parsing)
```

**5. No .Id == comparisons (except templates):**
```bash
grep -r "\.Id\s*==" src/ --include="*.cs" | grep -v Template | grep -v DTO

# Expected: Only PlacementId name comparisons, skeleton tracking, template references
```

**6. No facade methods with Id parameters:**
```bash
grep -r "public.*\(string.*Id\)" src/Subsystems/ --include="*Facade.cs"

# Expected: Only actionId, templateId (not entityId)
```

### 8.3: Manual Testing Checklist

**IF game runs:**

1. **Start new game**
   - Verify player spawns at initial location
   - Verify NPCs appear at locations
   - Verify routes are available

2. **Basic actions**
   - Talk to NPC (passes NPC object, not ID)
   - Travel to location (passes Location object, not ID)
   - View inventory
   - Check journal

3. **Scene system**
   - Start a scene
   - Verify choices display correctly
   - Complete a situation
   - Verify rewards apply

4. **Save/Load (if applicable)**
   - Save game
   - Load game
   - Verify all entities restored correctly

**IF game crashes:** Check browser console for null reference errors, fix broken object references.

---

## ROLLBACK PLAN (IF NEEDED)

**If refactoring breaks beyond repair:**

1. **Commit current broken state:**
   ```bash
   git add -A
   git commit -m "WIP: ID elimination refactoring - incomplete"
   ```

2. **Revert to previous commit:**
   ```bash
   git reset --hard HEAD~1
   ```

3. **Create new branch for incremental approach:**
   ```bash
   git checkout -b id-elimination-incremental
   ```

**However:** Incremental approach is NOT recommended. HIGHLANDER requires clean break.

---

## EXECUTION TIMELINE ESTIMATE

**Phase 1 (Domain Cleanup):** 2-3 hours
- 15-20 entity files
- Delete ~50 Id properties
- Expected: 500+ compilation errors

**Phase 2 (ViewModels):** 2-3 hours
- 30-50 ViewModel classes
- Replace Id → Object references
- Update facade ViewModel builders

**Phase 3 (UI Handlers):** 1-2 hours
- 10 component files
- 11 event handlers
- Update Razor templates

**Phase 4 (Facades):** 3-4 hours
- 22 facade files
- 138 method signatures
- Delete 5 GetById methods

**Phase 5 (Parsers/Services):** 3-4 hours
- 15 parser files
- 20 service files
- Fix object reference creation

**Phase 6 (Compilation Fix):** 2-4 hours
- 214 .Id == comparisons across 70 files
- LINQ query updates
- Debug logging fixes

**Phase 7 (Edge Cases):** 1-2 hours
- PlacementId system
- Special cases
- Template ID handling

**Phase 8 (Testing):** 1-2 hours
- Verification scripts
- Manual testing
- Bug fixes

**TOTAL ESTIMATED TIME:** 15-24 hours of focused work

**RECOMMENDED APPROACH:** Execute in one continuous session (or max 2-3 sessions) to maintain mental context.

---

## SUCCESS CRITERIA

**Refactoring is complete when:**

1. ✅ `dotnet build` succeeds with ZERO errors
2. ✅ ZERO `public string Id { get; set; }` in domain entities (except templates)
3. ✅ ZERO `GetLocationById` / `GetNPCById` / etc. methods
4. ✅ ZERO facade methods with `string entityId` parameters (only templateId allowed)
5. ✅ ZERO `.Id ==` comparisons in domain logic (except template/skeleton tracking)
6. ✅ All ViewModels use object references, not ID strings
7. ✅ All UI handlers pass objects, not IDs
8. ✅ Game runs without null reference exceptions
9. ✅ Manual testing passes: Talk to NPC, Travel, Start scene, Complete situation

**HIGHLANDER ACHIEVED:** One way to reference entities - Object references ONLY.

---

## NOTES AND WARNINGS

**1. This is a MASSIVE refactoring**
- ~100-120 files modified
- ~500-1000 individual changes
- Will break compilation for several hours

**2. NO partial completion**
- Cannot stop halfway through
- Must complete all phases for game to run
- Commit at END, not during

**3. NO compatibility layers**
- No `GetByIdOrName` hybrid methods
- No "deprecated" Id properties
- Clean break only

**4. Template IDs are EXCEPTION**
- SceneTemplate.Id → KEEP (catalogue reference)
- SituationTemplate.Id → KEEP (catalogue reference)
- Achievement.Id → KEEP (milestone template)
- State.Id → KEEP (condition template)
- These are configuration data, not runtime entities

**5. Name as Natural Key**
- NPC.Name is unique identifier (already implemented)
- Location.Name is unique identifier (after this refactoring)
- Item.Name is unique identifier
- Use object references in collections, not Name lookups

**6. Build verification is CRITICAL**
- Run `dotnet build` after EACH phase
- Fix errors before moving to next phase
- Track progress by error count reduction

**7. DTO vs Domain separation**
- DTOs KEEP Id properties (JSON structure)
- Domain entities DELETE Id properties
- Parsers map DTO → Domain (Name becomes key)

**8. GORDON RAMSAY STANDARD**
- "IDs are GONE. Object references ONLY. No compromises. No shortcuts. DONE."
