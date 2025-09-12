
// Conversation type config
public static class ConversationTypeConfig
{
    public static int GetAttentionCost(ConversationType type)
    {
        if (type == ConversationType.FriendlyChat) return 1;
        if (type == ConversationType.Request) return 2;
        if (type == ConversationType.Resolution) return 2;
        if (type == ConversationType.Delivery) return 1;
        return 1; // Default
    }
}
