/// <summary>
/// Context for route obstacle screens containing scene state and metadata.
/// Used for route obstacle encounters where player chooses approach to overcome scene.
/// Part of Physical challenge system (distinct from Physical card challenges).
/// </summary>
public class RouteObstacleContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Scene data
    public TravelScene scene { get; set; }
    // ADR-007: RouteId DELETED - unused dead code (never read)

    // Player state
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (PlayerStatType)
    public int InsightLevel { get; set; }
    public int RapportLevel { get; set; }
    public int AuthorityLevel { get; set; }
    public int DiplomacyLevel { get; set; }
    public int CunningLevel { get; set; }

    public int GetStatLevel(PlayerStatType stat) => stat switch
    {
        PlayerStatType.Insight => InsightLevel,
        PlayerStatType.Rapport => RapportLevel,
        PlayerStatType.Authority => AuthorityLevel,
        PlayerStatType.Diplomacy => DiplomacyLevel,
        PlayerStatType.Cunning => CunningLevel,
        _ => 0
    };

    // Display info
    public string TimeDisplay { get; set; }

    public RouteObstacleContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
