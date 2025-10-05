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
    public int ObserveActBalance { get; set; } = 0; // Range -10 to +10
    public Dictionary<DiscoveryType, List<string>> Discoveries { get; set; } = new Dictionary<DiscoveryType, List<string>>();

    // Timing
    public int TimeSegmentsSpent { get; set; } = 0;

    // Balance States
    public bool IsRecklessBalance() => ObserveActBalance >= 5;
    public bool IsOvercautiousBalance() => ObserveActBalance <= -5;
    public bool IsNeutralBalance() => ObserveActBalance > -5 && ObserveActBalance < 5;

    // Tier and Category Methods
    public int GetUnlockedMaxDepth() => UnlockedTiers.Max() * 2;
    public int GetCategoryCount(MentalCategory category) => CategoryCounts.TryGetValue(category, out int count) ? count : 0;

    // Facade helper methods
    public bool ShouldEnd() => CurrentProgress >= VictoryThreshold || CurrentExposure >= MaxExposure;
    public int GetDrawCount() => 3; // Base draw count
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

