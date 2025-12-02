/// <summary>
/// V4 Physical Session - runtime state for active physical challenge
/// Unified architecture with MentalSession
/// DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
/// </summary>
public class PhysicalSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string ChallengeId { get; set; }
    public PhysicalSessionDeck Deck { get; set; }
    public int CurrentPhaseIndex { get; set; } = 0; // Which phase (0-based)

    // Session Resources
    public int CurrentExertion { get; set; } = 0;
    public int MaxExertion { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;
    public List<int> UnlockedTiers { get; set; } = new List<int> { 1 };
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<PhysicalCategoryCountEntry> CategoryCounts { get; set; } = new List<PhysicalCategoryCountEntry>();

    // Challenge-Local Resources (reset between challenges)
    public int CurrentBreakthrough { get; set; } = 0; // Breakthrough toward victory threshold
    public int CurrentDanger { get; set; } = 0; // Cumulative risk/exposure
    public int Aggression { get; set; } = 0; // Range -10 to +10, balance tracker (EXECUTE increases, ASSESS decreases)
    public int ApproachHistory { get; set; } = 0; // Count of Execute actions (for Decisive card requirements)
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<DiscoveryEntry> Discoveries { get; set; } = new List<DiscoveryEntry>();

    // Timing
    public int TimeSegmentsSpent { get; set; } = 0;

    // Balance States
    public bool IsRecklessBalance()
    {
        return Aggression >= 5;
    }

    public bool IsOvercautiousBalance()
    {
        return Aggression <= -5;
    }

    public bool IsNeutralBalance()
    {
        return Aggression > -5 && Aggression < 5;
    }

    // Tier and Category Methods
    public int GetUnlockedMaxDepth()
    {
        return UnlockedTiers.Max() * 2;
    }

    public int GetCategoryCount(PhysicalCategory category)
    {
        return CategoryCounts.FirstOrDefault(e => e.Category == category)?.Count ?? 0;
    }

    public void IncrementCategoryCount(PhysicalCategory category)
    {
        PhysicalCategoryCountEntry entry = CategoryCounts.FirstOrDefault(e => e.Category == category);
        if (entry != null)
        {
            entry.Count++;
        }
        else
        {
            CategoryCounts.Add(new PhysicalCategoryCountEntry { Category = category, Count = 1 });
        }
    }

    // Facade helper methods
    public int MaxDanger { get; set; } = 10;
    public bool ShouldEnd()
    {
        // ONLY check failure condition - danger threshold reached
        // Victory condition (SituationCard play) is handled by facade
        return CurrentDanger >= MaxDanger;
    }

    public int GetDrawCount()
    {
        return 3; // Base draw count
    }
}
