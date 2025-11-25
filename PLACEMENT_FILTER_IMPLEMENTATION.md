# PlacementFilter System Implementation

## Root Cause (10/10 Certainty)

**Bug**: Private Room spawns in wrong venue during A1 tutorial Scene activation.

**Evidence from Server Logs**:
```
[PlaceLocation] 'Common Room' assigned to venue 'The Brass Bell Inn' at hex (1, 0)
[SceneActivation] Starting activation for scene 'Secure Lodging' (Template: a1_secure_lodging)
[PlaceLocation] '{NPCName}'s Lodging Room' assigned to venue 'The Old Mill' at hex (-3, 3)  ← BUG
[IntraVenueMovement] Source: 'Common Room' (Venue: 'The Brass Bell Inn', Hex: (1, 0))
  Candidate: '{NPCName}'s Lodging Room' (Venue: 'The Old Mill', VenueMatch: False, Adjacent: False, Distance: 4)
```

**Impact**:
- No "Move to Private Room" action generated (venue mismatch)
- Player cannot progress through A1 tutorial
- Verisimilitude violation: "your room at this inn" spawns at different inn

**Cause**: Dependent locations use standard procedural placement WITHOUT venue constraints, causing them to spawn anywhere that matches categorical properties.

## Solution: PlacementFilter System

Introduce categorical spatial relationships to constrain dependent location placement relative to activation location.

### Core Principle

When generating dynamic content (dependent locations/NPCs), ALWAYS supply PlacementFilter with categorical proximity enum defining spatial relationship to reference location.

### Architecture

**Categorical Spatial Relationships** (PlacementProximity enum):
- `Anywhere` - No constraint (existing behavior)
- `SameLocation` - Spawn at exact same location (co-located entities)
- `AdjacentLocation` - Spawn at hex-adjacent location
- `SameVenue` - Spawn within same 7-hex venue cluster
- `SameDistrict` - Spawn within same district boundary
- `SameRegion` - Spawn within same regional boundary

**PlacementFilter Domain Class**:
```csharp
public class PlacementFilter
{
    public PlacementProximity Proximity { get; set; } = PlacementProximity.Anywhere;
    public string ReferenceLocationKey { get; set; } = "current";
    public LocationProperties? ReferenceLocationProperties { get; set; }
}
```

**Scaffolding Pattern**: PlacementFilter is temporary metadata stored on Location during parsing, used during placement, then cleared. NOT persisted in game state.

## Implementation Plan (10 Phases)

### Phase 1: Create PlacementProximity Enum
**File**: `src/GameState/Enums/PlacementProximity.cs` (NEW)

```csharp
/// <summary>
/// Categorical spatial relationships defining placement constraints relative to reference location.
/// Used by PlacementFilter to constrain dependent location/NPC placement during procedural generation.
/// </summary>
public enum PlacementProximity
{
    /// <summary>No spatial constraint - place anywhere using standard categorical placement</summary>
    Anywhere = 0,

    /// <summary>Place at exact same location as reference (co-located entities)</summary>
    SameLocation = 1,

    /// <summary>Place at hex-adjacent location (distance = 1)</summary>
    AdjacentLocation = 2,

    /// <summary>Place within same venue (7-hex cluster)</summary>
    SameVenue = 3,

    /// <summary>Place within same district boundary</summary>
    SameDistrict = 4,

    /// <summary>Place within same region boundary</summary>
    SameRegion = 5
}
```

### Phase 2: Create PlacementFilter Domain Class
**File**: `src/GameState/PlacementFilter.cs` (NEW)

