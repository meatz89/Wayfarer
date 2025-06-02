public class AIGenerationQueue
{
    private object _queueLock = new object();
    private PriorityQueue<AIGenerationCommand, (int Priority, DateTime Timestamp)> _queue = new();
    private Dictionary<string, TaskCompletionSource<string>> _pendingResults = new();
    private IAIProvider _aiProvider;
    private string _gameInstanceId;
    private NarrativeLogManager _logManager;
    private ILogger<EncounterFactory> _logger;
    private bool _isProcessing = false;

    public AIGenerationQueue(
        IAIProvider aiProvider,
        string gameInstanceId,
        NarrativeLogManager logManager,
        ILogger<EncounterFactory> logger)
    {
        _aiProvider = aiProvider;
        _gameInstanceId = gameInstanceId;
        _logManager = logManager;
        _logger = logger;
    }

    public AIGenerationCommand EnqueueCommand(
        List<ConversationEntry> messages,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        string commandId = Guid.NewGuid().ToString();

        AIGenerationCommand command = new AIGenerationCommand(
            commandId, messages, watcher, priority, sourceSystem);

        lock (_queueLock)
        {
            _queue.Enqueue(command, (command.Priority, command.Timestamp));
            _pendingResults[commandId] = command.CompletionSource;

            _logger?.LogInformation(
                $"Enqueued request {commandId} from {sourceSystem} with priority {priority}. Queue depth: {_queue.Count}");

            // If not currently processing, start processing
            if (!_isProcessing)
            {
                _isProcessing = true;
                Task.Run(() => ProcessQueue());
            }
        }

        return command;
    }

    public Task<string> WaitForResult(string commandId)
    {
        lock (_queueLock)
        {
            if (_pendingResults.TryGetValue(commandId, out TaskCompletionSource<string> tcs))
            {
                return tcs.Task;
            }

            throw new KeyNotFoundException($"No pending command with ID {commandId} found");
        }
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
                _logger?.LogInformation($"Processing request {command.Id} from {command.SourceSystem} with priority {command.Priority}");

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
                    command.Watcher);

                result = result?.Trim();
                generatedContent = result;

                // Complete the task
                command.CompletionSource.SetResult(result);
                _logger?.LogInformation($"Completed request {command.Id} from {command.SourceSystem}");

                // Remove from pending results
                lock (_queueLock)
                {
                    _pendingResults.Remove(command.Id);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}\nStack Trace: {ex.StackTrace}";
                _logger?.LogError(ex, $"Error processing request {command.Id} from {command.SourceSystem}");
                command.CompletionSource.SetException(ex);

                // Remove from pending results
                lock (_queueLock)
                {
                    _pendingResults.Remove(command.Id);
                }
            }
            finally
            {
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