/// <summary>
/// Generates procedural locations from PlacementFilter specifications.
/// Used as fallback when no existing location matches filter criteria.
/// Integrates with venue generation budget system.
/// </summary>
public class LocationGeneratorService
{
    private readonly GameWorld _gameWorld;
    private readonly VenueGeneratorService _venueGenerator;
    private readonly HexSynchronizationService _hexSync;
    private readonly GeneratedLocationValidator _validator;

    public LocationGeneratorService(
        GameWorld gameWorld,
        VenueGeneratorService venueGenerator,
        HexSynchronizationService hexSync,
        GeneratedLocationValidator validator)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _venueGenerator = venueGenerator ?? throw new ArgumentNullException(nameof(venueGenerator));
        _hexSync = hexSync ?? throw new ArgumentNullException(nameof(hexSync));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Generate location matching PlacementFilter criteria.
    /// Returns null if generation fails or budget exhausted.
    /// Throws InvalidOperationException if validation fails (fail-fast).
    /// </summary>
    public Location GenerateLocation(PlacementFilter filter, SceneSpawnContext context, SceneProvenance provenance)
    {
        if (filter.PlacementType != PlacementType.Location)
        {
            throw new ArgumentException($"PlacementFilter must have PlacementType.Location, got {filter.PlacementType}");
        }

        // 1. Find or generate venue with budget
        Venue venue = FindOrGenerateVenue(filter, context);
        if (venue == null || !venue.CanGenerateMoreLocations())
        {
            Console.WriteLine($"[LocationGenerator] Cannot generate location - no venue with available budget");
            return null; // No venue or budget exhausted
        }

        // 2. Generate unique location ID
        string locationId = $"generated_location_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // 3. Generate name and description from filter properties
        string locationName = GenerateLocationName(filter, venue);
        string locationDescription = GenerateLocationDescription(filter, venue);

        // 4. Find unoccupied hex position (prefer adjacent to context location)
        AxialCoordinates? hexPosition = FindUnoccupiedHex(venue, context.CurrentLocation);
        if (!hexPosition.HasValue)
        {
            Console.WriteLine($"[LocationGenerator] Failed to find unoccupied hex for location in venue '{venue.Id}'");
            return null; // No space available
        }

        // 5. Create Location entity
        Location location = new Location(locationId, locationName)
        {
            Description = locationDescription,
            VenueId = venue.Id,
            Venue = venue,
            HexPosition = hexPosition.Value,
            LocationProperties = filter.LocationProperties?.ToList() ?? new List<LocationPropertyType>(),
            DomainTags = filter.LocationTags?.ToList() ?? new List<string>(),
            Tier = venue.Tier,
            HasBeenVisited = false,
            IsLocked = false,
            Provenance = provenance
        };

        // 6. Synchronize hex reference
        _hexSync.SyncLocationToHex(location, _gameWorld);

        // 7. Validate playability
        _validator.ValidateLocation(location, _gameWorld);

        // 8. Add to GameWorld and increment venue budget
        _gameWorld.Locations.Add(location);
        venue.IncrementGeneratedCount();

        Console.WriteLine($"[LocationGenerator] Generated location '{locationId}' in venue '{venue.Id}' at hex ({hexPosition.Value.Q}, {hexPosition.Value.R})");

        return location;
    }

    /// <summary>
    /// Find existing venue matching filter, or generate new venue if needed.
    /// Returns null if no suitable venue found and generation fails.
    /// </summary>
    private Venue FindOrGenerateVenue(PlacementFilter filter, SceneSpawnContext context)
    {
        // Try to find existing venue with budget
        List<Venue> matchingVenues = _gameWorld.Venues.Where(v =>
        {
            // Match district if specified
            if (!string.IsNullOrEmpty(filter.DistrictId))
            {
                if (v.District != filter.DistrictId)
                    return false;
            }

            // Check budget
            if (!v.CanGenerateMoreLocations())
                return false;

            return true;
        }).ToList();

        if (matchingVenues.Any())
        {
            // Select random matching venue with budget
            Random random = new Random();
            return matchingVenues[random.Next(matchingVenues.Count)];
        }

        // No existing venue with budget - generate new venue
        Console.WriteLine($"[LocationGenerator] No venue with budget found, generating new venue");

        VenueTemplate venueTemplate = new VenueTemplate
        {
            NamePattern = DetermineVenueName(filter, context),
            DescriptionPattern = "A procedurally generated venue.",
            Type = DetermineVenueType(filter),
            Tier = context.CurrentLocation?.Tier ?? 1,
            District = filter.DistrictId ?? context.CurrentLocation?.Venue?.District ?? "wilderness",
            MaxGeneratedLocations = 20,
            HexAllocation = HexAllocationStrategy.ClusterOf7
        };

        Venue generatedVenue = _venueGenerator.GenerateVenue(venueTemplate, context, _gameWorld);
        return generatedVenue;
    }

