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
    /// Token type for this promise (Trust, Commerce, Status, Shadow)
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
    /// Momentum threshold required to make this card playable
    /// </summary>
    public int MomentumThreshold { get; set; }

    // Categorical properties - define behavior through context
    public string Category { get; set; } // Expression/Realization/Regulation (optional - auto-determined from effect type if not specified)
    public string Persistence { get; set; } // Usually "Thought" for goal cards
    public string SuccessType { get; set; } // Usually "Promising" or "Advancing"
    public string FailureType { get; set; } // Usually "None" for goal cards
    public string ExhaustType { get; set; } // Usually "None" for goal cards

    /// <summary>
    /// Dialogue spoken when playing this card
    /// </summary>
    public string DialogueFragment { get; set; }
}