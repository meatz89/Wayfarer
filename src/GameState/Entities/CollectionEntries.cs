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
/// HIGHLANDER: Object reference only, no string ID
/// </summary>
public class NPCTokenEntry
{
    /// <summary>
    /// NPC entity this token entry tracks
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public NPC Npc { get; set; }
    public int Trust { get; set; }
    public int Diplomacy { get; set; }
    public int Status { get; set; }
    public int Shadow { get; set; }

    public int GetTokenCount(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => Trust,
            ConnectionType.Diplomacy => Diplomacy,
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
            case ConnectionType.Diplomacy:
                Diplomacy = value;
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
/// HIGHLANDER: Object reference, no string ID
/// </summary>
public class KnownRouteEntry
{
    public Location OriginLocation { get; set; }
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();
}

/// <summary>
/// Helper class for route familiarity entries (replaces Dictionary<string, int>)
/// HIGHLANDER: Object reference, not string ID
/// </summary>
public class RouteFamiliarityEntry
{
    public RouteOption Route { get; set; }
    public int Level { get; set; }
}

/// <summary>
/// Helper class for location familiarity entries (replaces Dictionary<string, int>)
/// HIGHLANDER: Object reference, not string ID
/// </summary>
public class LocationFamiliarityEntry
{
    public Location Location { get; set; }
    public int Level { get; set; }
}

/// <summary>
/// Helper class for NPC exchange card entries (replaces Dictionary<string, List<ExchangeCard>>)
/// HIGHLANDER: Object reference only, no string ID
/// </summary>
public class NPCExchangeCardEntry
{
    public NPC Npc { get; set; }
    public List<ExchangeCard> ExchangeCards { get; set; } = new List<ExchangeCard>();
}

/// <summary>
/// Helper class for skeleton registry entries (replaces Dictionary<string, string>)
/// </summary>
public class SkeletonRegistryEntry
{
    public string SkeletonKey { get; set; }
    public string ContentType { get; set; }
}

/// <summary>
/// Helper class for path card discovery entries (replaces Dictionary<string, bool>)
/// </summary>
public class PathCardDiscoveryEntry
{
    public string CardId { get; set; }
    public bool IsDiscovered { get; set; }
}

/// <summary>
/// Helper class for event deck position entries (replaces Dictionary<string, int>)
/// </summary>
public class EventDeckPositionEntry
{
    public string DeckId { get; set; }
    public int Position { get; set; }
}

/// <summary>
/// Helper class for path collection entries (replaces Dictionary<string, PathCardCollectionDTO>)
/// HIGHLANDER: Collection object contains Id - no need to store separately
/// </summary>
public class PathCollectionEntry
{
    // ADR-007: CollectionId DELETED - use Collection.Id instead (object has Id property)
    public PathCardCollectionDTO Collection { get; set; }
}

/// <summary>
/// Helper class for travel event entries (replaces Dictionary<string, TravelEventDTO>)
/// HIGHLANDER: TravelEvent object contains Id - no need to store separately
/// </summary>
public class TravelEventEntry
{
    // ADR-007: EventId DELETED - use TravelEvent.Id instead (object has Id property)
    public TravelEventDTO TravelEvent { get; set; }
}

/// <summary>
/// Helper class for stepped threshold entries (replaces Dictionary<int, float>)
/// Stores threshold levels with flat integer values
/// </summary>
public class SteppedThreshold
{
    public int Level { get; set; }
    public int Value { get; set; }
}

// ALL EXTENSION METHODS DELETED - Domain logic moved to Player.cs and GameWorld.cs
// Extension methods hide domain logic and violate architecture principles
// Use Player instance methods and GameWorld instance methods instead
