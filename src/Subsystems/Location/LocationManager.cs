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
    public List<Location> GetLocationsForVenue(Venue venue)
    {
        if (venue == null)
            throw new ArgumentNullException(nameof(venue));

        // HIGHLANDER: Object equality, no string comparisons
        return _gameWorld.Locations
            .Where(s => s.Venue == venue)
            .ToList();
    }

    // HIGHLANDER: GetLocation(string) DELETED - use _gameWorld.Locations LINQ queries

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
    /// Check if a Location exists by its Name.
    /// HIGHLANDER: Query GameWorld.Locations directly, GetLocation method deleted
    /// </summary>
    public bool LocationExists(string locationName)
    {
        return _gameWorld.Locations.Any(loc => loc.Name == locationName);
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
    public bool IsLocationKnown(Location location)
    {
        if (location == null) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability.Contains(location);
    }

    /// <summary>
    /// Get the travel hub location for a venue.
    /// HIGHLANDER: Accept Venue object
    /// </summary>
    public Location GetTravelHubLocation(Venue venue)
    {
        // Look for Locations with Connective or Hub role (travel hubs)
        List<Location> Locations = GetLocationsForVenue(venue);
        return Locations.FirstOrDefault(s => s.Role == LocationRole.Connective || s.Role == LocationRole.Hub);
    }

    /// <summary>
    /// Check if a location is the travel hub for its venue.
    /// </summary>
    public bool IsTravelHub(Location location)
    {
        if (location == null) return false;

        // Travel happens at locations with Connective or Hub role
        return location.Role == LocationRole.Connective || location.Role == LocationRole.Hub;
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

        // Get all Locations in this venue
        // HIGHLANDER: 'location' parameter is Venue object, pass directly (not location.Venue)
        List<Location> Locations = GetLocationsForVenue(location);

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
                Location = loc,  // HIGHLANDER: Object reference
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
    /// Uses orthogonal categorical dimensions (Role, Purpose, Privacy, Safety, Activity).
    /// </summary>
    private string GenerateLocationDetail(Location location)
    {
        if (location == null) return "";

        // Generate description from orthogonal categorical dimensions
        return $"{location.Purpose} • {location.Activity}";
    }

    /// <summary>
    /// Check if a location is a travel hub.
    /// Travel hubs use LocationRole.Connective or LocationRole.Hub.
    /// </summary>
    public bool IsLocationTravelHub(Location location)
    {
        // Travel happens at locations with Connective or Hub role
        return location.Role == LocationRole.Connective || location.Role == LocationRole.Hub;
    }

    /// <summary>
    /// Check if a location has a specific role.
    /// Replaces deprecated capability-based checks.
    /// </summary>
    public bool LocationHasRole(Location location, LocationRole role)
    {
        if (location == null) return false;
        return location.Role == role;
    }

    /// <summary>
    /// Get all Locations with a specific role in a venue.
    /// </summary>
    public List<Location> GetLocationsWithRole(Venue venue, LocationRole role)
    {
        List<Location> Locations = GetLocationsForVenue(venue);
        return Locations.Where(s => s.Role == role).ToList();
    }

    /// <summary>
    /// Get all Locations with a specific purpose in a venue.
    /// </summary>
    public List<Location> GetLocationsWithPurpose(Venue venue, LocationPurpose purpose)
    {
        List<Location> Locations = GetLocationsForVenue(venue);
        return Locations.Where(s => s.Purpose == purpose).ToList();
    }

    /// <summary>
    /// Check if player has discovered a location.
    /// </summary>
    public bool IsLocationDiscovered(Location location)
    {
        if (location == null) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability.Contains(location);
    }

    /// <summary>
    /// Mark a location as discovered by the player.
    /// </summary>
    public void MarkLocationDiscovered(Location location)
    {
        if (location == null) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.LocationActionAvailability.Contains(location))
        {
            player.LocationActionAvailability.Add(location);
        }
    }

    /// <summary>
    /// Get the default entrance location for a venue.
    /// HIGHLANDER: Accept Venue object
    /// </summary>
    public Location GetDefaultEntranceLocation(Venue venue)
    {
        // Look for Locations with Connective or Hub role (travel entry points)
        List<Location> Locations = GetLocationsForVenue(venue);
        Location travelHubLocation = Locations.FirstOrDefault(s => s.Role == LocationRole.Connective || s.Role == LocationRole.Hub);
        if (travelHubLocation != null) return travelHubLocation;

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
        // HIGHLANDER: Object reference only, no string identifiers
        return GetLocationsForVenue(currentSpot.Venue)
            .Where(s => s != currentSpot)
            .ToList();
    }

}
