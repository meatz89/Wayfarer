/// <summary>
/// Factory for creating Social challenge contexts.
/// Goal cards now handle domain logic - contexts are simple data containers.
/// Parallel to Mental/Physical challenge context creation.
/// </summary>
public static class SocialContextFactory
{
    /// <summary>
    /// Create a Social challenge context
    /// </summary>
    public static SocialChallengeContext CreateContext(
        string conversationTypeId,
        NPC npc,
        SocialSession session,
        List<CardInstance> observationCards,
        ResourceState playerResources,
        string locationName,
        string timeDisplay)
    {
        // Simple context creation - goal cards handle domain logic
        SocialChallengeContext context = new SocialChallengeContext
        {
            IsValid = true,
            NpcId = npc.ID,
            Npc = npc,
            ConversationTypeId = conversationTypeId,
            InitialState = ConnectionState.NEUTRAL,
            Session = session,
            ObservationCards = observationCards,
            PlayerResources = playerResources,
            LocationName = locationName,
            TimeDisplay = timeDisplay,
            RequestText = session.RequestText
        };

        return context;
    }

    /// <summary>
    /// Create an invalid context with error message
    /// </summary>
    public static SocialChallengeContext CreateInvalidContext(string errorMessage)
    {
        return new SocialChallengeContext
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }

}