# LocationSubsystem Implementation Context

## CRITICAL REQUIREMENTS
- **NO FALLBACKS** - Complete implementation only
- **NO COMPATIBILITY LAYERS** - Direct migration
- **NO TODOS** - Every method fully implemented
- **DELETE OLD CODE** - Remove after migration

## Files You MUST Read First

### Core Files to Analyze:
1. `/mnt/c/git/wayfarer/src/Services/GameFacade.cs` (Lines 251-3405 for location methods)
2. `/mnt/c/git/wayfarer/src/GameState/LocationRepository.cs` (Complete file)
3. `/mnt/c/git/wayfarer/src/GameState/LocationSpotRepository.cs` (Complete file)
4. `/mnt/c/git/wayfarer/src/GameState/Player.cs` (Lines 40-650 for location properties)
5. `/mnt/c/git/wayfarer/src/Content/NPCRepository.cs` (Lines 72-174 for location methods)

### UI Files That Use Location:
1. `/mnt/c/git/wayfarer/src/Pages/Components/LocationContent.razor.cs`
2. `/mnt/c/git/wayfarer/src/Pages/GameScreen.razor.cs`
3. `/mnt/c/git/wayfarer/src/Pages/MainGameplayView.razor.cs`

## Complete Method Migration Map

### From GameFacade.cs → LocationFacade.cs

**Public Methods (MUST ALL BE IMPLEMENTED):**
```csharp
// Line 251
public Location GetCurrentLocation()

// Line 258
public LocationSpot GetCurrentLocationSpot()

// Line 269
public bool MoveToSpot(string spotName)

// Line 464
public LocationScreenViewModel GetLocationScreen()

// Line 2851
public void RefreshLocationState()

// Line 3400
public List<NPC> GetNPCsAtLocation(string locationId)

// Line 3405
public List<NPC> GetNPCsAtCurrentSpot()
```

**Private Methods to Move to Internal Managers:**
```csharp
// Line 596 → LocationManager
private List<string> BuildLocationPath(Location location, LocationSpot spot)

// Line 614 → LocationSpotManager
private List<string> GetLocationTraits(Location location, LocationSpot spot)

// Line 621 → LocationActionManager
private List<LocationActionViewModel> GetLocationActions(Location location, LocationSpot spot)

// Line 766 → ObservationManager (keep reference)
private List<ObservationViewModel> GetLocationObservations(string locationId, string currentSpotId)

// Line 851 → LocationSpotManager
private List<AreaWithinLocationViewModel> GetAreasWithinLocation(Location location, LocationSpot currentSpot)

// Line 889 → LocationSpotManager
private string GetSpotDetail(LocationSpot spot)

// Line 904 → LocationNarrativeGenerator
private string GenerateSpotDetail(LocationSpot spot)

// Line 913 → LocationNarrativeGenerator
private string GenerateAtmosphereText(LocationSpot spot, Location location)

// Line 941 → RouteManager (keep reference)
private List<RouteOptionViewModel> GetRoutesFromLocation(Location location)

// Line 1327 → ObservationManager (keep reference)
private async Task<bool> ExecuteObserve(ObserveLocationIntent intent)

// Line 1722 → RouteManager (keep reference)
private List<DestinationViewModel> GetDestinations(Location currentLocation)

// Line 1785 → RouteManager (keep reference)
private List<LockedRouteViewModel> ConvertLockedRoutes(List<RouteOption> routes, string currentLocationId)

// Line 3066 → LocationActionManager
private void AddClosedServicesInfo(LocationActionsViewModel viewModel, LocationSpot currentSpot)
```

### From LocationRepository.cs → Incorporate into LocationManager

**All 14 Methods:**
```csharp
public Location GetCurrentLocation()
public LocationSpot GetCurrentLocationSpot()
public Location GetLocation(string locationId)
public Location GetLocationByName(string locationName)
public List<Location> GetAllLocations()
public List<LocationSpot> GetSpotsForLocation(string locationId)
public LocationSpot GetSpot(string locationId, string spotId)
public List<Location> GetConnectedLocations(string currentLocation)
public void AddLocation(Location location)
public void AddLocationSpot(LocationSpot spot)
public void SetCurrentLocation(Location location, LocationSpot spot)
public void SetCurrentSpot(LocationSpot spot)
public void RecordLocationVisit(string locationId)
public bool IsFirstVisit(string locationId)
```

### From NPCRepository.cs → NPCLocationTracker

