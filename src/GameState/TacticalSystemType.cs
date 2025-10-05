/// <summary>
/// THREE PARALLEL TACTICAL SYSTEMS
/// Defines which of the three tactical card systems an engagement uses
/// </summary>
public enum TacticalSystemType
{
    /// <summary>
    /// Social system (Conversation) - uses SessionCardDeck, ConversationCards, SocialEngagementTypes
    /// </summary>
    Social,

    /// <summary>
    /// Mental system (Investigation) - uses MentalSessionDeck, MentalCards, MentalEngagementTypes
    /// </summary>
    Mental,

    /// <summary>
    /// Physical system (Challenges) - uses PhysicalSessionDeck, PhysicalCards, PhysicalEngagementTypes
    /// </summary>
    Physical
}
