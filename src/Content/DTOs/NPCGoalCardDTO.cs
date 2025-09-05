using System.Collections.Generic;

/// <summary>
/// DTO for NPC-specific goal cards that appear in conversations
/// These are goal cards that NPCs offer when certain rapport thresholds are met
/// </summary>
public class NPCGoalCardDTO
{
    /// <summary>
    /// The NPC who has this promise card in their request deck
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// Unique identifier for this promise card
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Card type (usually "Promise" or "Request")
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Focus cost (usually 0 for promise cards)
    /// </summary>
    public int Focus { get; set; }

    /// <summary>
    /// Card properties like Persistent, DeliveryEligible
    /// </summary>
    public List<string> Properties { get; set; }

    /// <summary>
    /// Token type for this promise (Trust, Kinship, Heritage)
    /// </summary>
    public string ConnectionType { get; set; }

    /// <summary>
    /// Display name/description of the promise
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Difficulty level (usually VeryEasy for promise cards)
    /// </summary>
    public string Difficulty { get; set; }

    /// <summary>
    /// Rapport threshold required to make this card playable
    /// </summary>
    public int RapportThreshold { get; set; }

    /// <summary>
    /// Effect when the promise is accepted
    /// </summary>
    public CardEffectDTO SuccessEffect { get; set; }

    /// <summary>
    /// Dialogue spoken when playing this card
    /// </summary>
    public string DialogueFragment { get; set; }
}