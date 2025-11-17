/// <summary>
/// Manages location-specific operations within locations.
/// Handles location discovery, properties, and area navigation.
/// </summary>
public class LocationManager
{
    private readonly GameWorld _gameWorld;

    public LocationManager(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Get the player's current Location (spatial position).
    /// Venue is derived: GetCurrentLocation().Venue
    /// </summary>
    public Location GetCurrentLocation()
    {
        return _gameWorld.GetPlayerCurrentLocation();
    }

    /// <summary>
    /// Get a Venue by its ID.
    /// </summary>
    public Venue GetVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId))
            throw new ArgumentException("Venue ID cannot be null or empty", nameof(venueId));

        Venue venue = _gameWorld.Venues.FirstOrDefault(l =>
            l.Name.Equals(venueId, StringComparison.OrdinalIgnoreCase));

        if (venue == null)
            throw new InvalidOperationException($"Venue not found: {venueId}");

        return venue;
    }

    /// <summary>
    /// Get all venues in the world.
    /// </summary>
    public List<Venue> GetAllVenues()
    {
        return _gameWorld.Venues;
    }

    /// <summary>
    /// Get all locations for a specific venue.
    /// </summary>
    public List<Location> GetLocationsForVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId))
            throw new ArgumentException("Venue ID cannot be null or empty", nameof(venueId));

        // ADR-007: Use Venue.Name instead of deleted VenueId
        return _gameWorld.Locations
            .Where(s => s.Venue.Name.Equals(venueId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get a specific location by its ID.
    /// </summary>
    public Location GetLocation(string LocationId)
    {
        // Get location directly from GameWorld's primary storage
        return _gameWorld.GetLocation(LocationId);
    }

    /// <summary>
    /// Add a new venue to the world.
    /// </summary>
    public void AddVenue(Venue venue)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        if (_gameWorld.Venues.Contains(venue))
        {
            throw new InvalidOperationException($"Venue '{venue.Name}' already exists.");
        }

        _gameWorld.Venues.Add(venue);
    }

    /// <summary>
    /// Add a new location to the world.
    /// </summary>
    public void AddLocation(Location location)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        // Location IDs are globally unique - no need to check VenueId
        if (_gameWorld.Locations.Contains(location))
        {
            throw new InvalidOperationException($"Location '{location.Name}' already exists.");
        }

        _gameWorld.Locations.Add(location);
    }

    /// <summary>
    /// Set the player's current venue and location.
    /// </summary>
    public void SetCurrentLocation(Venue venue, Location location)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        // Validate that the location belongs to the venue
        // ADR-007: Use Venue object reference instead of deleted VenueId
        if (location != null && location.Venue != venue)
        {
            throw new InvalidOperationException($"location {location.Name} does not belong to venue {venue.Name}");
        }

        if (!location.HexPosition.HasValue)
            throw new InvalidOperationException($"Location '{location.Name}' has no HexPosition - cannot set player position");

        _gameWorld.GetPlayer().CurrentPosition = location.HexPosition.Value;
    }

    /// <summary>
    /// Set the player's current location directly.
    /// </summary>
    public void SetCurrentSpot(Location location)
    {
        if (!location.HexPosition.HasValue)
            throw new InvalidOperationException($"Location '{location.Name}' has no HexPosition - cannot set player position");

        _gameWorld.GetPlayer().CurrentPosition = location.HexPosition.Value;
    }

    /// <summary>
    /// Check if a Venue exists.
    /// </summary>
    public bool VenueExists(string venueId)
    {
        return GetVenue(venueId) != null;
    }

    /// <summary>
    /// Check if a Location exists by its ID.
    /// </summary>
    public bool LocationExists(string locationId)
    {
        return GetLocation(locationId) != null;
    }

    /// <summary>
    /// Get all locations in the world (from GameWorld).
    /// </summary>
    public List<Location> GetAllLocations()
    {
        return _gameWorld.Locations;
    }

    /// <summary>
    /// Check if player knows a specific Venue location.
    /// </summary>
    public bool IsLocationKnown(string LocationId)
    {
        if (string.IsNullOrEmpty(LocationId)) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability.Contains(LocationId);
    }

    /// <summary>
    /// Get the travel hub location for a location.
    /// </summary>
    public Location GetTravelHubLocation(string venueId)
    {
        Venue venue = GetVenue(venueId);

        // Look for Locations with Crossroads property
        List<Location> Locations = GetLocationsForVenue(venueId);
        return Locations.FirstOrDefault(s => s.LocationProperties.Contains(LocationPropertyType.Crossroads));
    }

    /// <summary>
    /// Check if a location is the travel hub for its location.
    /// </summary>
    public bool IsTravelHub(Location location)
    {
        if (location == null) return false;

        // ADR-007: Venue variable deleted (was unused, accessed deleted VenueId)
        // Travel happens at any location with Crossroads property
        return location.LocationProperties.Contains(LocationPropertyType.Crossroads);
    }

    /// <summary>
    /// Build the Venue path hierarchy for display.
    /// </summary>
    public List<string> BuildLocationPath(Venue venue, Location location)
    {
        List<string> path = new List<string>();

        if (venue != null)
        {
            // Build path from Venue hierarchy
            // For now, use Venue name as the primary path element
            path.Add(venue.Name);
        }

        if (location != null && location.Name != venue.Name)
        {
            path.Add(location.Name);
        }

        return path;
    }

    /// <summary>
    /// Get Venue traits for display based on current time.
    /// Location is the gameplay entity with all properties.
    /// </summary>
    public List<string> GetLocationTraits(Venue venue, Location location, TimeBlocks currentTime)
    {
        if (location == null) return new List<string>();

        // Use LocationTraitsParser for systematic trait generation from Location
        return LocationTraitsParser.ParseLocationTraits(location, currentTime);
    }

    /// <summary>
    /// Get all areas within a Venue for navigation.
    /// </summary>
    public List<AreaWithinLocationViewModel> GetAreasWithinVenue(
        Venue location,
        Location currentSpot,
        TimeBlocks currentTime,
        NPCRepository npcRepository)
    {
        List<AreaWithinLocationViewModel> areas = new List<AreaWithinLocationViewModel>();

        if (location == null) return areas;

        // Get all Locations in the same location
        List<Location> Locations = GetLocationsForVenue(location.Name);

        foreach (Location loc in Locations)
        {
            // Skip the current location - don't show it in the list
            if (currentSpot != null && loc == currentSpot)
                continue;

            // Get NPCs at this location
            List<NPC> npcsAtSpot = npcRepository.GetNPCsForLocationAndTime(loc, currentTime);
            List<string> npcNames = npcsAtSpot.Select(n => n.Name).ToList();

            // Build detail string with NPCs if present
            string detail = GetLocationDetail(loc);
            if (npcNames.Any())
            {
                detail = $"{detail} • {string.Join(", ", npcNames)}";
            }

            areas.Add(new AreaWithinLocationViewModel
            {
                Name = loc.Name,
                Detail = detail,
                LocationId = loc.Name,
                IsCurrent = false, // Never current since we skip the current location
                IsTravelHub = IsLocationTravelHub(loc)
            });
        }

        return areas;
    }

    /// <summary>
    /// Get a brief detail description for a location.
    /// </summary>
    public string GetLocationDetail(Location location)
    {
        if (location == null) return "";

        // Generate detail based on location ID (using categorical approach)
        return location.Name switch
        {
            "marcus_stall" => "Cloth merchant's stall",
            "central_fountain" => "Gathering place",
            "north_entrance" => "To Noble District",
            "main_hall" => "Common room",
            "bar_counter" => "Bertram's domain",
            "corner_table" => "Private conversations",
            _ => GenerateLocationDetail(location)
        };
    }

    /// <summary>
    /// Generate a detail description for a location based on its properties.
    /// </summary>
    private string GenerateLocationDetail(Location location)
    {
        if (location == null) return "";

        LocationDescriptionGenerator descGenerator = new LocationDescriptionGenerator();
        TimeBlocks currentTime = TimeBlocks.Morning; // Default for brief descriptions
        List<LocationPropertyType> activeProperties = location.GetActiveProperties(currentTime);
        return descGenerator.GenerateBriefDescription(activeProperties);
    }

    /// <summary>
    /// Check if a location is a travel hub.
    /// </summary>
    public bool IsLocationTravelHub(Location location)
    {
        // Travel happens at any location with Crossroads property
        return location.LocationProperties.Contains(LocationPropertyType.Crossroads);
    }

    /// <summary>
    /// Get active properties for a location at a specific time.
    /// </summary>
    public List<LocationPropertyType> GetActiveLocationProperties(Location location, TimeBlocks timeBlock)
    {
        if (location == null) return new List<LocationPropertyType>();
        return location.GetActiveProperties(timeBlock);
    }

    /// <summary>
    /// Check if a location has a specific property at a given time.
    /// </summary>
    public bool LocationHasProperty(Location location, LocationPropertyType property, TimeBlocks timeBlock)
    {
        List<LocationPropertyType> activeProperties = GetActiveLocationProperties(location, timeBlock);
        return activeProperties.Contains(property);
    }

    /// <summary>
    /// Get all Locations with a specific property in a location.
    /// </summary>
    public List<Location> GetLocationsWithProperty(string venueId, LocationPropertyType property, TimeBlocks timeBlock)
    {
        List<Location> Locations = GetLocationsForVenue(venueId);
        return Locations.Where(s => LocationHasProperty(s, property, timeBlock)).ToList();
    }

    /// <summary>
    /// Check if player has discovered a location.
    /// </summary>
    public bool IsLocationDiscovered(string LocationId)
    {
        if (string.IsNullOrEmpty(LocationId)) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability.Contains(LocationId);
    }

    /// <summary>
    /// Mark a location as discovered by the player.
    /// </summary>
    public void MarkLocationDiscovered(string LocationId)
    {
        if (string.IsNullOrEmpty(LocationId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.LocationActionAvailability.Contains(LocationId))
        {
            player.LocationActionAvailability.Add(LocationId);
        }
    }

    /// <summary>
    /// Get the default entrance location for a location.
    /// </summary>
    public Location GetDefaultEntranceLocation(string venueId)
    {
        Venue Venue = GetVenue(venueId);

        // Look for Locations with Crossroads property
        List<Location> Locations = GetLocationsForVenue(venueId);
        Location crossroads = Locations.FirstOrDefault(s => s.LocationProperties.Contains(LocationPropertyType.Crossroads));
        if (crossroads != null) return crossroads;

        // Finally, just return the first available location
        return Locations.FirstOrDefault();
    }

    /// <summary>
    /// Get accessible Locations from the current location.
    /// </summary>
    public List<Location> GetAccessibleLocationsFromCurrent(Location currentSpot)
    {
        if (currentSpot == null) return new List<Location>();

        // For now, all Locations in the same Venue are accessible
        // This could be enhanced with specific connectivity rules
        // ADR-007: Use Venue.Name instead of deleted VenueId
        return GetLocationsForVenue(currentSpot.Venue.Name)
            .Where(s => s != currentSpot)
            .ToList();
    }

}
