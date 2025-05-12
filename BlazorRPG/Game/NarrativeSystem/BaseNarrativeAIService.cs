public abstract class BaseNarrativeAIService : IAIService
{
    protected readonly AIClient _aiClient;
    protected readonly PromptManager _promptManager;
    protected readonly string _gameInstanceId;

    protected BaseNarrativeAIService(
        IAIProvider aiProvider,
        IConfiguration configuration,
        NarrativeLogManager narrativeLogManager
        )
    {
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _promptManager = new PromptManager(configuration);
        _aiClient = new AIClient(aiProvider, _gameInstanceId, narrativeLogManager);
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
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent,
        WorldStateInput worldStateInput);
    public abstract Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput);
    public abstract Task<string> GenerateEndingAsync(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput);
    public abstract Task<Dictionary<CardDefinition, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<CardDefinition> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput);

    public abstract Task<LocationDetails> GenerateLocationDetailsAsync(
        LocationCreationInput context,
        WorldStateInput worldStateInput);
    public abstract Task<string> GenerateActionsAsync(
        ActionGenerationContext context,
        WorldStateInput worldStateInput);
    public abstract Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput);
    public abstract Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput);
}
