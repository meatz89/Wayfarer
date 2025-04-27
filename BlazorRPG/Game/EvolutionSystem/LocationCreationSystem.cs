public class LocationCreationSystem
{
    private readonly GameContentRegistry contentRegistry;
    private readonly NarrativeService narrativeService;
    private readonly WorldState worldState;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly GameState gameState;
    private readonly ActionGenerator actionGenerator;
    private readonly ActionRepository actionRepository;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly GameContentRegistry registry;

    public LocationCreationSystem(
        GameContentRegistry contentRegistry,
        NarrativeService narrativeService,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository,
        WorldStateInputBuilder worldStateInputCreator,
        GameContentRegistry registry
        )
    {
        this.contentRegistry = contentRegistry;
        this.narrativeService = narrativeService;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.gameState = gameState;
        this.actionGenerator = actionGenerator;
        this.actionRepository = actionRepository;
        this.worldStateInputCreator = worldStateInputCreator;
        this.registry = registry;
        this.narrativeService = narrativeService;
        this.worldState = gameState.WorldState;
    }

    public async Task<Location> PopulateLocation(
        string locationToPopulate,
        string travelOrigin,
        int locationDepth)
    {
        LocationCreationInput input = CreateLocationInput(travelOrigin, locationToPopulate, locationDepth);
        WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(locationToPopulate);

        // Get location details from AI
        LocationDetails details = await narrativeService.GenerateLocationDetailsAsync(input, worldStateInput);

        // Convert SpotDetails to LocationSpot objects
        return await IntegrateNewLocation(details);
    }

    // Update registry usage in IntegrateNewLocation method
    public async Task<Location> IntegrateNewLocation(LocationDetails details)
    {
        string locId = details.LocationUpdate.NewLocationName;
        if (!registry.TryGetLocation(locId, out Location location))
        {
            location = new Location(locId);
            registry.RegisterLocation(locId, location);
        }
        location.Description = details.Description;
        location.DetailedDescription = details.DetailedDescription;
        location.ConnectedTo = details.ConnectedLocationIds;

        foreach (SpotDetails spotDetail in details.NewLocationSpots)
        {
            string spotId = $"{locId}:{spotDetail.Name}";
            LocationSpot spot = new LocationSpot(spotDetail.Name, locId)
            {
                Name = spotDetail.Name,
                LocationName = locId,
                Description = spotDetail.Description,
                PlayerKnowledge = true
            };

            foreach (string? actionId in spotDetail.ActionIds.ToList())
            {
                spot.RegisterActionDefinition(actionId);
            }

            contentRegistry.RegisterLocationSpot(spotId, spot);
        }

        foreach (NewAction newAction in details.NewActions)
        {
            string actionId = await actionGenerator.GenerateActionAndEncounter(
                newAction.Name,
                newAction.SpotName,
                locId,
                newAction.Goal,
                newAction.Complication,
                newAction.ActionType);

            ActionDefinition actionDef = actionRepository.GetAction(actionId);
            string spotId = $"{locId}:{newAction.SpotName}";

            if (contentRegistry.TryGetLocationSpot(spotId, out LocationSpot? spot))
                spot.RegisterActionDefinition(actionDef.Name);
        }

        return location;
    }

    private async Task ProcessNewActions(LocationDetails details, WorldState worldState)
    {
        foreach (NewAction newAction in details.NewActions)
        {
            Location targetLocation = worldState.GetLocation(newAction.LocationName);
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
                    string actionId = await actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString());

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

        // Get current depth and hub depth
        int currentDepth = worldState.GetLocationDepth(worldState.CurrentLocation?.Name ?? "");

        // Get all locations
        List<Location> allLocations = worldState.GetLocations();

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
            LastHubDepth = worldState.LastHubDepth,
        };
        return context;
    }
}