```csharp
/// <summary>
/// Defines categorical spatial constraint for placing dependent locations/NPCs.
/// Scaffolding class: stored temporarily during parsing, used during placement, then cleared.
/// NOT persisted in game state - purely procedural generation metadata.
/// </summary>
public class PlacementFilter
{
    /// <summary>
    /// Categorical proximity relationship to reference location.
    /// Determines spatial constraint during placement.
    /// </summary>
    public PlacementProximity Proximity { get; set; } = PlacementProximity.Anywhere;

    /// <summary>
    /// Key identifying reference location in activation context.
    /// Common values: "current" (activation location), "origin" (scene origin), "player" (player's location).
    /// </summary>
    public string ReferenceLocationKey { get; set; } = "current";

    /// <summary>
    /// Optional categorical properties to match when finding reference location.
    /// Used when ReferenceLocationKey requires disambiguation.
    /// </summary>
    public LocationProperties? ReferenceLocationProperties { get; set; }
}
```

### Phase 3: Update LocationDTO with PlacementFilterDTO
**File**: `src/Content/DTOs/LocationDTO.cs` (MODIFY)

Add property to LocationDTO:
```csharp
[JsonPropertyName("placementFilter")]
public PlacementFilterDTO? PlacementFilter { get; set; }
```

Add new DTO class:
```csharp
/// <summary>
/// DTO for PlacementFilter - defines spatial placement constraints in JSON.
/// </summary>
public class PlacementFilterDTO
{
    [JsonPropertyName("proximity")]
    public string Proximity { get; set; } = "Anywhere";

    [JsonPropertyName("referenceLocation")]
    public string ReferenceLocation { get; set; } = "current";

    [JsonPropertyName("referenceLocationProperties")]
    public LocationPropertiesDTO? ReferenceLocationProperties { get; set; }
}
```

### Phase 4: SceneInstantiator Generates PlacementFilter
**File**: `src/Content/SceneInstantiator.cs` (MODIFY)

**Location**: `BuildLocationDTO` method (approximately line 1147)

Add PlacementFilter to generated LocationDTO:
```csharp
private LocationDTO BuildLocationDTO(DependentLocationSpec spec, string sceneId, SceneSpawnContext context)
{
    // ... existing code for categorical properties ...

    LocationDTO dto = new LocationDTO
    {
        Id = GenerateLocationId(spec, sceneId),
        Name = spec.Name,
        DistanceFromPlayer = spec.DistanceHint ?? "near",
        Description = spec.Description ?? string.Empty,
        Capabilities = spec.RequiredProperties.Capabilities.ToList(),
        Privacy = spec.RequiredProperties.Privacy,
        Safety = spec.RequiredProperties.Safety,
        Activity = spec.RequiredProperties.Activity,
        Purpose = spec.RequiredProperties.Purpose,
        CanInvestigate = false,

        // NEW: Add PlacementFilter for dependent locations
        PlacementFilter = new PlacementFilterDTO
        {
            Proximity = "SameVenue",  // Default for dependent locations
            ReferenceLocation = "current"  // Relative to activation location
        }
    };

    return dto;
}
```

### Phase 5: LocationParser Converts PlacementFilterDTO
**File**: `src/Content/LocationParser.cs` (MODIFY)

**Location**: `ConvertDTOToLocation` method

Add PlacementFilter parsing:
```csharp
public static Location ConvertDTOToLocation(LocationDTO dto, GameWorld gameWorld)
{
    Location location = new Location
    {
        // ... existing property mappings ...
    };

    // Parse PlacementFilter (scaffolding metadata)
    if (dto.PlacementFilter != null)
    {
        PlacementProximity proximity;
        if (!Enum.TryParse<PlacementProximity>(dto.PlacementFilter.Proximity, ignoreCase: true, out proximity))
        {
            Console.WriteLine($"[LocationParser] Invalid proximity value '{dto.PlacementFilter.Proximity}', defaulting to Anywhere");
            proximity = PlacementProximity.Anywhere;
        }

        location.PlacementFilterForPlacement = new PlacementFilter
        {
            Proximity = proximity,
            ReferenceLocationKey = dto.PlacementFilter.ReferenceLocation ?? "current",
            ReferenceLocationProperties = dto.PlacementFilter.ReferenceLocationProperties != null
                ? ConvertDTOToLocationProperties(dto.PlacementFilter.ReferenceLocationProperties)
                : null
        };
    }

    return location;
}
```

