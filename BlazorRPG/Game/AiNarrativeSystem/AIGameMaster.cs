public class AIGameMaster : IAIService
{
    private AIResponse pendingResponse;
    private bool hasResponse;
    public bool HasResponse
    {
        get
        {
            return hasResponse;
        }
    }

    private AIPromptBuilder _promptManager;
    private EncounterContextManager _contextManager;
    private EncounterChoiceResponseParser _EncounterChoiceResponseParser;
    private AIClient _aiClient;
    private IResponseStreamWatcher _watcher;

    public AIGameMaster(
        EncounterContextManager contextManager,
        EncounterChoiceResponseParser EncounterChoiceResponseParser,
        AIClient aiClient,
        IConfiguration configuration,
        IResponseStreamWatcher responseStreamWatcher)
    {
        this._contextManager = contextManager;
        this._EncounterChoiceResponseParser = EncounterChoiceResponseParser;
        this._aiClient = aiClient;
        this._watcher = responseStreamWatcher;
        this._promptManager = new AIPromptBuilder(configuration);

        pendingResponse = null;
        hasResponse = false;
    }

    public async Task<List<EncounterChoice>> GenerateChoices(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager
            .BuildChoicesPrompt(context, state, worldStateInput);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.Choice.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.ChoicesGeneration;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        List<EncounterChoice> choices = _EncounterChoiceResponseParser.ParseMultipleChoicesResponse(response);

        return choices;
    }

    public async Task<string> GenerateReaction(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager
            .BuildReactionPrompt(context, state, chosenOption, outcome);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.Reaction;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public async Task<string> GenerateIntroduction(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);

        string memory = await MemoryFileAccess.ReadFromMemoryFile();

        AIPrompt prompt = _promptManager.BuildInitialPrompt(context, state, memory);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.Choice.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.Introduction;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public async Task<string> GenerateConclusion(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager
            .BuildEncounterEndPrompt(context, state.EncounterOutcome, chosenOption);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.Reaction;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
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
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager
            .BuildLocationCreationPrompt(new LocationCreationInput());

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.LocationCreation;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
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
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildPostEncounterEvolutionPrompt(
            new PostEncounterEvolutionInput());

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.PostEncounterEvolution;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return new PostEncounterEvolutionResult
        {
        };
    }

    public async Task<string> ProcessMemoryConsolidation(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();
        AIPrompt prompt = _promptManager.BuildMemoryPrompt(
            new MemoryConsolidationInput
            {
                OldMemory = oldMemory
            });

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.MemoryUpdate;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public async Task<string> GenerateActions(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        ActionGenerationContext context1 = new();
        AIPrompt prompt = _promptManager.BuildActionGenerationPrompt(context1);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt.Content);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            string choiceDescription = chosenOption.NarrativeText;
            _contextManager.AddUserMessage(conversationId, prompt.Content, MessageType.PlayerChoice, choiceDescription);
        }

        MessageType messageType = MessageType.PostEncounterEvolution;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
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
