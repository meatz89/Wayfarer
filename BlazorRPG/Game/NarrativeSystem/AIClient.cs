public class AIClient
{
    private readonly AIGenerationQueue _queue;
    private readonly string _gameInstanceId;
    private readonly LoadingStateService _loadingStateService;

    // Priority constants
    public const int PRIORITY_IMMEDIATE = 1;
    public const int PRIORITY_HIGH = 3;
    public const int PRIORITY_NORMAL = 5;
    public const int PRIORITY_LOW = 7;
    public const int PRIORITY_BACKGROUND = 10;

    public AIClient(
        IAIProvider aiProvider,
        string gameInstanceId,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager logManager,
        LoadingStateService loadingStateService)
    {
        _gameInstanceId = gameInstanceId;
        _loadingStateService = loadingStateService;

        // Create the queue internally
        _queue = new AIGenerationQueue(aiProvider, gameInstanceId, logManager, logger);
    }

    public async Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        try
        {
            // Start loading with a message specific to this generation type
            _loadingStateService.StartLoading($"Generating {FormatSourceSystem(sourceSystem)}...");

            // Create a custom watcher that updates the loading state
            IResponseStreamWatcher combinedWatcher = watcher != null
                ? new ProgressTrackingWatcher(watcher, _loadingStateService)
                : new ProgressTrackingWatcher(null, _loadingStateService);

            // Use the queue to get the completion
            return await _queue.EnqueueCommand(
                messages, model, fallbackModel, combinedWatcher, priority, sourceSystem);
        }
        finally
        {
            // Make sure we stop loading even if there's an exception
            _loadingStateService.StopLoading();
        }
    }

    private string FormatSourceSystem(string sourceSystem)
    {
        return sourceSystem switch
        {
            "IntroductionGeneration" => "introduction",
            "ChoiceGeneration" => "choices",
            "EncounterNarrative" => "response",
            "PostEncounterEvolution" => "world update",
            _ => sourceSystem.ToLower()
        };
    }

    public string GetProviderName()
    {
        return _queue.GetProviderName();
    }

    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}