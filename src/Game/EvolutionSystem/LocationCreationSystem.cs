public class LocationCreationSystem
{
    private LocationSystem locationSystem;
    private CharacterSystem characterSystem;
    private GameWorld gameWorld;
    private LocationRepository locationRepository;
    private WorldStateInputBuilder worldStateInputCreator;
    private AIGameMaster aiService;

    public LocationCreationSystem(
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        GameWorld gameWorld,
        LocationRepository locationRepository,
        WorldStateInputBuilder worldStateInputCreator,
        AIGameMaster aiService
        )
    {
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.gameWorld = gameWorld;
        this.locationRepository = locationRepository;
        this.worldStateInputCreator = worldStateInputCreator;
        this.aiService = aiService;
    }

    public async Task<Location> CreateLocation(string locationId)
    {
        Location location = locationRepository.GetCurrentLocation();
        string travelOrigin = location.Id;
        int locationDepth = location.Depth + 1;

        LocationCreationInput input = CreateLocationInput(travelOrigin, locationId, locationDepth);
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(locationId);

        // Get location details from AI
        LocationDetails details = null;// = await aiService.GenerateLocationDetails(input, worldStateInput);

        // Convert SpotDetails to LocationSpot objects
        return await IntegrateNewLocation(details);
    }

    // Update registry usage in IntegrateNewLocation method
    public async Task<Location> IntegrateNewLocation(LocationDetails details)
    {
        string locationId = details.LocationUpdate.NewLocationName.Replace(" ", "_").ToLowerInvariant();
        string locationName = details.LocationUpdate.NewLocationName;
        Location location = locationRepository.GetLocation(locationName);
        if (location == null)
        {
            location = new Location(locationId, locationName);
            locationRepository.AddLocation(location);
        }
        location.Description = details.Description;
        location.ConnectedLocationIds = details.ConnectedLocationIds;

        foreach (SpotDetails spotDetail in details.NewLocationSpots)
        {
            string spotId = $"{locationName}:{spotDetail.Name}";
            LocationSpot spot = new LocationSpot(spotDetail.Name, locationName)
            {
                SpotID = spotDetail.Name.Replace(" ", "_").ToLowerInvariant(),
                Name = spotDetail.Name,
                LocationId = locationName,
                Description = spotDetail.Description,
                PlayerKnowledge = true
            };

            locationRepository.AddLocationSpot(spot);
        }

        // Action system removed - location actions handled by LocationActionManager

        return location;
    }


    public async Task<LocationSpot> CreateLocationSpot(string locationId, string locationSpotId)
    {
        Location location = locationRepository.GetLocation(locationId);
        LocationSpot locationSpot = new LocationSpot(locationSpotId, locationId)
        {
            SpotID = locationSpotId,
            LocationId = locationId,
            Description = "A new location spot.",
            PlayerKnowledge = true
        };

        // Register the new location spot
        locationRepository.AddLocationSpot(locationSpot);

        // Register the new location spot in the location
        location.AvailableSpots.Add(locationSpot);

        return locationSpot;
    }

    private LocationCreationInput CreateLocationInput(
        string travelOrigin,
        string travelDestination,
        int locationDepth
        )
    {
        WorldState worldState = gameWorld.WorldState;
        Player player = gameWorld.GetPlayer();

        // Get all locations
        List<Location> allLocations = locationRepository.GetAllLocations();

        // Create context for location generation
        LocationCreationInput context = new LocationCreationInput
        {
            CharacterArchetype = player.Archetype.ToString(),
            LocationName = worldState.CurrentLocation?.Id ?? "Unknown",

            KnownLocations = this.locationSystem.FormatLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveContracts = "", // Contract system removed

            CurrentLocationSpots = this.locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            ConnectedLocations = this.locationSystem.FormatLocations(locationSystem.GetConnectedLocations(worldState.CurrentLocation.Id)),

            WasConversationContext = true,
            TravelOrigin = travelOrigin,
            TravelDestination = travelDestination,

            CurrentDepth = locationDepth,
            LastHubDepth = worldState.CurrentLocation.Depth,
        };
        return context;
    }
}
