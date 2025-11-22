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
    /// PURE PROCEDURAL PLACEMENT: Place single location via categorical matching.
    ///
    /// SEVEN-PHASE ALGORITHM (PRODUCTION):
    ///   1. Distance translation (hint → radius range)
    ///   2. Venue matching (Purpose → Type categorical matching)
    ///   3. Distance filtering (venues within radius from player)
    ///   4. Capacity check (query location count &lt; venue.MaxLocations)
    ///   5. Density check (prefer venues with fewer locations)
    ///   6. Selection strategy (closest to player)
    ///   7. Hex assignment within venue (atomic venue + hex assignment)
    /// </summary>
    public void PlaceLocation(Location location, string distanceHint, Player player)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        Console.WriteLine($"[LocationPlacement] === PHASE 1: Distance Translation ===");
        Console.WriteLine($"[LocationPlacement] Placing location '{location.Name}' (Purpose: {location.Purpose}) with distance hint '{distanceHint}'");

        // PHASE 1: Categorical distance translation
        DistanceRange distanceRange = TranslateDistanceHint(distanceHint);
        Console.WriteLine($"[LocationPlacement] Distance range: {distanceRange.MinRadius}-{distanceRange.MaxRadius} hexes from player at ({player.CurrentPosition.Q}, {player.CurrentPosition.R})");

        // PHASE 2: Venue matching via categorical properties
        Console.WriteLine($"[LocationPlacement] === PHASE 2: Venue Matching ===");
        List<Venue> matchingVenues = FindMatchingVenues(location);
        Console.WriteLine($"[LocationPlacement] Found {matchingVenues.Count} venues matching Purpose={location.Purpose}");

        if (matchingVenues.Count == 0)
        {
            string message = $"No venues match location '{location.Name}' Purpose={location.Purpose}. " +
                           $"Available venue types: {string.Join(", ", _gameWorld.Venues.Select(v => v.Type))}. " +
                           $"Add matching venue or change location Purpose.";
            Console.WriteLine($"[LocationPlacement] ERROR: {message}");
            throw new InvalidOperationException(message);
        }

        // PHASE 3: Distance filtering
        Console.WriteLine($"[LocationPlacement] === PHASE 3: Distance Filtering ===");
        List<Venue> distanceFiltered = FilterVenuesByDistance(matchingVenues, distanceRange, player.CurrentPosition);
        Console.WriteLine($"[LocationPlacement] {distanceFiltered.Count} venues within distance range");

        // Fallback: If no venues within distance range, ignore distance constraint
        if (distanceFiltered.Count == 0)
        {
            Console.WriteLine($"[LocationPlacement] WARNING: No venues within distance '{distanceHint}', ignoring distance constraint");
            distanceFiltered = matchingVenues; // Use all matching venues
        }

        // PHASE 4: Capacity budget check
        Console.WriteLine($"[LocationPlacement] === PHASE 4: Capacity Budget Check ===");
        List<Venue> capacityFiltered = FilterVenuesByCapacity(distanceFiltered);
        Console.WriteLine($"[LocationPlacement] {capacityFiltered.Count} venues with available capacity");

        if (capacityFiltered.Count == 0)
        {
            string message = $"All matching venues at capacity for location '{location.Name}' Purpose={location.Purpose}. " +
                           $"Generate new venue or increase MaxLocations on existing venues.";
            Console.WriteLine($"[LocationPlacement] ERROR: {message}");
            throw new InvalidOperationException(message);
        }

        // PHASE 5: Density check (sort by location count ascending)
        Console.WriteLine($"[LocationPlacement] === PHASE 5: Density Check ===");
        List<Venue> sortedByDensity = capacityFiltered
            .OrderBy(v => _gameWorld.Locations.Count(loc => loc.Venue == v))
            .ToList();
        Console.WriteLine($"[LocationPlacement] Venues sorted by density (prefer less populated)");

        // PHASE 6: Venue selection strategy (closest to player as tiebreaker)
        Console.WriteLine($"[LocationPlacement] === PHASE 6: Venue Selection ===");
        Venue selectedVenue = SelectVenue(sortedByDensity, player.CurrentPosition);
        int selectedVenueCount = _gameWorld.Locations.Count(loc => loc.Venue == selectedVenue);
        Console.WriteLine($"[LocationPlacement] Selected venue: '{selectedVenue.Name}' (Type: {selectedVenue.Type}, Locations: {selectedVenueCount}/{selectedVenue.MaxLocations})");

        // PHASE 7: Hex assignment within venue (atomic venue + hex assignment)
        Console.WriteLine($"[LocationPlacement] === PHASE 7: Hex Assignment ===");
        PlaceLocationsInVenue(selectedVenue, new List<Location> { location });

        Console.WriteLine($"[LocationPlacement] SUCCESS: Placed '{location.Name}' at ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) in venue '{selectedVenue.Name}'");
    }

    /// <summary>
    /// PHASE 1: Translate categorical distance hint to hex radius range.
    /// Deterministic lookup table, no randomness.
    /// </summary>
    private DistanceRange TranslateDistanceHint(string hint)
    {
        DistanceRange range = hint switch
        {
            "start" => new DistanceRange(0, 1),     // Player origin area (tutorial locations)
            "near" => new DistanceRange(2, 5),      // Early game (short travel)
            "medium" => new DistanceRange(6, 12),   // Mid game (moderate travel)
            "far" => new DistanceRange(13, 25),     // Late game (long travel)
            "distant" => new DistanceRange(26, 50), // End game (epic travel)
            _ => new DistanceRange(0, 100)          // Unknown hint: accept all distances
        };

        return range;
    }

    /// <summary>
    /// PHASE 2: Find all venues matching location's Purpose via compatibility table.
    /// Uses VenuePurposeCompatibility static lookup for semantic bridge.
    /// Many-to-many mapping: One purpose → Multiple venue types.
    /// Example: Commerce purpose matches Market, Merchant, Workshop venues.
    /// </summary>
    private List<Venue> FindMatchingVenues(Location location)
    {
        // Get compatible venue types from lookup table
        List<VenueType> compatibleTypes = VenuePurposeCompatibility.GetCompatibleTypes(location.Purpose);

        // Filter venues by compatibility
        List<Venue> matching = _gameWorld.Venues
            .Where(venue => compatibleTypes.Contains(venue.Type))
            .ToList();

        // Log matches for debugging
        foreach (Venue venue in matching)
        {
            Console.WriteLine($"[LocationPlacement] Venue '{venue.Name}' (Type: {venue.Type}) matches Purpose: {location.Purpose}");
        }

        return matching;
    }

    /// <summary>
    /// PHASE 3: Filter venues by distance from player (within min-max radius range).
    /// Uses hex distance formula: venue.CenterHex.DistanceTo(playerPosition).
    /// </summary>
    private List<Venue> FilterVenuesByDistance(List<Venue> venues, DistanceRange range, AxialCoordinates playerPosition)
    {
        List<Venue> filtered = new List<Venue>();

        foreach (Venue venue in venues)
        {
            int distance = venue.CenterHex.DistanceTo(playerPosition);
            bool withinRange = distance >= range.MinRadius && distance <= range.MaxRadius;

            Console.WriteLine($"[LocationPlacement] Venue '{venue.Name}' distance: {distance} hexes (range: {range.MinRadius}-{range.MaxRadius}) - {(withinRange ? "ACCEPT" : "REJECT")}");

            if (withinRange)
            {
                filtered.Add(venue);
            }
        }

        return filtered;
    }

    /// <summary>
    /// PHASE 4: Filter venues with available capacity (location count &lt; MaxLocations).
    /// Enforces capacity budget to prevent unlimited venue expansion.
    /// Queries GameWorld.Locations to count current locations per venue.
    /// </summary>
    private List<Venue> FilterVenuesByCapacity(List<Venue> venues)
    {
        List<Venue> filtered = new List<Venue>();

        foreach (Venue venue in venues)
        {
            bool hasCapacity = venue.CanAddLocation(_gameWorld);
            int currentCount = _gameWorld.Locations.Count(loc => loc.Venue == venue);
            Console.WriteLine($"[LocationPlacement] Venue '{venue.Name}' capacity: {currentCount}/{venue.MaxLocations} - {(hasCapacity ? "AVAILABLE" : "FULL")}");

            if (hasCapacity)
            {
                filtered.Add(venue);
            }
        }

        return filtered;
    }

    /// <summary>
    /// PHASE 6: Select single venue from filtered list.
    /// Strategy: Closest to player (deterministic tiebreaker).
    /// Assumes venues already sorted by density (PHASE 5), so first match is best.
    /// </summary>
    private Venue SelectVenue(List<Venue> venues, AxialCoordinates playerPosition)
    {
        if (venues.Count == 0)
            throw new InvalidOperationException("SelectVenue called with empty venue list");

        // Venues already sorted by density (location count ascending)
        // Select closest to player as tiebreaker
        Venue closest = venues
            .OrderBy(v => v.CenterHex.DistanceTo(playerPosition))
            .First();

        int distance = closest.CenterHex.DistanceTo(playerPosition);
        Console.WriteLine($"[LocationPlacement] Selected '{closest.Name}' at distance {distance} hexes (closest among {venues.Count} candidates)");

        return closest;
    }

    /// <summary>
    /// Helper struct for distance range (min/max radius in hexes).
    /// </summary>
    private struct DistanceRange
    {
        public int MinRadius { get; }
        public int MaxRadius { get; }

        public DistanceRange(int minRadius, int maxRadius)
        {
            MinRadius = minRadius;
            MaxRadius = maxRadius;
        }
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

            // ATOMIC ASSIGNMENT: Set venue simultaneously with hex position
            location.AssignVenue(venue);

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
