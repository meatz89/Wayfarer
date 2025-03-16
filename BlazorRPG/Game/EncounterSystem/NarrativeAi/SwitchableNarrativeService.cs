public class SwitchableNarrativeService : INarrativeAIService
{
    private readonly INarrativeAIService _openAIService;
    private readonly INarrativeAIService _gemma3Service;
    private AIProviderType _currentProvider;
    private readonly ILogger _logger;

    public SwitchableNarrativeService(IConfiguration configuration, ILogger logger = null)
    {
        _openAIService = new GPTNarrativeService(configuration);
        _gemma3Service = new Gemma3NarrativeService(configuration);
        _currentProvider = AIProviderType.OpenAI; // Default to OpenAI
        _logger = logger;
    }

    public void SwitchProvider(AIProviderType providerType)
    {
        _currentProvider = providerType;
        _logger?.LogInformation($"Switched to {GetCurrentProviderName()} AI provider");
    }

    public AIProviderType CurrentProvider => _currentProvider;

    public string GetCurrentProviderName()
    {
        return _currentProvider == AIProviderType.OpenAI
            ? (_openAIService as BaseNarrativeAIService).GetProviderName()
            : (_gemma3Service as BaseNarrativeAIService).GetProviderName();
    }

    public string GetGameInstanceId()
    {
        return _currentProvider == AIProviderType.OpenAI
            ? (_openAIService as BaseNarrativeAIService).GetGameInstanceId()
            : (_gemma3Service as BaseNarrativeAIService).GetGameInstanceId();
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        return _currentProvider == AIProviderType.OpenAI
            ? await _openAIService.GenerateIntroductionAsync(location, incitingAction, state)
            : await _gemma3Service.GenerateIntroductionAsync(location, incitingAction, state);
    }

    public async Task<string> GenerateReactionAndSceneAsync(NarrativeContext context, IChoice chosenOption, ChoiceNarrative choiceDescription, ChoiceOutcome outcome, EncounterStatus newState)
    {
        return _currentProvider == AIProviderType.OpenAI
            ? await _openAIService.GenerateReactionAndSceneAsync(context, chosenOption, choiceDescription, outcome, newState)
            : await _gemma3Service.GenerateReactionAndSceneAsync(context, chosenOption, choiceDescription, outcome, newState);
    }

    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(NarrativeContext context, List<IChoice> choices, List<ChoiceProjection> projections, EncounterStatus state)
    {
        return _currentProvider == AIProviderType.OpenAI
            ? await _openAIService.GenerateChoiceDescriptionsAsync(context, choices, projections, state)
            : await _gemma3Service.GenerateChoiceDescriptionsAsync(context, choices, projections, state);
    }
}