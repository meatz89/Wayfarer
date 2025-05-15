public class LocationCreationSystem
{
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly GameState gameState;
    private readonly ActionGenerator actionGenerator;
    private readonly LocationRepository locationRepository;
    private readonly ActionRepository actionRepository;
    private readonly ActionSystem actionSystem;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly IAIService aiService;

    public LocationCreationSystem(
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        GameState gameState,
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
                Id = spotDetail.Name.Replace(" ", "_").ToLowerInvariant(),
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
            Id = locationSpotId,
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

    private async Task ProcessNewActions(LocationDetails details, WorldState worldState)
    {
        foreach (NewAction newAction in details.NewActions)
        {
            Location targetLocation = locationRepository.GetLocationById(newAction.LocationName);
            List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(targetLocation.Id);
            if (targetLocation != null && locationSpots != null)
            {
                LocationSpot spotForAction = locationSpots.FirstOrDefault((Func<LocationSpot, bool>)(s =>
                {
                    return (bool)s.Id.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase);
                }));

                if (spotForAction != null)
                {
                    string newActionId = newAction.Name.Replace(" ", "");
                    string actionId = await actionGenerator.GenerateAction(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName);

                    Console.WriteLine($"Created new action {newAction.Name} at {newAction.LocationName}/{newAction.SpotName}");
                }
                else
                {
                    Console.WriteLine($"WARNING: Could not find spot {newAction.SpotName} at location {newAction.LocationName} for action {newAction.Name}");
                }
            }
            else
            {
                Console.WriteLine($"WARNING: Could not find location {newAction.LocationName} for action {newAction.Name}");
            }
        }
    }


    private EncounterCategories ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<EncounterCategories>(actionTypeStr, true, out EncounterCategories actionType))
        {
            return actionType;
        }

        // Default fallback
        return EncounterCategories.Persuasion;
    }

    private LocationCreationInput CreateLocationInput(
        string travelOrigin,
        string travelDestination,
        int locationDepth
        )
    {
        WorldState worldState = gameState.WorldState;
        PlayerState playerState = gameState.PlayerState;

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
