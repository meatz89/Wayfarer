public class ClaudeNarrativeService : BaseNarrativeAIService
{
    public PostEncounterEvolutionParser PostEncounterEvolutionParser { get; }
    public NarrativeContextManager _contextManager { get; }
    public NarrativeLogManager NarrativeLogManager { get; }
    public IConfiguration Configuration { get; }

    private readonly string _modelHigh;
    private readonly string _modelLow;

    public ClaudeNarrativeService(
        PostEncounterEvolutionParser postEncounterEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager
        )
        : base(new ClaudeProvider(configuration, logger), configuration, logger, narrativeLogManager)
    {
        PostEncounterEvolutionParser = postEncounterEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Configuration = configuration;
        _modelHigh = configuration.GetValue<string>("Anthropic:Model") ?? "claude-3-7-sonnet-latest";
        _modelLow = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";

    }

    public override async Task<string> GenerateIntroductionAsync(
        NarrativeContext context, 
        EncounterStatusModel state, 
        string memoryContent,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("introductionLow"))
        {
            model = _modelLow;
        }

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Introduction);

        return response;
    }

    public override async Task<Dictionary<ChoiceCard, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<ChoiceCard> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter";
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

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("choicesLow"))
        {
            model = _modelLow;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        _contextManager.AddAssistantMessage(conversationId, jsonResponse, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }

    public override async Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        ChoiceCard chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter";
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

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("reactionLow"))
        {
            model = _modelLow;
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        ChoiceCard chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter";
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

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("endingLow"))
        {
            model = _modelLow;
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }


    public override async Task<LocationDetails> GenerateLocationDetailsAsync(
        LocationCreationInput context,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter"; 
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildLocationCreationPrompt(context);

        ConversationEntry entrySystem = new ConversationEntry { Role = "system", Content = systemMessage };
        ConversationEntry entryUser = new ConversationEntry { Role = "user", Content = prompt };

        List<ConversationEntry> messages = [entrySystem, entryUser];

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("locationLow"))
        {
            model = _modelLow;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            model, fallbackModel);

        // Parse the JSON response into location details
        return LocationJsonParser.ParseLocationDetails(jsonResponse);
    }


    public override async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context,
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter"; 
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

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("worldLow"))
        {
            model = _modelLow;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        PostEncounterEvolutionResult postEncounterEvolutionResponse = await PostEncounterEvolutionParser.ParsePostEncounterEvolutionResponseAsync(jsonResponse);
        return postEncounterEvolutionResponse;
    }

    public override async Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"{context.LocationName}_encounter";
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

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("memoryLow"))
        {
            model = _modelLow;
        }

        string memoryContentResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel);

        return memoryContentResponse;
    }

    public override async Task<string> GenerateActionsAsync(
        ActionGenerationContext context,
        WorldStateInput worldStateInput)
    {
        string conversationId = $"action_generation_{context.SpotName}";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildActionGenerationPrompt(context);

        ConversationEntry entrySystem = new ConversationEntry { Role = "system", Content = systemMessage };
        ConversationEntry entryUser = new ConversationEntry { Role = "user", Content = prompt };

        List<ConversationEntry> messages = [entrySystem, entryUser];

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("actionsLow"))
        {
            model = _modelLow;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            model, fallbackModel);

        return jsonResponse;
    }

}