### Phase 6: Add PlacementFilterForPlacement to Location
**File**: `src/GameState/Location.cs` (MODIFY)

Add scaffolding property:
```csharp
/// <summary>
/// SCAFFOLDING PROPERTY: Temporary placement constraint metadata.
/// Set by parser, used by LocationPlacementService, then cleared.
/// NOT persisted in game state.
/// </summary>
public PlacementFilter? PlacementFilterForPlacement { get; set; }
```

### Phase 7: LocationPlacementService Applies PlacementFilter
**File**: `src/Subsystems/Location/LocationPlacementService.cs` (MODIFY)

**Location**: `PlaceLocation` method (main entry point)

Add PlacementFilter check at top:
```csharp
public void PlaceLocation(Location location, string distanceHint, Player player)
{
    // PRIORITY 1: Check for PlacementFilter (dependent location constraint)
    if (location.PlacementFilterForPlacement != null &&
        location.PlacementFilterForPlacement.Proximity != PlacementProximity.Anywhere)
    {
        Location? referenceLocation = ResolveReferenceLocation(
            location.PlacementFilterForPlacement.ReferenceLocationKey,
            player
        );

        if (referenceLocation == null)
        {
            throw new InvalidOperationException(
                $"Cannot resolve reference location '{location.PlacementFilterForPlacement.ReferenceLocationKey}' " +
                $"for placement filter on location '{location.Name}'"
            );
        }

        PlaceLocationWithProximityFilter(location, referenceLocation, location.PlacementFilterForPlacement.Proximity);
        return;
    }

    // PRIORITY 2: Standard categorical placement (existing code continues unchanged)
    // ... rest of existing PlaceLocation logic ...
}
```

