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
        OpportunityDefinition opportunity,
        ApproachDefinition approach,
        Location location,
        LocationSpot locationSpot,
        WorldState worldState,
        Player playerState,
        LocationAction locationAction)
    {
        logger.LogInformation(
            "GenerateEncounterContext called with id: {Id}, opportunity: {OpportunityId}, approachId: {ApproachId}, location: {LocationId}, locationSpot: {LocationSpotId}",
            id, opportunity?.Id, approach.Id, location?.Id, locationSpot?.SpotID);

        this.worldState = worldState;

        EncounterContext encounterContext = encounterFactory.InitializeEncounter(
            opportunity, approach, playerState, location);

        // TODO 
        locationAction.Execute(playerState, locationSpot);

        string situation = $"{locationAction.ActionId} ({locationAction.RequiredCardType} Action)";
        logger.LogInformation("EncounterContext generated for situation: {Situation}", situation);
        return encounterManager;
    }

    public async Task<EncounterManager> StartEncounter(
        Location location,
        WorldState worldState,
        Player playerState,
        LocationAction locationAction)
    {
        logger.LogInformation("StartEncounterContext called for encounter: {EncounterId}, location: {LocationId}", encounter?.Id, location?.Id);

        EncounterManager encounterManager = new EncounterManager(
            encounterContext,
            locationAction,
            worldStateInputCreator,
            choiceProjectionService,
            configuration,
            logger);

        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

        AIResponse initialResult = await encounterManager.InitializeEncounter(
            location,
            encounterContext,
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

        logger.LogInformation("EncounterContext started: {EncounterId}", encounter?.Id);
        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteChoice(
        AIResponse AIResponse,
        EncounterChoice choice)
    {
        logger.LogInformation("ExecuteChoice called for choice: {ChoiceId}", choice?.ChoiceID);
        AIResponse currentNarrative = AIResponse;
        AIResponse cachedResult = null;

        EncounterManager encounterManager = GetEncounterManager();
        bool isInitialChoice = encounterManager.IsInitialState;

        _preGenerationManager.CancelAllPendingGenerations();

        List<EncounterChoice> choices = currentNarrative.Choices;

        currentNarrative = await encounterManager.ProcessPlayerSelection(
            encounterManager.Encounter.LocationName,
            choice,
            AIClient.PRIORITY_IMMEDIATE);

        encounterManager.IsInitialState = false;
        encounterManager.EncounterResult = CreateEncounterResult(encounterManager, currentNarrative);

        logger.LogInformation("Choice executed: {ChoiceId}, EncounterOver: {IsEncounterOver}", choice?.ChoiceID, currentNarrative.IsEncounterOver);
        return encounterManager.EncounterResult;
    }

    private EncounterResult CreateEncounterResult(EncounterManager encounterContext, AIResponse currentNarrative)
    {
        if (currentNarrative.IsEncounterOver)
        {
            if (currentNarrative.Outcome == BeatOutcomes.Failure)
            {
                EncounterResult failureResult = new EncounterResult()
                {
                    locationAction = encounter.locationAction,
                    ActionResult = ActionResults.EncounterFailure,
                    EncounterEndMessage = $"=== EncounterContext Over: {currentNarrative.Outcome} ===",
                    AIResponse = currentNarrative,
                    EncounterContext = encounter.GetEncounterContext()
                };
                return failureResult;
            }
            else
            {
                logger.LogInformation("EncounterContext ended in success. ActionId: {ActionId}, Outcome: {Outcome}, Narrative: {Narrative}, Choices: {Choices}, IsEncounterOver: {IsEncounterOver}",
                    encounter.locationAction?.ActionId,
                    currentNarrative.Outcome,
                    currentNarrative?.ToString(),
                    currentNarrative?.Choices?.Count ?? 0,
                    currentNarrative.IsEncounterOver);

                EncounterResult successResult = new EncounterResult()
                {
                    locationAction = encounter.locationAction,
                    ActionResult = ActionResults.EncounterSuccess,
                    EncounterEndMessage = $"=== EncounterContext Over: {currentNarrative.Outcome} ===",
                    AIResponse = currentNarrative,
                    EncounterContext = encounter.GetEncounterContext()
                };
                return successResult;
            }
        }

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
        if (result.locationAction.Opportunity != null)
        {
            OpportunityDefinition opportunity = result.locationAction.Opportunity;

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

            opportunity.AddProgress(progress, gameState);
        }
    }

    public List<EncounterChoice> GetChoices()
    {
        EncounterManager encounterManager = GetEncounterManager();
        List<EncounterChoice> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounterContext, EncounterChoice choice)
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