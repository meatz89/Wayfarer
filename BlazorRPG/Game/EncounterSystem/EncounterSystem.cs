public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly ChoiceRepository choiceRepository;
    private readonly ILogger<EncounterSystem> logger;
    private AIProviderType currentAIProvider;

    private ResourceManager resourceManager;
    private readonly NarrativeService narrativeService;
    private CardSelectionAlgorithm cardSelector;

    public WorldState worldState;

    public EncounterFactory encounterFactory;
    private readonly WorldStateInputBuilder worldStateInputCreator;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        ResourceManager resourceManager,
        NarrativeContextManager narrativeContextManager,
        NarrativeService narrativeService,
        ChoiceRepository choiceRepository,
        EncounterFactory encounterFactory,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.configuration = configuration;
        this.choiceRepository = choiceRepository;
        this.encounterFactory = encounterFactory;
        this.worldStateInputCreator = worldStateInputCreator;
        this.logger = logger;

        this.resourceManager = resourceManager;
        this.narrativeService = narrativeService;

        SetAiProviderFromConfig(configuration);
    }

    private void SetAiProviderFromConfig(IConfiguration configuration)
    {
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";
        switch (defaultProvider.ToLower())
        {
            case "claude":
                currentAIProvider = AIProviderType.Claude;
                break;
            case "gemma":
                currentAIProvider = AIProviderType.Gemini;
                break;
            default:
                currentAIProvider = AIProviderType.OpenAI;
                break;
        }
    }

    public async Task<EncounterManager> GenerateEncounter(
        string id,
        Location location,
        LocationSpot locationSpot,
        EncounterContext context,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        this.worldState = worldState;

        Location loc = context.Location;
        EncounterTypes encounterType = actionImplementation.EncounterType;
        EncounterTemplate template = actionImplementation.EncounterTemplate;

        Encounter encounter = encounterFactory.CreateEncounterFromTemplate(
            template, location, locationSpot, encounterType);

        // Create encounter manager
        EncounterManager encounterManager = await StartEncounter(encounter, location, this.worldState, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";
        return encounterManager;
    }

    public async Task<EncounterManager> StartEncounter(
        Encounter encounter,
        Location location,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        // Create the core components
        cardSelector = new CardSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the switchable service
        EncounterManager encounterManager = new EncounterManager(
            encounter,
            actionImplementation,
            cardSelector,
            narrativeService,
            resourceManager,
            worldStateInputCreator,
            configuration,
            logger);

        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

        // Set the current AI provider
        encounterManager.SwitchAIProvider(currentAIProvider);

        // Start the encounter with narrative
        NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
            location,
            encounter,
            worldState,
            playerState,
            actionImplementation,
            currentAIProvider);

        encounterManager.EncounterResult = new EncounterResult()
        {
            ActionImplementation = actionImplementation,
            ActionResult = ActionResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = encounterManager.GetNarrativeContext()
        };

        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteChoice(
        NarrativeResult narrativeResult,
        CardDefinition choice)
    {
        NarrativeResult currentNarrative = narrativeResult;

        Dictionary<CardDefinition, ChoiceNarrative> choiceDescriptions = currentNarrative.ChoiceDescriptions;

        ChoiceNarrative? selectedDescription = null;
        if (currentNarrative.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
        {
            selectedDescription = currentNarrative.ChoiceDescriptions[choice];
        }

        EncounterManager encounterManager = GetCurrentEncounter();
        currentNarrative = await encounterManager.ApplyChoiceWithNarrativeAsync(
            encounterManager.Encounter.LocationName,
            choice,
            selectedDescription);

        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);
        return encounterManager.EncounterResult;
    }

    private EncounterResult CreateEncounterResult(EncounterManager encounter, NarrativeResult currentNarrative)
    {
        if (currentNarrative.IsEncounterOver)
        {
            if (currentNarrative.Outcome == EncounterOutcomes.Failure)
            {
                EncounterResult failureResult = new EncounterResult()
                {
                    ActionImplementation = encounter.ActionImplementation,
                    ActionResult = ActionResults.EncounterFailure,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    NarrativeResult = currentNarrative,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return failureResult;
            }
            else
            {
                EncounterResult successResult = new EncounterResult()
                {
                    ActionImplementation = encounter.ActionImplementation,
                    ActionResult = ActionResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    NarrativeResult = currentNarrative,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return successResult;
            }
        }

        EncounterResult ongoingResult = new EncounterResult()
        {
            ActionImplementation = encounter.ActionImplementation,
            ActionResult = ActionResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentNarrative,
            NarrativeContext = encounter.GetNarrativeContext()
        };
        return ongoingResult;
    }

    public List<CardDefinition> GetChoices()
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        List<CardDefinition> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    private bool IsGameOver(PlayerState player)
    {
        if (player.Health <= 0) return true;
        if (player.Focus <= 0) return true;
        if (player.Spirit <= 0) return true;

        return false;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, CardDefinition choice)
    {
        if (IsGameOver(gameState.PlayerState))
        {
            throw new Exception("Game Over");
        }

        EncounterManager encounterManager = GetCurrentEncounter();
        ChoiceProjection choiceProjection = encounterManager.ProjectChoice(choice);
        return choiceProjection;
    }

    public EncounterManager GetCurrentEncounter()
    {
        return gameState.ActionStateTracker.CurrentEncounter;
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions()
    {
        return gameState.ActionStateTracker.UserEncounterChoiceOptions;
    }

}