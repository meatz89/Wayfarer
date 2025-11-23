/// <summary>
/// Immutable snapshot of choice requirements at execution time
/// Captures stat and resource requirements for trace debugging
/// </summary>
public class RequirementSnapshot
{
    // Five Stats requirements
    public int? RequiredRapport { get; set; }
    public int? RequiredInsight { get; set; }
    public int? RequiredAuthority { get; set; }
    public int? RequiredDiplomacy { get; set; }
    public int? RequiredCunning { get; set; }

    // Resource requirements
    public int? RequiredCoins { get; set; }

    // State requirements (captured as string names for simplicity)
    public List<string> RequiredStates { get; set; }
}
