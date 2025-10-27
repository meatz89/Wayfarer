using System;

/// <summary>
/// Represents a specific conversation option that maps to a situation card from the NPC's requests.
/// Each conversation option corresponds to exactly one situation card.
/// </summary>
public class SocialChallengeOption
{
    /// <summary>
    /// The request ID that drives this conversation option
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS: The engagement deck ID from the request (Social system)
    /// </summary>
    public string DeckId { get; set; }

    /// <summary>
    /// The specific card ID from the NPC's requests that will be used as the situation card
    /// </summary>
    public string SituationCardId { get; set; }

    /// <summary>
    /// Display name for the UI (e.g., "Letter: Marriage Refusal", "Request: Trade Agreement")
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Full description of what this conversation option entails
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The token type associated with this situation card
    /// </summary>
    public ConnectionType TokenType { get; set; }

    /// <summary>
    /// The momentum threshold required to play this situation card
    /// </summary>
    public int MomentumThreshold { get; set; }

    /// <summary>
    /// Obligation ID if this conversation option is part of an active obligation
    /// </summary>
    public string ObligationId { get; set; }
}