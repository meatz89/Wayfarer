/// <summary>
/// Context for TravelScene screens containing scene state and metadata.
/// Used for route obstacle encounters where player chooses approach to overcome scene.
/// Part of Physical challenge system (distinct from Physical card challenges).
/// </summary>
public class TravelSceneContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Scene data
    public TravelScene Scene { get; set; }
    public string RouteId { get; set; }

    // Player state
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public Dictionary<PlayerStatType, int> PlayerStats { get; set; }

    // Display info
    public string TimeDisplay { get; set; }

    public TravelSceneContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
        PlayerStats = new Dictionary<PlayerStatType, int>();
    }
}
