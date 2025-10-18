/// <summary>
/// Analysis results from examining active cards for backwards construction.
/// Used to determine narrative constraints and generate appropriate NPC dialogue.
/// </summary>
public class CardAnalysis
{

    /// <summary>
    /// Pattern of Initiative costs across all cards.
    /// Determines the intensity level required in NPC dialogue.
    /// </summary>
    public InitiativePattern InitiativePattern { get; set; }

    /// <summary>
    /// Most common narrative category among the cards.
    /// Examples: "risk", "support", "atmosphere", "utility"
    /// Used to generate contextually appropriate NPC dialogue.
    /// </summary>
    public string DominantCategory { get; set; }

    /// <summary>
    /// True if cards require urgent NPC response.
    /// Typically when high-risk cards dominate.
    /// </summary>
    public bool RequiresUrgency { get; set; }

    /// <summary>
    /// List of card IDs that are categorized as risk cards.
    /// These require NPC dialogue that invites bold responses.
    /// </summary>
    public List<string> RiskCards { get; set; } = new List<string>();

    /// <summary>
    /// List of card IDs that can set atmosphere.
    /// These need dialogue that can be disrupted or enhanced.
    /// </summary>
    public List<string> AtmosphereSetters { get; set; } = new List<string>();

    /// <summary>
    /// Breakdown of all cards by their narrative category.
    /// Maps card ID to category for backwards construction.
    /// </summary>
    public Dictionary<string, string> CategoryBreakdown { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// Pattern of Initiative costs across active cards.
/// Used to determine narrative intensity requirements.
/// </summary>
public enum InitiativePattern
{
    /// <summary>
    /// All cards are Foundation tier (0-2 Initiative).
    /// NPC should be verbose with many response angles.
    /// </summary>
    AllFoundation,

    /// <summary>
    /// Cards have mixed Initiative costs.
    /// NPC should present layered statements with multiple engagement levels.
    /// </summary>
    Mixed,

    /// <summary>
    /// All cards are Standard/Decisive tier (3+ Initiative).
    /// NPC should say something provocative warranting strong response.
    /// </summary>
    AllHighTier
}