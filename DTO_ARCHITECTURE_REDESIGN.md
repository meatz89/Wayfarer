# DTO ARCHITECTURE REDESIGN

**STATUS**: PAUSED FOR STABILIZATION

**Problem**: DTOs use entity instance IDs (LocationId, NpcId, VenueId) instead of categorical properties. This breaks procedural content generation - same template can't work across different procedural worlds.

**Solution**: Use PlacementFilterDTO with categorical properties everywhere. EntityResolver.FindOrCreate queries existing entities first, generates new from categories if no match.

**STABILIZATION COMPLETED** (2025-11-17):
1. ✅ Planning bloat deleted (8 files, ~120KB)
2. ✅ Venue logic restored (LocationDTO.VenueId, LocationParser venue resolution)
3. ✅ DTO migration path restored (NPCDTO.LocationId/VenueId temporarily kept for parsing existing JSON)
4. ⚠️ **REMAINING**: String lookup methods elimination

**REMAINING WORK BEFORE DTO REDESIGN**:

### String Lookup Methods to DELETE (Not Rename, COMPLETELY REMOVE):

**GameWorld.cs**:
- `GetLocation(string locationName)` - DELETE + fix 40+ callers with 9-step pattern
- `GetDistrictForLocation(string venueName)` - DELETE + trace callers
- `AddStrangerToLocation(string locationName, NPC stranger)` - DELETE + fix callers
- `GetJobByLocations(string originLocationName, string destinationLocationName)` - DELETE + fix callers
- `GetLocationCubes(string locationName)` - DELETE + fix callers
- `GrantLocationCubes(string locationName, int amount)` - DELETE + fix callers
- `GetLocationCountInVenue(string venueName)` - DELETE + fix callers
- `GetAvailableStrangers(string locationName, TimeBlocks currentTimeBlock)` - DELETE + fix callers
- `GetStrangerByName(string strangerName)` - DELETE + fix callers
- `GetNPCCubes(string npcName)` - DELETE + fix callers
- `GrantNPCCubes(string npcName, int amount)` - DELETE + fix callers

**NPCRepository.cs**:
- `GetById(string id)` - DELETE + fix callers
- `GetByName(string name)` - DELETE + fix callers

**9-Step Holistic Pattern for Each Method**:
1. Find method with string parameter
2. Grep ALL callers
3. Read caller files completely
4. Trace where caller got the string (object.Name extraction)
5. Change caller to pass object instead
6. Change method signature to accept object
7. Delete lookup from method body
8. Repeat up entire chain to UI
9. Verify: 0 .Name lookups for entity resolution

After string lookup elimination complete, THEN proceed with DTO layer redesign.

---

## 1. CORRECT DTO PATTERNS

### PlacementFilterDTO (Already Correct)
```csharp
// Entity matching via categorical properties
public class PlacementFilterDTO
{
    // NPC categorical dimensions
    public List<string> PersonalityTypes { get; set; }
    public List<string> Professions { get; set; }
    public List<string> SocialStandings { get; set; }
    public List<string> StoryRoles { get; set; }
    public List<string> KnowledgeLevels { get; set; }

    // Location categorical dimensions
    public List<string> LocationTypes { get; set; }
    public List<string> LocationProperties { get; set; }
    public List<string> PrivacyLevels { get; set; }
    public List<string> SafetyLevels { get; set; }
    public List<string> ActivityLevels { get; set; }
    public List<string> Purposes { get; set; }

    // Route categorical dimensions
    public List<string> TerrainTypes { get; set; }
    public int? MinDifficulty { get; set; }
    public int? MaxDifficulty { get; set; }

    // Selection strategy
    public string SelectionStrategy { get; set; } // "Random", "Closest", "LeastRecent"
}
```

### Entity Reference Pattern (Wrong vs Correct)

**❌ WRONG - Entity Instance IDs**:
```csharp
public class NPCDTO
{
    public string Id { get; set; }
    public string VenueId { get; set; }      // Instance ID - VIOLATION
    public string LocationId { get; set; }    // Instance ID - VIOLATION
}
```

**✅ CORRECT - Categorical Placement**:
```csharp
public class NPCDTO
{
    public string Id { get; set; }            // Template ID only (immutable archetype)
    public PlacementFilterDTO SpawnLocation { get; set; }  // Categorical properties
}
```

### Template ID Exception

**ACCEPTABLE**: Template IDs (SceneTemplate.Id, SituationTemplate.Id, NPCTemplate.Id)
- Templates are immutable archetypes (content library)
- Not mutable entity instances (runtime game state)
- Resolved ONCE at boundary, then domain uses objects

**VIOLATION**: Entity instance IDs (locationId, npcId, venueId)
- Should use PlacementFilters with categorical properties
- EntityResolver finds matching entity OR creates new

