using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages location-specific operations within locations.
/// Handles location discovery, properties, and area navigation.
/// </summary>
public class LocationManager
{
    private readonly GameWorld _gameWorld;
    private readonly LocationManager _locationManager;

    public LocationManager(GameWorld gameWorld, LocationManager locationManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _locationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
    }

    /// <summary>
    /// Get the player's current Venue based on their current location.
    /// </summary>
    public Venue GetCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocation == null) return null;
        return GetVenue(player.CurrentLocation.VenueId);
    }

    /// <summary>
    /// Get the player's current Venue location.
    /// </summary>
    public Location GetCurrentLocationSpot()
    {
        return _gameWorld.GetPlayer().CurrentLocation;
    }

    /// <summary>
    /// Get a Venue by its ID.
    /// </summary>
    public Venue GetVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return null;

        Venue venue = _gameWorld.WorldState.venues.FirstOrDefault(l =>
            l.Id.Equals(venueId, StringComparison.OrdinalIgnoreCase));

        return venue;
    }


    /// <summary>
    /// Get all venues in the world.
    /// </summary>
    public List<Venue> GetAllVenues()
    {
        return _gameWorld.WorldState.venues ?? new List<Venue>();
    }

    /// <summary>
    /// Get all locations for a specific venue.
    /// </summary>
    public List<Location> GetLocationsForVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<Location>();

        return _gameWorld.WorldState.locations
            .Where(s => s.VenueId.Equals(venueId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get a specific location within a location.
    /// </summary>
    public Location GetLocation(string venueId, string LocationId)
    {
        Venue venue = GetVenue(venueId);
        // Get location directly from GameWorld's primary storage
        return _gameWorld.GetLocation(LocationId);
    }


    /// <summary>
    /// Add a new venue to the world.
    /// </summary>
    public void AddLocation(Venue venue)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        if (_gameWorld.WorldState.venues.Any(l =>
            l.Id.Equals(venue.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Venue '{venue.Id}' already exists.");
        }

        _gameWorld.WorldState.venues.Add(venue);
    }

    /// <summary>
    /// Add a new Venue location to the world.
    /// </summary>
    public void AddLocationSpot(Location location)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        if (_gameWorld.WorldState.locations.Any(s =>
            s.VenueId.Equals(location.VenueId, StringComparison.OrdinalIgnoreCase) &&
            s.Id.Equals(location.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"location '{location.Id}' already exists in '{location.VenueId}'.");
        }

        _gameWorld.WorldState.locations.Add(location);
    }

    /// <summary>
    /// Set the player's current venue and location.
    /// </summary>
    public void SetCurrentLocation(Venue venue, Location location)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        // Validate that the location belongs to the venue
        if (location != null && !location.VenueId.Equals(venue.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"location {location.Id} does not belong to venue {venue.Id}");
        }

        _gameWorld.GetPlayer().CurrentLocation = location;
    }

    /// <summary>
    /// Set the player's current location directly.
    /// </summary>
    public void SetCurrentSpot(Location location)
    {
        _gameWorld.GetPlayer().CurrentLocation = location;
    }

    /// <summary>
    /// Record that the player has visited a location.
    /// </summary>
    public void RecordLocationVisit(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return;
        _gameWorld.WorldState.RecordLocationVisit(venueId);
    }

    /// <summary>
    /// Check if this is the player's first visit to a location.
    /// </summary>
    public bool IsFirstVisit(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return false;
        return _gameWorld.WorldState.IsFirstVisit(venueId);
    }

    /// <summary>
    /// Check if a Venue exists.
    /// </summary>
    public bool LocationExists(string venueId)
    {
        return GetVenue(venueId) != null;
    }

    /// <summary>
    /// Check if a location exists within a location.
    /// </summary>
    public bool LocationExists(string venueId, string LocationId)
    {
        return GetLocation(venueId, LocationId) != null;
    }

    /// <summary>
    /// Get all locations in the world (from WorldState).
    /// </summary>
    public List<Location> GetAllLocations()
    {
        return _gameWorld.WorldState.locations ?? new List<Location>();
    }

    /// <summary>
    /// Check if player knows a specific Venue location.
    /// </summary>
    public bool IsLocationKnown(string LocationId)
    {
        if (string.IsNullOrEmpty(LocationId)) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability?.Contains(LocationId) == true;
    }

    /// <summary>
    /// Get the travel hub location for a location.
    /// </summary>
    public Location GetTravelHubLocation(string venueId)
    {
        Venue venue = GetVenue(venueId);
        if (venue == null) return null;

        // Look for Locations with Crossroads property
        List<Location> Locations = GetLocationsForVenue(venueId);
        return Locations.FirstOrDefault(s => s.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true);
    }

    /// <summary>
    /// Check if a location is the travel hub for its location.
    /// </summary>
    public bool IsTravelHub(Location location)
    {
        if (location == null) return false;

        Venue venue = GetVenue(location.VenueId);
        if (venue == null) return false;

        // Travel happens at any location with Crossroads property
        return location.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true;
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

        if (location != null && location.Name != venue?.Name)
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
        List<Location> Locations = GetLocationsForVenue(location.Id);

        foreach (Location loc in Locations)
        {
            // Skip the current location - don't show it in the list
            if (loc.Id == currentSpot?.Id)
                continue;

            // Get NPCs at this location
            List<NPC> npcsAtSpot = npcRepository.GetNPCsForLocationAndTime(loc.Id, currentTime);
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
                LocationId = loc.Id,
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
        return location.Id switch
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
        return location.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true;
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
        return player.LocationActionAvailability?.Contains(LocationId) == true;
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
        Venue Venue = _locationManager.GetVenue(venueId);

        // Look for Locations with Crossroads property
        List<Location> Locations = GetLocationsForVenue(venueId);
        Location? crossroads = Locations.FirstOrDefault(s => s.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true);
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
        return GetLocationsForVenue(currentSpot.VenueId)
            .Where(s => s.Id != currentSpot.Id)
            .ToList();
    }

    /// <summary>
    /// Get Locations that provide a specific service.
    /// </summary>
    public List<Location> GetServiceLocations(string venueId, ServiceTypes service, TimeBlocks timeBlock)
    {
        List<Location> Locations = GetLocationsForVenue(venueId);
        List<Location> serviceSpots = new List<Location>();

        foreach (Location location in Locations)
        {
            // Check if location has properties that indicate this service
            List<LocationPropertyType> properties = GetActiveLocationProperties(location, timeBlock);

            bool hasService = service switch
            {
                ServiceTypes.Market => properties.Contains(LocationPropertyType.Commercial),
                _ => false
            };

            if (hasService)
            {
                serviceSpots.Add(location);
            }
        }

        return serviceSpots;
    }
}
