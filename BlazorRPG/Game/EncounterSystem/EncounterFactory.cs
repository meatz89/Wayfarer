public class EncounterFactory
{
    public WorldState worldState;
    private AIGameMaster aiGameMaster;
    private ChoiceProjectionService choiceProjectionService;
    private readonly WorldStateInputBuilder worldStateInputBuilder;
    private IConfiguration configuration;
    private ILogger<EncounterFactory> logger;

    public EncounterFactory(
        GameWorld gameWorld,
        AIGameMaster aiGameMaster,
        ChoiceProjectionService choiceProjectionService,
        AIPromptBuilder promptBuilder,
        WorldStateInputBuilder worldStateInputBuilder,
        IConfiguration configuration,
        ILogger<EncounterFactory> logger)
    {
        this.aiGameMaster = aiGameMaster;
        this.choiceProjectionService = choiceProjectionService;
        this.worldStateInputBuilder = worldStateInputBuilder;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<EncounterManager> GenerateEncounter(
        EncounterContext context,
        Player player,
        LocationAction locationAction)
    {
        string situation = $"{locationAction.ActionId} ({locationAction.RequiredCardType} Action)";
        logger.LogInformation("EncounterContext generated for situation: {Situation}", situation);

        EncounterState state = new EncounterState(
            player,
            10,
            10,
            ""); // Placeholder values for focus points and complexity

        EncounterManager encounterManager = new EncounterManager(
            context,
            state,
            locationAction,
            choiceProjectionService,
            aiGameMaster,
            worldStateInputBuilder);

        return encounterManager;
    }
}