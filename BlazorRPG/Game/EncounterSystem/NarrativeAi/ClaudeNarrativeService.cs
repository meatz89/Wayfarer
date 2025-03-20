public class ClaudeNarrativeService : BaseNarrativeAIService
{
    private readonly NarrativeContextManager _contextManager;

    public ClaudeNarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
        : base(new ClaudeProvider(configuration, logger), configuration, logger)
    {
        _contextManager = new NarrativeContextManager();
    }

    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, EncounterStatusModel state, string memoryContent)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Introduction);

        return response;
    }

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state)
    {
        string conversationId = $"{context.LocationName}_encounter";
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
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.ChoiceGeneration, null);
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, jsonResponse, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }

    public override async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildReactionPrompt(
            context, chosenOption, choiceNarrative, outcome, newState);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.PlayerChoice, choiceNarrative);
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildEncounterEndPrompt(
            context, newState, outcome.Outcome, chosenOption, choiceNarrative);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.PlayerChoice, choiceNarrative);
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateMemoryFileAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        string oldMemory)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();

        string prompt = _promptManager.BuildMemoryPrompt(
            context, outcome, newState, oldMemory);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.MemoryUpdate, null);
        }
        string memoryContentResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        return memoryContentResponse;
    }

}