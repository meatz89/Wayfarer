/// <summary>
/// V4 Physical Session - runtime state for active physical challenge
/// Unified architecture with MentalSession
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enums
/// </summary>
public class PhysicalSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string ChallengeId { get; set; }
    public PhysicalSessionDeck Deck { get; set; }
    public int CurrentPhaseIndex { get; set; } = 0;

    // Session Resources
    public int CurrentExertion { get; set; } = 0;
    public int MaxExertion { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;
    public List<int> UnlockedTiers { get; set; } = new List<int> { 1 };

    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for PhysicalCategory (fixed enum)
    public int AggressiveCount { get; set; }
    public int DefensiveCount { get; set; }
    public int TacticalCount { get; set; }
    public int EvasiveCount { get; set; }
    public int EnduranceCount { get; set; }

    // Challenge-Local Resources
    public int CurrentBreakthrough { get; set; } = 0;
    public int CurrentDanger { get; set; } = 0;
    public int Aggression { get; set; } = 0;
    public int ApproachHistory { get; set; } = 0;

    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for DiscoveryType (fixed enum)
    public List<string> StructuralDiscoveries { get; set; } = new List<string>();
    public List<string> HistoricalDiscoveries { get; set; } = new List<string>();
    public List<string> EnvironmentalDiscoveries { get; set; } = new List<string>();
    public List<string> SocialDiscoveries { get; set; } = new List<string>();
    public List<string> HiddenDiscoveries { get; set; } = new List<string>();

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
        return category switch
        {
            PhysicalCategory.Aggressive => AggressiveCount,
            PhysicalCategory.Defensive => DefensiveCount,
            PhysicalCategory.Tactical => TacticalCount,
            PhysicalCategory.Evasive => EvasiveCount,
            PhysicalCategory.Endurance => EnduranceCount,
            _ => 0
        };
    }

    public void IncrementCategoryCount(PhysicalCategory category)
    {
        switch (category)
        {
            case PhysicalCategory.Aggressive: AggressiveCount++; break;
            case PhysicalCategory.Defensive: DefensiveCount++; break;
            case PhysicalCategory.Tactical: TacticalCount++; break;
            case PhysicalCategory.Evasive: EvasiveCount++; break;
            case PhysicalCategory.Endurance: EnduranceCount++; break;
        }
    }

    public List<string> GetDiscoveriesForType(DiscoveryType type)
    {
        return type switch
        {
            DiscoveryType.Structural => StructuralDiscoveries,
            DiscoveryType.Historical => HistoricalDiscoveries,
            DiscoveryType.Environmental => EnvironmentalDiscoveries,
            DiscoveryType.Social => SocialDiscoveries,
            DiscoveryType.Hidden => HiddenDiscoveries,
            _ => new List<string>()
        };
    }

    public void AddDiscovery(DiscoveryType type, string discovery)
    {
        switch (type)
        {
            case DiscoveryType.Structural: StructuralDiscoveries.Add(discovery); break;
            case DiscoveryType.Historical: HistoricalDiscoveries.Add(discovery); break;
            case DiscoveryType.Environmental: EnvironmentalDiscoveries.Add(discovery); break;
            case DiscoveryType.Social: SocialDiscoveries.Add(discovery); break;
            case DiscoveryType.Hidden: HiddenDiscoveries.Add(discovery); break;
        }
    }

    public int GetTotalDiscoveryCount()
    {
        return StructuralDiscoveries.Count + HistoricalDiscoveries.Count +
               EnvironmentalDiscoveries.Count + SocialDiscoveries.Count +
               HiddenDiscoveries.Count;
    }

    // Facade helper methods
    public int MaxDanger { get; set; } = 10;

    public bool ShouldEnd()
    {
        return CurrentDanger >= MaxDanger;
    }

    public int GetDrawCount()
    {
        return 3;
    }
}
