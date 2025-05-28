public class EncounterSystem
{
    private GameWorld gameState;
    private MessageSystem messageSystem;
    private IConfiguration configuration;
    private ILogger<EncounterSystem> logger;

    public WorldState worldState;
    public LocationActionProcessor encounterFactory;
    private WorldStateInputBuilder worldStateInputCreator;
    private ChoiceProjectionService choiceProjectionService;
    private PreGenerationManager _preGenerationManager;


    private PayloadRegistry payloadRegistry;
    private EncounterManager encounterManager;
    private AIGameMaster aiGameMaster;
    private EncounterUIController uiController;

    public EncounterSystem(
        GameWorld gameState,
        MessageSystem messageSystem,
        EncounterContextManager EncounterContextManager,
        LocationActionProcessor encounterFactory,
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
        LocationAction locationAction)
    {
        logger.LogInformation(
            "GenerateEncounter called with id: {Id}, commission: {CommissionId}, approachId: {ApproachId}, location: {LocationId}, locationSpot: {LocationSpotId}",
            id, commission?.Id, approach.Id, location?.Id, locationSpot?.SpotID);

        this.worldState = worldState;

        Encounter encounter = encounterFactory.InitializeEncounter(
            commission, approach, playerState, location);

        // TODO 
        locationAction.Execute(playerState, locationSpot);

        string situation = $"{locationAction.ActionId} ({locationAction.RequiredCardType} Action)";
        logger.LogInformation("Encounter generated for situation: {Situation}", situation);
        return encounterManager;
    }

    public async Task<EncounterManager> StartEncounter(
        Encounter encounter,
        Location location,
        WorldState worldState,
        Player playerState,
        LocationAction locationAction)
    {
        logger.LogInformation("StartEncounter called for encounter: {EncounterId}, location: {LocationId}", encounter?.Id, location?.Id);

        EncounterManager encounterManager = new EncounterManager(
            encounter,
            locationAction,
            worldStateInputCreator,
            choiceProjectionService,
            configuration,
            logger);

        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

        AIGameMasterResponse initialResult = await encounterManager.StartEncounter(
            location,
            encounter,
            worldState,
            playerState,
            locationAction);

        encounterManager.IsInitialState = true;

        encounterManager.EncounterResult = new EncounterResult()
        {
            locationAction = locationAction,
            ActionResult = ActionResults.Ongoing,
            EncounterEndMessage = "",
            AIResponse = initialResult,
            EncounterContext = encounterManager.GetEncounterContext()
        };

        logger.LogInformation("Encounter started: {EncounterId}", encounter?.Id);
        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteChoice(
        AIGameMasterResponse AIResponse,
        EncounterChoice choice)
    {
        logger.LogInformation("ExecuteChoice called for choice: {ChoiceId}", choice?.ChoiceID);
        AIGameMasterResponse currentNarrative = AIResponse;
        AIGameMasterResponse cachedResult = null;

        EncounterManager encounterManager = GetEncounterManager();
        bool isInitialChoice = encounterManager.IsInitialState;

        _preGenerationManager.CancelAllPendingGenerations();

        List<EncounterChoice> choices = currentNarrative.AvailableChoices;

        currentNarrative = await encounterManager.ProcessPlayerSelection(
            encounterManager.Encounter.LocationName,
            choice,
            AIClient.PRIORITY_IMMEDIATE);

        encounterManager.IsInitialState = false;
        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);

        logger.LogInformation("Choice executed: {ChoiceId}, EncounterOver: {IsEncounterOver}", choice?.ChoiceID, currentNarrative.IsEncounterOver);
        return encounterManager.EncounterResult;
    }

    private EncounterResult CreateEncounterResult(EncounterManager encounter, AIGameMasterResponse currentNarrative)
    {
        if (currentNarrative.IsEncounterOver)
        {
            if (currentNarrative.Outcome == EncounterOutcomes.Failure)
            {
                logger.LogInformation("Encounter ended in failure. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
                    encounter.locationAction?.ActionId,
                    currentNarrative.Outcome,
                    currentNarrative?.ToString(),
                    currentNarrative?.AvailableChoices?.Count ?? 0,
                    currentNarrative.IsEncounterOver);

                EncounterResult failureResult = new EncounterResult()
                {
                    locationAction = encounter.locationAction,
                    ActionResult = ActionResults.EncounterFailure,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    AIResponse = currentNarrative,
                    EncounterContext = encounter.GetEncounterContext()
                };
                return failureResult;
            }
            else
            {
                logger.LogInformation("Encounter ended in success. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
                    encounter.locationAction?.ActionId,
                    currentNarrative.Outcome,
                    currentNarrative?.ToString(),
                    currentNarrative?.AvailableChoices?.Count ?? 0,
                    currentNarrative.IsEncounterOver);

                EncounterResult successResult = new EncounterResult()
                {
                    locationAction = encounter.locationAction,
                    ActionResult = ActionResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    AIResponse = currentNarrative,
                    EncounterContext = encounter.GetEncounterContext()
                };
                return successResult;
            }
        }

        logger.LogInformation("Encounter ongoing. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
            encounter.locationAction?.ActionId,
            currentNarrative.Outcome,
            currentNarrative?.ToString(),
            currentNarrative?.AvailableChoices?.Count ?? 0,
            currentNarrative.IsEncounterOver);

        EncounterResult ongoingResult = new EncounterResult()
        {
            locationAction = encounter.locationAction,
            ActionResult = ActionResults.Ongoing,
            EncounterEndMessage = "",
            AIResponse = currentNarrative,
            EncounterContext = encounter.GetEncounterContext()
        };

        return ongoingResult;
    }
    public void ProcessEncounterProgress(EncounterResult result, GameWorld gameState)
    {
        if (result.locationAction.Commission != null)
        {
            CommissionDefinition commission = result.locationAction.Commission;

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

    private void CompleteCommission(CommissionDefinition commission, GameWorld gameState)
    {
        gameState.Player.AddSilver(commission.SilverReward);
        gameState.Player.AddReputation(commission.ReputationReward);
        gameState.Player.AddInsightPoints(commission.InsightPointReward);

        messageSystem.AddSystemMessage($"Commission completed: {commission.Name}");

        gameState.WorldState.CompletedCommissions.Add(commission);
        gameState.WorldState.ActiveCommissions.Remove(commission);
    }

    public List<EncounterChoice> GetChoices()
    {
        EncounterManager encounterManager = GetEncounterManager();
        List<EncounterChoice> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, EncounterChoice choice)
    {
        EncounterManager encounterManager = GetEncounterManager();
        ChoiceProjection choiceProjection = encounterManager.ProjectChoice(choiceProjectionService, encounter.state, choice);
        return choiceProjection;
    }

    public EncounterManager GetEncounterManager()
    {
        return encounterManager;
    }
}