public class ClaudeNarrativeService : IAIService
{
    private readonly AIClient _aiClient;
    private readonly PromptManager _promptManager;
    public PostEncounterEvolutionParser PostEncounterEvolutionParser { get; }
    public NarrativeContextManager _contextManager { get; }
    public NarrativeLogManager NarrativeLogManager { get; }
    public IConfiguration Configuration { get; }
    private IResponseStreamWatcher Watcher { get; }

    private readonly string _modelHigh;
    private readonly string _modelLow;

    public ClaudeNarrativeService(
        PostEncounterEvolutionParser postEncounterEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager,
        LoadingStateService loadingStateService,
        IResponseStreamWatcher watcher)
    {
        string gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        IAIProvider claudeProvider = new ClaudeProvider(configuration, logger);
        _aiClient = new AIClient(claudeProvider, gameInstanceId, logger, narrativeLogManager, loadingStateService);

        _promptManager = NarrativeServiceUtils.CreatePromptManager(configuration);
        PostEncounterEvolutionParser = postEncounterEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Watcher = watcher;
        Configuration = configuration;
        _modelHigh = configuration.GetValue<string>("Anthropic:Model") ?? "claude-3-7-sonnet-latest";
        _modelLow = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";
    }

    public string GetProviderName()
    {
        return _aiClient.GetProviderName();
    }

    public string GetGameInstanceId()
    {
        return _aiClient.GetGameInstanceId();
    }

    public async Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string model = _modelHigh;
        string fallbackModel = _modelLow;
        if (Configuration.GetValue<bool>("introductionLow"))
        {
            model = _modelLow;
        }

        // Use high priority for introduction
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "IntroductionGeneration");

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Introduction);

        return response;
    }

    public async Task<Dictionary<NarrativeChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<NarrativeChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput,
        int priority)
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

        // Use high priority for choices
        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "ChoiceGeneration");

        _contextManager.AddAssistantMessage(conversationId, jsonResponse, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }


    public async Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        NarrativeChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput,
        int priority)
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

        // Using PRIORITY_HIGH for player-facing content
        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "EncounterNarrative");

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        NarrativeChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput,
        int priority)
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

        // Using PRIORITY_HIGH for player-facing content
        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "EncounterEnding");

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public async Task<LocationDetails> GenerateLocationDetailsAsync(
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

        // Using PRIORITY_NORMAL for location details
        string jsonResponse = await _aiClient.GetCompletionAsync(
            messages,
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_NORMAL, "LocationGeneration");

        // Parse the JSON response into location details
        return LocationJsonParser.ParseLocationDetails(jsonResponse);
    }

    public async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
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

        // Using PRIORITY_LOW for post-encounter evolution
        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_LOW, "PostEncounterEvolution");

        PostEncounterEvolutionResult postEncounterEvolutionResponse = await PostEncounterEvolutionParser.ParsePostEncounterEvolutionResponseAsync(jsonResponse);
        return postEncounterEvolutionResponse;
    }

    public async Task<string> ProcessMemoryConsolidation(
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

        // Using PRIORITY_LOW for memory consolidation
        string memoryContentResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_LOW, "MemoryConsolidation");

        return memoryContentResponse;
    }

    public async Task<string> GenerateActionsAsync(
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

        // Using PRIORITY_NORMAL for action generation
        string jsonResponse = await _aiClient.GetCompletionAsync(
            messages,
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_NORMAL, "ActionGeneration");

        return jsonResponse;
    }
}