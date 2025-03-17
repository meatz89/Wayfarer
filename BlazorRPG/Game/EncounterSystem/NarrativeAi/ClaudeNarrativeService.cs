public class ClaudeNarrativeService : BaseNarrativeAIService
{
    public ClaudeNarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
        : base(new ClaudeProvider(configuration, logger), configuration, logger)
    {
    }
    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_{DateTime.Now.Ticks}";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, incitingAction, state);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    // Implement the remaining methods identically to GPTNarrativeService
    // (GenerateReactionAndSceneAsync and GenerateChoiceDescriptionsAsync)
    // since the implementation logic is the same across providers
    public override async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = $"{context.LocationName}_narrative";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildNarrativePrompt(
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

        NarrativeEvent narrativeEvent = new NarrativeEvent(
            turnNumber: context.Events.Count + 1,
            sceneDescription: narrativeResponse);

        narrativeEvent.SetChosenOption(chosenOption);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);

        context.AddEvent(narrativeEvent);

        return narrativeResponse;
    }

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_choices";
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