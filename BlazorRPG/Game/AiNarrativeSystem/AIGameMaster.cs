public class AIGameMaster : IAIService
{
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
        AIPrompt prompt = _promptManager.BuildChoicesPrompt(context, state);

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
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildReactionPrompt(context, state);

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
        AIPrompt prompt = _promptManager.BuildInitialPrompt(context, state);

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

    public async Task<string> GenerateEnding(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildEncounterEndPrompt(context, state);

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
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildLocationCreationPrompt(context, state);

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

        MessageType messageType = MessageType.LocationCreation;
        AIGenerationCommand aiGenerationCommand = await _aiClient.CreateAndQueueCommand(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            _watcher,
            priority,
            messageType.ToString());

        string response = await _aiClient.ProcessCommand(aiGenerationCommand);
        _contextManager.AddAssistantMessage(conversationId, response, messageType);

        return response;
    }

    public async Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildPostEncounterEvolutionPrompt(context, state);

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

    public async Task<string> ProcessMemoryConsolidation(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildMemoryPrompt(context, state);

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
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage(worldStateInput);
        AIPrompt prompt = _promptManager.BuildActionGenerationPrompt(context, state);

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

    internal EncounterResult GenerateConclusion(EncounterState state, EncounterContext context)
    {
        throw new NotImplementedException();
    }
}
