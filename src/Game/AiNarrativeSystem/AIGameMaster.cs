public class AIGameMaster
{
    private AIPromptBuilder _promptBuilder;
    private ConversationHistoryManager _contextManager;
    private EncounterChoiceResponseParser _encounterChoiceResponseParser;
    private AIClient _aiClient;
    private GameWorld _gameWorld;
    private bool hasResponse;

    public AIGameMaster(
        ConversationHistoryManager contextManager,
        EncounterChoiceResponseParser EncounterChoiceResponseParser,
        AIClient aiClient,
        IConfiguration configuration,
        GameWorld gameWorld)
    {
        _contextManager = contextManager;
        _encounterChoiceResponseParser = EncounterChoiceResponseParser;
        _gameWorld = gameWorld;
        _promptBuilder = new AIPromptBuilder(configuration);
        hasResponse = false;
        _aiClient = aiClient;
    }

    public bool HasResponse
    {
        get
        {
            return hasResponse;
        }
    }

    private static string GetConversationId(EncounterContext context)
    {
        return $"{context.LocationName}_encounter";
    }

    public async Task<bool> CanReceiveRequests()
    {
        return await _aiClient.CanReceiveRequests();
    }

    public async Task<string> GenerateIntroduction(
        EncounterContext context,
        EncounterState state,
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

    public async Task<List<EncounterChoice>> RequestChoices(
        EncounterContext context,
        EncounterState state,
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

        List<EncounterChoice> choices = _encounterChoiceResponseParser.ParseMultipleChoicesResponse(response);
        return choices;
    }

    public async Task<string> GenerateReaction(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
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

    public async Task<string> GenerateConclusion(
        EncounterContext context,
        EncounterState state,
        EncounterChoice finalChoice,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder
            .BuildEncounterConclusionPrompt(context, state, state.EncounterOutcome, finalChoice);
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
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
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

    public async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        AIPrompt prompt = _promptBuilder.BuildPostEncounterEvolutionPrompt(new PostEncounterEvolutionInput());
        MessageType messageType = MessageType.PostEncounterEvolution;

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

        return new PostEncounterEvolutionResult
        {
        };
    }
    public async Task<string> GenerateActions(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = GetConversationId(context);
        string systemMessage = _promptBuilder.GetSystemMessage(worldStateInput);

        ActionGenerationContext actionContext = new ActionGenerationContext();

        AIPrompt prompt = _promptBuilder.BuildActionGenerationPrompt(actionContext);
        MessageType messageType = MessageType.ActionGeneration;

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

    public async Task<string> ProcessMemoryConsolidation(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
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
