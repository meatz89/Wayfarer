public class Gemma3NarrativeService : BaseNarrativeAIService
{
    public Gemma3NarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
        : base(new Gemma3Provider(configuration, logger), configuration, logger)
    {
    }

    // Implementation identical to GPTNarrativeService but with appropriate Gemma-specific tweaks if needed
    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, string incitingAction, EncounterStatus state)
    {
        // Same implementation as GPTNarrativeService
        string conversationId = $"{context.LocationName}_{DateTime.Now.Ticks}";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, incitingAction, state);
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        string jsonResponse = await _aiClient.GetCompletionAsync(_contextManager.GetConversationHistory(conversationId));
        _contextManager.AddAssistantMessage(conversationId, jsonResponse);
        return jsonResponse;
    }

    public override async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        // Same implementation as GPTNarrativeService
        string conversationId = $"{context.LocationName}_narrative";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildNarrativePrompt(context, chosenOption, choiceDescription, outcome, newState);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(_contextManager.GetConversationHistory(conversationId));
        _contextManager.AddAssistantMessage(conversationId, jsonResponse);

        NarrativeEvent narrativeEvent = new NarrativeEvent(
            turnNumber: context.Events.Count + 1,
            sceneDescription: jsonResponse);

        narrativeEvent.SetChosenOption(chosenOption);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);

        context.AddEvent(narrativeEvent);

        return jsonResponse;
    }

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        // Same implementation as GPTNarrativeService
        string conversationId = $"{context.LocationName}_choices";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildChoicesPrompt(context, choices, projections, state);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(_contextManager.GetConversationHistory(conversationId));
        _contextManager.AddAssistantMessage(conversationId, jsonResponse);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }
}