---

## 2. ENTITY RESOLUTION ARCHITECTURE

### EntityResolver.FindOrCreate Algorithm

```csharp
public Location FindOrCreateLocation(PlacementFilter filter)
{
    // 1. Query existing entities
    List<Location> matches = _gameWorld.Locations
        .Where(loc => LocationMatchesFilter(loc, filter))
        .ToList();

    // 2. Apply selection strategy if multiple matches
    if (matches.Count > 0)
        return ApplySelectionStrategy(matches, filter.SelectionStrategy);

    // 3. No match - create from categorical properties
    Location newLocation = CreateLocationFromCategories(filter);
    _gameWorld.Locations.Add(newLocation);
    return newLocation;
}
```

### Matching Priority

1. **Exact Match**: Entity has ALL specified categorical properties
2. **Partial Match**: Entity has SOME properties (if filter allows loose matching)
3. **Create**: No match found, generate new entity from filter properties

### Property-Based Search

```csharp
private bool LocationMatchesFilter(Location loc, PlacementFilter filter)
{
    // All specified properties must match (AND logic)
    if (filter.LocationProperties != null && filter.LocationProperties.Count > 0)
        if (!filter.LocationProperties.All(prop => loc.LocationProperties.Contains(prop)))
            return false;

    if (filter.LocationTypes != null && filter.LocationTypes.Count > 0)
        if (!filter.LocationTypes.Contains(loc.LocationType))
            return false;

    // Orthogonal dimensions (must match ONE OF)
    if (filter.PrivacyLevels != null && filter.PrivacyLevels.Count > 0)
        if (!filter.PrivacyLevels.Contains(loc.Privacy))
            return false;

    return true;
}
```

---

## 3. DTO LAYER AUDIT

### High-Priority Violations (Most Used)

**NPCDTO.cs**
- Current: `VenueId`, `LocationId` (entity instance IDs)
- Target: `SpawnLocation` (PlacementFilterDTO)
- Impact: ~150 NPCs in tutorial

**LocationDTO.cs**
- Current: `VenueId` (entity instance ID)
- Target: `SpawnVenue` (PlacementFilterDTO for venue categorical matching)
- Impact: ~80 locations

**ObligationDTO.cs**
- Current: `TargetLocationId`, `TargetNpcId` (entity instance IDs)
- Target: `TargetLocation`, `TargetNpc` (PlacementFilterDTO)
- Impact: ~20 obligations

**ConversationTreeDTO.cs**
- Current: `ParticipantNpcId` (entity instance ID)
- Target: `ParticipantFilter` (PlacementFilterDTO)
- Impact: ~30 conversations

**ExchangeDTO.cs**
- Current: `ProviderNpcId`, `LocationId` (entity instance IDs)
- Target: `ProviderFilter`, `LocationFilter` (PlacementFilterDTO)
- Impact: ~15 exchanges

### Medium-Priority Violations

**ObservationSceneDTO.cs, NavigationPayloadDTO.cs, RouteDTO.cs, EmergencySituationDTO.cs, StrangerNPCDTO.cs, VenueDTO.cs, HexDTO.cs, SituationCardDTO.cs, BondChangeDTO.cs**
- Pattern: All reference entity instances via ID strings
- Target: PlacementFilterDTO for categorical matching

### Migration Path (Per DTO)

1. Add `PlacementFilterDTO` property (e.g., `SpawnLocation`, `TargetLocation`)
2. Remove entity instance ID property (e.g., `LocationId`, `NpcId`)
3. Update JSON to use categorical properties instead of IDs
4. Update parser to call `EntityResolver.FindOrCreate(filter)` instead of lookup
5. Pass resolved object (not ID) to domain entity constructor

---

## 4. PARSER REFACTORING STRATEGY

### Current Pattern (WRONG)

```csharp
// NPCParser.cs - LOOKUP PATTERN
NPC npc = new NPC
{
    Id = dto.Id,
    Location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId),  // ❌ LOOKUP
    Venue = gameWorld.Venues.FirstOrDefault(v => v.Id == dto.VenueId)            // ❌ LOOKUP
};
```

### Target Pattern (CORRECT)

```csharp
// NPCParser.cs - RESOLUTION PATTERN
Location spawnLocation = entityResolver.FindOrCreateLocation(dto.SpawnLocation);  // ✅ RESOLVE

NPC npc = new NPC
{
    Id = dto.Id,
    Location = spawnLocation,  // ✅ OBJECT REFERENCE (no lookup)
    Venue = spawnLocation.Venue  // ✅ DERIVED FROM LOCATION
};
```

### EntityResolver Integration Points

