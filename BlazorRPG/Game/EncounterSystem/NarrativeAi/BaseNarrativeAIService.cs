public abstract class BaseNarrativeAIService : IAIService
{
    protected readonly AIClientService _aiClient;
    protected readonly PromptManager _promptManager;
    protected readonly NarrativeContextManager _contextManager;
    protected readonly ILogger<EncounterSystem> _logger;
    protected readonly string _gameInstanceId;

    protected BaseNarrativeAIService(
        IAIProvider aiProvider,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _aiClient = new AIClientService(aiProvider, _gameInstanceId, logger);
        _promptManager = new PromptManager(configuration);
        _contextManager = new NarrativeContextManager();
        _logger = logger;

        _logger?.LogInformation($"Initialized NarrativeAIService with {aiProvider.Name} and game instance ID: {_gameInstanceId}");
    }

    public abstract Task<string> GenerateIntroductionAsync(
        NarrativeContext context, EncounterStatusModel state, string memoryContent);
    public abstract Task<string> GenerateEncounterNarrative(
        NarrativeContext context, IChoice chosenOption, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, EncounterStatusModel newState);
    public abstract Task<string> GenerateEndingAsync(
        NarrativeContext context, IChoice chosenOption, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, EncounterStatusModel newState);
    public abstract Task<string> GenerateMemoryFileAsync(
        NarrativeContext context, ChoiceOutcome outcome, EncounterStatusModel newState, string oldMemory);
    public abstract Task<string> GenerateStateChangesAsync(
        NarrativeContext context, ChoiceOutcome outcome, EncounterStatusModel newState);
    public abstract Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(NarrativeContext context, List<IChoice> choices, List<ChoiceProjection> projections, EncounterStatusModel state);

    public string GetProviderName()
    {
        return _aiClient.GetProviderName();
    }

    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }

    public abstract Task<DiscoveredEntities> ExtractWorldDiscoveries(string encounterNarrative, WorldContext worldContext);
    public abstract Task<EntityDetails> DevelopEntityDetails(string entityType, string entityId, EntityContext entityContext);
    public abstract Task<StateChangeRecommendations> GenerateStateChanges(string encounterOutcome, EncounterContext context);

    public abstract Task<LocationDetails> GenerateLocationDetailsAsync(LocationGenerationContext context);
}