    /// <summary>
    /// Find unoccupied hex position for new location.
    /// Prefers adjacent to context location, falls back to any hex in venue cluster.
    /// </summary>
    private AxialCoordinates? FindUnoccupiedHex(Venue venue, Location contextLocation)
    {
        // Strategy 1: Adjacent to context location (if in same venue)
        if (contextLocation != null && contextLocation.VenueId == venue.Id && contextLocation.HexPosition.HasValue)
        {
            List<Hex> neighbors = _gameWorld.WorldHexGrid.GetNeighbors(_gameWorld.WorldHexGrid.GetHex(contextLocation.HexPosition.Value));
            foreach (Hex neighborHex in neighbors)
            {
                bool hexOccupied = _gameWorld.Locations.Any(loc => loc.HexPosition.HasValue && loc.HexPosition.Value.Equals(neighborHex.Coordinates));
                if (!hexOccupied)
                {
                    return neighborHex.Coordinates;
                }
            }
        }

        // Strategy 2: Any unoccupied hex in venue (scan all locations in venue for occupied hexes)
        List<Location> venueLocations = _gameWorld.Locations.Where(loc => loc.VenueId == venue.Id).ToList();
        if (venueLocations.Any(loc => loc.HexPosition.HasValue))
        {
            // Get hexes occupied by venue
            List<AxialCoordinates> occupiedHexes = venueLocations
                .Where(loc => loc.HexPosition.HasValue)
                .Select(loc => loc.HexPosition.Value)
                .ToList();

            // Find first occupied hex and check its neighbors
            AxialCoordinates firstHex = occupiedHexes.First();
            List<Hex> neighbors = _gameWorld.WorldHexGrid.GetNeighbors(_gameWorld.WorldHexGrid.GetHex(firstHex));

            foreach (Hex neighborHex in neighbors)
            {
                bool hexOccupied = _gameWorld.Locations.Any(loc => loc.HexPosition.HasValue && loc.HexPosition.Value.Equals(neighborHex.Coordinates));
                if (!hexOccupied)
                {
                    return neighborHex.Coordinates;
                }
            }
        }

        // Strategy 3: Fallback to VenueGeneratorService logic (find new cluster)
        // For now, return null - venue should already have allocated space
        return null;
    }

    /// <summary>
    /// Generate location name from filter properties.
    /// Uses properties to determine semantic name pattern.
    /// </summary>
    private string GenerateLocationName(PlacementFilter filter, Venue venue)
    {
        // Simple pattern: "<Property> <VenueType>"
        // Example: "Private Tavern", "Secluded Chamber", "Indoor Marketplace"

        if (filter.LocationProperties != null && filter.LocationProperties.Any())
        {
            string primaryProperty = filter.LocationProperties.First().ToString();
            return $"{primaryProperty} {venue.Type}";
        }

        if (filter.LocationTags != null && filter.LocationTags.Any())
        {
            string primaryTag = filter.LocationTags.First();
            return $"{primaryTag} Location";
        }

        return $"Generated {venue.Type}";
    }

    /// <summary>
    /// Generate location description from filter properties and venue context.
    /// </summary>
    private string GenerateLocationDescription(PlacementFilter filter, Venue venue)
    {
        string propertiesText = filter.LocationProperties != null && filter.LocationProperties.Any()
            ? string.Join(", ", filter.LocationProperties.Select(p => p.ToString().ToLower()))
            : "various characteristics";

        return $"A procedurally generated location in {venue.Name} with {propertiesText}.";
    }

    /// <summary>
    /// Determine venue name pattern based on filter criteria.
    /// </summary>
    private string DetermineVenueName(PlacementFilter filter, SceneSpawnContext context)
    {
        if (!string.IsNullOrEmpty(filter.DistrictId))
        {
            return $"Generated {filter.DistrictId} Venue";
        }

        return "Generated Venue";
    }

    /// <summary>
    /// Determine venue type based on location properties in filter.
    /// Maps location properties to appropriate venue types.
    /// </summary>
    private VenueType DetermineVenueType(PlacementFilter filter)
    {
        if (filter.LocationProperties == null || !filter.LocationProperties.Any())
        {
            return VenueType.Wilderness;
        }

        // Map properties to venue types
        if (filter.LocationProperties.Contains(LocationPropertyType.Indoor))
        {
            if (filter.LocationProperties.Contains(LocationPropertyType.Mercantile))
                return VenueType.Market;
            if (filter.LocationProperties.Contains(LocationPropertyType.Social))
                return VenueType.Tavern;
            if (filter.LocationProperties.Contains(LocationPropertyType.Private))
                return VenueType.Inn;

            return VenueType.Building;
        }

        return VenueType.Wilderness;
    }
}
