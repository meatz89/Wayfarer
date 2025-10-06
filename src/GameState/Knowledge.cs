using System.Collections.Generic;

/// <summary>
/// Knowledge entity - structured discoveries that connect investigations
/// Knowledge serves as connective tissue: unlocks investigations, unlocks goals, enhances conversations
/// </summary>
public class Knowledge
{
    /// <summary>
    /// Unique knowledge identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name shown to player in journal
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Description of what player learned
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Which investigation granted this knowledge (empty if from conversation/other source)
    /// </summary>
    public string InvestigationContext { get; set; }

    /// <summary>
    /// Investigation intro triggers unlocked by this knowledge
    /// Used for ConversationalDiscovery trigger type
    /// </summary>
    public List<string> UnlocksInvestigationIntros { get; set; } = new List<string>();

    /// <summary>
    /// Investigation goals unlocked by this knowledge
    /// Used in investigation phase requirements
    /// </summary>
    public List<string> UnlocksInvestigationGoals { get; set; } = new List<string>();
}
