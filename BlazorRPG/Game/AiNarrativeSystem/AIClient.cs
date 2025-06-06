public class AIClient
{
    private AIGenerationQueue _queue;
    private string _gameInstanceId;
    private LoadingStateService _loadingStateService;
    private GameWorld _gameWorld;

    // Priority constants
    public const int PRIORITY_IMMEDIATE = 1;
    public const int PRIORITY_HIGH = 3;
    public const int PRIORITY_NORMAL = 5;
    public const int PRIORITY_LOW = 7;
    public const int PRIORITY_BACKGROUND = 10;

    public AIClient(
        IAIProvider aiProvider,
        ILogger<EncounterFactory> logger,
        NarrativeLogManager logManager,
        LoadingStateService loadingStateService,
        GameWorld gameWorld)
    {
        _loadingStateService = loadingStateService;
        _gameWorld = gameWorld;
        _queue = new AIGenerationQueue(aiProvider, "gameInstanceId", logManager, logger);
    }

    public async Task<string> ProcessCommand(AIGenerationCommand command)
    {
        string result = string.Empty;
        bool isBackgroundRequest = command.Priority >= PRIORITY_BACKGROUND;

        try
        {
            if (!isBackgroundRequest)
            {
                _loadingStateService.StartLoading($"Generating {FormatSourceSystem(command.SourceSystem)}...");
            }
            result = await _queue.WaitForResult(command.Id);
        }
        finally
        {
            if (!isBackgroundRequest)
            {
                _loadingStateService.StopLoading();
            }
        }
        return result;
    }

    public async Task<AIGenerationCommand> CreateAndQueueCommand(
        List<ConversationEntry> messages,
        int priority,
        string sourceSystem)
    {
        // Create list of watchers
        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        // Add streaming watcher for non-background tasks
        bool isBackgroundRequest = priority >= PRIORITY_BACKGROUND;
        if (!isBackgroundRequest && _gameWorld?.StreamingContentState != null)
        {
            watchers.Add(new ConsoleResponseWatcher());
            watchers.Add(new StreamingContentStateWatcher(_gameWorld.StreamingContentState));
        }

        // Queue the command with all watchers
        AIGenerationCommand command = _queue.EnqueueCommand(
            messages, watchers, priority, sourceSystem);

        return command;
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