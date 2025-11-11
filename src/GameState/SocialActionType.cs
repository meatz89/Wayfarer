/// <summary>
/// Action type for Conversation tactical system
/// Determines action-based balance shift (combines with Delivery for total balance change)
/// Listen (draw) = -2 balance, Speak (play) = +1 balance
/// </summary>
public enum SocialActionType
{
    /// <summary>
    /// Listen action: Drawing cards during conversation = -2 Cadence
    /// </summary>
    Listen,

    /// <summary>
    /// Speak action: Playing cards during conversation = +1 Cadence
    /// </summary>
    Speak
}
