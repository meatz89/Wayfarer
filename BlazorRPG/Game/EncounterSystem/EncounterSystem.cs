public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly ILogger<EncounterSystem> logger;
    private AIProviderType currentAIProvider;

    private EncounterManager Encounter;
    public EncounterResult CurrentResult;

    private ResourceManager resourceManager;
    private readonly NarrativeService narrativeService;
    private CardSelectionAlgorithm cardSelector;

    public WorldState worldState;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        ResourceManager resourceManager,
        NarrativeContextManager narrativeContextManager,
        NarrativeService narrativeService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.configuration = configuration;
        this.logger = logger;

        // Create the switchable narrative service
        this.resourceManager = resourceManager;
        this.narrativeService = narrativeService;

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

        EncounterTemplate template = actionImplementation.EncounterTemplate;

        EncounterInfo encounter = EncounterFactory.CreateEncounterFromTemplate(template,
            location, locationSpot, encounterType);

        // Create encounter manager
        CurrentResult = await StartEncounterAt(location, encounter, this.worldState, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        return CurrentResult;
    }


    public List<EnvironmentPropertyTag> GetActiveStrategicTags(string locationId, EncounterContext encounterContext)
    {
        Location location = worldState.GetLocation(locationId);
        List<IEnvironmentalProperty> properties = GetCurrentEnvironmentalProperties(locationId, encounterContext.TimeOfDay);

        // Base tags from location
        List<EnvironmentPropertyTag> activeTags = new List<EnvironmentPropertyTag>(location.StrategicTags);

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
        ChoiceCardRepository choiceRepository = new ChoiceCardRepository();
        cardSelector = new CardSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the switchable service
        EncounterManager encounterManager = new EncounterManager(
            actionImplementation,
            cardSelector,
            narrativeService,
            resourceManager,
            configuration,
            logger);

        // Set the current AI provider
        encounterManager.SwitchAIProvider(currentAIProvider);

        this.Encounter = encounterManager;

        //SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(encounter);
        //choiceRepository.AddSpecialChoice(encounter.Name, negotiatePriceChoice);

        WorldStateInput worldStateInput = new WorldStateInput();

        // Start the encounter with narrative
        NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
            location,
            encounterInfo,
            worldState,
            playerState,
            actionImplementation,
            currentAIProvider,
            worldStateInput);

        CurrentResult = new EncounterResult()
        {
            Encounter = encounterManager,
            EncounterResults = EncounterResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = encounterManager.GetNarrativeContext()
        };
        return CurrentResult;
    }

    public async Task<EncounterResult> ExecuteChoice(
        EncounterManager encounter,
        NarrativeResult narrativeResult,
        ChoiceCard choice,
        WorldStateInput worldStateInput)
    {
        NarrativeResult currentResult = narrativeResult;

        Dictionary<ChoiceCard, ChoiceNarrative> choiceDescriptions = currentResult.ChoiceDescriptions;

        ChoiceNarrative? selectedDescription = null;
        if (currentResult.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
        {
            selectedDescription = currentResult.ChoiceDescriptions[choice];
        }

        if (currentResult.IsEncounterOver)
        {
            CurrentResult = new EncounterResult()
            {
                Encounter = encounter,
                EncounterResults = EncounterResults.EncounterSuccess,
                EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                NarrativeResult = currentResult,
                NarrativeContext = encounter.GetNarrativeContext()
            };
            return CurrentResult;
        }

        currentResult = await Encounter.ApplyChoiceWithNarrativeAsync(
            choice,
            encounter.playerState,
            encounter.worldState,
            selectedDescription,
            worldStateInput);

        if (currentResult.IsEncounterOver)
        {
            if (currentResult.Outcome == EncounterOutcomes.Failure)
            {
                CurrentResult = new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterFailure,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return CurrentResult;
            }
            else
            {
                CurrentResult = new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return CurrentResult;
            }
        }

        CurrentResult = new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentResult,
            NarrativeContext = encounter.GetNarrativeContext()
        };
        return CurrentResult;

    }

    public List<ChoiceCard> GetChoices()
    {
        return Encounter.GetCurrentChoices();
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions()
    {
        return gameState.Actions.UserEncounterChoiceOptions;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, ChoiceCard choice)
    {
        return Encounter.ProjectChoice(choice);
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

    private static EncounterTypes GetPresentationStyleFromBaseAction(ActionImplementation actionImplementation)
    {
        EncounterTypes encounterTypes = actionImplementation.BasicActionType switch
        {
            BasicActionTypes.Rest => EncounterTypes.Social,
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