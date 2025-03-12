using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class GPTNarrativeService : INarrativeAIService
{
    private readonly AIClientService _aiClient;
    private readonly PromptManager _promptManager;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<GPTNarrativeService> _logger;
    private readonly string _gameInstanceId;

    public GPTNarrativeService(
        IConfiguration configuration)
    {
        // Generate a game instance ID that will be consistent for this service instance
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _aiClient = new AIClientService(configuration, _gameInstanceId);
        _promptManager = new PromptManager(configuration);
        _contextManager = new NarrativeContextManager();

        _logger?.LogInformation($"Initialized GPTNarrativeService with game instance ID: {_gameInstanceId}");
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Get system message and introduction prompt from prompt manager
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(location, incitingAction, state);

        // Store conversation context
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        try
        {
            // Call AI service and get response
            string response = await _aiClient.GetCompletionAsync(
                _contextManager.GetConversationHistory(conversationId));

            // Update conversation history with response
            _contextManager.AddAssistantMessage(conversationId, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating introduction for {location}");
            // Return a simple fallback if API call fails completely
            return $"I arrive at {location} after {incitingAction}. The journey has been long, and I must find a way to secure some food and shelter before nightfall.";
        }
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

        try
        {
            // Call AI service and get response
            string response = await _aiClient.GetCompletionAsync(
                _contextManager.GetConversationHistory(conversationId));

            // Update conversation history with response
            _contextManager.AddAssistantMessage(conversationId, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating reaction for {context.LocationName}");

            // Generate a simple fallback based on the momentum/pressure outcome
            if (outcome.MomentumGain > outcome.PressureGain)
            {
                return $"I make some progress with my approach. The situation seems to be improving slightly, but I need to remain cautious and observant.";
            }
            else
            {
                return $"My actions have made the situation more tense. I need to reconsider my approach if I want to succeed here.";
            }
        }
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

        try
        {
            // Call AI service and get response
            string response = await _aiClient.GetCompletionAsync(
                _contextManager.GetConversationHistory(conversationId));

            // Update conversation history with response
            _contextManager.AddAssistantMessage(conversationId, response);

            // Process response into dictionary with action summaries and descriptions
            return ChoiceResponseParser.ParseChoiceNarratives(response, choices);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating choices for {context.LocationName}");

            // Create more realistic fallback choices based on the mechanical choices
            Dictionary<IChoice, ChoiceNarrative> fallbackChoices = new Dictionary<IChoice, ChoiceNarrative>();

            foreach (var (choice, index) in choices.Select((c, i) => (c, i)))
            {
                // Create more detailed and specific fallback descriptions
                string description = "fallback";

                // Create a summary from the description
                string[] words = description.Split(' ');
                string actionSummary = string.Join(" ", words.Take(Math.Min(8, words.Length)));

                fallbackChoices[choice] = new ChoiceNarrative(actionSummary, description);
            }

            return fallbackChoices;
        }
    }

    // Method to get the current game instance ID
    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}