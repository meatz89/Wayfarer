/// <summary>
/// Factory for creating conversation instances
/// </summary>
public class ConversationFactory
{
    private readonly INarrativeProvider _narrativeProvider;
    private readonly ConnectionTokenManager _tokenManager;

    public ConversationFactory(
        INarrativeProvider narrativeProvider,
        ConnectionTokenManager tokenManager)
    {
        _narrativeProvider = narrativeProvider;
        _tokenManager = tokenManager;
    }

    public async Task<ConversationManager> CreateConversation(
        ConversationContext context,
        Player player)
    {
        // Get current relationship tokens with the NPC
        if (context.TargetNPC != null)
        {
            context.CurrentTokens = _tokenManager.GetTokensWithNPC(context.TargetNPC.ID);
            context.RelationshipLevel = context.CurrentTokens.Values.Sum();
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

        return conversationManager;
    }
}