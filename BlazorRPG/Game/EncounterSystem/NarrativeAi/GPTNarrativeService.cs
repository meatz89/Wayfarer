using System.Text;

public class GPTNarrativeService : INarrativeAIService
{
    private readonly AIClientService _aiClient;
    private readonly PromptManager _promptManager;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<GPTNarrativeService> _logger;
    private readonly string _gameInstanceId;

    public GPTNarrativeService(IConfiguration configuration, ILogger<GPTNarrativeService> logger = null)
    {
        // Generate a game instance ID
        _gameInstanceId = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        _aiClient = new AIClientService(configuration, _gameInstanceId);
        _promptManager = new PromptManager(configuration);
        _contextManager = new NarrativeContextManager();
        _logger = logger;

        _logger?.LogInformation($"Initialized GPTNarrativeService with game instance ID: {_gameInstanceId}");
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        try
        {
            // Get system message and introduction prompt
            string systemMessage = _promptManager.GetSystemMessage();
            string prompt = _promptManager.BuildIntroductionPrompt(location, incitingAction, state);

            // Store conversation context
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

            // Call AI service and get response
            string response = await _aiClient.GetCompletionAsync(
                _contextManager.GetConversationHistory(conversationId));

            // Update conversation history
            _contextManager.AddAssistantMessage(conversationId, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating introduction for {location}");

            // Return a basic introduction that maintains game flow
            return CreateBasicIntroduction(location, incitingAction);
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

        try
        {
            // Step 1: Generate the action outcome
            string actionOutcome = await GenerateActionOutcomeAsync(context, chosenOption, choiceDescription, outcome, newState);

            // Step 2: Generate the new situation
            string newSituation = await GenerateNewSituationAsync(context, newState, actionOutcome);

            // Combine the two sections with a blank line between them
            string combinedNarrative = $"{actionOutcome}\n\n{newSituation}";

            // Create a new NarrativeEvent with the combined narrative
            NarrativeEvent narrativeEvent = new NarrativeEvent(
                turnNumber: context.Events.Count + 1,
                sceneDescription: combinedNarrative,
                chosenOption: chosenOption,
                choiceNarrative: choiceDescription,
                outcome: outcome.ToString());

            // Add the event to the context
            context.AddEvent(narrativeEvent);

            return combinedNarrative;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating reaction for {context.LocationName}");

            // Create a basic reaction
            return CreateBasicReaction(context, chosenOption, choiceDescription, outcome, newState);
        }
    }

    private async Task<string> GenerateActionOutcomeAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = $"{context.LocationName}_outcome";

        try
        {
            // Get system message and action outcome prompt
            string systemMessage = _promptManager.GetSystemMessage();
            string prompt = _promptManager.BuildActionOutcomePrompt(
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

            // Update conversation history
            _contextManager.AddAssistantMessage(conversationId, response);

            return response.Trim();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating action outcome for {context.LocationName}");

            // Create a simple fallback
            return CreateBasicActionOutcome(chosenOption, outcome);
        }
    }

    private async Task<string> GenerateNewSituationAsync(
        NarrativeContext context,
        EncounterStatus state,
        string recentOutcome)
    {
        string conversationId = $"{context.LocationName}_situation";

        try
        {
            // Get system message and new situation prompt
            string systemMessage = _promptManager.GetSystemMessage();
            string prompt = _promptManager.BuildNewSituationPrompt(
                context, state, recentOutcome);

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

            // Update conversation history
            _contextManager.AddAssistantMessage(conversationId, response);

            return response.Trim();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating new situation for {context.LocationName}");

            // Create a simple fallback
            return CreateBasicNewSituation(state);
        }
    }

    public async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = context.LocationName;

        try
        {
            // Get system message and choices prompt
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

            // Update conversation history
            _contextManager.AddAssistantMessage(conversationId, response);

            // Parse the response into choice narratives
            return ChoiceResponseParser.ParseChoiceNarratives(response, choices);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating choice descriptions for {context.LocationName}");

            // Create basic choice descriptions
            return CreateBasicChoiceDescriptions(choices);
        }
    }

    // Fallback methods

    private string CreateBasicIntroduction(string location, string incitingAction)
    {
        return $"I arrive at {location} after {incitingAction}. The place is busy with local activity, and I need to find a way to accomplish my goals here despite my limited resources. A few people glance my way, but most are focused on their own concerns.";
    }

    private string CreateBasicActionOutcome(IChoice chosenOption, ChoiceOutcome outcome)
    {
        StringBuilder reaction = new StringBuilder();

        reaction.Append($"I {chosenOption.Name.ToLower()} as planned. ");

        if (outcome.MomentumGain > 0)
        {
            reaction.Append($"My approach seems to be working, making some progress toward my goal. ");
        }

        if (outcome.PressureGain > 0)
        {
            reaction.Append($"This creates some complications I'll need to address. ");
        }

        if (outcome.HealthChange < 0)
        {
            reaction.Append($"I've been injured in the process and need to be more careful. ");
        }

        if (outcome.FocusChange < 0)
        {
            reaction.Append($"This was a bit too much for my mental state. I need to be more careful. ");
        }

        if (outcome.ConfidenceChange < 0)
        {
            reaction.Append($"I've made a social misstep and need to be more careful. ");
        }

        return reaction.ToString().Trim();
    }

    private string CreateBasicNewSituation(EncounterStatus state)
    {
        return $"The situation has changed, and I must adapt my approach accordingly. There are several possible ways forward, each with its own risks and potential rewards. I need to decide quickly before circumstances change again.";
    }

    private string CreateBasicReaction(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string actionOutcome = CreateBasicActionOutcome(chosenOption, outcome);
        string newSituation = CreateBasicNewSituation(newState);

        string combinedNarrative = $"{actionOutcome}\n\n{newSituation}";

        // Create a new NarrativeEvent with the basic reaction
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            turnNumber: context.Events.Count + 1,
            sceneDescription: combinedNarrative,
            chosenOption: chosenOption,
            choiceNarrative: choiceNarrative,
            outcome: outcome.ToString());

        // Add the event to the context
        context.AddEvent(narrativeEvent);

        return combinedNarrative;
    }

    private Dictionary<IChoice, ChoiceNarrative> CreateBasicChoiceDescriptions(List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> results = new Dictionary<IChoice, ChoiceNarrative>();

        foreach (IChoice choice in choices)
        {
            string name = $"I use {choice.Approach} approach";
            string description = $"I focus on {choice.Focus} using a {choice.Approach} approach to address the current situation.";

            results[choice] = new ChoiceNarrative(name, description);
        }

        return results;
    }

    // Method to get the current game instance ID
    public string GetGameInstanceId()
    {
        return _gameInstanceId;
    }
}