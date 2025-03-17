public class AIClientService
{
    private readonly IAIProvider _aiProvider;
    private readonly string _gameInstanceId;
    private readonly ILogger _logger;
    private readonly string _logDirectoryPath;

    public AIClientService(IAIProvider aiProvider, string gameInstanceId, ILogger logger)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _gameInstanceId = gameInstanceId;
        _logger = logger;

        // Create a unique log directory for this game instance
        _logDirectoryPath = Path.Combine("C:\\logs", $"{gameInstanceId}_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(_logDirectoryPath);

        logger?.LogInformation($"Initialized AIClientService with provider: {aiProvider.Name}, game instance ID: {_gameInstanceId}");
        logger?.LogInformation($"Logging conversation to: {_logDirectoryPath}");
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages)
    {
        try
        {
            // Log the request with full details
            _logger?.LogInformation($"Sending prompt to {_aiProvider.Name} (Game ID: {_gameInstanceId}). Message count: {messages.Count}");

            // Log the full request to a file
            await LogConversationToFileAsync(messages, "request");

            // The actual API call is delegated to the provider implementation
            string result = await _aiProvider.GetCompletionAsync(messages);

            // Trim any potential whitespace
            result = result?.Trim();

            // Log the response
            _logger?.LogInformation($"Received response from {_aiProvider.Name} (Game ID: {_gameInstanceId}). Character count: {result?.Length ?? 0}");

            // Log the full response to a file
            await LogConversationToFileAsync(new List<ConversationEntry>
            {
                new ConversationEntry
                {
                    Role = "assistant",
                    Content = result
                }
            }, "response");

            return result;
        }
        catch (Exception ex)
        {
            // Log the error
            _logger?.LogError(ex, $"Error getting completion from {_aiProvider.Name} (Game ID: {_gameInstanceId}): {ex.Message}");

            // Log the error to a file
            await LogErrorToFileAsync(ex);

            throw; // Rethrow to let the caller handle it
        }
    }

    // Overload to handle NarrativeContextManager's conversation history format
    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> conversationMessages)
    {
        // Convert from ConversationMessage to Message format
        List<ConversationEntry> messages = conversationMessages.Select(conversationMessage => new ConversationEntry
        {
            Role = conversationMessage.Role.ToLower(), // Ensure role is lowercase (system, user, assistant)
            Content = conversationMessage.Content
        }).ToList();

        return await GetCompletionAsync(messages);
    }

    private async Task LogConversationToFileAsync(List<ConversationEntry> messages, string filePrefix)
    {
        try
        {
            // Create a timestamped filename
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string filename = Path.Combine(_logDirectoryPath, $"{filePrefix}_{timestamp}.txt");

            // Prepare the content to write
            var contentToWrite = new List<string>();
            foreach (var message in messages)
            {
                contentToWrite.Add($"[{message.Role.ToUpper()}]");
                contentToWrite.Add(message.Content);
                contentToWrite.Add("---"); // Separator between messages
            }

            // Write to file
            await File.WriteAllLinesAsync(filename, contentToWrite);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error logging conversation to file: {ex.Message}");
        }
    }

    private async Task LogErrorToFileAsync(Exception ex)
    {
        try
        {
            // Create a timestamped filename for errors
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string filename = Path.Combine(_logDirectoryPath, $"error_{timestamp}.txt");

            // Prepare the error details
            var errorDetails = new List<string>
            {
                $"Error Time: {DateTime.Now}",
                $"Game Instance ID: {_gameInstanceId}",
                $"Error Message: {ex.Message}",
                $"Stack Trace: {ex.StackTrace}"
            };

            // Write to file
            await File.WriteAllLinesAsync(filename, errorDetails);
        }
        catch (Exception logEx)
        {
            // If logging the error fails, at least log to the standard logger
            _logger?.LogError(logEx, $"Failed to log error to file: {logEx.Message}");
        }
    }

    public string GetProviderName()
    {
        return _aiProvider.Name;
    }

    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}