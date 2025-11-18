/// <summary>
/// Represents how a route can be discovered through NPC relationships
/// HIGHLANDER: All entity references are typed objects
/// </summary>
public class RouteDiscovery
{
    /// <summary>
    /// The route this discovery unlocks (object reference)
    /// </summary>
    public RouteOption Route { get; set; }

    /// <summary>
    /// NPCs who know about this route and can teach it (typed objects)
    /// </summary>
    public List<NPC> KnownByNPCs { get; set; } = new List<NPC>();

    /// <summary>
    /// How many tokens (total across all types) the player needs with an NPC to learn this route
    /// </summary>
    public int RequiredTokensWithNPC { get; set; } = 3;

    /// <summary>
    /// Context-specific discovery information for each NPC who knows this route (keyed by NPC object)
    /// </summary>
    public Dictionary<NPC, RouteDiscoveryContext> DiscoveryContexts { get; set; } = new Dictionary<NPC, RouteDiscoveryContext>();
}

/// <summary>
/// Context-specific requirements and narrative for discovering a route from a specific NPC
/// HIGHLANDER: Equipment is List of Item objects
/// </summary>
public class RouteDiscoveryContext
{
    /// <summary>
    /// Equipment that must be owned to convince this specific NPC to share the route (typed objects)
    /// </summary>
    public List<Item> RequiredEquipment { get; set; } = new List<Item>();

    /// <summary>
    /// Narrative shown when discovering this route from this NPC
    /// </summary>
    public string Narrative { get; set; } = "";
}
