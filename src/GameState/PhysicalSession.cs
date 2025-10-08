using System;
using System.Collections.Generic;

/// <summary>
/// V4 Physical Session - runtime state for active physical challenge
/// Unified architecture with MentalSession
/// </summary>
public class PhysicalSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string ChallengeId { get; set; }
    public int CurrentPhaseIndex { get; set; } = 0; // Which phase (0-based)

    // Session Resources
    public int CurrentExertion { get; set; } = 0;
    public int MaxExertion { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;
    public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };
    public Dictionary<PhysicalCategory, int> CategoryCounts { get; set; } = new Dictionary<PhysicalCategory, int>();

    // Challenge-Local Resources (reset between challenges)
    public int CurrentBreakthrough { get; set; } = 0; // Breakthrough toward victory threshold
    public int CurrentDanger { get; set; } = 0; // Cumulative risk/exposure
    public int Commitment { get; set; } = 0; // Range -10 to +10, balance tracker
    public int ApproachHistory { get; set; } = 0; // Count of Execute actions (for Decisive card requirements)
    public Dictionary<DiscoveryType, List<string>> Discoveries { get; set; } = new Dictionary<DiscoveryType, List<string>>();

    // Timing
    public int TimeSegmentsSpent { get; set; } = 0;

    // Balance States
    public bool IsRecklessBalance()
    {
        return Commitment >= 5;
    }

    public bool IsOvercautiousBalance()
    {
        return Commitment <= -5;
    }

    public bool IsNeutralBalance()
    {
        return Commitment > -5 && Commitment < 5;
    }

    // Tier and Category Methods
    public int GetUnlockedMaxDepth()
    {
        return UnlockedTiers.Max() * 2;
    }

    public int GetCategoryCount(PhysicalCategory category)
    {
        return CategoryCounts.TryGetValue(category, out int count) ? count : 0;
    }

    // Facade helper methods
    public int MaxDanger { get; set; } = 10;
    public int VictoryThreshold { get; set; } = 20; // Configured via EngagementType
    public bool ShouldEnd()
    {
        return CurrentBreakthrough >= VictoryThreshold || CurrentDanger >= MaxDanger;
    }

    public int GetDrawCount()
    {
        return 3; // Base draw count
    }
}
