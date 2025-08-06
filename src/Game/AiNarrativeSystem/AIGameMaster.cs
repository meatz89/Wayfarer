public class AIGameMaster : INarrativeProvider
{
    private AIPromptBuilder _promptBuilder;
    private ConversationHistoryManager _contextManager;
    private ConversationChoiceResponseParser _conversationChoiceResponseParser;
    private AIClient _aiClient;
    private GameWorld _gameWorld;
    private bool hasResponse;
    private WorldStateInputBuilder _worldStateInputBuilder;

    public AIGameMaster(
        ConversationHistoryManager contextManager,
        ConversationChoiceResponseParser conversationChoiceResponseParser,
        AIClient aiClient,
        IConfiguration configuration,
        GameWorld gameWorld,
        WorldStateInputBuilder worldStateInputBuilder)
    {
        _contextManager = contextManager;
        _conversationChoiceResponseParser = conversationChoiceResponseParser;
        _gameWorld = gameWorld;
        _promptBuilder = new AIPromptBuilder(configuration);
        hasResponse = false;
        _aiClient = aiClient;
        _worldStateInputBuilder = worldStateInputBuilder;
    }

    public bool HasResponse => hasResponse;

    private static string GetConversationId(SceneContext context)
    {
        return $"{context.LocationName}_encounter";
    }

    public async Task<bool> CanReceiveRequests()
    {
        return await _aiClient.CanReceiveRequests();
    }

    // INarrativeProvider implementation
    public async Task<bool> IsAvailable()
    {
        return await CanReceiveRequests();
    }

    // INarrativeProvider implementation
    public async Task<string> GenerateIntroduction(SceneContext context, ConversationState state)
    {
        // Create world state input
        WorldStateInput worldStateInput = _worldStateInputBuilder.CreateWorldStateInput(context.LocationName, context.Player);
        return await GenerateIntroduction(context, state, worldStateInput, 1);
    }

    // Original method with full parameters
    public async Task<string> GenerateIntroduction(
        SceneContext context,
        ConversationState state,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        Guid gameInstanceId = context.GameWorld.GameInstanceId;
        MemoryFileAccess memoryFileAccess = new MemoryFileAccess(gameInstanceId);
        List<string> memoryContent = await memoryFileAccess.GetAllMemories();
        string memory = string.Join("\n", memoryContent.Where(x => !string.IsNullOrWhiteSpace(x)).Take(5));

        AIPrompt prompt = _promptBuilder.BuildIntroductionPrompt(context, state, memory);
        MessageType messageType = MessageType.Introduction;

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        }

        List<IResponseStreamWatcher> watchers =
        [
            new StreamingContentStateWatcher(_gameWorld.StreamingContentState)
        ];

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    // INarrativeProvider implementation
    public async Task<List<ConversationChoice>> GenerateChoices(
        SceneContext context,
        ConversationState state,
        List<ChoiceTemplate> availableTemplates)
    {
        WorldStateInput worldStateInput = _worldStateInputBuilder.CreateWorldStateInput(context.LocationName, context.Player);
        return await RequestChoices(context, state, worldStateInput, availableTemplates, 1);
    }

    public async Task<List<ConversationChoice>> RequestChoices(
        SceneContext context,
        ConversationState state,
        WorldStateInput worldStateInput,
        List<ChoiceTemplate> allTemplates,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder.BuildChoicesPrompt(context, state, allTemplates);
        MessageType messageType = MessageType.ChoicesGeneration;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.ChoicesGeneration);

        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        List<ConversationChoice> choices = _conversationChoiceResponseParser.ParseMultipleChoicesResponse(response);
        return choices;
    }

    // INarrativeProvider implementation
    public async Task<string> GenerateReaction(
        SceneContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        WorldStateInput worldStateInput = _worldStateInputBuilder.CreateWorldStateInput(context.LocationName, context.Player);
        return await GenerateReaction(context, state, selectedChoice, success, worldStateInput, 1);
    }

    public async Task<string> GenerateReaction(
        SceneContext context,
        ConversationState state,
        ConversationChoice chosenOption,
        bool choiceSuccess,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder.BuildReactionPrompt(context, state, chosenOption);
        MessageType messageType = MessageType.Reaction;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        _contextManager.AddUserChoiceSelectionMessage(conversationId, prompt.Content, chosenOption.NarrativeText);

        List<IResponseStreamWatcher> watchers =
        [
            new StreamingContentStateWatcher(_gameWorld.StreamingContentState)
        ];

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    // INarrativeProvider implementation
    public async Task<string> GenerateConclusion(
        SceneContext context,
        ConversationState state,
        ConversationChoice lastChoice)
    {
        WorldStateInput worldStateInput = _worldStateInputBuilder.CreateWorldStateInput(context.LocationName, context.Player);
        return await GenerateConclusion(context, state, lastChoice, worldStateInput, 1);
    }

    public async Task<string> GenerateConclusion(
        SceneContext context,
        ConversationState state,
        ConversationChoice finalChoice,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder
            .BuildConversationConclusionPrompt(context, state, state.Outcome, finalChoice);
        MessageType messageType = MessageType.Conclusion;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice);

        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public async Task<LocationDetails> GenerateLocationDetails(
        SceneContext context,
        ConversationState state,
        ConversationChoice chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder.BuildLocationCreationPrompt(new LocationCreationInput());
        MessageType messageType = MessageType.LocationCreation;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        string choiceDescription = chosenOption.NarrativeText;
        _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice);

        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return new LocationDetails
        {
            Description = response,
            Name = context.LocationName,
        };
    }

    public async Task<PostConversationEvolutionResult> ProcessPostConversationEvolution(
        SceneContext context,
        ConversationState state,
        ConversationChoice chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder.BuildPostConversationEvolutionPrompt(new PostConversationEvolutionInput());
        MessageType messageType = MessageType.PostConversationEvolution;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        string choiceDescription = chosenOption.NarrativeText;
        _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice);

        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return new PostConversationEvolutionResult
        {
        };
    }
    // Action generation removed - using location actions and conversations

    public async Task<string> ProcessMemoryConsolidation(
        SceneContext context,
        ConversationState state,
        ConversationChoice chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        Guid gameInstanceId = context.GameWorld.GameInstanceId;
        MemoryFileAccess memoryFileAccess = new MemoryFileAccess(gameInstanceId);
        List<string> memoryContent = await memoryFileAccess.GetAllMemories();
        string oldMemory = string.Join("\n", memoryContent.Where(x => !string.IsNullOrWhiteSpace(x)).Take(5));
        MemoryConsolidationInput input = new()
        {
            OldMemory = oldMemory
        };

        AIPrompt prompt = _promptBuilder.BuildMemoryPrompt(input);
        MessageType messageType = MessageType.MemoryUpdate;

        _contextManager.UpdateSystemMessage(conversationId, systemMessage);
        string choiceDescription = chosenOption.NarrativeText;
        _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice);

        List<IResponseStreamWatcher> watchers = new List<IResponseStreamWatcher>();

        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            watchers,
            _contextManager.GetConversationHistory(conversationId),
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public string GetProviderName()
    {
        return "gemma3-12b"; // This should match the model name used in AIClient
    }
}
