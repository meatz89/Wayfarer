public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly MessageSystem messageSystem;
    private readonly IConfiguration configuration;
    private readonly ILogger<EncounterSystem> logger;

    private readonly IAIService _aiService;

    public WorldState worldState;
    public EncounterFactory encounterFactory;
    private readonly WorldStateInputBuilder worldStateInputCreator;
    private readonly ChoiceProjectionService choiceProjectionService;
    private readonly PreGenerationManager _preGenerationManager;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        NarrativeContextManager narrativeContextManager,
        IAIService aiService,
        EncounterFactory encounterFactory,
        WorldStateInputBuilder worldStateInputCreator,
        ChoiceProjectionService choiceProjectionService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.messageSystem = messageSystem;
        this.configuration = configuration;
        this.encounterFactory = encounterFactory;
        this.worldStateInputCreator = worldStateInputCreator;
        this.choiceProjectionService = choiceProjectionService;
        this.logger = logger;
        this._aiService = aiService;

        _preGenerationManager = new PreGenerationManager();
    }

    public async Task<EncounterManager> GenerateEncounter(
        string id,
        CommissionDefinition commission,
        ApproachDefinition approach,
        Location location,
        LocationSpot locationSpot,
        WorldState worldState,
        Player playerState,
        ActionImplementation actionImplementation)
    {
        logger.LogInformation(
            "GenerateEncounter called with id: {Id}, commission: {CommissionId}, approachId: {ApproachId}, location: {LocationId}, locationSpot: {LocationSpotId}", 
            id, commission?.Id, approach.Id, location?.Id, locationSpot?.Id);

        this.worldState = worldState;

        Encounter encounter = encounterFactory.CreateEncounterFromCommission(
            commission, approach, playerState, location);

        EncounterManager encounterManager = await StartEncounter(
            encounter,
            location,
            this.worldState,
            playerState,
            actionImplementation);

        string situation = $"{actionImplementation.Id} ({actionImplementation.ActionType} Action)";
        logger.LogInformation("Encounter generated for situation: {Situation}", situation);
        return encounterManager;
    }

    public async Task<EncounterManager> StartEncounter(
        Encounter encounter,
        Location location,
        WorldState worldState,
        Player playerState,
        ActionImplementation actionImplementation)
    {
        logger.LogInformation("StartEncounter called for encounter: {EncounterId}, location: {LocationId}", encounter?.Id, location?.Id);

        EncounterManager encounterManager = new EncounterManager(
            encounter,
            actionImplementation,
            _aiService,
            worldStateInputCreator,
            choiceProjectionService,
            configuration,
            logger);

        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

        NarrativeResult initialResult = await encounterManager.StartEncounterWithNarrativeAsync(
            location,
            encounter,
            worldState,
            playerState,
            actionImplementation);

        encounterManager.IsInitialState = true;

        encounterManager.EncounterResult = new EncounterResult()
        {
            ActionImplementation = actionImplementation,
            ActionResult = ActionResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = encounterManager.GetNarrativeContext()
        };

        logger.LogInformation("Encounter started: {EncounterId}", encounter?.Id);
        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteChoice(
        NarrativeResult narrativeResult,
        AiChoice choice)
    {
        logger.LogInformation("ExecuteChoice called for choice: {ChoiceId}", choice?.ChoiceID);
        NarrativeResult currentNarrative = narrativeResult;
        NarrativeResult cachedResult = null;

        EncounterManager encounterManager = GetCurrentEncounter();
        bool isInitialChoice = encounterManager.IsInitialState;

        _preGenerationManager.CancelAllPendingGenerations();

        List<AiChoice> choices = currentNarrative.Choices;

        currentNarrative = await encounterManager.ApplyChoiceWithNarrativeAsync(
            encounterManager.Encounter.LocationName,
            choice,
            AIClient.PRIORITY_IMMEDIATE);

        encounterManager.IsInitialState = false;
        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);

        logger.LogInformation("Choice executed: {ChoiceId}, EncounterOver: {IsEncounterOver}", choice?.ChoiceID, currentNarrative.IsEncounterOver);
        return encounterManager.EncounterResult;
    }

    private EncounterResult CreateEncounterResult(EncounterManager encounter, NarrativeResult currentNarrative)
    {
        if (currentNarrative.IsEncounterOver)
        {
            if (currentNarrative.Outcome == EncounterOutcomes.Failure)
            {
                logger.LogInformation("Encounter ended in failure. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
                    encounter.ActionImplementation?.Id,
                    currentNarrative.Outcome,
                    currentNarrative?.ToString(),
                    currentNarrative?.Choices?.Count ?? 0,
                    currentNarrative.IsEncounterOver);

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
                logger.LogInformation("Encounter ended in success. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
                    encounter.ActionImplementation?.Id,
                    currentNarrative.Outcome,
                    currentNarrative?.ToString(),
                    currentNarrative?.Choices?.Count ?? 0,
                    currentNarrative.IsEncounterOver);

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

        logger.LogInformation("Encounter ongoing. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
            encounter.ActionImplementation?.Id,
            currentNarrative.Outcome,
            currentNarrative?.ToString(),
            currentNarrative?.Choices?.Count ?? 0,
            currentNarrative.IsEncounterOver);

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
    public void ProcessEncounterProgress(EncounterResult result, GameState gameState)
    {
        if (result.ActionImplementation.Commission != null)
        {
            CommissionDefinition commission = result.ActionImplementation.Commission;

            int progress = 0;
            switch (result.ActionResult)
            {
                case ActionResults.EncounterSuccess:
                    progress = 5;
                    break;
                case ActionResults.EncounterFailure:
                    progress = 1;
                    break;
                default:
                    progress = 0; // No progress for failure
                    break;
            }

            commission.AddProgress(progress, gameState);

            if (commission.IsComplete())
            {
                CompleteCommission(commission, gameState);
            }
        }
    }

    private void CompleteCommission(CommissionDefinition commission, GameState gameState)
    {
        gameState.PlayerState.AddSilver(commission.SilverReward);
        gameState.PlayerState.AddReputation(commission.ReputationReward);
        gameState.PlayerState.AddInsightPoints(commission.InsightPointReward);

        messageSystem.AddSystemMessage($"Commission completed: {commission.Name}");

        gameState.WorldState.CompletedCommissions.Add(commission);
        gameState.WorldState.ActiveCommissions.Remove(commission);
    }

    public List<AiChoice> GetChoices()
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        List<AiChoice> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, AiChoice choice)
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        ChoiceProjection choiceProjection = encounterManager.ProjectChoice(choiceProjectionService, encounter.encounterState, choice);
        return choiceProjection;
    }

    public EncounterManager GetCurrentEncounter()
    {
        return gameState.ActionStateTracker.CurrentEncounter;
    }
}