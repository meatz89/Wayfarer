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
        // Only show loading screens for non-background priority requests
        bool isBackgroundRequest = priority >= PRIORITY_BACKGROUND;

        try
        {
            // Only start loading UI for non-background requests
            if (!isBackgroundRequest)
            {
                _loadingStateService.StartLoading($"Generating {FormatSourceSystem(sourceSystem)}...");
            }

            // Determine which watcher to use
            IResponseStreamWatcher effectiveWatcher;

            if (isBackgroundRequest)
            {
                // For background tasks, use raw watcher without progress tracking
                effectiveWatcher = watcher;
            }
            else
            {
                // For foreground tasks, use progress tracking watcher
                effectiveWatcher = watcher != null
                    ? new ProgressTrackingWatcher(watcher, _loadingStateService)
                    : new ProgressTrackingWatcher(null, _loadingStateService);
            }

            // Use the queue to get the completion
            return await _queue.EnqueueCommand(
                messages, model, fallbackModel, effectiveWatcher, priority, sourceSystem);
        }
        finally
        {
            // Only stop loading if we started it (non-background requests)
            if (!isBackgroundRequest)
            {
                _loadingStateService.StopLoading();
            }
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
