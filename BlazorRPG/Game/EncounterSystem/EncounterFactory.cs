public class EncounterFactory
{
    private IConfiguration configuration;
    private ILogger<EncounterFactory> logger;

    public WorldState worldState;

    public EncounterFactory(
        GameWorld gameState,
        MessageSystem messageSystem,
        EncounterContextManager encounterContextManager,
        WorldStateInputBuilder worldStateInputCreator,
        ChoiceProjectionService choiceProjectionService,
        IConfiguration configuration,
        ILogger<EncounterFactory> logger)
    {
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
            locationAction);

        return encounterManager;
    }
}