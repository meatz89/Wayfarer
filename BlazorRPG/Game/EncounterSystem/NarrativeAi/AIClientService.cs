public class AIClientService
{
    private readonly IAIProvider _aiProvider;
    private readonly string _gameInstanceId;
    private readonly ILogger _logger;

    public AIClientService(IAIProvider aiProvider, string gameInstanceId, ILogger logger = null)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _gameInstanceId = gameInstanceId;
        _logger = logger;

        _logger?.LogInformation($"Initialized AIClientService with provider: {_aiProvider.Name}, game instance ID: {_gameInstanceId}");
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages)
    {
        try
        {
            // Log the request with basic info
            _logger?.LogInformation($"Sending prompt to {_aiProvider.Name} (Game ID: {_gameInstanceId}). Message count: {messages.Count}");

            // The actual API call is delegated to the provider implementation
            string result = await _aiProvider.GetCompletionAsync(messages);

            // Trim any potential whitespace
            result = result?.Trim();

            // Log success
            _logger?.LogInformation($"Received response from {_aiProvider.Name} (Game ID: {_gameInstanceId}). Character count: {result?.Length ?? 0}");

            return result;
        }
        catch (Exception ex)
        {
            // Log the error
            _logger?.LogError(ex, $"Error getting completion from {_aiProvider.Name} (Game ID: {_gameInstanceId}): {ex.Message}");
            throw; // Rethrow to let the caller handle it
        }
    }

    // Overload to handle NarrativeContextManager's conversation history format
    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> conversationMessages)
    {
        // Convert from ConversationMessage to Message format
        List<ConversationEntry> messages = new List<ConversationEntry>();

        foreach (ConversationEntry conversationMessage in conversationMessages)
        {
            messages.Add(new ConversationEntry
            {
                Role = conversationMessage.Role.ToLower(), // Ensure role is lowercase (system, user, assistant)
                Content = conversationMessage.Content
            });
        }

        return await GetCompletionAsync(messages);
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
