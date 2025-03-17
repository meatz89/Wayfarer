public class SwitchableNarrativeService : INarrativeAIService
{
    private readonly Dictionary<AIProviderType, INarrativeAIService> _providers;
    private AIProviderType _currentProvider;
    private readonly ILogger<EncounterSystem> _logger;

    public SwitchableNarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _logger = logger;

        // Initialize all providers in a dictionary for easier access
        _providers = new Dictionary<AIProviderType, INarrativeAIService>
        {
            { AIProviderType.OpenAI, new GPTNarrativeService(configuration, _logger) },
            { AIProviderType.Gemma3, new Gemma3NarrativeService(configuration, _logger) },
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
        return (_providers[_currentProvider] as BaseNarrativeAIService)?.GetProviderName() ?? "Unknown Provider";
    }

    public string GetGameInstanceId()
    {
        return (_providers[_currentProvider] as BaseNarrativeAIService)?.GetGameInstanceId() ?? "Unknown";
    }

    public async Task<string> GenerateIntroductionAsync(NarrativeContext context, string incitingAction, EncounterStatus state)
    {
        return await _providers[_currentProvider].GenerateIntroductionAsync(context, incitingAction, state);
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        return await _providers[_currentProvider].GenerateReactionAndSceneAsync(
            context, chosenOption, choiceDescription, outcome, newState);
    }

    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        return await _providers[_currentProvider].GenerateChoiceDescriptionsAsync(
            context, choices, projections, state);
    }

}