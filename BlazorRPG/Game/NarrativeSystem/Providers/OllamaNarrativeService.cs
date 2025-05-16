public class OllamaNarrativeService : IAIService
{
    private readonly AIClient _aiClient;
    private readonly PromptManager _promptManager;
    public PostEncounterEvolutionParser PostEncounterEvolutionParser { get; }
    public NarrativeContextManager _contextManager { get; }
    public NarrativeLogManager NarrativeLogManager { get; }
    public IResponseStreamWatcher Watcher { get; }
    public IConfiguration Configuration { get; }

    private readonly string _primaryModel;
    private readonly string _fallbackModel;

    public OllamaNarrativeService(
        PostEncounterEvolutionParser postEncounterEvolutionParser,
        NarrativeContextManager narrativeContextManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger,
        NarrativeLogManager narrativeLogManager,
        LoadingStateService loadingStateService,
        IResponseStreamWatcher watcher)
    {
        // Create the AIClient directly with the Ollama provider
        string gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        IAIProvider ollamaProvider = new OllamaProvider(configuration, logger);
        _aiClient = new AIClient(ollamaProvider, gameInstanceId, logger, narrativeLogManager, loadingStateService);

        _promptManager = NarrativeServiceUtils.CreatePromptManager(configuration);
        PostEncounterEvolutionParser = postEncounterEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Watcher = watcher;
        Configuration = configuration;

        // Get model names from configuration
        _primaryModel = configuration.GetValue<string>("Ollama:Model") ?? "gemma3:12b-it-qat";
        _fallbackModel = configuration.GetValue<string>("Ollama:BackupModel") ?? "gemma3:2b-it";
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
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("introductionLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_HIGH for player-facing content
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "IntroductionGeneration");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("choicesLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_HIGH for player-facing content
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "ChoiceGeneration");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(response, choices);
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("reactionLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_HIGH for player-facing content
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "EncounterNarrative");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Narrative);
        return response;
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("endingLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_HIGH for player-facing content
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            priority, "EncounterEnding");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Narrative);
        return response;
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("locationLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_NORMAL for location generation
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            messages,
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_NORMAL, "LocationGeneration");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        // Parse the JSON response into location details
        return LocationJsonParser.ParseLocationDetails(response);
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("worldLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_LOW for post-encounter evolution
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_LOW, "PostEncounterEvolution");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        PostEncounterEvolutionResult postEncounterEvolutionResponse = await PostEncounterEvolutionParser.ParsePostEncounterEvolutionResponseAsync(response);
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("memoryLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_LOW for memory consolidation
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_LOW, "MemoryConsolidation");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        return response;
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("actionsLow"))
        {
            model = _fallbackModel;
        }

        // Using PRIORITY_NORMAL for action generation
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            messages,
            model, fallbackModel, Watcher,
            AIClient.PRIORITY_NORMAL, "ActionGeneration");

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);

        return response;
    }

}
