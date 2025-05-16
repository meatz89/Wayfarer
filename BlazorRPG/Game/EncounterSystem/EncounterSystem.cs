public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly NarrativeChoiceRepository choiceRepository;
    private readonly ILogger<EncounterSystem> logger;

    private readonly IAIService _aiService;
    private ChoiceSelectionAlgorithm cardSelector;

    public WorldState worldState;
    public EncounterFactory encounterFactory;
    private readonly WorldStateInputBuilder worldStateInputCreator;

    private readonly PreGenerationManager _preGenerationManager;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        NarrativeContextManager narrativeContextManager,
        IAIService aiService,
        NarrativeChoiceRepository choiceRepository,
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
        this._aiService = aiService;

        _preGenerationManager = new PreGenerationManager();
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
        EncounterCategories encounterType = actionImplementation.EncounterType;
        EncounterTemplate template = actionImplementation.EncounterTemplate;

        if (template == null)
        {
            template = encounterFactory.GetDefaultEncounterTemplate();
        }

        Encounter encounter = encounterFactory.CreateEncounterFromTemplate(
            template, location, locationSpot, encounterType);

        EncounterManager encounterManager = await StartEncounter(encounter, location, this.worldState, playerState, actionImplementation);

        string situation = $"{actionImplementation.Id} ({actionImplementation.ActionType} Action)";
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
        cardSelector = new ChoiceSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the direct service
        EncounterManager encounterManager = new EncounterManager(
            encounter,
            actionImplementation,
            cardSelector,
            _aiService,
            worldStateInputCreator,
            configuration,
            logger);

        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

        // Start the encounter with narrative - generate synchronously
        NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
            location,
            encounter,
            worldState,
            playerState,
            actionImplementation);

        // For the initial encounter setup, we don't want to use any pre-generated content
        // Make sure the narrative and choices are fully generated before continuing

        // Explicitly mark as not using pre-generated content
        encounterManager.IsInitialState = true;

        // Store the initial result in the encounter manager
        encounterManager.EncounterResult = new EncounterResult()
        {
            ActionImplementation = actionImplementation,
            ActionResult = ActionResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = encounterManager.GetNarrativeContext()
        };

        // IMPORTANT: For subsequent choices, start pre-generation AFTER we return
        // This keeps the initial encounter setup fully synchronous
        Task.Run(() => StartPreGenerationsAsync(encounterManager, initialResult));

        // Return the completely initialized encounter manager
        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteChoice(
        NarrativeResult narrativeResult,
        NarrativeChoice choice)
    {
        NarrativeResult currentNarrative = narrativeResult;
        NarrativeResult cachedResult = null;

        EncounterManager encounterManager = GetCurrentEncounter();

        // Check if this is the initial state of the encounter
        bool isInitialChoice = encounterManager.IsInitialState;

        // If this is the initial choice, always generate synchronously
        // Otherwise, try to use pregenerated results
        if (!isInitialChoice && _preGenerationManager.TryGetCachedResult(choice.Id, out cachedResult))
        {
            // Use the pre-generated result
            currentNarrative = cachedResult;
        }
        else
        {
            // Cancel any pending pre-generations and generate the response
            _preGenerationManager.CancelAllPendingGenerations();

            // Continue with existing code for generating the response in real-time
            Dictionary<NarrativeChoice, ChoiceNarrative> choiceDescriptions = currentNarrative.ChoiceDescriptions;
            ChoiceNarrative selectedDescription = null;

            if (currentNarrative.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
            {
                selectedDescription = currentNarrative.ChoiceDescriptions[choice];
            }

            // Generate with immediate priority since player is waiting
            currentNarrative = await encounterManager.ApplyChoiceWithNarrativeAsync(
                encounterManager.Encounter.LocationName,
                choice,
                selectedDescription,
                AIClient.PRIORITY_IMMEDIATE);
        }

        // After the first choice is made, set the flag to false
        encounterManager.IsInitialState = false;

        // Create and return encounter result
        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);

        // If encounter continues, start pre-generating for the next set of choices
        if (!currentNarrative.IsEncounterOver)
        {
            // Start pre-generations for the next set of choices
            // This is fire-and-forget to avoid blocking
            _ = Task.Run(() => StartPreGenerationsAsync(encounterManager, currentNarrative));
        }

        return encounterManager.EncounterResult;
    }

    public async Task StartPreGenerationsAsync(EncounterManager encounterManager, NarrativeResult currentNarrative)
    {
        // Clear any existing pre-generated content
        _preGenerationManager.Clear();

        // If no choices or encounter is over, don't pre-generate
        if (currentNarrative.IsEncounterOver || currentNarrative.Choices == null ||
            currentNarrative.Choices.Count == 0)
        {
            return;
        }

        List<NarrativeChoice> choices = currentNarrative.Choices;

        // Start pre-generating responses for each choice
        foreach (NarrativeChoice choice in choices)
        {
            ChoiceNarrative choiceNarrative = null;
            if (currentNarrative.ChoiceDescriptions != null &&
                currentNarrative.ChoiceDescriptions.ContainsKey(choice))
            {
                choiceNarrative = currentNarrative.ChoiceDescriptions[choice];
            }

            // Create a copy of variables needed in the task to avoid closure issues
            NarrativeChoice choiceCopy = choice;
            ChoiceNarrative narrativeCopy = choiceNarrative;
            CancellationToken token = _preGenerationManager.GetCancellationToken();

            // Start pre-generation as a background task with low priority
            Task<NarrativeResult> preGenTask = Task.Run(async () =>
            {
                try
                {
                    // Use the simulation method instead of the regular method
                    NarrativeResult result = await encounterManager.SimulateChoiceForPreGeneration(
                        encounterManager.Encounter.LocationName,
                        choiceCopy,
                        narrativeCopy,
                        AIClient.PRIORITY_BACKGROUND);

                    if (!token.IsCancellationRequested)
                    {
                        _preGenerationManager.StoreCompletedResult(choiceCopy.Id, result);
                    }

                    return result;
                }
                catch (Exception ex) when (token.IsCancellationRequested)
                {
                    // Task was cancelled, return null
                    return null;
                }
                catch (Exception ex)
                {
                    // Log the error but don't rethrow - pre-generation errors shouldn't block gameplay
                    logger?.LogError(ex, $"Error during pre-generation for choice {choiceCopy.Id}");
                    return null;
                }
            }, token);

            _preGenerationManager.StartPreGeneration(choice.Id, preGenTask);
        }
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

    public List<NarrativeChoice> GetChoices()
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        List<NarrativeChoice> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    private bool IsGameOver(PlayerState player)
    {
        if (player.Health <= 0) return true;
        if (player.Concentration <= 0) return true;

        return false;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, NarrativeChoice choice)
    {
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