public class AIClient
{
    private readonly IAIProvider _aiProvider;
    private readonly string _gameInstanceId;
    private readonly ILogger _logger;
    private readonly NarrativeLogManager _logManager;

    public AIClient(
        IAIProvider aiProvider,
        string gameInstanceId,
        ILogger logger,
        NarrativeLogManager logManager)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _gameInstanceId = gameInstanceId;
        _logger = logger;
        this._logManager = logManager;
        logger?.LogInformation($"Initialized AIClientService with provider: {aiProvider.Name}, game instance ID: {_gameInstanceId}");
        logger?.LogInformation($"Logging conversation to: {_logManager.GetSessionDirectory()}");
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> conversationMessages, string model, string fallbackModel)
    {
        List<ConversationEntry> messages = conversationMessages.Select(conversationMessage => new ConversationEntry
        {
            Role = conversationMessage.Role.ToLower(), // Ensure role is lowercase (system, user, assistant)
            Content = conversationMessage.Content
        }).ToList();

        return await GetCompletionAsync(messages, model, fallbackModel);
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages, string model, string fallbackModel)
    {
        string conversationId = Guid.NewGuid().ToString();
        string jsonResponse = null;
        string generatedContent = null;
        string errorMessage = null;
        object requestBody = null;

        try
        {
            // Prepare request body for logging
            requestBody = new
            {
                Provider = _aiProvider.Name,
                GameInstanceId = _gameInstanceId,
                MessageCount = messages.Count,
                Timestamp = DateTime.UtcNow
            };

            // Log the request with basic info
            _logger?.LogInformation($"Sending prompt to {_aiProvider.Name} (Game ID: {_gameInstanceId}). Message count: {messages.Count}");

            // The actual API call is delegated to the provider implementation
            string result = await _aiProvider.GetCompletionAsync(messages, model, fallbackModel);

            // Trim any potential whitespace
            result = result?.Trim();
            generatedContent = result;

            // Log success
            _logger?.LogInformation($"Received response from {_aiProvider.Name} (Game ID: {_gameInstanceId}). Character count: {result?.Length ?? 0}");

            return result;
        }
        catch (Exception ex)
        {
            // Capture error details
            errorMessage = $"Error: {ex.Message}\nStack Trace: {ex.StackTrace}";

            // Log the error
            _logger?.LogError(ex, $"Error getting completion from {_aiProvider.Name} (Game ID: {_gameInstanceId}): {ex.Message}");

            throw; // Rethrow to let the caller handle it
        }
        finally
        {
            // Log the entire interaction, regardless of success or failure
            try
            {
                await _logManager.LogApiInteractionAsync(
                    conversationId,
                    messages,
                    requestBody,
                    jsonResponse,
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

    public string GetProviderName()
    {
        return _aiProvider.Name;
    }

    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}