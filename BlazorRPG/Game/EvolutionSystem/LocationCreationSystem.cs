using Microsoft.Win32;

public class LocationCreationSystem
{
    private readonly ContentRegistry contentRegistry;
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

    public LocationCreationSystem(
        ContentRegistry contentRegistry,
        NarrativeService narrativeService,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository,
        WorldStateInputBuilder worldStateInputCreator
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

    public async Task<Location> IntegrateNewLocation(LocationDetails details)
    {
        string locId = details.LocationUpdate.NewLocationName;
        if (!contentRegistry.TryResolve<Location>(locId, out Location? location))
        {
            location = new Location { Name = locId };
            contentRegistry.Register<Location>(locId, location);
        }
        location.Description = details.Description;
        location.DetailedDescription = details.DetailedDescription;
        location.ConnectedTo = details.ConnectedLocationIds;

        foreach (SpotDetails spotDetail in details.NewLocationSpots)
        {
            string spotId = $"{locId}:{spotDetail.Name}";
            LocationSpot spot = new LocationSpot
            {
                Name = spotDetail.Name,
                LocationName = locId,
                Description = spotDetail.Description,
                BaseActionIds = spotDetail.ActionIds.ToList(),
                PlayerKnowledge = true
            };
            contentRegistry.Register<LocationSpot>(spotId, spot);
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
            if (contentRegistry.TryResolve<LocationSpot>(spotId, out LocationSpot? spot))
                spot.BaseActionIds.Add(actionDef.Id);
        }

        return location;
    }

    private async Task ProcessNewActions(LocationDetails details, WorldState worldState)
    {
        foreach (NewAction newAction in details.NewActions)
        {
            Location targetLocation = worldState.GetLocation(newAction.LocationName);
            if (targetLocation != null && targetLocation.LocationSpots != null)
            {
                LocationSpot spotForAction = targetLocation.LocationSpots.FirstOrDefault(s =>
                    s.Name.Equals(newAction.SpotName, StringComparison.OrdinalIgnoreCase));

                if (spotForAction != null)
                {
                    if (spotForAction.BaseActionIds == null)
                    {
                        spotForAction.BaseActionIds = new List<string>();
                    }

                    string newActionId = newAction.Name.Replace(" ", "");
                    string actionId = await actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString());

                    ActionDefinition actionTemplate = actionRepository.GetAction(actionId);
                    spotForAction.BaseActionIds.Add(actionTemplate.Id);

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
