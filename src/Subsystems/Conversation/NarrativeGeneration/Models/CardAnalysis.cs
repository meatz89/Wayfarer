/// <summary>
/// Analysis results from examining active cards for backwards construction.
/// Used to determine narrative constraints and generate appropriate NPC dialogue.
/// </summary>
public class CardAnalysis
{
    /// <summary>
    /// True if any cards have Impulse persistence (need urgent response).
    /// NPC dialogue must include urgent elements requiring immediate response.
    /// </summary>
    public bool HasImpulse { get; set; }
    
    /// <summary>
    /// True if any cards have Opening persistence (need inviting response).
    /// NPC dialogue must include inviting elements encouraging elaboration.
    /// </summary>
    public bool HasOpening { get; set; }
    
    /// <summary>
    /// Pattern of focus costs across all cards.
    /// Determines the intensity level required in NPC dialogue.
    /// </summary>
    public FocusPattern FocusPattern { get; set; }
    
    /// <summary>
    /// Most common narrative category among the cards.
    /// Examples: "risk", "support", "atmosphere", "utility"
    /// Used to generate contextually appropriate NPC dialogue.
    /// </summary>
    public string DominantCategory { get; set; }
    
    /// <summary>
    /// True if cards require urgent NPC response.
    /// Typically when Impulse cards are present or high-risk cards dominate.
    /// </summary>
    public bool RequiresUrgency { get; set; }
    
    /// <summary>
    /// True if cards require inviting NPC response.
    /// Typically when Opening cards are present or utility cards dominate.
    /// </summary>
    public bool RequiresInvitation { get; set; }
    
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
/// Pattern of focus costs across active cards.
/// Used to determine narrative intensity requirements.
/// </summary>
public enum FocusPattern
{
    /// <summary>
    /// All cards have low focus costs (1-2).
    /// NPC should be verbose with many response angles.
    /// </summary>
    AllLow,
    
    /// <summary>
    /// Cards have mixed focus costs.
    /// NPC should present layered statements with multiple engagement levels.
    /// </summary>
    Mixed,
    
    /// <summary>
    /// All cards have high focus costs (3+).
    /// NPC should say something provocative warranting strong response.
    /// </summary>
    AllHigh
}