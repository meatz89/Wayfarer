/// <summary>
/// Immutable snapshot of choice reward at execution time
/// Captures what player received (resources, stats, etc.)
/// </summary>
public class RewardSnapshot
{
    // Resources
    public int CoinsGained { get; set; }
    public int ResolveGained { get; set; }
    public int HealthGained { get; set; }
    public int StaminaGained { get; set; }
    public int FocusGained { get; set; }

    // Five Stats
    public int InsightGained { get; set; }
    public int RapportGained { get; set; }
    public int AuthorityGained { get; set; }
    public int DiplomacyGained { get; set; }
    public int CunningGained { get; set; }

    // Other rewards (summarized as strings for simplicity)
    public List<string> BondChanges { get; set; }
    public List<string> ItemsGranted { get; set; }
    public List<string> StatesApplied { get; set; }
    public List<string> AchievementsGranted { get; set; }
}
