using System.Collections.Generic;
using Wayfarer.GameState.Enums;

/// <summary>
/// Helper classes to replace Dictionary and HashSet with List-based implementations
/// following deterministic principles and avoiding non-deterministic data structures
/// </summary>

/// <summary>
/// Helper class for resource entries (replaces Dictionary<string, int>)
/// </summary>
public class ResourceEntry
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for NPC token entries (replaces nested Dictionary)
/// </summary>
public class NPCTokenEntry
{
    public string NpcId { get; set; }
    public int Trust { get; set; }
    public int Commerce { get; set; }
    public int Status { get; set; }
    public int Shadow { get; set; }

    public int GetTokenCount(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => Trust,
            ConnectionType.Commerce => Commerce,
            ConnectionType.Status => Status,
            ConnectionType.Shadow => Shadow,
            _ => 0
        };
    }

    public void SetTokenCount(ConnectionType type, int value)
    {
        switch (type)
        {
            case ConnectionType.Trust:
                Trust = value;
                break;
            case ConnectionType.Commerce:
                Commerce = value;
                break;
            case ConnectionType.Status:
                Status = value;
                break;
            case ConnectionType.Shadow:
                Shadow = value;
                break;
        }
    }
}

/// <summary>
/// Helper class for route entries (replaces Dictionary<string, List<RouteOption>>)
/// </summary>
public class KnownRouteEntry
{
    public string OriginSpotId { get; set; }
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();
}

/// <summary>
/// Helper class for letter history entries (replaces Dictionary<string, LetterHistory>)
/// </summary>
public class LetterHistoryEntry
{
    public string NpcId { get; set; }
    public LetterHistory History { get; set; }
}

/// <summary>
/// Helper class for familiarity entries (replaces Dictionary<string, int>)
/// </summary>
public class FamiliarityEntry
{
    public string EntityId { get; set; }
    public int Level { get; set; }
}