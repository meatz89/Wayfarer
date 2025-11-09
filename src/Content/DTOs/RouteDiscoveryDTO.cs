/// <summary>
/// Data Transfer Object for deserializing route discovery data from JSON.
/// Maps to the structure in route_discovery.json.
/// </summary>
public class RouteDiscoveryDTO
{
public string RouteId { get; set; }
public List<string> KnownByNPCs { get; set; } = new List<string>();
public int RequiredTokensWithNPC { get; set; } = 3;
public Dictionary<string, RouteDiscoveryContextDTO> DiscoveryContexts { get; set; } = new Dictionary<string, RouteDiscoveryContextDTO>();
}

/// <summary>
/// Context for how a specific NPC can teach a route
/// </summary>
public class RouteDiscoveryContextDTO
{
public List<string> RequiredEquipment { get; set; } = new List<string>();
public string Narrative { get; set; }
}