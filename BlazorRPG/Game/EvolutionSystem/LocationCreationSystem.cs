public class LocationCreationSystem
{
    private readonly NarrativeService narrativeService;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly GameState gameState;
    private readonly ActionGenerator actionGenerator;
    private readonly LocationRepository locationRepository;
    private readonly ActionRepository actionRepository;
    private readonly WorldStateInputBuilder worldStateInputCreator;

    public LocationCreationSystem(
        NarrativeService narrativeService,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState,
        ActionGenerator actionGenerator,
        LocationRepository locationRepository,
        ActionRepository actionRepository,
        WorldStateInputBuilder worldStateInputCreator
        )
    {
        this.narrativeService = narrativeService;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.gameState = gameState;
        this.actionGenerator = actionGenerator;
        this.locationRepository = locationRepository;
        this.actionRepository = actionRepository;
        this.worldStateInputCreator = worldStateInputCreator;
        this.narrativeService = narrativeService;
    }

    public async Task<Location> CreateLocation(string locationId)
    {
        Location location = locationRepository.GetCurrentLocation();
        string travelOrigin = location.Name;
        int locationDepth = location.Depth + 1;

        LocationCreationInput input = CreateLocationInput(travelOrigin, locationId, locationDepth);
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(locationId);

        // Get location details from AI
        LocationDetails details = await narrativeService.GenerateLocationDetailsAsync(input, worldStateInput);

        // Convert SpotDetails to LocationSpot objects
        return await IntegrateNewLocation(details);
    }

    // Update registry usage in IntegrateNewLocation method
    public async Task<Location> IntegrateNewLocation(LocationDetails details)
    {
        string locationName = details.LocationUpdate.NewLocationName;
        Location location = locationRepository.GetLocation(locationName);
        if (location == null)
        {
            location = new Location(locationName);
            locationRepository.AddLocation(location);
        }
        location.Description = details.Description;
        location.DetailedDescription = details.DetailedDescription;
        location.ConnectedTo = details.ConnectedLocationIds;

        foreach (SpotDetails spotDetail in details.NewLocationSpots)
        {
            string spotId = $"{locationName}:{spotDetail.Name}";
            LocationSpot spot = new LocationSpot(spotDetail.Name, locationName)
            {
                Name = spotDetail.Name,
                LocationId = locationName,
                Description = spotDetail.Description,
                PlayerKnowledge = true
            };

            foreach (string? actionId in spotDetail.ActionIds.ToList())
            {
                spot.RegisterActionDefinition(actionId);
            }

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

            LocationSpot spot = locationRepository.GetSpot(locationName, newAction.SpotName);
            if (spot != null)
                spot.RegisterActionDefinition(actionDef.Name);
        }

        return location;
    }

    private async Task ProcessNewActions(LocationDetails details, WorldState worldState)
    {
        foreach (NewAction newAction in details.NewActions)
        {
            Location targetLocation = locationRepository.GetLocation(newAction.LocationName);
            List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(targetLocation.Name);
            if (targetLocation != null && locationSpots != null)
            {
                LocationSpot spotForAction = locationSpots.FirstOrDefault(s =>
                {
                    return s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase);
                });

                if (spotForAction != null)
                {
                    string newActionId = newAction.Name.Replace(" ", "");
                    string actionId = await actionGenerator.GenerateAction(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName);

                    ActionDefinition actionTemplate = actionRepository.GetAction(actionId);
                    spotForAction.RegisterActionDefinition(actionTemplate.Name);

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


    private EncounterTypes ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<EncounterTypes>(actionTypeStr, true, out EncounterTypes actionType))
        {
            return actionType;
        }

        // Default fallback
        return EncounterTypes.Exploration;
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
            LocationName = worldState.CurrentLocation?.Name ?? "Unknown",

            KnownLocations = this.locationSystem.FormatLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = opportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            CurrentLocationSpots = this.locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            ConnectedLocations = this.locationSystem.FormatLocations(locationSystem.GetConnectedLocations(worldState.CurrentLocation.Name)),
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
