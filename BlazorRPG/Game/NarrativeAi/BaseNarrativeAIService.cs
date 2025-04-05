public abstract class BaseNarrativeAIService : IAIService
{
    protected readonly AIClientService _aiClient;
    protected readonly PromptManager _promptManager;
    protected readonly ILogger<EncounterSystem> _logger;
    private readonly NarrativeLogManager narrativeLogManager;
    protected readonly string _gameInstanceId;

    protected BaseNarrativeAIService(
        IAIProvider aiProvider,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager
        )
    {
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _promptManager = new PromptManager(configuration);
        _logger = logger;
        this.narrativeLogManager = narrativeLogManager;
        _aiClient = new AIClientService(aiProvider, _gameInstanceId, logger, narrativeLogManager);

        _logger?.LogInformation($"Initialized NarrativeAIService with {aiProvider.Name} and game instance ID: {_gameInstanceId}");
    }

    public string GetProviderName()
    {
        return _aiClient.GetProviderName();
    }

    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }

    public abstract Task<string> GenerateIntroductionAsync(
        NarrativeContext context, EncounterStatusModel state, string memoryContent);
    public abstract Task<string> GenerateEncounterNarrative(
        NarrativeContext context, IChoice chosenOption, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, EncounterStatusModel newState);
    public abstract Task<string> GenerateEndingAsync(
        NarrativeContext context, IChoice chosenOption, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, EncounterStatusModel newState);
    public abstract Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(NarrativeContext context, List<IChoice> choices, List<ChoiceProjection> projections, EncounterStatusModel state);

    public abstract Task<LocationDetails> GenerateLocationDetailsAsync(LocationCreationContext context);
    public abstract Task<string> GenerateActionsAsync(ActionGenerationContext context);
    public abstract Task<EvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context,
        PostEncounterEvolutionInput input);
    public abstract Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input);
}