Add helper methods:
```csharp
/// <summary>
/// Resolve reference location from activation context key.
/// </summary>
private Location? ResolveReferenceLocation(string referenceKey, Player player)
{
    if (referenceKey == "current" || referenceKey == "player")
    {
        return player.CurrentLocation;
    }

    // Future: support other keys like "origin", "destination", etc.
    Console.WriteLine($"[LocationPlacementService] Unknown reference location key: {referenceKey}");
    return null;
}

/// <summary>
/// Place location using categorical proximity constraint relative to reference location.
/// </summary>
private void PlaceLocationWithProximityFilter(Location location, Location referenceLocation, PlacementProximity proximity)
{
    switch (proximity)
    {
        case PlacementProximity.SameLocation:
            PlaceAtSameLocation(location, referenceLocation);
            break;

        case PlacementProximity.AdjacentLocation:
            PlaceAtAdjacentLocation(location, referenceLocation);
            break;

        case PlacementProximity.SameVenue:
            PlaceInSameVenue(location, referenceLocation);
            break;

        case PlacementProximity.SameDistrict:
            PlaceInSameDistrict(location, referenceLocation);
            break;

        case PlacementProximity.SameRegion:
            PlaceInSameRegion(location, referenceLocation);
            break;

        default:
            throw new InvalidOperationException($"Unsupported proximity: {proximity}");
    }
}

/// <summary>
/// Place at exact same location as reference (co-located).
/// </summary>
private void PlaceAtSameLocation(Location location, Location referenceLocation)
{
    if (!referenceLocation.HexPosition.HasValue)
    {
        throw new InvalidOperationException($"Reference location '{referenceLocation.Name}' has no hex position");
    }

    location.HexPosition = referenceLocation.HexPosition.Value;
    location.Venue = referenceLocation.Venue;
    location.District = referenceLocation.District;
    location.Region = referenceLocation.Region;
}

/// <summary>
/// Place at hex-adjacent location (distance = 1).
/// </summary>
private void PlaceAtAdjacentLocation(Location location, Location referenceLocation)
{
    if (!referenceLocation.HexPosition.HasValue)
    {
        throw new InvalidOperationException($"Reference location '{referenceLocation.Name}' has no hex position");
    }

    // Get neighbors of reference location
    List<AxialCoordinates> neighbors = referenceLocation.HexPosition.Value.GetNeighbors();

    // Filter to unoccupied hexes
    List<AxialCoordinates> availableHexes = neighbors
        .Where(hex => !_gameWorld.Locations.Any(loc => loc.HexPosition == hex))
        .ToList();

    if (!availableHexes.Any())
    {
        throw new InvalidOperationException($"No available adjacent hexes near '{referenceLocation.Name}'");
    }

    // Select random available hex
    AxialCoordinates selectedHex = availableHexes[_random.Next(availableHexes.Count)];

    location.HexPosition = selectedHex;
    location.Venue = referenceLocation.Venue;
    location.District = referenceLocation.District;
    location.Region = referenceLocation.Region;
}

/// <summary>
/// Place within same venue (7-hex cluster).
/// </summary>
private void PlaceInSameVenue(Location location, Location referenceLocation)
{
    if (referenceLocation.Venue == null)
    {
        throw new InvalidOperationException($"Reference location '{referenceLocation.Name}' has no venue");
    }

    // Get all hexes allocated to venue
    List<AxialCoordinates> allocatedHexes = referenceLocation.Venue.GetAllocatedHexes();

    // Filter to unoccupied hexes
    List<AxialCoordinates> availableHexes = allocatedHexes
        .Where(hex => !_gameWorld.Locations.Any(loc => loc.HexPosition == hex))
        .ToList();

    if (!availableHexes.Any())
    {
        throw new InvalidOperationException($"No available hexes in venue '{referenceLocation.Venue.Name}'");
    }

    // Select hex closest to reference location (prefer proximity within venue)
    AxialCoordinates selectedHex = availableHexes
        .OrderBy(hex => CalculateHexDistance(hex, referenceLocation.HexPosition.Value))
        .First();

    location.HexPosition = selectedHex;
    location.Venue = referenceLocation.Venue;
    location.District = referenceLocation.District;
    location.Region = referenceLocation.Region;
}

/// <summary>
/// Place within same district boundary.
/// </summary>
private void PlaceInSameDistrict(Location location, Location referenceLocation)
{
    if (referenceLocation.District == null)
    {
        throw new InvalidOperationException($"Reference location '{referenceLocation.Name}' has no district");
    }

    // Get all venues in same district
    List<Venue> districtVenues = _gameWorld.Venues
        .Where(v => v.District == referenceLocation.District)
        .ToList();

    if (!districtVenues.Any())
    {
        throw new InvalidOperationException($"No venues in district '{referenceLocation.District.Name}'");
    }

    // Collect all available hexes from all venues in district
    List<AxialCoordinates> availableHexes = new List<AxialCoordinates>();
    foreach (Venue venue in districtVenues)
    {
        List<AxialCoordinates> venueHexes = venue.GetAllocatedHexes()
            .Where(hex => !_gameWorld.Locations.Any(loc => loc.HexPosition == hex))
            .ToList();
        availableHexes.AddRange(venueHexes);
    }

    if (!availableHexes.Any())
    {
        throw new InvalidOperationException($"No available hexes in district '{referenceLocation.District.Name}'");
    }

    // Select hex closest to reference location
    AxialCoordinates selectedHex = availableHexes
        .OrderBy(hex => CalculateHexDistance(hex, referenceLocation.HexPosition.Value))
        .First();

    // Find venue containing selected hex
    Venue? targetVenue = districtVenues
        .FirstOrDefault(v => v.GetAllocatedHexes().Contains(selectedHex));

    location.HexPosition = selectedHex;
    location.Venue = targetVenue;
    location.District = referenceLocation.District;
    location.Region = referenceLocation.Region;
}

/// <summary>
/// Place within same region boundary.
/// </summary>
private void PlaceInSameRegion(Location location, Location referenceLocation)
{
    if (referenceLocation.Region == null)
    {
        throw new InvalidOperationException($"Reference location '{referenceLocation.Name}' has no region");
    }

    // Get all districts in same region
    List<District> regionDistricts = _gameWorld.Districts
        .Where(d => d.Region == referenceLocation.Region)
        .ToList();

    if (!regionDistricts.Any())
    {
        throw new InvalidOperationException($"No districts in region '{referenceLocation.Region.Name}'");
    }

    // Get all venues in those districts
    List<Venue> regionVenues = _gameWorld.Venues
        .Where(v => regionDistricts.Contains(v.District))
        .ToList();

    if (!regionVenues.Any())
    {
        throw new InvalidOperationException($"No venues in region '{referenceLocation.Region.Name}'");
    }

    // Collect all available hexes from all venues in region
    List<AxialCoordinates> availableHexes = new List<AxialCoordinates>();
    foreach (Venue venue in regionVenues)
    {
        List<AxialCoordinates> venueHexes = venue.GetAllocatedHexes()
            .Where(hex => !_gameWorld.Locations.Any(loc => loc.HexPosition == hex))
            .ToList();
        availableHexes.AddRange(venueHexes);
    }

    if (!availableHexes.Any())
    {
        throw new InvalidOperationException($"No available hexes in region '{referenceLocation.Region.Name}'");
    }

    // Select hex closest to reference location
    AxialCoordinates selectedHex = availableHexes
        .OrderBy(hex => CalculateHexDistance(hex, referenceLocation.HexPosition.Value))
        .First();

    // Find venue containing selected hex
    Venue? targetVenue = regionVenues
        .FirstOrDefault(v => v.GetAllocatedHexes().Contains(selectedHex));

    // Find district containing target venue
    District? targetDistrict = targetVenue?.District;

    location.HexPosition = selectedHex;
    location.Venue = targetVenue;
    location.District = targetDistrict;
    location.Region = referenceLocation.Region;
}

/// <summary>
/// Calculate hex distance for proximity ordering.
/// </summary>
private int CalculateHexDistance(AxialCoordinates hex1, AxialCoordinates hex2)
{
    int dq = Math.Abs(hex1.Q - hex2.Q);
    int dr = Math.Abs(hex1.R - hex2.R);
    int ds = Math.Abs((hex1.Q + hex1.R) - (hex2.Q + hex2.R));
    return Math.Max(Math.Max(dq, dr), ds);
}
```

