public class LocationCreationSystem
{
    private readonly NarrativeService narrativeService;
    private readonly WorldState worldState;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly GameState gameState;
    private readonly ActionGenerator actionGenerator;
    private readonly ActionRepository actionRepository;

    public LocationCreationSystem(
        NarrativeService narrativeService,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState,
        ActionGenerator actionGenerator,
        ActionRepository actionRepository
        )
    {
        this.narrativeService = narrativeService;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.gameState = gameState;
        this.actionGenerator = actionGenerator;
        this.actionRepository = actionRepository;
        this.narrativeService = narrativeService;
        this.worldState = gameState.WorldState;
    }

    public async Task<Location> PopulateLocation(
        string locationToPopulate,
        bool wasTravelEncounter,
        string travelOrigin,
        int locationDepth
        )
    {
        LocationCreationInput input = CreateLocationInput(
            wasTravelEncounter, travelOrigin, locationToPopulate, locationDepth);

        // Get location details from AI
        LocationDetails details = await narrativeService.GenerateLocationDetailsAsync(input);

        // Convert SpotDetails to LocationSpot objects
        return await IntegrateNewLocation(input, details);
    }

    private async Task<Location> IntegrateNewLocation(LocationCreationInput input, LocationDetails details)
    {
        string locationName = details.LocationUpdate.NewLocationName;

        Location location = worldState.GetLocation(locationName);
        if (location == null)
        {
            location = new Location();
            worldState.AddLocation(location);
        }

        location.Name = !string.IsNullOrWhiteSpace(location.Name) ? location.Name : details.Name;
        location.Description = details.Description;
        location.DetailedDescription = details.DetailedDescription;
        location.History = details.History;
        location.PointsOfInterest = details.PointsOfInterest;
        location.ConnectedTo = details.ConnectedLocationIds;
        location.EnvironmentalProperties = details.EnvironmentalProperties;
        location.LocationSpots = new();
        location.StrategicTags = details.StrategicTags;
        location.NarrativeTags = details.NarrativeTags;

        // Verify location has spots collection (shouldn't be needed with proper prompting)
        if (location.LocationSpots == null)
        {
            location.LocationSpots = new List<LocationSpot>();
        }

        // Update hub tracking if applicable
        worldState.UpdateHubTracking(location);

        List<LocationSpot> locationSpots = new List<LocationSpot>();
        foreach (SpotDetails spotDetail in details.NewLocationSpots)
        {
            LocationSpot spot = new LocationSpot
            {
                Name = spotDetail.Name,
                Description = spotDetail.Description,
                LocationName = details.Name,
                InteractionType = spotDetail.InteractionType,
                InteractionDescription = spotDetail.InteractionDescription,
                Position = spotDetail.Position,
                ActionTemplates = new List<string>(spotDetail.ActionNames)
            };

            locationSystem.AddSpot(location.Name, spot);
            locationSpots.Add(spot);
        }

        // Process new actions and associate them with the appropriate spots
        await ProcessNewActions(details, worldState);

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
                    if (spotForAction.ActionTemplates == null)
                    {
                        spotForAction.ActionTemplates = new List<string>();
                    }

                    // Create action template linked to the encounter
                    string actionTemplateName = await actionGenerator.GenerateActionAndEncounter(
                        newAction.Name,
                        newAction.SpotName,
                        newAction.LocationName,
                        newAction.Goal,
                        newAction.Complication,
                        ParseActionType(newAction.ActionType).ToString());

                    SpotAction actionTemplate = actionRepository.GetAction(newAction.Name);
                    string encounterTemplateName = actionTemplate.EncounterTemplateName;

                    EncounterTemplate encounterTemplate = actionRepository.GetEncounterTemplate(encounterTemplateName);
                    spotForAction.ActionTemplates.Add(actionTemplate.Name);

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


    private BasicActionTypes ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<BasicActionTypes>(actionTypeStr, true, out BasicActionTypes actionType))
        {
            return actionType;
        }

        // Default fallback
        return BasicActionTypes.Discuss;
    }


    private LocationCreationInput CreateLocationInput(
        bool wasTravelEncounter, 
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
            ConnectedLocations = this.locationSystem.FormatLocations(locationSystem.GetConnectedLocations()),
            AllExistingActions = actionSystem.FormatExistingActions(allLocations),

            WasTravelEncounter = wasTravelEncounter,
            TravelOrigin = travelOrigin,
            TravelDestination = travelDestination,

            CurrentDepth = locationDepth,
            LastHubDepth = worldState.LastHubDepth,
        };
        return context;
    }
}
