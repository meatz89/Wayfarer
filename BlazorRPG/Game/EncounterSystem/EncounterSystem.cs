public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly ILogger<EncounterSystem> logger;
    private readonly NarrativeService narrativeService;
    private AIProviderType currentAIProvider;

    private EncounterManager Encounter;
    public EncounterResult encounterResult;

    private bool useAiNarrative = false;
    private CardSelectionAlgorithm cardSelector;
    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.configuration = configuration;
        this.logger = logger;

        // Create the switchable narrative service
        this.narrativeService = new NarrativeService(configuration, logger);

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

        useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
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

    public async Task<EncounterResult> GenerateEncounter(
        Location location,
        string locationSpot,
        EncounterContext context,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        Location loc = context.Location;
        EncounterTypes encounterType = GetPresentationStyleFromBaseAction(actionImplementation);

        // Create encounter from location and action
        LocationNames locationName = location.LocationName;

        EncounterTemplate template = actionImplementation.EncounterTemplate;

        EncounterInfo encounter = EncounterInfoFactory.CreateEncounter(
            locationName, locationSpot, encounterType, template);

        // Create encounter manager
        encounterResult = await StartEncounterAt(location, encounter, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        return encounterResult;
    }

    private static EncounterTypes GetPresentationStyleFromBaseAction(ActionImplementation actionImplementation)
    {
        return actionImplementation.ActionType switch
        {
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
    }

    public async Task<EncounterResult> StartEncounterAt(
        Location location,
        EncounterInfo encounterInfo,
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
            useAiNarrative,
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
            playerState,
            actionImplementation,
            currentAIProvider);  // Pass the current provider type

        return new EncounterResult()
        {
            Encounter = encounterManager,
            EncounterResults = EncounterResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult
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

        if (!currentResult.IsEncounterOver)
        {
            currentResult = await Encounter.ApplyChoiceWithNarrativeAsync(
                choice,
                selectedDescription);

            if (currentResult.IsEncounterOver)
            {
                if (currentResult.Outcome == EncounterOutcomes.Failure)
                {
                    return new EncounterResult()
                    {
                        Encounter = encounter,
                        EncounterResults = EncounterResults.EncounterFailure,
                        EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                        NarrativeResult = currentResult
                    };
                }

                return new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult
                };
            }
        }

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentResult
        };
    }


    private static SpecialChoice GetSpecialChoiceFor(EncounterInfo location)
    {
        // Add special choices for this location
        return null;
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

}