### Phase 8: PackageLoader Clears Scaffolding
**File**: `src/Content/PackageLoader.cs` (MODIFY)

**Location**: End of `PlaceLocations` method (approximately line 1960)

Add cleanup:
```csharp
// Clear scaffolding metadata after placement
foreach (Location location in orderedLocations)
{
    location.DistanceHintForPlacement = null;
    location.PlacementFilterForPlacement = null;  // NEW
}
```

### Phase 9: Update Architecture Documentation

#### File: `arc42/08_crosscutting_concepts.md`

Add new section under "Crosscutting Concepts":

```markdown
### PlacementFilter System

**Category**: Procedural Content Generation Pattern

**Problem**: Dependent locations/NPCs spawned during scene activation must maintain spatial coherence with activation context. Without placement constraints, dependent locations can spawn anywhere matching categorical properties, leading to verisimilitude violations like "your room at this inn" spawning at a different inn.

**Solution**: PlacementFilter system provides categorical spatial relationships defining placement constraints relative to reference location.

#### Components

**PlacementProximity Enum**: Categorical spatial relationships defining placement constraints:
- `Anywhere` - No constraint (standard categorical placement)
- `SameLocation` - Co-located at exact same hex
- `AdjacentLocation` - Hex-adjacent (distance = 1)
- `SameVenue` - Within same 7-hex venue cluster
- `SameDistrict` - Within same district boundary
- `SameRegion` - Within same regional boundary

**PlacementFilter Class**: Domain class defining placement constraint:
- `Proximity` - PlacementProximity enum value
- `ReferenceLocationKey` - Key identifying reference location ("current", "player", etc.)
- `ReferenceLocationProperties` - Optional categorical properties for disambiguation

**Scaffolding Pattern**: PlacementFilter is temporary metadata stored on Location during parsing, used during placement, then cleared. NOT persisted in game state.

#### Data Flow

1. **Scene Activation**: SceneInstantiator generates dependent LocationDTO with PlacementFilterDTO
2. **Parsing**: LocationParser converts PlacementFilterDTO to PlacementFilter domain object, stores on Location.PlacementFilterForPlacement
3. **Placement**: LocationPlacementService checks PlacementFilter BEFORE standard categorical placement
4. **Constraint Application**: If PlacementFilter exists, resolve reference location and apply proximity constraint
5. **Cleanup**: PackageLoader clears PlacementFilterForPlacement after placement complete

#### Placement Priority

LocationPlacementService applies placement logic in priority order:
1. **PlacementFilter** (if present) - Categorical spatial constraint relative to reference
2. **Standard Categorical Placement** (if no filter) - Purpose → VenueType matching with distance hint

PlacementFilter takes absolute priority when present. This ensures dependent locations maintain spatial coherence with activation context.

#### Example Usage

Scene "Secure Lodging" spawns Private Room as dependent location:
```csharp
LocationDTO privateRoomDto = new LocationDTO
{
    Name = "Private Room",
    Capabilities = new List<string> { "SleepingSpace", "Indoor" },
    PlacementFilter = new PlacementFilterDTO
    {
        Proximity = "SameVenue",      // Must be in same 7-hex venue cluster
        ReferenceLocation = "current"  // Relative to activation location (Common Room)
    }
};
```

Result: Private Room spawns within The Brass Bell Inn venue (same as Common Room), ensuring narrative coherence for "your room at this inn".

#### Architectural Benefits

**Verisimilitude**: Spatial relationships match narrative descriptions ("your room at this inn" is actually at this inn)

**Categorical Purity**: Proximity defined as categorical enum values, not hardcoded distances or coordinates

**Scaffolding Pattern**: Placement metadata never persists in game state, only used during procedural generation

**Flexibility**: Reference location resolution extensible to support various keys ("current", "origin", "destination")

**Fail-Fast**: Invalid proximity or missing reference location throws immediately, preventing silent bugs
```

