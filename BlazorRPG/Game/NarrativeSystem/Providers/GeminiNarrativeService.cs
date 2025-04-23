public class GeminiNarrativeService : BaseNarrativeAIService
{
    public PostEncounterEvolutionParser PostEncounterEvolutionParser { get; }
    public NarrativeContextManager _contextManager { get; }
    public NarrativeLogManager NarrativeLogManager { get; }
    public IConfiguration Configuration { get; }

    private readonly string _model = "gemini-2.5-flash"; // Default model

    public GeminiNarrativeService(
        PostEncounterEvolutionParser postEncounterEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager
        )
        : base(new GeminiProvider(configuration, logger), configuration, logger, narrativeLogManager)
    {
        PostEncounterEvolutionParser = postEncounterEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Configuration = configuration;

        // Allow model override from config - default to 2.5 Flash
        _model = configuration.GetValue<string>("Google:Model") ?? "gemini-2.5-flash";
    }

    public override async Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini"; // Consistent ID with marker
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model); // Same model for both - optimizing for speed

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Introduction);

        return response;
    }

    public override async Task<Dictionary<CardDefinition, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<CardDefinition> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
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
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model);

        _contextManager.AddAssistantMessage(conversationId, jsonResponse, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }

    public override async Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
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
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model);

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        CardDefinition chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
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
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model);

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<LocationDetails> GenerateLocationDetailsAsync(
        LocationCreationInput context,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildLocationCreationPrompt(context);

        ConversationEntry entrySystem = new ConversationEntry { Role = "system", Content = systemMessage };
        ConversationEntry entryUser = new ConversationEntry { Role = "user", Content = prompt };

        List<ConversationEntry> messages = [entrySystem, entryUser];

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            _model, _model);

        return LocationJsonParser.ParseLocationDetails(jsonResponse);
    }

    public override async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildPostEncounterEvolutionPrompt(input);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.PostEncounterEvolution, null);
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model);

        PostEncounterEvolutionResult postEncounterEvolutionResponse = await PostEncounterEvolutionParser.ParsePostEncounterEvolutionResponseAsync(jsonResponse);
        return postEncounterEvolutionResponse;
    }

    public override async Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildMemoryPrompt(input);

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
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _model, _model);

        return memoryContentResponse;
    }

    public override async Task<string> GenerateActionsAsync(
        ActionGenerationContext context,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"action_generation_{context.SpotName}_gemini";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildActionGenerationPrompt(context);

        ConversationEntry entrySystem = new ConversationEntry { Role = "system", Content = systemMessage };
        ConversationEntry entryUser = new ConversationEntry { Role = "user", Content = prompt };

        List<ConversationEntry> messages = [entrySystem, entryUser];

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            _model, _model);

        return jsonResponse;
    }
}