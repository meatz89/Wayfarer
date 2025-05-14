public class AIGenerationQueue
{
    private readonly object _queueLock = new object();
    private readonly PriorityQueue<AIGenerationCommand, (int Priority, DateTime Timestamp)> _queue = new();
    private readonly IAIProvider _aiProvider;
    private readonly string _gameInstanceId;
    private readonly NarrativeLogManager _logManager;
    private readonly ILogger<EncounterSystem> _logger;
    private bool _isProcessing = false;

    public AIGenerationQueue(
        IAIProvider aiProvider,
        string gameInstanceId,
        NarrativeLogManager logManager,
        ILogger<EncounterSystem> logger)
    {
        _aiProvider = aiProvider;
        _gameInstanceId = gameInstanceId;
        _logManager = logManager;
        _logger = logger;
    }

    public Task<string> EnqueueCommand(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        AIGenerationCommand command = new AIGenerationCommand(
            messages, model, fallbackModel, watcher, priority, sourceSystem);

        lock (_queueLock)
        {
            _queue.Enqueue(command, (command.Priority, command.Timestamp));
            _logger?.LogInformation(
                $"Enqueued request from {sourceSystem} with priority {priority}. Queue depth: {_queue.Count}");

            // If not currently processing, start processing
            if (!_isProcessing)
            {
                _isProcessing = true;
                Task.Run(() => ProcessQueue());
            }
        }

        return command.CompletionSource.Task;
    }

    private async Task ProcessQueue()
    {
        while (true)
        {
            AIGenerationCommand command;

            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    _isProcessing = false;
                    return;
                }

                command = _queue.Dequeue();
            }

            string conversationId = Guid.NewGuid().ToString();
            string generatedContent = null;
            string errorMessage = null;
            object requestBody = null;

            try
            {
                _logger?.LogInformation($"Processing request from {command.SourceSystem} with priority {command.Priority}");

                // Prepare request body for logging
                requestBody = new
                {
                    Provider = _aiProvider.Name,
                    GameInstanceId = _gameInstanceId,
                    MessageCount = command.Messages.Count,
                    Timestamp = DateTime.UtcNow
                };

                // Call the provider
                string result = await _aiProvider.GetCompletionAsync(
                    command.Messages,
                    command.Model,
                    command.FallbackModel,
                    command.Watcher);

                result = result?.Trim();
                generatedContent = result;

                // Complete the task
                command.CompletionSource.SetResult(result);
                _logger?.LogInformation($"Completed request from {command.SourceSystem}");
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}\nStack Trace: {ex.StackTrace}";
                _logger?.LogError(ex, $"Error processing request from {command.SourceSystem}");
                command.CompletionSource.SetException(ex);
            }
            finally
            {
                // Log the interaction, regardless of success or failure
                try
                {
                    await _logManager.LogApiInteractionAsync(
                        conversationId,
                        command.Messages,
                        requestBody,
                        null,
                        generatedContent,
                        errorMessage
                    );
                }
                catch (Exception logEx)
                {
                    _logger?.LogError(logEx, $"Failed to log API interaction: {logEx.Message}");
                }
            }
        }
    }

    public string GetProviderName()
    {
        return _aiProvider.Name;
    }

    public int GetQueueDepth()
    {
        lock (_queueLock)
        {
            return _queue.Count;
        }
    }
}