/// <summary>
/// V4 Mental Session - runtime state for active obligation
/// Unified architecture with ConversationSession
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enums
/// </summary>
public class MentalSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public Obligation Obligation { get; set; }
    public Location Location { get; set; }
    public MentalSessionDeck Deck { get; set; }
    public int CurrentPhaseIndex { get; set; } = 0;

    // Session Resources
    public int CurrentAttention { get; set; } = 0;
    public int MaxAttention { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;
    public int CurrentLeads { get; set; } = 0;
    public List<int> UnlockedTiers { get; set; } = new List<int> { 1 };

    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for MentalCategory (fixed enum)
    public int AnalyticalCount { get; set; }
    public int PhysicalCount { get; set; }
    public int ObservationalCount { get; set; }
    public int SocialCount { get; set; }
    public int SynthesisCount { get; set; }

    // Obligation-Local Resources
    public int CurrentProgress { get; set; } = 0;
    public int CurrentExposure { get; set; } = 0;
    public int MaxExposure { get; set; } = 10;
    public int VictoryThreshold { get; set; } = 20;

    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for DiscoveryType (fixed enum)
    public List<string> StructuralDiscoveries { get; set; } = new List<string>();
    public List<string> HistoricalDiscoveries { get; set; } = new List<string>();
    public List<string> EnvironmentalDiscoveries { get; set; } = new List<string>();
    public List<string> SocialDiscoveries { get; set; } = new List<string>();
    public List<string> HiddenDiscoveries { get; set; } = new List<string>();

    // Timing
    public int TimeSegmentsSpent { get; set; } = 0;

    // Tier and Category Methods
    public int GetUnlockedMaxDepth()
    {
        return UnlockedTiers.Max() * 2;
    }

    public int GetCategoryCount(MentalCategory category)
    {
        return category switch
        {
            MentalCategory.Analytical => AnalyticalCount,
            MentalCategory.Physical => PhysicalCount,
            MentalCategory.Observational => ObservationalCount,
            MentalCategory.Social => SocialCount,
            MentalCategory.Synthesis => SynthesisCount,
            _ => 0
        };
    }

    public void IncrementCategoryCount(MentalCategory category)
    {
        switch (category)
        {
            case MentalCategory.Analytical: AnalyticalCount++; break;
            case MentalCategory.Physical: PhysicalCount++; break;
            case MentalCategory.Observational: ObservationalCount++; break;
            case MentalCategory.Social: SocialCount++; break;
            case MentalCategory.Synthesis: SynthesisCount++; break;
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

    public bool ShouldEnd()
    {
        return CurrentExposure >= MaxExposure;
    }

    public int GetDrawCount()
    {
        return CurrentLeads;
    }
}

/// <summary>
/// Discovery types (for categorization)
/// </summary>
public enum DiscoveryType
{
    Structural,
    Historical,
    Environmental,
    Social,
    Hidden
}

