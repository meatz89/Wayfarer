public class ClaudeNarrativeService : BaseNarrativeAIService
{
    public ClaudeNarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
        : base(new ClaudeProvider(configuration, logger), configuration, logger)
    {
    }

    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, state);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public override async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildReactionPrompt(
            context, chosenOption, choiceDescription, outcome, newState);
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }
        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));
        _contextManager.AddAssistantMessage(conversationId, narrativeResponse);
        return narrativeResponse;
    }

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID for whole encounter
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildChoicesPrompt(
            context, choices, projections, state);
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }
        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));
        _contextManager.AddAssistantMessage(conversationId, jsonResponse);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }
}