/// <summary>
/// Handles spatial placement, hex sync, validation, and procedural route generation.
/// COMPOSITION OVER INHERITANCE: Extracted from PackageLoader for single responsibility.
/// </summary>
public class SpatialPlacementLoader
{
    private readonly GameWorld _gameWorld;
    private readonly LocationPlayabilityValidator _locationValidator;
    private readonly LocationPlacementService _locationPlacementService;

    public SpatialPlacementLoader(
        GameWorld gameWorld,
        LocationPlayabilityValidator locationValidator,
        LocationPlacementService locationPlacementService)
    {
        _gameWorld = gameWorld;
        _locationValidator = locationValidator;
        _locationPlacementService = locationPlacementService;
    }

    /// <summary>
    /// Place and validate all spatial entities after loading.
    /// Called ONCE after all packages loaded.
    /// </summary>
    public void PlaceAndValidateSpatialEntities(List<Venue> venues, List<Location> locations)
    {
        // HIGHLANDER: Procedural venue placement for ALL authored venues
        PlaceVenues(venues);

        // HIGHLANDER: Procedural hex placement for ALL locations
        PlaceLocations(locations);

        // PURE PROCEDURAL VALIDATION: Spatial venue assignment verification
        PackageLoaderValidation.ValidateVenueAssignmentsSpatially(_gameWorld);

        // HEX-BASED TRAVEL SYSTEM: Sync hex positions ONCE after all packages loaded
        SyncLocationHexPositions();
        EnsureHexGridCompleteness();
    }

    /// <summary>
    /// Validate all locations for playability.
    /// Called AFTER hex positions are synced.
    /// </summary>
    public void ValidateAllLocations()
    {
        foreach (Location location in _gameWorld.Locations)
        {
            _locationValidator.ValidateLocation(location, _gameWorld);
        }
    }

    /// <summary>
    /// Generate procedural routes between locations using hex-based pathfinding.
    /// Called ONCE after all packages loaded.
    /// </summary>
    public void GenerateProceduralRoutes()
    {
        if (_gameWorld.WorldHexGrid == null || _gameWorld.WorldHexGrid.Hexes.Count == 0)
            return;

        HexRouteGenerator generator = new HexRouteGenerator(_gameWorld);
        List<RouteOption> generatedRoutes = generator.GenerateAllRoutes();

        int addedCount = 0;
        foreach (RouteOption route in generatedRoutes)
        {
            bool routeExists = _gameWorld.Routes.Any(r => r.Name == route.Name);
            if (!routeExists)
            {
                _gameWorld.Routes.Add(route);
                addedCount++;
            }
        }
    }

    private void PlaceVenues(List<Venue> venues)
    {
        if (venues.Count == 0)
        {
            Console.WriteLine("[VenuePlacement] No venues to place");
            return;
        }

        Console.WriteLine($"[VenuePlacement] Starting procedural placement for {venues.Count} venues");

        List<Venue> venuesToPlace = venues
            .Where(v => !v.IsSkeleton)
            .OrderBy(v => v.Name)
            .ToList();

        Console.WriteLine($"[VenuePlacement] Found {venuesToPlace.Count} authored venues to place");

        VenueGeneratorService venueGenerator = new VenueGeneratorService();
        venueGenerator.PlaceAuthoredVenues(venuesToPlace, _gameWorld);

        Console.WriteLine($"[VenuePlacement] Completed procedural placement for {venuesToPlace.Count} authored venues");
    }

    private void PlaceLocations(List<Location> locations)
    {
        if (locations.Count == 0)
        {
            Console.WriteLine("[LocationPlacement] No locations to place");
            return;
        }

        Console.WriteLine($"[LocationPlacement] Starting PURE PROCEDURAL placement for {locations.Count} locations");

        Player player = _gameWorld.GetPlayer();
        if (player == null)
        {
            throw new InvalidOperationException("Cannot place locations without Player");
        }

        List<Location> orderedLocations = locations.OrderBy(l => l.Name).ToList();

        foreach (Location location in orderedLocations)
        {
            string distanceHint = location.DistanceHintForPlacement ?? "medium";
            Console.WriteLine($"[LocationPlacement] Processing location '{location.Name}' with distance hint '{distanceHint}'");
            _locationPlacementService.PlaceLocation(location, distanceHint, player);
        }

        Console.WriteLine($"[LocationPlacement] Completed PURE PROCEDURAL placement for {orderedLocations.Count} locations");

        // Clear temporary placement metadata
        foreach (Location location in orderedLocations)
        {
            location.DistanceHintForPlacement = null;
            location.ProximityConstraintForPlacement = null;
        }
        Console.WriteLine("[LocationPlacement] Cleared temporary placement metadata");
    }

    private void SyncLocationHexPositions()
    {
        Console.WriteLine("[HexSync] SyncLocationHexPositions called");

        if (_gameWorld.WorldHexGrid == null || _gameWorld.WorldHexGrid.Hexes.Count == 0)
        {
            Console.WriteLine("[HexSync] Skipping hex sync - no hex grid loaded");
            return;
        }

        HexParser.SyncLocationHexPositions(_gameWorld.WorldHexGrid, _gameWorld.Locations);
    }

    private void EnsureHexGridCompleteness()
    {
        Console.WriteLine("[HexGridCompleteness] Method called");

        if (_gameWorld.WorldHexGrid == null)
        {
            Console.WriteLine("[HexGridCompleteness] WorldHexGrid is null, returning early");
            return;
        }

        Console.WriteLine($"[HexGridCompleteness] Processing {_gameWorld.Locations.Count} locations");
        HexParser.EnsureHexGridCompleteness(_gameWorld.WorldHexGrid, _gameWorld.Locations);
    }
}
