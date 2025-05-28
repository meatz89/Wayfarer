public class LocationCreationSystem
{
    private LocationSystem locationSystem;
    private CharacterSystem characterSystem;
    private OpportunitySystem opportunitySystem;
    private GameWorld gameState;
    private ActionGenerator actionGenerator;
    private LocationRepository locationRepository;
    private ActionRepository actionRepository;
    private ActionSystem actionSystem;
    private WorldStateInputBuilder worldStateInputCreator;
    private IAIService aiService;

    public LocationCreationSystem(
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        GameWorld gameState,
        ActionGenerator actionGenerator,
        LocationRepository locationRepository,
        ActionRepository actionRepository,
        ActionSystem actionSystem,
        WorldStateInputBuilder worldStateInputCreator,
        IAIService aiService
        )
    {
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.gameState = gameState;
        this.actionGenerator = actionGenerator;
        this.locationRepository = locationRepository;
        this.actionRepository = actionRepository;
        this.actionSystem = actionSystem;
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
        LocationDetails details = await aiService.GenerateLocationDetailsAsync(input, worldStateInput);

        // Convert SpotDetails to LocationSpot objects
        return await IntegrateNewLocation(details);
    }

    // Update registry usage in IntegrateNewLocation method
    public async Task<Location> IntegrateNewLocation(LocationDetails details)
    {
        string locationId = details.LocationUpdate.NewLocationName.Replace(" ", "_").ToLowerInvariant();
        string locationName = details.LocationUpdate.NewLocationName;
        Location location = locationRepository.GetLocationById(locationName);
        if (location == null)
        {
            location = new Location(locationId, locationName);
            locationRepository.AddLocation(location);
        }
        location.Description = details.Description;
        location.ConnectedTo = details.ConnectedLocationIds;

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

        foreach (NewAction newAction in details.NewActions)
        {
            string actionId = await actionGenerator.GenerateAction(
                newAction.Name,
                newAction.SpotName,
                locationName);

            ActionDefinition actionDef = actionRepository.GetAction(actionId);
            string spotId = $"{locationName}:{newAction.SpotName}";
        }

        return location;
    }


    public async Task<LocationSpot> CreateLocationSpot(string locationId, string locationSpotId)
    {
        Location location = locationRepository.GetLocationById(locationId);
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
        location.LocationSpots.Add(locationSpot);

        return locationSpot;
    }

    private LocationCreationInput CreateLocationInput(
        string travelOrigin,
        string travelDestination,
        int locationDepth
        )
    {
        WorldState worldState = gameState.WorldState;
        Player playerState = gameState.Player;

        // Get all locations
        List<Location> allLocations = locationRepository.GetAllLocations();

        // Create context for location generation
        LocationCreationInput context = new LocationCreationInput
        {
            CharacterArchetype = playerState.Archetype.ToString(),
            LocationName = worldState.CurrentLocation?.Id ?? "Unknown",

            KnownLocations = this.locationSystem.FormatLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = opportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            CurrentLocationSpots = this.locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            ConnectedLocations = this.locationSystem.FormatLocations(locationSystem.GetConnectedLocations(worldState.CurrentLocation.Id)),
            AllExistingActions = actionSystem.FormatExistingActions(allLocations),

            WasTravelEncounter = true,
            TravelOrigin = travelOrigin,
            TravelDestination = travelDestination,

            CurrentDepth = locationDepth,
            LastHubDepth = worldState.CurrentLocation.Depth,
        };
        return context;
    }
}
