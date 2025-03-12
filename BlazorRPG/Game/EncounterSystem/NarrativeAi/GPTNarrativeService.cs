// Main service class - slim coordinator
using BlazorRPG.Game.EncounterManager.NarrativeAi;
using BlazorRPG.Game.EncounterManager;

public class GPTNarrativeService : INarrativeAIService
{
    private readonly AIClientService _aiClient;
    private readonly PromptManager _promptManager;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<GPTNarrativeService> _logger;

    public GPTNarrativeService(
        IConfiguration configuration,
        ILogger<GPTNarrativeService> logger)
    {
        _logger = logger;
        _aiClient = new AIClientService(configuration);
        _promptManager = new PromptManager(configuration);
        _contextManager = new NarrativeContextManager();
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Get system message and introduction prompt from prompt manager
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(location, incitingAction, state);

        // Store conversation context
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = context.LocationName;

        // Get system message and reaction prompt from prompt manager
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildReactionPrompt(
            context, chosenOption, choiceDescription, outcome, newState);

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = context.LocationName;

        // Get system message and choices prompt from prompt manager
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildChoicesPrompt(context, choices, projections, state);

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        // Process response into dictionary with shorthand names and descriptions
        return ChoiceResponseParser.ParseChoiceNarratives(response, choices);
    }
}