#### File: `arc42/12_glossary.md`

Add new entries:

```markdown
#### PlacementFilter
Domain class defining categorical spatial constraint for placing dependent locations/NPCs. Scaffolding property: stored temporarily during parsing, used during placement, then cleared. NOT persisted in game state. Contains:
- **Proximity**: PlacementProximity enum defining spatial relationship
- **ReferenceLocationKey**: Key identifying reference location in activation context
- **ReferenceLocationProperties**: Optional categorical properties for disambiguation

See: PlacementProximity, Scaffolding Pattern

#### PlacementProximity
Enum defining categorical spatial relationships for placement constraints. Values:
- **Anywhere**: No spatial constraint (standard categorical placement)
- **SameLocation**: Co-located at exact same hex as reference
- **AdjacentLocation**: Hex-adjacent to reference (distance = 1)
- **SameVenue**: Within same 7-hex venue cluster as reference
- **SameDistrict**: Within same district boundary as reference
- **SameRegion**: Within same regional boundary as reference

Used by PlacementFilter to constrain dependent location/NPC placement during procedural generation.

See: PlacementFilter, Spatial Scaffolding Pattern

#### Reference Location
Location used as spatial anchor for PlacementFilter constraint. Identified by ReferenceLocationKey:
- **"current"**: Activation location (where scene spawned)
- **"player"**: Player's current location
- **"origin"**: Scene origin location (future use)
- **"destination"**: Scene destination location (future use)

Resolved at placement time to determine spatial constraint baseline.

See: PlacementFilter, PlacementProximity
```

