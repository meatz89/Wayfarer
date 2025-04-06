public class LocationCreationSystem
{
    private readonly NarrativeService narrativeService;
    private readonly LocationSystem locationSystem;
    private readonly CharacterSystem characterSystem;
    private readonly OpportunitySystem opportunitySystem;
    private readonly ActionSystem actionSystem;
    private readonly GameState gameState;
    private readonly ActionGenerator actionGenerator;

    public LocationCreationSystem(
        NarrativeService narrativeService,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        ActionSystem actionSystem,
        GameState gameState,
        ActionGenerator actionGenerator)
    {
        this.narrativeService = narrativeService;
        this.locationSystem = locationSystem;
        this.characterSystem = characterSystem;
        this.opportunitySystem = opportunitySystem;
        this.actionSystem = actionSystem;
        this.gameState = gameState;
        this.actionGenerator = actionGenerator;
        this.actionGenerator = actionGenerator;
        this.narrativeService = narrativeService;
    }

    public async Task<Location> CreateLocation(
        bool wasTravelEncounter,
        string travelOrigin,
        string travelDestination,
        int locationDepth
        )
    {
        LocationCreationInput input = CreateLocationInput(
            wasTravelEncounter, travelOrigin, travelDestination, locationDepth);

        // Get location details from AI
        LocationDetails details = await narrativeService.GenerateLocationDetailsAsync(input);

        // Convert SpotDetails to LocationSpot objects
        return IntegrateNewLocation(input, details);
    }

    private Location IntegrateNewLocation(LocationCreationInput input, LocationDetails details)
    {
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

            locationSpots.Add(spot);
        }

        // Create the location with converted spots
        Location location = new Location
        {
            Name = details.Name,
            Description = details.Description,
            DetailedDescription = details.DetailedDescription,
            History = details.History,
            PointsOfInterest = details.PointsOfInterest,
            TravelTimeMinutes = details.TravelTimeMinutes,
            TravelDescription = details.TravelDescription,
            ConnectedTo = details.ConnectedLocationIds,
            EnvironmentalProperties = details.EnvironmentalProperties,
            LocationSpots = locationSpots,
            StrategicTags = details.StrategicTags,
            NarrativeTags = details.NarrativeTags
        };

        return location;
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
            CharacterBackground = playerState.Archetype.ToString(),
            CurrentLocation = worldState.CurrentLocation?.Name ?? "Unknown",

            KnownLocations = locationSystem.FormatKnownLocations(allLocations),
            KnownCharacters = characterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = opportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            CurrentLocationSpots = locationSystem.FormatLocationSpots(worldState.CurrentLocation),
            AllKnownLocationSpots = locationSystem.FormatAllLocationSpots(allLocations),
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