**Location-Related Methods:**
```csharp
public List<NPC> GetNPCsForLocation(string locationId)
public List<NPC> GetNPCsForLocationAndTime(string locationId, TimeBlocks currentTime)
public List<NPC> GetNPCsForLocationSpotAndTime(string locationSpotId, TimeBlocks currentTime)
public NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime)
public List<ServiceTypes> GetAllLocationServices(string locationId)
```

## Required Directory Structure

```
/mnt/c/git/wayfarer/src/Subsystems/Location/
├── LocationFacade.cs         (250+ lines, all public methods)
├── LocationManager.cs         (200+ lines, core location logic)
├── LocationSpotManager.cs     (200+ lines, spot operations)
├── MovementValidator.cs       (100+ lines, movement rules)
├── NPCLocationTracker.cs      (150+ lines, NPC tracking)
├── LocationActionManager.cs   (150+ lines, action generation)
└── LocationNarrativeGenerator.cs (100+ lines, atmosphere text)
```

## Dependencies to Inject

### LocationFacade Constructor:
```csharp
public LocationFacade(
    GameWorld gameWorld,
    LocationManager locationManager,
    LocationSpotManager spotManager,
    MovementValidator movementValidator,
    NPCLocationTracker npcTracker,
    LocationActionManager actionManager,
    LocationNarrativeGenerator narrativeGenerator,
    // External dependencies for references:
    ObservationSystem observationSystem,
    RouteRepository routeRepository,
    NPCRepository npcRepository,
    TimeManager timeManager
)
```

## GameFacade Updates Required

### Constructor Change:
```csharp
// ADD:
private readonly LocationFacade _locationFacade;

// In constructor, ADD:
LocationFacade locationFacade,

// In constructor body, ADD:
_locationFacade = locationFacade;
```

### Method Replacements:
```csharp
// REPLACE Line 251-256:
public Location GetCurrentLocation()
{
    return _locationFacade.GetCurrentLocation();
}

// REPLACE Line 258-266:
public LocationSpot GetCurrentLocationSpot()
{
    return _locationFacade.GetCurrentLocationSpot();
}

// REPLACE Line 269-295:
public bool MoveToSpot(string spotName)
{
    return _locationFacade.MoveToSpot(spotName);
}

// REPLACE Line 464-550:
public LocationScreenViewModel GetLocationScreen()
{
    return _locationFacade.GetLocationScreen();
}

// And so on for ALL location methods
```

## UI Component Updates

### LocationContent.razor.cs:
- No changes needed if GameFacade methods preserved
- Just ensure GameFacade delegates to LocationFacade

### GameScreen.razor.cs:
- No changes needed if GameFacade methods preserved

### MainGameplayView.razor.cs:
- No changes needed if GameFacade methods preserved

## Validation Requirements

1. **All methods implemented** - No empty bodies
2. **Compiles without errors** - Run `dotnet build`
3. **No TODOs** - Search for "TODO" should find nothing
4. **No NotImplementedException** - Search should find nothing
5. **GameFacade updated** - All location methods delegate
6. **Old repositories work** - Don't delete LocationRepository yet
7. **UI still functions** - Test with game running

## Testing Commands

```bash
# Check all files created
ls -la /mnt/c/git/wayfarer/src/Subsystems/Location/

# Check file sizes (no small files)
wc -l /mnt/c/git/wayfarer/src/Subsystems/Location/*.cs

# Check for TODOs
grep -r "TODO" /mnt/c/git/wayfarer/src/Subsystems/Location/

# Check for NotImplementedException
grep -r "NotImplementedException" /mnt/c/git/wayfarer/src/Subsystems/Location/

# Compile
cd /mnt/c/git/wayfarer/src && dotnet build

# Run game
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run --no-build
```

## Common Pitfalls to Avoid

1. **Empty Methods** - Every method must have real implementation
2. **Returning null** - Return proper defaults or empty collections
3. **Missing Dependencies** - Ensure all injected services available
4. **Partial Migration** - Move ALL related code
5. **Breaking UI** - Maintain exact same public API

## Success Criteria

- [ ] All 7 public methods in LocationFacade
- [ ] All 14 private methods distributed to managers
- [ ] All 14 LocationRepository methods incorporated
- [ ] All 5 NPC location methods in NPCLocationTracker
- [ ] GameFacade delegates all location calls
- [ ] Compiles without errors
- [ ] Game runs and location features work
- [ ] No TODOs or placeholders