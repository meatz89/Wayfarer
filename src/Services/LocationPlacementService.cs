/// <summary>
/// Service responsible for procedural hex placement of ALL locations within venue clusters.
/// HIGHLANDER: Single algorithm for both authored and generated locations.
/// Eliminates dual placement system (JSON Q,R vs runtime calculation).
/// </summary>
public class LocationPlacementService
{
    private readonly GameWorld _gameWorld;

    public LocationPlacementService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Place all locations within a venue's hex cluster using procedural algorithm.
    /// First location: venue.CenterHex
    /// Subsequent locations: Adjacent to previous location
    /// Validates venue capacity, hex availability, venue separation.
    /// </summary>
    public void PlaceLocationsInVenue(Venue venue, List<Location> locations)
    {
        if (venue == null)
            throw new ArgumentNullException(nameof(venue));
        if (locations == null || locations.Count == 0)
            return;

        // Validate capacity budget
        if (locations.Count > venue.MaxLocations)
        {
            throw new InvalidOperationException(
                $"Venue '{venue.Name}' has {locations.Count} locations but MaxLocations={venue.MaxLocations}. " +
                $"Reduce location count or increase venue capacity.");
        }

        Console.WriteLine($"[LocationPlacement] Placing {locations.Count} locations in venue '{venue.Name}' (capacity: {venue.MaxLocations})");

        Location previousLocation = null;

        foreach (Location location in locations)
        {
            if (previousLocation == null)
            {
                // First location: Place at venue center
                location.HexPosition = venue.CenterHex;
                Console.WriteLine($"[LocationPlacement] Placed '{location.Name}' at venue center ({venue.CenterHex.Q}, {venue.CenterHex.R})");
            }
            else
            {
                // Subsequent locations: Find adjacent hex
                AxialCoordinates? adjacentHex = FindAdjacentHex(previousLocation, venue);

                if (!adjacentHex.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Could not find adjacent hex for location '{location.Name}' in venue '{venue.Name}'. " +
                        $"Venue may have reached spatial density limit.");
                }

                location.HexPosition = adjacentHex.Value;
                Console.WriteLine($"[LocationPlacement] Placed '{location.Name}' at ({adjacentHex.Value.Q}, {adjacentHex.Value.R}) adjacent to '{previousLocation.Name}'");
            }

            previousLocation = location;
        }
    }

    /// <summary>
    /// Find unoccupied adjacent hex that maintains venue separation.
    /// EXTRACTED from SceneInstantiator (HIGHLANDER: single implementation).
    /// </summary>
    private AxialCoordinates? FindAdjacentHex(Location baseLocation, Venue targetVenue)
    {
        if (!baseLocation.HexPosition.HasValue)
            throw new InvalidOperationException($"Base location '{baseLocation.Name}' has no HexPosition - cannot find adjacent hex");

        // Get neighbors from hex map
        HexMap hexMap = _gameWorld.WorldHexGrid;
        if (hexMap == null)
            throw new InvalidOperationException("GameWorld.WorldHexGrid is null - cannot find adjacent hex");

        Hex baseHex = hexMap.GetHex(baseLocation.HexPosition.Value);
        if (baseHex == null)
            throw new InvalidOperationException($"Base location hex position {baseLocation.HexPosition.Value} not found in HexMap");

        List<Hex> neighbors = hexMap.GetNeighbors(baseHex);

        // Find first unoccupied neighbor that doesn't violate venue separation
        string targetVenueName = targetVenue.Name;

        foreach (Hex neighborHex in neighbors)
        {
            // Check if any location occupies this hex
            bool hexOccupied = _gameWorld.Locations.Any(loc => loc.HexPosition.HasValue &&
                                                               loc.HexPosition.Value.Equals(neighborHex.Coordinates));
            if (hexOccupied)
            {
                Console.WriteLine($"[LocationPlacement] Skipping hex ({neighborHex.Coordinates.Q}, {neighborHex.Coordinates.R}): Already occupied");
                continue;
            }

            // SPATIAL CONSTRAINT: Check that placing location here maintains venue separation
            // The new location's neighbors must not contain locations from OTHER venues
            bool violatesSeparation = IsAdjacentToOtherVenue(neighborHex.Coordinates, targetVenueName);
            if (violatesSeparation)
            {
                Console.WriteLine($"[LocationPlacement] Skipping hex ({neighborHex.Coordinates.Q}, {neighborHex.Coordinates.R}): Would violate venue separation");
                continue;
            }

            Console.WriteLine($"[LocationPlacement] Selected hex ({neighborHex.Coordinates.Q}, {neighborHex.Coordinates.R}) for placement");
            return neighborHex.Coordinates;
        }

        // No valid hex found
        return null;
    }

    /// <summary>
    /// Check if a hex coordinate is adjacent to any location from a different venue.
    /// Used to enforce venue separation during placement.
    /// EXTRACTED from SceneInstantiator (HIGHLANDER: single implementation).
    /// </summary>
    private bool IsAdjacentToOtherVenue(AxialCoordinates hexCoords, string currentVenueName)
    {
        AxialCoordinates[] neighbors = hexCoords.GetNeighbors();

        foreach (AxialCoordinates neighborCoords in neighbors)
        {
            // Check if any location exists at this neighbor position
            Location neighborLocation = _gameWorld.Locations.FirstOrDefault(loc =>
                loc.HexPosition.HasValue &&
                loc.HexPosition.Value.Equals(neighborCoords));

            if (neighborLocation != null)
            {
                // If location exists and belongs to DIFFERENT venue, separation violated
                if (neighborLocation.Venue != null &&
                    neighborLocation.Venue.Name != currentVenueName)
                {
                    return true; // Adjacent to other venue
                }
            }
        }

        return false; // No adjacent venues found
    }
}