### Phase 10: Remove Temporary Debug Logging

**File**: `src/Content/PackageLoader.cs` (line ~1937)
**Remove**:
```csharp
Console.WriteLine($"[PlaceLocation] '{location.Name}' assigned to venue '{location.Venue?.Name ?? "NULL"}' at hex {location.HexPosition?.ToString() ?? "NULL"}");
```

**File**: `src/Content/Catalogs/LocationActionCatalog.cs` (lines ~152-173)
**Remove all debug logging**:
- Line ~152: `Console.WriteLine($"[IntraVenueMovement] Source: ...")`
- Lines ~158-166: Candidate evaluation logging in foreach loop
- Line ~198: `Console.WriteLine($"[IntraVenueMovement] ✅ Generated action: ...")`
- Method `CalculateHexDistance` (lines ~224-230) - can be removed if only used for debug logging

**File**: `src/Subsystems/Location/LocationFacade.cs` (lines ~455-473)
**Remove**:
```csharp
Console.WriteLine($"[SceneActivation] Starting activation for scene '{scene.DisplayName}' (Template: {scene.TemplateId})");
int locationCountBefore = _gameWorld.Locations.Count;
// ... existing code ...
int locationCountAfter = _gameWorld.Locations.Count;
int locationsAdded = locationCountAfter - locationCountBefore;
Console.WriteLine($"[SceneActivation] Loaded dependent resources for scene '{scene.DisplayName}' ({locationsAdded} locations added, total: {locationCountAfter})");
```

## Testing Strategy

### Unit Tests Required

1. **PlacementProximity Enum**: Verify all enum values defined correctly
2. **PlacementFilter Parsing**: Test LocationParser converts PlacementFilterDTO correctly
3. **Reference Location Resolution**: Test ResolveReferenceLocation for known keys
4. **Proximity Placement Methods**: Test each PlaceInSameVenue, PlaceAtAdjacentLocation, etc.

### Integration Test: A1 Tutorial End-to-End

1. Start game at Town Square Center
2. Navigate to Common Room
3. Trigger Situation 1 (Elena: Secure Lodging)
4. Verify Private Room spawns in The Brass Bell Inn venue
5. Verify "Move to Private Room" action appears
6. Click action and verify successful navigation
7. Verify Player.CurrentLocation is Private Room
8. Verify Private Room is adjacent to Common Room in same venue

### Verification Commands

**Build**: `cd src && dotnet build`
**Test**: `cd src && dotnet test`
**Run**: `ASPNETCORE_URLS="http://localhost:6900" dotnet run --no-build`

## Implementation Checklist

- [ ] Phase 1: Create PlacementProximity.cs
- [ ] Phase 2: Create PlacementFilter.cs
- [ ] Phase 3: Update LocationDTO with PlacementFilterDTO
- [ ] Phase 4: SceneInstantiator generates PlacementFilter
- [ ] Phase 5: LocationParser converts PlacementFilterDTO
- [ ] Phase 6: Add PlacementFilterForPlacement to Location
- [ ] Phase 7: LocationPlacementService applies PlacementFilter
- [ ] Phase 8: PackageLoader clears scaffolding
- [ ] Phase 9: Update architecture documentation
- [ ] Phase 10: Remove debug logging
- [ ] Build and verify no compilation errors
- [ ] Run A1 tutorial end-to-end test
- [ ] Commit with atomic changeset

## Key Architectural Principles Applied

**Categorical Properties**: PlacementProximity enum defines spatial relationships, no hardcoded distances
**Scaffolding Pattern**: PlacementFilter temporary metadata, cleared after use
**Fail-Fast**: Invalid proximity or missing reference throws immediately
**Object References**: Reference location resolved to object, not ID lookup
**Single Responsibility**: LocationPlacementService owns ALL placement logic
**Priority Ordering**: PlacementFilter takes priority over standard categorical placement
