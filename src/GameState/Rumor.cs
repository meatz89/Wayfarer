using System;

/// <summary>
/// Represents a piece of information that may or may not be true.
/// Rumors can be discovered through conversation and observation.
/// </summary>
public class Rumor
{
    /// <summary>
    /// Unique identifier for the rumor
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The narrative text of the rumor
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Who or where the rumor came from
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The NPC who shared this rumor (if any)
    /// </summary>
    public string SourceNpcId { get; set; }

    /// <summary>
    /// The location where this rumor was learned
    /// </summary>
    public string SourceLocationId { get; set; }

    /// <summary>
    /// Current confidence level in the rumor's veracity
    /// </summary>
    public RumorConfidence Confidence { get; set; } = RumorConfidence.Unknown;

    /// <summary>
    /// How valuable this rumor is for trading
    /// </summary>
    public int TradeValue { get; set; }

    /// <summary>
    /// When this rumor becomes irrelevant (optional)
    /// </summary>
    public int? ExpiryDay { get; set; }

    /// <summary>
    /// Whether this rumor has been verified as true
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Whether this rumor has been proven false
    /// </summary>
    public bool IsDisproven { get; set; }

    /// <summary>
    /// Category of rumor for organization
    /// </summary>
    public RumorCategory Category { get; set; }

    /// <summary>
    /// The day this rumor was discovered
    /// </summary>
    public int DiscoveredDay { get; set; }

    /// <summary>
    /// Get the confidence symbol for UI display
    /// </summary>
    public string GetConfidenceSymbol()
    {
        return Confidence switch
        {
            RumorConfidence.Unknown => "???",
            RumorConfidence.Doubtful => "?",
            RumorConfidence.Possible => "◐",
            RumorConfidence.Likely => "◕",
            RumorConfidence.Verified => "✓",
            RumorConfidence.False => "✗",
            _ => "?"
        };
    }

    /// <summary>
    /// Get narrative description of confidence
    /// </summary>
    public string GetConfidenceNarrative()
    {
        return Confidence switch
        {
            RumorConfidence.Unknown => "You have no idea if this is true",
            RumorConfidence.Doubtful => "This seems unlikely to be true",
            RumorConfidence.Possible => "This might be true",
            RumorConfidence.Likely => "This is probably true",
            RumorConfidence.Verified => "You know this to be true",
            RumorConfidence.False => "You know this is false",
            _ => "The truth remains unclear"
        };
    }
}

/// <summary>
/// Confidence levels for rumor veracity
/// </summary>
public enum RumorConfidence
{
    Unknown,    // No idea if true (???)
    Doubtful,   // Probably false (?)
    Possible,   // Might be true (◐)
    Likely,     // Probably true (◕)
    Verified,   // Confirmed true (✓)
    False       // Confirmed false (✗)
}

/// <summary>
/// Categories for organizing rumors
/// </summary>
public enum RumorCategory
{
    Trade,          // Market prices, trade routes
    Social,         // NPC relationships, scandals
    Political,      // Noble affairs, laws
    Location,       // Hidden places, shortcuts
    Opportunity,    // Jobs, quests, special deliveries
    Danger,         // Threats, warnings
    General         // Miscellaneous information
}