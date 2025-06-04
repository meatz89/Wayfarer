public class EncounterFactory
{
    public WorldState worldState;
    private AIGameMaster aiGameMaster;
    private ChoiceProjectionService choiceProjectionService;
    private readonly AIPromptBuilder promptBuilder;
    private readonly WorldStateInputBuilder worldStateInputBuilder;
    private readonly EncounterChoiceProcessor choiceProcessor;
    private IConfiguration configuration;
    private ILogger<EncounterFactory> logger;

    public EncounterFactory(
        GameWorld gameWorld,
        AIGameMaster aiGameMaster,
        ChoiceProjectionService choiceProjectionService,
        AIPromptBuilder promptBuilder,
        WorldStateInputBuilder worldStateInputBuilder,
        EncounterChoiceProcessor choiceProcessor,
        IConfiguration configuration,
        ILogger<EncounterFactory> logger)
    {
        this.aiGameMaster = this.aiGameMaster;
        this.choiceProjectionService = choiceProjectionService;
        this.promptBuilder = promptBuilder;
        this.worldStateInputBuilder = worldStateInputBuilder;
        this.choiceProcessor = choiceProcessor;
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
            promptBuilder,
            worldStateInputBuilder,
            choiceProcessor);

        return encounterManager;
    }
}