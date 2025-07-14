// Supporting classes for player strategic overview
public class PlayerStrategicOverview
{
    public List<string> EquipmentCapabilities { get; set; } = new();
    public int AccessibleRoutes { get; set; }
    public int BlockedRoutes { get; set; }
    public List<string> CriticalMissingEquipment { get; set; } = new();
    public int ReadyContracts { get; set; }
    public int PendingContracts { get; set; }
    public int UrgentContracts { get; set; }
}

public class TimeAwarenessAnalysis
{
    public string CurrentStatus { get; set; } = "";
    public string Recommendation { get; set; } = "";
}