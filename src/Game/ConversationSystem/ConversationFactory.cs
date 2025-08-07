using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Factory for creating conversation instances with systemic generation
/// </summary>
public class ConversationFactory
{
    private readonly INarrativeProvider _narrativeProvider;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly LetterQueueManager _queueManager;

    public ConversationFactory(
        INarrativeProvider narrativeProvider,
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        LetterQueueManager queueManager)
    {
        _narrativeProvider = narrativeProvider;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _queueManager = queueManager;
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

        // Initialize attention manager for graduated system (0/1/2 points)
        if (context.AttentionManager == null)
        {
            context.AttentionManager = new AttentionManager();
        }

        // Calculate NPC emotional state from queue
        if (context.TargetNPC != null)
        {
            var npcState = _stateCalculator.CalculateState(context.TargetNPC);
            
            // Find their most urgent letter for context
            var npcLetters = _queueManager.GetActiveLetters()
                .Where(l => l.SenderId == context.TargetNPC.ID || l.SenderName == context.TargetNPC.Name)
                .OrderBy(l => l.DeadlineInDays)
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

        // Create the conversation manager
        ConversationManager conversationManager = new ConversationManager(
            context,
            state,
            _narrativeProvider,
            context.GameWorld);

        // Initialize the conversation to generate initial narrative
        await conversationManager.InitializeConversation();
        
        // Generate initial choices
        await conversationManager.ProcessNextBeat();

        return conversationManager;
    }
}