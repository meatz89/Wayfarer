public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly ILogger<EncounterSystem> logger;
    private readonly NarrativeService narrativeService;
    private AIProviderType currentAIProvider;

    private EncounterManager Encounter;
    public EncounterResult encounterResult;

    private ResourceManager resourceManager;
    private RelationshipManager relationshipManager;
    private WorldEvolutionService evolutionService;
    private CardSelectionAlgorithm cardSelector;

    public WorldState worldState;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        ResourceManager resourceManager,
        RelationshipManager relationshipManager,
        WorldEvolutionService worldEvolutionService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.configuration = configuration;
        this.logger = logger;

        // Create the switchable narrative service
        this.narrativeService = new NarrativeService(configuration, logger);
        this.resourceManager = resourceManager;
        this.relationshipManager = relationshipManager;
        this.evolutionService = worldEvolutionService;

        // Initialize with the default provider from config
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";

        // Set current provider based on configuration value
        switch (defaultProvider.ToLower())
        {
            case "claude":
                currentAIProvider = AIProviderType.Claude;
                break;
            case "gemma":
                currentAIProvider = AIProviderType.Gemma3;
                break;
            default:
                currentAIProvider = AIProviderType.OpenAI;
                break;
        }
    }

    public async Task<EncounterResult> GenerateEncounter(
        Location location,
        string locationSpot,
        EncounterContext context,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        this.worldState = worldState;

        Location loc = context.Location;
        EncounterTypes encounterType = GetPresentationStyleFromBaseAction(actionImplementation);

        // Create encounter from location and action
        string locationName = location.Name;

        EncounterTemplate template = actionImplementation.EncounterTemplate;

        EncounterInfo encounter = EncounterFactory.CreateEncounter(
            locationName, locationSpot, encounterType, template);

        // Create encounter manager
        encounterResult = await StartEncounterAt(location, encounter, this.worldState, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        return encounterResult;
    }


    public List<StrategicTag> GetActiveStrategicTags(string locationId, EncounterContext encounterContext)
    {
        Location location = worldState.GetLocation(locationId);
        List<IEnvironmentalProperty> properties = GetCurrentEnvironmentalProperties(locationId, encounterContext.TimeOfDay);

        // Base tags from location
        List<StrategicTag> activeTags = new List<StrategicTag>(location.StrategicTags);

        return activeTags;
    }

    public List<IEnvironmentalProperty> GetCurrentEnvironmentalProperties(string locationId, string timeOfDay)
    {
        Location location = worldState.GetLocation(locationId);

        // Fall back to general properties
        return location.EnvironmentalProperties;
    }

    public async Task<EncounterResult> StartEncounterAt(
        Location location,
        EncounterInfo encounterInfo,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        // Create the core components
        ChoiceRepository choiceRepository = new ChoiceRepository();
        cardSelector = new CardSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the switchable service
        EncounterManager encounterManager = new EncounterManager(
            actionImplementation,
            cardSelector,
            narrativeService,
            resourceManager,
            relationshipManager,
            configuration,
            logger);

        // Set the current AI provider
        encounterManager.SwitchAIProvider(currentAIProvider);

        this.Encounter = encounterManager;

        //SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(encounter);
        //choiceRepository.AddSpecialChoice(encounter.Name, negotiatePriceChoice);

        // Start the encounter with narrative
        NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
            location,
            encounterInfo,
            worldState,
            playerState,
            actionImplementation,
            currentAIProvider);  // Pass the current provider type


        return new EncounterResult()
        {
            Encounter = encounterManager,
            EncounterResults = EncounterResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = encounterManager.GetNarrativeContext()
        };
    }

    public async Task<EncounterResult> ExecuteChoice(
        EncounterManager encounter,
        NarrativeResult narrativeResult,
        IChoice choice)
    {
        NarrativeResult currentResult = narrativeResult;

        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = currentResult.ChoiceDescriptions;

        ChoiceNarrative selectedDescription = null;
        if (currentResult.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
        {
            selectedDescription = currentResult.ChoiceDescriptions[choice];
        }

        if (currentResult.IsEncounterOver)
        {
            return new EncounterResult()
            {
                Encounter = encounter,
                EncounterResults = EncounterResults.EncounterSuccess,
                EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                NarrativeResult = currentResult,
                NarrativeContext = encounter.GetNarrativeContext()
            };
        }

        currentResult = await Encounter.ApplyChoiceWithNarrativeAsync(choice, selectedDescription);

        if (currentResult.IsEncounterOver)
        {
            if (currentResult.Outcome == EncounterOutcomes.Failure)
            {
                return new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterFailure,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
            }
            else
            {
                return new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
            }
        }

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentResult,
            NarrativeContext = encounter.GetNarrativeContext()
        };
    }

    public List<IChoice> GetChoices()
    {
        return Encounter.GetCurrentChoices();
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions()
    {
        return gameState.Actions.UserEncounterChoiceOptions;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, IChoice choice)
    {
        return Encounter.ProjectChoice(choice);
    }


    // Update toggle method to cycle through all three providers
    public void ToggleAIProvider()
    {
        // Cycle through providers: OpenAI -> Gemma3 -> Claude -> OpenAI
        switch (currentAIProvider)
        {
            case AIProviderType.OpenAI:
                SwitchAIProvider(AIProviderType.Gemma3);
                break;
            case AIProviderType.Gemma3:
                SwitchAIProvider(AIProviderType.Claude);
                break;
            case AIProviderType.Claude:
                SwitchAIProvider(AIProviderType.OpenAI);
                break;
            default:
                SwitchAIProvider(AIProviderType.OpenAI);
                break;
        }
    }

    // New method to switch AI providers
    public void SwitchAIProvider(AIProviderType providerType)
    {
        currentAIProvider = providerType;
        narrativeService.SwitchProvider(providerType);

        // If we have an active encounter, update its provider too
        if (Encounter != null)
        {
            Encounter.SwitchAIProvider(providerType);
        }
    }

    // New method to get current AI provider name for UI
    public string GetCurrentAIProviderName()
    {
        return narrativeService.GetCurrentProviderName();
    }


    private static EncounterTypes GetPresentationStyleFromBaseAction(ActionImplementation actionImplementation)
    {
        EncounterTypes encounterTypes = actionImplementation.ActionType switch
        {
            BasicActionTypes.Travel => EncounterTypes.Physical,

            BasicActionTypes.Labor => EncounterTypes.Physical,
            BasicActionTypes.Gather => EncounterTypes.Physical,
            BasicActionTypes.Fight => EncounterTypes.Physical,

            BasicActionTypes.Study => EncounterTypes.Intellectual,
            BasicActionTypes.Investigate => EncounterTypes.Intellectual,
            BasicActionTypes.Analyze => EncounterTypes.Intellectual,

            BasicActionTypes.Discuss => EncounterTypes.Social,
            BasicActionTypes.Persuade => EncounterTypes.Social,
            BasicActionTypes.Perform => EncounterTypes.Social,
        };
        return encounterTypes;
    }
}