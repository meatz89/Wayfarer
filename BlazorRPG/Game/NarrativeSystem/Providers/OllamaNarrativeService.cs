public class OllamaNarrativeService : BaseNarrativeAIService
{
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
        IResponseStreamWatcher watcher)
        : base(new OllamaProvider(configuration, logger), configuration, narrativeLogManager)
    {
        PostEncounterEvolutionParser = postEncounterEvolutionParser;
        _contextManager = narrativeContextManager;
        NarrativeLogManager = narrativeLogManager;
        Watcher = watcher;
        Configuration = configuration;

        // Get model names from configuration
        _primaryModel = configuration.GetValue<string>("Ollama:Model") ?? "gemma3:12b-it-qat";
        _fallbackModel = configuration.GetValue<string>("Ollama:BackupModel") ?? "gemma3:2b-it";
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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("introductionLow"))
        {
            model = _fallbackModel;
        }

        // Pass the response watcher for streaming if available
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("locationLow"))
        {
            model = _fallbackModel;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            model, fallbackModel, Watcher);

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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("worldLow"))
        {
            model = _fallbackModel;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("memoryLow"))
        {
            model = _fallbackModel;
        }

        string memoryContentResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId),
            model, fallbackModel, Watcher);

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

        string model = _primaryModel;
        string fallbackModel = _fallbackModel;
        if (Configuration.GetValue<bool>("actionsLow"))
        {
            model = _fallbackModel;
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(messages,
            model, fallbackModel, Watcher);

        return jsonResponse;
    }

    // Character generation method specifically for Ollama
    public async Task<NpcCharacter> GenerateCharacterAsync(
        CharacterGenerationRequest request,
        IResponseStreamWatcher watcher)
    {
        string conversationId = $"character_generation_{Guid.NewGuid()}";

        List<ConversationEntry> messages = new List<ConversationEntry>();

        // System message with instructions
        messages.Add(new ConversationEntry
        {
            Role = "system",
            Content = @"You are a medieval character generator for the text-based RPG 'Wayfarer'. 
Generate a detailed medieval character following the narrative style principles:
- Write with measured elegance, focusing on ordinary moments and intimate details
- Create characters that feel flesh and blood real with private hopes and quiet sorrows
- Include background that shapes who they are, revealed through subtle details
- Focus on intimate conflicts: relationships, unfulfilled dreams, daily bread, personal honor
- Be historically authentic for medieval life without fantasy elements

Respond ONLY with a JSON object matching this exact structure:
{
  ""name"": ""[Character's full name]"",
  ""age"": [age as integer],
  ""gender"": ""[male or female]"",
  ""occupation"": ""[Primary occupation]"",
  ""appearance"": ""[Brief physical description, 2-3 sentences]"",
  ""background"": ""[Life history and key events, 3-5 sentences]"",
  ""personality"": ""[Core traits and behaviors, 2-3 sentences]"",
  ""motivation"": ""[What drives this character, 1-2 sentences]"",
  ""quirk"": ""[A distinctive habit or trait, 1 sentence]"",
  ""secret"": ""[Something this person doesn't want others to know, 1-2 sentences]"",
  ""possessions"": [Array of 3-5 notable items they own, as strings],
  ""skills"": [Array of 2-4 things they're good at, as strings],
  ""relationships"": [Array of 2-3 important connections to other people, as strings]
}

Your response must be ONLY this JSON with no other text, headers, or explanations."
        });

        // User message with specific request
        messages.Add(new ConversationEntry
        {
            Role = "user",
            Content = $"Generate a {request.Archetype} character from {request.Region} with the following specifications:\n" +
                    $"Gender: {(string.IsNullOrEmpty(request.Gender) ? "any" : request.Gender)}\n" +
                    $"Age range: {request.MinAge}-{request.MaxAge}\n" +
                    $"Additional traits: {request.AdditionalTraits}"
        });

        // Use the primary model for character generation
        string response = await _aiClient.GetCompletionAsync(
            messages,
            _primaryModel,
            _fallbackModel,
            watcher ?? Watcher);

        // Parse the response to get a character
        NpcCharacter character = OllamaResponseParser.ParseCharacterJson(response);
        return character;
    }
}