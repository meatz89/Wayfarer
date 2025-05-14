public class AIClient
{
    private readonly AIGenerationQueue _queue;
    private readonly string _gameInstanceId;

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
        NarrativeLogManager logManager)
    {
        _gameInstanceId = gameInstanceId;

        // Create the queue internally
        _queue = new AIGenerationQueue(aiProvider, gameInstanceId, logManager, logger);
    }

    public async Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher,
        int priority = PRIORITY_NORMAL,
        string sourceSystem = "Unknown")
    {
        return await _queue.EnqueueCommand(
            messages,
            model,
            fallbackModel,
            watcher,
            priority,
            sourceSystem);
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