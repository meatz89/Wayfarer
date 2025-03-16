public class GPTNarrativeService : INarrativeAIService
{
    private readonly AIClientService _aiClient;
    private readonly PromptManager _promptManager;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<GPTNarrativeService> _logger;
    private readonly string _gameInstanceId;

    public GPTNarrativeService(IConfiguration configuration, ILogger<GPTNarrativeService> logger = null)
    {
        // Generate a game instance ID
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _aiClient = new AIClientService(configuration, _gameInstanceId);
        _promptManager = new PromptManager(configuration);
        _contextManager = new NarrativeContextManager();
        _logger = logger;

        _logger?.LogInformation($"Initialized GPTNarrativeService with game instance ID: {_gameInstanceId}");
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Get system message and introduction prompt
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(location, incitingAction, state);

        // Store conversation context
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history
        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = $"{context.LocationName}_narrative";

        // Get system message and narrative prompt
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildJsonNarrativePrompt(
            context, chosenOption, choiceDescription, outcome, newState);

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history
        _contextManager.AddAssistantMessage(conversationId, narrativeResponse);

        // Create and store the narrative event
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            turnNumber: context.Events.Count + 1,
            sceneDescription: narrativeResponse);

        narrativeEvent.SetChosenOption(chosenOption);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);

        // Add the event to the context
        context.AddEvent(narrativeEvent);

        return narrativeResponse;
    }

    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_choices";
        string systemMessage = _promptManager.GetSystemMessage();

        // Pass the most recent narrative explicitly tothe prompt builder
        string prompt = _promptManager.BuildJsonChoicesPrompt(
            context,
            choices,
            projections,
            state
            );  

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history
        _contextManager.AddAssistantMessage(conversationId, jsonResponse);

        // Parse the JSON response into choice narratives - removed projections since we're not enhancing
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }

    
    // Method to get the current game instance ID
    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}
