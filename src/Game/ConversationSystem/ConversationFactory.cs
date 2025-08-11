using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Factory for creating conversation instances with systemic generation
/// </summary>
public class ConversationFactory
{
    private readonly INarrativeProvider _narrativeProvider;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCStateResolver _stateCalculator;
    private readonly LetterQueueManager _queueManager;
    private readonly ITimeManager _timeManager;
    private readonly AtmosphereCalculator _atmosphereCalculator;
    private readonly Wayfarer.GameState.ConsequenceEngine _consequenceEngine;
    private readonly Wayfarer.GameState.TimeBlockAttentionManager _timeBlockAttentionManager;

    public ConversationFactory(
        INarrativeProvider narrativeProvider,
        TokenMechanicsManager tokenManager,
        NPCStateResolver stateCalculator,
        LetterQueueManager queueManager,
        ITimeManager timeManager,
        AtmosphereCalculator atmosphereCalculator,
        Wayfarer.GameState.ConsequenceEngine consequenceEngine,
        Wayfarer.GameState.TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _narrativeProvider = narrativeProvider;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _queueManager = queueManager;
        _timeManager = timeManager;
        _atmosphereCalculator = atmosphereCalculator;
        _consequenceEngine = consequenceEngine;
        _timeBlockAttentionManager = timeBlockAttentionManager;
    }

    public async Task<ConversationManager> CreateConversation(
        SceneContext context,
        Player player)
    {
        // Get current relationship tokens with the NPC
        if (context.TargetNPC != null)
        {
            context.CurrentTokens = _tokenManager.GetTokensWithNPC(context.TargetNPC.ID);
            context.RelationshipLevel = context.CurrentTokens.Values.Sum();
        }

        // CRITICAL CHANGE: Do NOT reset attention per conversation!
        // Attention now persists across conversations within a time block
        if (context.AttentionManager == null)
        {
            // This should only happen if attention wasn't provided from GameFacade
            // In that case, create a default one (but this is an error condition)
            Console.WriteLine("[ConversationFactory] WARNING: No AttentionManager provided! Creating default.");
            context.AttentionManager = new AttentionManager();
            context.AttentionManager.ResetForNewScene(); // Only for emergency fallback
        }
        else
        {
            // DO NOT RESET! Use the existing attention from the time block
            Console.WriteLine($"[ConversationFactory] Using existing attention: Current={context.AttentionManager.Current}, Max={context.AttentionManager.Max}");
        }

        // Calculate NPC emotional state from queue
        if (context.TargetNPC != null)
        {
            var npcState = _stateCalculator.CalculateState(context.TargetNPC);
            
            // Find their most urgent letter for context
            var npcLetters = _queueManager.GetActiveLetters()
                .Where(l => l.SenderId == context.TargetNPC.ID || l.SenderName == context.TargetNPC.Name)
                .OrderBy(l => l.DeadlineInHours)
                .ToList();
                
            // Store in context for narrative generation
            context.ConversationTopic = npcLetters.Any() 
                ? $"Letter to {npcLetters.First().RecipientName}"
                : "General conversation";
        }

        // Create conversation state
        ConversationState state = new ConversationState(
            player,
            context.TargetNPC,
            context.GameWorld,
            context.StartingFocusPoints > 0 ? context.StartingFocusPoints : 10,
            8); // Default max duration

        // Create choice generator with player and gameWorld for additive system
        // Pass TimeBlockAttentionManager to share attention pool with ActionGenerator
        var choiceGenerator = new ConversationChoiceGenerator(
            _queueManager,
            _tokenManager,
            _stateCalculator,
            _timeManager,
            player,
            context.GameWorld,
            _consequenceEngine,
            _timeBlockAttentionManager);

        // Create the conversation manager
        ConversationManager conversationManager = new ConversationManager(
            context,
            state,
            _narrativeProvider,
            context.GameWorld,
            choiceGenerator);

        // Initialize the conversation to generate initial narrative
        await conversationManager.InitializeConversation();
        
        // Generate initial choices
        await conversationManager.ProcessNextBeat();

        return conversationManager;
    }
}