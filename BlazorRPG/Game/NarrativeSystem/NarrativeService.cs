public class NarrativeService : IAIService
{
    private readonly Dictionary<AIProviderType, IAIService> _providers;
    private AIProviderType _currentProvider;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<EncounterSystem> _logger;

    public NarrativeService(
        PostEncounterEvolutionParser postEncounterEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        NarrativeLogManager narrativeLogManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        _contextManager = narrativeContextManager;
        _logger = logger;

        _providers = new Dictionary<AIProviderType, IAIService>
        {
            { AIProviderType.Claude, new ClaudeNarrativeService(postEncounterEvolutionParser, _contextManager, configuration, _logger, narrativeLogManager) },
            { AIProviderType.OpenAI, new OpenAINarrativeService(postEncounterEvolutionParser, _contextManager, configuration, _logger, narrativeLogManager) },
            { AIProviderType.Gemini, new GeminiNarrativeService(postEncounterEvolutionParser, _contextManager, configuration, _logger, narrativeLogManager) }
        };

        // Set default provider from configuration
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";

        // Map string provider name to enum
        switch (defaultProvider.ToLower())
        {
            case "claude":
                _currentProvider = AIProviderType.Claude;
                break;
            case "gemini":
                _currentProvider = AIProviderType.Gemini;
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

    public async Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent,
        WorldStateInput worldStateInput)
    {
        IAIService narrativeAIService = _providers[_currentProvider];
        return await narrativeAIService.GenerateIntroductionAsync(context, state, memoryContent, worldStateInput);
    }

    public async Task<Dictionary<CardDefinition, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<CardDefinition> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].GenerateChoiceDescriptionsAsync(
            context, choices, projections, state, worldStateInput);
    }

    public async Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].GenerateEncounterNarrative(
            context, chosenOption, choiceDescription, outcome, newState, worldStateInput);
    }

    public async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].GenerateEndingAsync(
            context, chosenOption, choiceDescription, outcome, newState, worldStateInput);
    }


    public async Task<LocationDetails> GenerateLocationDetailsAsync(
        LocationCreationInput context,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].GenerateLocationDetailsAsync(context, worldStateInput);
    }

    public async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].ProcessPostEncounterEvolution(context, input, worldStateInput);
    }

    public async Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].ProcessMemoryConsolidation(context, input, worldStateInput);
    }

    public async Task<string> GenerateActionsAsync(
        ActionGenerationContext input,
        WorldStateInput worldStateInput)
    {
        return await _providers[_currentProvider].GenerateActionsAsync(input, worldStateInput);
    }
}