**All Parsers Need EntityResolver**:
- NPCParser → resolve spawn locations
- ObligationParser → resolve target locations/NPCs
- ConversationTreeParser → resolve participants
- ExchangeParser → resolve providers/locations
- SceneTemplateParser → resolve placement filters for all scenes

**Inject via Constructor**:
```csharp
public NPCParser(GameWorld gameWorld, EntityResolver entityResolver)
{
    _gameWorld = gameWorld;
    _entityResolver = entityResolver;
}
```

### Procedural Route Generation

**Current (WRONG)**: RouteDTO contains origin/destination IDs
```json
{
  "id": "route_001",
  "originLocationId": "westmarch",
  "destinationLocationId": "crossroads"
}
```

**Target (CORRECT)**: Routes generated procedurally from hex grid
```csharp
// Route generation happens at GameWorld initialization
foreach (Location origin in locations)
{
    foreach (Location destination in locations.Where(d => d != origin))
    {
        List<AxialCoordinates> hexPath = pathfinder.FindPath(
            origin.HexPosition,
            destination.HexPosition
        );

        RouteOption route = new RouteOption
        {
            OriginLocation = origin,
            DestinationLocation = destination,
            HexPath = hexPath
        };

        gameWorld.Routes.Add(route);
    }
}
```

**No RouteDTO needed** - routes are pure spatial data derived from hex grid.

---

## 5. HEX SPATIAL SYSTEM

### Location Placement

```csharp
public class Location
{
    public AxialCoordinates HexPosition { get; set; }  // Source of truth
    public Venue Venue { get; set; }  // Spatial container
}
```

**Hex coordinates are mandatory** - all locations must have spatial position.

### Venue Definition

```csharp
public class Venue
{
    public AxialCoordinates CenterHex { get; set; }
    public int Radius { get; set; }  // Hex distance from center
}
```

**Venue = hex radius grouping around center point.**

Locations within radius belong to venue:
```csharp
bool IsInVenue(Location loc, Venue venue)
{
    int distance = loc.HexPosition.DistanceTo(venue.CenterHex);
    return distance <= venue.Radius;
}
```

### Route Generation (Procedural)

```csharp
// A* pathfinding on hex grid
public List<AxialCoordinates> FindPath(AxialCoordinates start, AxialCoordinates goal)
{
    // Standard A* with hex distance heuristic
    // Returns list of hex coordinates from start to goal
}
```

**Routes are generated, not authored** - hex grid is source of truth.

### Distance Calculations

```csharp
public struct AxialCoordinates
{
    public int Q { get; set; }
    public int R { get; set; }

    public int DistanceTo(AxialCoordinates other)
    {
        return (Math.Abs(Q - other.Q) +
                Math.Abs(Q + R - other.Q - other.R) +
                Math.Abs(R - other.R)) / 2;
    }
}
```

---

## IMPLEMENTATION PRIORITY

### Phase 1: Core Entity DTOs (Week 1)
1. NPCDTO - remove VenueId/LocationId, add SpawnLocation filter
2. LocationDTO - remove VenueId, add SpawnVenue filter (or derive from hex position)
3. Update NPCParser, LocationParser to use EntityResolver

### Phase 2: Relationship DTOs (Week 2)
4. ObligationDTO - remove target IDs, add target filters
5. ConversationTreeDTO - remove participant IDs, add participant filters
6. ExchangeDTO - remove provider/location IDs, add filters

### Phase 3: Remaining DTOs (Week 3)
7. All other DTOs with entity instance ID violations
8. Route generation system (replace RouteDTO entirely)

### Phase 4: Verification (Week 4)
9. Search codebase for all `.Id` property accesses in parsers
10. Verify 0 entity instance ID lookups remain
11. Test procedural content generation end-to-end

---

## SUCCESS CRITERIA

✅ **Zero entity instance IDs in DTOs** (only template IDs)
✅ **All parsers use EntityResolver.FindOrCreate**
✅ **Same scene template works in different procedural worlds**
✅ **Routes generated from hex grid, not authored**
✅ **All locations have hex positions**

---

## ARCHITECTURE VALIDATION

**Test**: Can a scene template spawn in 3 different procedural worlds without modification?

**Before (BROKEN)**:
```json
{
  "sceneId": "inn_scene",
  "npcId": "elena_innkeeper",
  "locationId": "rusty_tankard"
}
```
❌ Fails - elena and rusty_tankard don't exist in world 2.

**After (CORRECT)**:
```json
{
  "sceneId": "inn_scene",
  "npcFilter": { "professions": ["Innkeeper"], "storyRoles": ["Facilitator"] },
  "locationFilter": { "locationTypes": ["Inn"], "purposes": ["Dwelling"] }
}
```
✅ Works - EntityResolver finds existing innkeeper/inn OR creates new.

**This is DDR-006 (Categorical Scaling) - the ENTIRE architecture.**
