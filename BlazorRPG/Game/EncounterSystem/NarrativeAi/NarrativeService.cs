public class NarrativeService : INarrativeAIService
{
    private readonly Dictionary<AIProviderType, INarrativeAIService> _providers;
    private AIProviderType _currentProvider;
    private readonly ILogger<EncounterSystem> _logger;

    public NarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _logger = logger;

        // Initialize all providers in a dictionary for easier access
        _providers = new Dictionary<AIProviderType, INarrativeAIService>
        {
            { AIProviderType.Claude, new ClaudeNarrativeService(configuration, _logger) }
        };

        // Set default provider from configuration
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";

        // Map string provider name to enum
        switch (defaultProvider.ToLower())
        {
            case "claude":
                _currentProvider = AIProviderType.Claude;
                break;
            case "gemma":
                _currentProvider = AIProviderType.Gemma3;
                break;
            default:
                _currentProvider = AIProviderType.OpenAI;
                break;
        }

        _logger?.LogInformation($"Initialized with {GetCurrentProviderName()} provider");
    }

    public void SwitchProvider(AIProviderType providerType)
    {
        if (_providers.ContainsKey(providerType))
        {
            _currentProvider = providerType;
            _logger?.LogInformation($"Switched to {GetCurrentProviderName()} AI provider");
        }
        else
        {
            _logger?.LogWarning($"Provider {providerType} not implemented, staying with current provider");
        }
    }

    public AIProviderType CurrentProvider => _currentProvider;

    public string GetCurrentProviderName()
    {
        BaseNarrativeAIService? baseNarrativeAIService = (_providers[_currentProvider] as BaseNarrativeAIService);
        return baseNarrativeAIService?.GetProviderName() ?? "Unknown Provider";
    }

    public string GetGameInstanceId()
    {
        BaseNarrativeAIService? baseNarrativeAIService = (_providers[_currentProvider] as BaseNarrativeAIService);
        return baseNarrativeAIService?.GetGameInstanceId() ?? "Unknown";
    }

    public async Task<string> GenerateIntroductionAsync(NarrativeContext context, EncounterStatusModel state, string memoryContent)
    {
        INarrativeAIService narrativeAIService = _providers[_currentProvider];
        return await narrativeAIService.GenerateIntroductionAsync(context, state, memoryContent);
    }
    
    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state)
    {
        return await _providers[_currentProvider].GenerateChoiceDescriptionsAsync(
            context, choices, projections, state);
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        return await _providers[_currentProvider].GenerateReactionAndSceneAsync(
            context, chosenOption, choiceDescription, outcome, newState);
    }

    public async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        return await _providers[_currentProvider].GenerateEndingAsync(
            context, chosenOption, choiceDescription, outcome, newState);
    }

    public async Task<string> GenerateMemoryFileAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        string oldMemory
        )
    {
        return await _providers[_currentProvider].GenerateMemoryFileAsync(
            context, outcome, newState, oldMemory);
    }
}