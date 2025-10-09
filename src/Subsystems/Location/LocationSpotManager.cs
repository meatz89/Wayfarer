using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages spot-specific operations within locations.
/// Handles spot discovery, properties, and area navigation.
/// </summary>
public class LocationSpotManager
{
    private readonly GameWorld _gameWorld;
    private readonly LocationSpotManager _locationManager;

    public LocationSpotManager(GameWorld gameWorld, LocationSpotManager locationManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _locationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
    }

    /// <summary>
    /// Get the player's current Venue based on their current spot.
    /// </summary>
    public Venue GetCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return null;
        return GetVenue(player.CurrentLocationSpot.VenueId);
    }

    /// <summary>
    /// Get the player's current Venue spot.
    /// </summary>
    public LocationSpot GetCurrentLocationSpot()
    {
        return _gameWorld.GetPlayer().CurrentLocationSpot;
    }

    /// <summary>
    /// Get a Venue by its ID.
    /// </summary>
    public Venue GetVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return null;

        Venue venue = _gameWorld.WorldState.locations.FirstOrDefault(l =>
            l.Id.Equals(venueId, StringComparison.OrdinalIgnoreCase));

        return venue;
    }


    /// <summary>
    /// Get all locations in the world.
    /// </summary>
    public List<Venue> GetAllLocations()
    {
        return _gameWorld.WorldState.locations ?? new List<Venue>();
    }

    /// <summary>
    /// Get all spots for a specific location.
    /// </summary>
    public List<LocationSpot> GetSpotsForVenue(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<LocationSpot>();
        // Get from GameWorld's primary Spots dictionary
        return _gameWorld.Spots.GetAllSpots()
            .Where(s => s.VenueId.Equals(venueId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get a specific spot within a location.
    /// </summary>
    public LocationSpot GetSpot(string venueId, string spotId)
    {
        Venue venue = GetVenue(venueId);
        // Get spot directly from GameWorld's primary storage
        return _gameWorld.GetSpot(spotId);
    }


    /// <summary>
    /// Add a new venue to the world.
    /// </summary>
    public void AddLocation(Venue venue)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        if (_gameWorld.WorldState.locations.Any(l =>
            l.Id.Equals(venue.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Venue '{venue.Id}' already exists.");
        }

        _gameWorld.WorldState.locations.Add(venue);
    }

    /// <summary>
    /// Add a new Venue spot to the world.
    /// </summary>
    public void AddLocationSpot(LocationSpot spot)
    {
        if (spot == null) throw new ArgumentNullException(nameof(spot));

        if (_gameWorld.WorldState.locationSpots.Any(s =>
            s.VenueId.Equals(spot.VenueId, StringComparison.OrdinalIgnoreCase) &&
            s.Id.Equals(spot.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Spot '{spot.Id}' already exists in '{spot.VenueId}'.");
        }

        _gameWorld.WorldState.locationSpots.Add(spot);
    }

    /// <summary>
    /// Set the player's current venue and spot.
    /// </summary>
    public void SetCurrentLocation(Venue venue, LocationSpot spot)
    {
        if (venue == null) throw new ArgumentNullException(nameof(venue));

        // Validate that the spot belongs to the venue
        if (spot != null && !spot.VenueId.Equals(venue.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Spot {spot.Id} does not belong to venue {venue.Id}");
        }

        _gameWorld.GetPlayer().CurrentLocationSpot = spot;
    }

    /// <summary>
    /// Set the player's current spot directly.
    /// </summary>
    public void SetCurrentSpot(LocationSpot spot)
    {
        _gameWorld.GetPlayer().CurrentLocationSpot = spot;
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
    /// Check if a spot exists within a location.
    /// </summary>
    public bool SpotExists(string venueId, string spotId)
    {
        return GetSpot(venueId, spotId) != null;
    }

    /// <summary>
    /// Get all Venue spots in the world (from WorldState).
    /// </summary>
    public List<LocationSpot> GetAllLocationSpots()
    {
        return _gameWorld.WorldState.locationSpots ?? new List<LocationSpot>();
    }

    /// <summary>
    /// Get Venue spots filtered by Venue ID.
    /// </summary>
    public List<LocationSpot> GetLocationSpotsForLocation(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<LocationSpot>();

        return _gameWorld.WorldState.locationSpots
            .Where(s => s.VenueId.Equals(venueId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Check if player knows a specific Venue spot.
    /// </summary>
    public bool IsSpotKnown(string spotId)
    {
        if (string.IsNullOrEmpty(spotId)) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability?.Contains(spotId) == true;
    }

    /// <summary>
    /// Get the travel hub spot for a location.
    /// </summary>
    public LocationSpot GetTravelHubSpot(string venueId)
    {
        Venue venue = GetVenue(venueId);
        if (venue == null) return null;

        // Look for spots with Crossroads property
        List<LocationSpot> spots = GetSpotsForLocation(venueId);
        return spots.FirstOrDefault(s => s.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true);
    }

    /// <summary>
    /// Check if a spot is the travel hub for its location.
    /// </summary>
    public bool IsTravelHub(LocationSpot spot)
    {
        if (spot == null) return false;

        Venue venue = GetVenue(spot.VenueId);
        if (venue == null) return false;

        // Travel happens at any spot with Crossroads property
        return spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true;
    }

    /// <summary>
    /// Get all spots for a specific location.
    /// </summary>
    public List<LocationSpot> GetSpotsForLocation(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<LocationSpot>();

        // Get from GameWorld's primary Spots dictionary
        return _gameWorld.Spots.GetAllSpots()
            .Where(s => s.VenueId.Equals(venueId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Build the Venue path hierarchy for display.
    /// </summary>
    public List<string> BuildLocationPath(Venue location, LocationSpot spot)
    {
        List<string> path = new List<string>();

        if (location != null)
        {
            // Build path from Venue hierarchy
            // For now, use Venue name as the primary path element
            path.Add(location.Name);
        }

        if (spot != null && spot.Name != location?.Name)
        {
            path.Add(spot.Name);
        }

        return path;
    }

    /// <summary>
    /// Get Venue traits for display based on current time.
    /// LocationSpot is the gameplay entity with all properties.
    /// </summary>
    public List<string> GetLocationTraits(Venue location, LocationSpot spot, TimeBlocks currentTime)
    {
        if (spot == null) return new List<string>();

        // Use LocationTraitsParser for systematic trait generation from LocationSpot
        return LocationTraitsParser.ParseLocationTraits(spot, currentTime);
    }

    /// <summary>
    /// Get all areas within a Venue for navigation.
    /// </summary>
    public List<AreaWithinLocationViewModel> GetAreasWithinVenue(
        Venue location,
        LocationSpot currentSpot,
        TimeBlocks currentTime,
        NPCRepository npcRepository)
    {
        List<AreaWithinLocationViewModel> areas = new List<AreaWithinLocationViewModel>();

        if (location == null) return areas;

        // Get all spots in the same location
        List<LocationSpot> spots = GetSpotsForLocation(location.Id);

        foreach (LocationSpot spot in spots)
        {
            // Skip the current spot - don't show it in the list
            if (spot.Id == currentSpot?.Id)
                continue;

            // Get NPCs at this spot
            List<NPC> npcsAtSpot = npcRepository.GetNPCsForLocationSpotAndTime(spot.Id, currentTime);
            List<string> npcNames = npcsAtSpot.Select(n => n.Name).ToList();

            // Build detail string with NPCs if present
            string detail = GetSpotDetail(spot);
            if (npcNames.Any())
            {
                detail = $"{detail} • {string.Join(", ", npcNames)}";
            }

            areas.Add(new AreaWithinLocationViewModel
            {
                Name = spot.Name,
                Detail = detail,
                SpotId = spot.Id,
                IsCurrent = false, // Never current since we skip the current spot
                IsTravelHub = IsSpotTravelHub(spot)
            });
        }

        return areas;
    }

    /// <summary>
    /// Get a brief detail description for a spot.
    /// </summary>
    public string GetSpotDetail(LocationSpot spot)
    {
        if (spot == null) return "";

        // Generate detail based on spot ID (using categorical approach)
        return spot.Id switch
        {
            "marcus_stall" => "Cloth merchant's stall",
            "central_fountain" => "Gathering place",
            "north_entrance" => "To Noble District",
            "main_hall" => "Common room",
            "bar_counter" => "Bertram's domain",
            "corner_table" => "Private conversations",
            _ => GenerateSpotDetail(spot)
        };
    }

    /// <summary>
    /// Generate a detail description for a spot based on its properties.
    /// </summary>
    private string GenerateSpotDetail(LocationSpot spot)
    {
        if (spot == null) return "";

        SpotDescriptionGenerator descGenerator = new SpotDescriptionGenerator();
        TimeBlocks currentTime = TimeBlocks.Morning; // Default for brief descriptions
        List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);
        return descGenerator.GenerateBriefDescription(activeProperties);
    }

    /// <summary>
    /// Check if a spot is a travel hub.
    /// </summary>
    public bool IsSpotTravelHub(LocationSpot spot)
    {
        // Travel happens at any spot with Crossroads property
        return spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true;
    }

    /// <summary>
    /// Get active properties for a spot at a specific time.
    /// </summary>
    public List<SpotPropertyType> GetActiveSpotProperties(LocationSpot spot, TimeBlocks timeBlock)
    {
        if (spot == null) return new List<SpotPropertyType>();
        return spot.GetActiveProperties(timeBlock);
    }

    /// <summary>
    /// Check if a spot has a specific property at a given time.
    /// </summary>
    public bool SpotHasProperty(LocationSpot spot, SpotPropertyType property, TimeBlocks timeBlock)
    {
        List<SpotPropertyType> activeProperties = GetActiveSpotProperties(spot, timeBlock);
        return activeProperties.Contains(property);
    }

    /// <summary>
    /// Get all spots with a specific property in a location.
    /// </summary>
    public List<LocationSpot> GetSpotsWithProperty(string venueId, SpotPropertyType property, TimeBlocks timeBlock)
    {
        List<LocationSpot> spots = GetSpotsForLocation(venueId);
        return spots.Where(s => SpotHasProperty(s, property, timeBlock)).ToList();
    }

    /// <summary>
    /// Check if player has discovered a spot.
    /// </summary>
    public bool IsSpotDiscovered(string spotId)
    {
        if (string.IsNullOrEmpty(spotId)) return false;

        Player player = _gameWorld.GetPlayer();
        return player.LocationActionAvailability?.Contains(spotId) == true;
    }

    /// <summary>
    /// Mark a spot as discovered by the player.
    /// </summary>
    public void MarkSpotDiscovered(string spotId)
    {
        if (string.IsNullOrEmpty(spotId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.LocationActionAvailability.Contains(spotId))
        {
            player.LocationActionAvailability.Add(spotId);
        }
    }

    /// <summary>
    /// Get the default entrance spot for a location.
    /// </summary>
    public LocationSpot GetDefaultEntranceSpot(string venueId)
    {
        Venue Venue = _locationManager.GetVenue(venueId);

        // Look for spots with Crossroads property
        List<LocationSpot> spots = GetSpotsForLocation(venueId);
        LocationSpot? crossroads = spots.FirstOrDefault(s => s.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true);
        if (crossroads != null) return crossroads;

        // Finally, just return the first available spot
        return spots.FirstOrDefault();
    }

    /// <summary>
    /// Get accessible spots from the current spot.
    /// </summary>
    public List<LocationSpot> GetAccessibleSpotsFromCurrent(LocationSpot currentSpot)
    {
        if (currentSpot == null) return new List<LocationSpot>();

        // For now, all spots in the same Venue are accessible
        // This could be enhanced with specific connectivity rules
        return GetSpotsForLocation(currentSpot.VenueId)
            .Where(s => s.Id != currentSpot.Id)
            .ToList();
    }

    /// <summary>
    /// Get spots that provide a specific service.
    /// </summary>
    public List<LocationSpot> GetServiceSpots(string venueId, ServiceTypes service, TimeBlocks timeBlock)
    {
        List<LocationSpot> spots = GetSpotsForLocation(venueId);
        List<LocationSpot> serviceSpots = new List<LocationSpot>();

        foreach (LocationSpot spot in spots)
        {
            // Check if spot has properties that indicate this service
            List<SpotPropertyType> properties = GetActiveSpotProperties(spot, timeBlock);

            bool hasService = service switch
            {
                ServiceTypes.Market => properties.Contains(SpotPropertyType.Commercial),
                _ => false
            };

            if (hasService)
            {
                serviceSpots.Add(spot);
            }
        }

        return serviceSpots;
    }
}
