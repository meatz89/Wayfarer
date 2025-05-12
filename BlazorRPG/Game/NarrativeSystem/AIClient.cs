public class AIClient
{
    private readonly IAIProvider _aiProvider;
    private readonly string _gameInstanceId;
    private readonly NarrativeLogManager _logManager;

    public AIClient(
        IAIProvider aiProvider,
        string gameInstanceId,
        NarrativeLogManager logManager)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _gameInstanceId = gameInstanceId;
        this._logManager = logManager;
    }

    public async Task<string> GetCompletionAsync(
        IEnumerable<ConversationEntry> conversationMessages, 
        string model, 
        string fallbackModel,
        IResponseStreamWatcher watcher)
    {
        List<ConversationEntry> messages = conversationMessages.Select(conversationMessage =>
        {
            return new ConversationEntry
            {
                Role = conversationMessage.Role.ToLower(), // Ensure role is lowercase (system, user, assistant)
                Content = conversationMessage.Content
            };
        }).ToList();

        return await GetCompletionAsync(messages, model, fallbackModel, watcher);
    }

    public async Task<string> GetCompletionAsync(
        List<ConversationEntry> messages, 
        string model, 
        string fallbackModel,
        IResponseStreamWatcher watcher)
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

            // The actual API call is delegated to the provider implementation
            string result = await _aiProvider.GetCompletionAsync(
                messages, 
                model, 
                fallbackModel,
                watcher);

            // Trim any potential whitespace
            result = result?.Trim();
            generatedContent = result;


            return result;
        }
        catch (Exception ex)
        {
            // Capture error details
            errorMessage = $"Error: {ex.Message}\nStack Trace: {ex.StackTrace}";

            throw; // Rethrow to let the caller handle it
        }
        finally
        {
            // Log the entire interaction, regardless of success or failure
            await _logManager.LogApiInteractionAsync(
                conversationId,
                messages,
                requestBody,
                jsonResponse,
                generatedContent,
                errorMessage
            );
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