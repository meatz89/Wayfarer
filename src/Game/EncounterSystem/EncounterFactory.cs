public class EncounterFactory
{
    public WorldState worldState;
    private GameWorld gameWorld;
    private AIGameMaster? aiGameMaster;
    private ChoiceProjectionService choiceProjectionService;
    private WorldStateInputBuilder? worldStateInputBuilder;
    private ILogger<EncounterFactory> logger;
    private bool aiAvailable;

    // Full constructor with AI services
    public EncounterFactory(
        GameWorld gameWorld,
        AIGameMaster aiGameMaster,
        ChoiceProjectionService choiceProjectionService,
        AIPromptBuilder promptBuilder,
        WorldStateInputBuilder worldStateInputBuilder,
        ILogger<EncounterFactory> logger)
    {
        this.gameWorld = gameWorld;
        this.aiGameMaster = aiGameMaster;
        this.choiceProjectionService = choiceProjectionService;
        this.worldStateInputBuilder = worldStateInputBuilder;
        this.logger = logger;
        this.aiAvailable = true;
    }

    //  constructor without AI services
    public EncounterFactory(
        GameWorld gameWorld,
        ChoiceProjectionService choiceProjectionService,
        ILogger<EncounterFactory> logger)
    {
        this.gameWorld = gameWorld;
        this.aiGameMaster = null;
        this.choiceProjectionService = choiceProjectionService;
        this.worldStateInputBuilder = null;
        this.logger = logger;
        this.aiAvailable = false;
    }

    public async Task<EncounterManager?> GenerateEncounter(
        EncounterContext context,
        Player player,
        LocationAction locationAction)
    {
        string situation = $"{locationAction.ActionId} ({locationAction.RequiredCardType} Action)";

        if (!aiAvailable)
        {
            logger.LogInformation(" POC: Skipping AI encounter generation for action: {ActionId}", locationAction.ActionId);
            //  actions are processed instantly, no encounters
            return null;
        }

        logger.LogInformation("EncounterContext generated for situation: {Situation}", situation);

        EncounterState state = new EncounterState(
            player,
            10,
            10,
            10); // Placeholder values for focus points and complexity

        EncounterManager encounterManager = new EncounterManager(
            context,
            state,
            locationAction,
            choiceProjectionService,
            aiGameMaster!,
            worldStateInputBuilder!,
            gameWorld);

        return encounterManager;
    }
}