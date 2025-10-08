using System;
using System.Collections.Generic;

/// <summary>
/// V4 Mental Session - runtime state for active investigation
/// Unified architecture with ConversationSession
/// </summary>
public class MentalSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string InvestigationId { get; set; }
    public string LocationId { get; set; } // Track location for familiarity bonuses
    public int CurrentPhaseIndex { get; set; } = 0; // Which phase (0-based)

    // Session Resources
    public int CurrentAttention { get; set; } = 0;
    public int MaxAttention { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;
    public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };
    public Dictionary<MentalCategory, int> CategoryCounts { get; set; } = new Dictionary<MentalCategory, int>();

    // Investigation-Local Resources (reset between investigations)
    public int CurrentProgress { get; set; } = 0; // Progress toward current phase threshold
    public int CurrentExposure { get; set; } = 0; // Cumulative disturbance/detection
    public int MaxExposure { get; set; } = 10; // Configured via EngagementType
    public int VictoryThreshold { get; set; } = 20; // Configured via EngagementType
    public Dictionary<DiscoveryType, List<string>> Discoveries { get; set; } = new Dictionary<DiscoveryType, List<string>>();

    // Timing
    public int TimeSegmentsSpent { get; set; } = 0;

    // Tier and Category Methods
    public int GetUnlockedMaxDepth()
    {
        return UnlockedTiers.Max() * 2;
    }

    public int GetCategoryCount(MentalCategory category)
    {
        return CategoryCounts.TryGetValue(category, out int count) ? count : 0;
    }

    // Facade helper methods
    public bool ShouldEnd()
    {
        return CurrentProgress >= VictoryThreshold || CurrentExposure >= MaxExposure;
    }

    public int GetDrawCount()
    {
        return 3; // Base draw count
    }
}

/// <summary>
/// Discovery types (for categorization)
/// </summary>
public enum DiscoveryType
{
    Structural,    // Physical features, architectural elements
    Historical,    // Past events, documented information
    Environmental, // Natural conditions, spatial layout
    Social,        // People involved, relationships
    Hidden         // Secrets, concealed information
}

