public class GPTNarrativeService : BaseNarrativeAIService
{
    public GPTNarrativeService(IConfiguration configuration, ILogger<GPTNarrativeService> logger = null)
        : base(new OpenAIProvider(configuration), configuration, logger)
    {
    }

    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_{DateTime.Now.Ticks}";

        // Get system message and introduction prompt
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, incitingAction, state);

        // Store conversation context
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history
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
        string conversationId = $"{context.LocationName}_narrative";

        // Get system message and narrative prompt
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildNarrativePrompt(
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

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = $"{context.LocationName}_choices";
        string systemMessage = _promptManager.GetSystemMessage();

        // Pass the most recent narrative explicitly to the prompt builder
        string prompt = _promptManager.BuildChoicesPrompt(
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

        // Parse the JSON response into choice narratives
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }
}
