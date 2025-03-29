public class ClaudeNarrativeService : BaseNarrativeAIService
{
    public WorldEvolutionParser WorldEvolutionParser { get; }
    public NarrativeContextManager _contextManager { get; }
    public NarrativeLogManager NarrativeLogManager { get; }
    public IConfiguration Configuration { get; }

    private readonly string _modelHigh;
    private readonly string _modelLow;

    public ClaudeNarrativeService(
        WorldEvolutionParser worldEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager
        )
        : base(new ClaudeProvider(configuration, logger), configuration, logger, narrativeLogManager)
    {
        WorldEvolutionParser = worldEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Configuration = configuration;
        _modelHigh = configuration.GetValue<string>("Anthropic:Model") ?? "claude-3-7-sonnet-latest";
        _modelLow = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";

    }

    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, EncounterStatusModel state, string memoryContent)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage();
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


    public override async Task<LocationDetails> GenerateLocationDetailsAsync(LocationGenerationContext context)
    {
        string conversationId = $"location_generation_{context.LocationType}"; // Unique conversation ID
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildLocationGenerationPrompt(context);

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

    public override async Task<string> GenerateActionsAsync(ActionGenerationContext context)
    {
        string conversationId = $"action_generation_{context.SpotName}"; // Unique conversation ID
        string systemMessage = _promptManager.GetSystemMessage();
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

    public override async Task<WorldEvolutionResponse> ProcessWorldEvolution(
        NarrativeContext context,
        WorldEvolutionInput input)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();

        string prompt = _promptManager.BuildWorldEvolutionPrompt(input);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.WorldEvolution, null);
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

        WorldEvolutionResponse worldEvolutionResponse = WorldEvolutionParser.ParseWorldEvolutionResponse(jsonResponse);
        return worldEvolutionResponse;
    }

    public override async Task<string> ProcessMemoryConsolidation(
        NarrativeContext context,
        MemoryConsolidationInput input)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();

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
}

