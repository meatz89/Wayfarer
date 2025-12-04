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
    /// PURE PROCEDURAL PLACEMENT: Place single location via distance-based selection.
    ///
    /// SEVEN-PHASE ALGORITHM (PRODUCTION):
    ///   0. Venue search space determination (proximity constraints)
    ///   1. Distance translation (hint → radius range)
    ///   2. Venue candidates (all venues - no Purpose→Type restriction)
    ///   3. Distance filtering (venues within radius from player)
    ///   4. Capacity check (query location count &lt; venue.MaxLocations)
    ///   5. Density check (prefer venues with fewer locations)
    ///   6. Selection strategy (closest to player)
    ///   7. Hex assignment within venue (atomic venue + hex assignment)
    ///
    /// NOTE: VenuePurposeCompatibility was REMOVED - it artificially restricted venues
    /// before distance filtering, causing locations to be placed in wrong venues.
    /// Any venue type can host any location Purpose (taverns have commerce, dwelling, etc.).
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// </summary>
    public void PlaceLocation(Location location, string distanceHint)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        Player player = _gameWorld.GetPlayer();
        if (player == null)
            throw new InvalidOperationException("Player not found in GameWorld");

        Console.WriteLine($"[LocationPlacement] === UNIFIED PLACEMENT ALGORITHM ===");
        Console.WriteLine($"[LocationPlacement] Placing location '{location.Name}' (Purpose: {location.Purpose}) with distance hint '{distanceHint}'");

        if (location.ProximityConstraintForPlacement != null)
        {
            Console.WriteLine($"[LocationPlacement] ProximityConstraint: {location.ProximityConstraintForPlacement.Proximity} relative to '{location.ProximityConstraintForPlacement.ReferenceLocationKey}'");
        }

        // PHASE 0: Determine venue search space (ALL venues OR constraint-filtered)
        Console.WriteLine($"[LocationPlacement] === PHASE 0: Venue Search Space Determination ===");
        List<Venue> venueSearchSpace = DetermineVenueSearchSpace(location, player);
        Console.WriteLine($"[LocationPlacement] Venue search space: {venueSearchSpace.Count} venues");

        // PHASE 1: Categorical distance translation
        Console.WriteLine($"[LocationPlacement] === PHASE 1: Distance Translation ===");

        // PHASE 1: Categorical distance translation
        DistanceRange distanceRange = TranslateDistanceHint(distanceHint);
        Console.WriteLine($"[LocationPlacement] Distance range: {distanceRange.MinRadius}-{distanceRange.MaxRadius} hexes from player at ({player.CurrentPosition.Q}, {player.CurrentPosition.R})");

        // PHASE 2: All venues are candidates (no Purpose→Type filtering)
        Console.WriteLine($"[LocationPlacement] === PHASE 2: Venue Candidates ===");
        List<Venue> matchingVenues = FindMatchingVenues(location, venueSearchSpace);

        if (matchingVenues.Count == 0)
        {
            string message = $"No venues available for location '{location.Name}'. " +
                           $"Ensure at least one venue exists in the game world.";
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
        // Hex-level constraints (SameLocation, AdjacentLocation) handled inside PlaceLocationsInVenue
        Console.WriteLine($"[LocationPlacement] === PHASE 7: Hex Assignment ===");
        PlaceLocationsInVenue(selectedVenue, new List<Location> { location });

        Console.WriteLine($"[LocationPlacement] SUCCESS: Placed '{location.Name}' at ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) in venue '{selectedVenue.Name}'");
    }

    /// <summary>
    /// PHASE 0: Determine venue search space based on ProximityConstraint.
    /// ProximityConstraint filters venues BEFORE Purpose matching.
    /// This is the HIGHLANDER integration: constraint is a filter, not a separate algorithm.
    /// </summary>
    private List<Venue> DetermineVenueSearchSpace(Location location, Player player)
    {
        ProximityConstraint constraint = location.ProximityConstraintForPlacement;

        // No constraint: search ALL venues (standard categorical placement)
        if (constraint == null || constraint.Proximity == PlacementProximity.Anywhere)
        {
            Console.WriteLine($"[LocationPlacement] No proximity constraint - searching all {_gameWorld.Venues.Count} venues");
            return _gameWorld.Venues.ToList();
        }

        // Resolve reference location from constraint key
        Location referenceLocation = ResolveReferenceLocation(constraint.ReferenceLocationKey, player, location.Name);
        Console.WriteLine($"[LocationPlacement] Reference location: '{referenceLocation.Name}' at venue '{referenceLocation.Venue?.Name}'");

        // Filter venues based on proximity type
        List<Venue> searchSpace = constraint.Proximity switch
        {
            PlacementProximity.SameVenue => new List<Venue> { referenceLocation.Venue },

            PlacementProximity.SameDistrict => _gameWorld.Venues
                .Where(v => v.District == referenceLocation.Venue?.District)
                .ToList(),

            PlacementProximity.SameRegion => _gameWorld.Venues
                .Where(v => v.District?.Region == referenceLocation.Venue?.District?.Region)
                .ToList(),

            // SameLocation and AdjacentLocation are hex-level constraints
            // They don't filter venues, they filter hexes (handled in Phase 7)
            // For now, return the reference venue as search space
            PlacementProximity.SameLocation => new List<Venue> { referenceLocation.Venue },
            PlacementProximity.AdjacentLocation => new List<Venue> { referenceLocation.Venue },

            _ => throw new InvalidOperationException(
                $"Unknown PlacementProximity value '{constraint.Proximity}' for location '{location.Name}'")
        };

        Console.WriteLine($"[LocationPlacement] Proximity '{constraint.Proximity}' narrowed search space to {searchSpace.Count} venues");
        return searchSpace;
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
    /// PHASE 2: Return all venues from search space as candidates.
    /// ARCHITECTURAL FIX: VenuePurposeCompatibility REMOVED - it artificially restricted venues
    /// before distance filtering, causing locations to be placed in wrong venues.
    /// Distance, capacity, and density are the correct constraints (not Purpose→Type mapping).
    /// Any venue type can host any location Purpose (taverns have commerce, dwelling, etc.).
    /// </summary>
    private List<Venue> FindMatchingVenues(Location location, List<Venue> venueSearchSpace)
    {
        // No filtering - all venues are valid candidates
        // Distance, capacity, and density determine final selection
        Console.WriteLine($"[LocationPlacement] All {venueSearchSpace.Count} venues are candidates (no Purpose→Type restriction)");
        return venueSearchSpace;
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
    /// Each location placed at first available unoccupied hex in venue cluster.
    /// HIGHLANDER: FindUnoccupiedHexInVenue is single source for hex availability.
    /// Validates venue capacity, hex availability.
    /// Player accessed via _gameWorld.GetPlayer() (never passed as parameter).
    /// </summary>
    public void PlaceLocationsInVenue(Venue venue, List<Location> locations)
    {
        Player player = _gameWorld.GetPlayer();
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

        foreach (Location location in locations)
        {
            // HIGHLANDER INTEGRATION: Check hex-level constraints (SameLocation, AdjacentLocation)
            ProximityConstraint constraint = location.ProximityConstraintForPlacement;
            bool hexConstraintApplied = false;

            if (constraint != null && player != null)
            {
                if (constraint.Proximity == PlacementProximity.SameLocation)
                {
                    // Place at EXACT same hex as reference location
                    Location referenceLocation = ResolveReferenceLocation(constraint.ReferenceLocationKey, player, location.Name);
                    location.HexPosition = referenceLocation.HexPosition.Value;
                    Console.WriteLine($"[LocationPlacement] Placed '{location.Name}' at SameLocation ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) as '{referenceLocation.Name}'");
                    hexConstraintApplied = true;
                }
                else if (constraint.Proximity == PlacementProximity.AdjacentLocation)
                {
                    // Place at hex ADJACENT to reference location
                    Location referenceLocation = ResolveReferenceLocation(constraint.ReferenceLocationKey, player, location.Name);
                    AxialCoordinates? adjacentHex = FindAdjacentHex(referenceLocation, venue);

                    if (!adjacentHex.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"Cannot place location '{location.Name}' AdjacentTo '{referenceLocation.Name}': " +
                            $"No available adjacent hexes. Venue '{venue.Name}' may have reached spatial density limit.");
                    }

                    location.HexPosition = adjacentHex.Value;
                    Console.WriteLine($"[LocationPlacement] Placed '{location.Name}' at ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) adjacent to '{referenceLocation.Name}'");
                    hexConstraintApplied = true;
                }
            }

            // Standard hex assignment (if no hex-level constraint applied)
            if (!hexConstraintApplied)
            {
                // HIGHLANDER: Find unoccupied hex in venue cluster
                // CRITICAL: previousLocation only tracks THIS batch - venue may already have locations
                AxialCoordinates? availableHex = FindUnoccupiedHexInVenue(venue);

                if (!availableHex.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Cannot place location '{location.Name}' in venue '{venue.Name}': " +
                        $"No unoccupied hexes available. Venue capacity: {venue.MaxLocations}, " +
                        $"Allocation: {venue.HexAllocation}. All hexes in venue cluster are occupied.");
                }

                location.HexPosition = availableHex;
                Console.WriteLine($"[LocationPlacement] Placed '{location.Name}' at ({availableHex.Value.Q}, {availableHex.Value.R}) in venue '{venue.Name}'");
            }

            // ATOMIC ASSIGNMENT: Set venue simultaneously with hex position
            location.AssignVenue(venue);

            // Calculate difficulty from hex distance to world center (0,0)
            // Formula: distance / 5 (integer division, DDR-007 compliant)
            // World center = starting area, outer hexes = higher difficulty
            CalculateLocationDifficulty(location);
        }
    }

    /// <summary>
    /// Calculate Location.Difficulty from hex distance to world center.
    /// Called after hex position assignment.
    /// Formula: DistanceTo(0,0) / 5 (integer division, DDR-007 compliant)
    /// HIGHLANDER: Single method for difficulty calculation, called from:
    /// - PlaceLocationsInVenue() for authored locations
    /// - PackageLoader.CreateSingleLocation() for scene-created locations
    /// </summary>
    public void CalculateLocationDifficulty(Location location)
    {
        if (!location.HexPosition.HasValue)
        {
            throw new InvalidOperationException(
                $"Cannot calculate difficulty for location '{location.Name}': HexPosition not set");
        }

        AxialCoordinates worldCenter = new AxialCoordinates(0, 0);
        int hexDistance = location.HexPosition.Value.DistanceTo(worldCenter);
        location.Difficulty = hexDistance / 5;

        Console.WriteLine($"[LocationPlacement] Calculated difficulty for '{location.Name}': " +
            $"distance={hexDistance} hexes from center, difficulty={location.Difficulty}");
    }

    /// <summary>
    /// Find first unoccupied hex in venue's cluster.
    /// HIGHLANDER: Single method for finding available hex in venue.
    /// Called by PackageLoader.CreateSingleLocation and PlaceLocationsInVenue.
    ///
    /// Search order:
    /// 1. Center hex (if unoccupied)
    /// 2. Adjacent hexes (for ClusterOf7 venues)
    /// </summary>
    public AxialCoordinates? FindUnoccupiedHexInVenue(Venue venue)
    {
        if (venue == null)
            throw new ArgumentNullException(nameof(venue));

        // Get all hexes allocated to this venue
        List<AxialCoordinates> venueHexes = venue.GetAllocatedHexes();
        Console.WriteLine($"[LocationPlacement] Searching for unoccupied hex in venue '{venue.Name}' (allocation: {venue.HexAllocation}, {venueHexes.Count} hexes)");

        // Search hexes in order: center first, then neighbors
        foreach (AxialCoordinates hexCoord in venueHexes)
        {
            // Check if any location occupies this hex
            bool hexOccupied = _gameWorld.Locations.Any(loc =>
                loc.HexPosition.HasValue &&
                loc.HexPosition.Value.Q == hexCoord.Q &&
                loc.HexPosition.Value.R == hexCoord.R);

            if (!hexOccupied)
            {
                Console.WriteLine($"[LocationPlacement] Found unoccupied hex at ({hexCoord.Q}, {hexCoord.R})");
                return hexCoord;
            }

            Console.WriteLine($"[LocationPlacement] Hex ({hexCoord.Q}, {hexCoord.R}) is occupied, checking next...");
        }

        // No unoccupied hex found
        Console.WriteLine($"[LocationPlacement] WARNING: No unoccupied hex found in venue '{venue.Name}'");
        return null;
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

    /// <summary>
    /// Resolve reference location key to actual Location object.
    /// FAIL-FAST: Throws exception if key is unknown or reference location invalid.
    /// </summary>
    private Location ResolveReferenceLocation(string key, Player player, string locationName)
    {
        if (key == "current")
        {
            Location current = _gameWorld.GetPlayerCurrentLocation();

            if (current == null)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve reference location key 'current' for location '{locationName}': " +
                    $"Player current location is null. Ensure player has valid current position before placing dependent locations.");
            }

            return current;
        }

        // Unknown reference location key
        throw new InvalidOperationException(
            $"Unknown reference location key '{key}' for location '{locationName}'. " +
            $"Valid keys: 'current'. Add new key resolution logic or fix JSON data.");
    }
}
