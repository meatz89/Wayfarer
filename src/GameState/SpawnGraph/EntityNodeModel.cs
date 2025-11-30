using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

public enum SpawnGraphEntityType
{
    Location,
    NPC,
    Route
}

public class EntityNodeModel : NodeModel
{
    public SpawnGraphEntityType EntityType { get; }
    public string EntityName { get; }
    public bool IsHighlighted { get; set; }

    public LocationSnapshot LocationSnapshot { get; }
    public NPCSnapshot NPCSnapshot { get; }
    public RouteSnapshot RouteSnapshot { get; }

    /// <summary>
    /// Resolution metadata showing how this entity was discovered or created
    /// Captures outcome, filter used, and property sources
    /// </summary>
    public EntityResolutionMetadata Resolution { get; }

    public EntityNodeModel(Point position, LocationSnapshot location, EntityResolutionMetadata resolution = null) : base(position)
    {
        EntityType = SpawnGraphEntityType.Location;
        EntityName = location?.Name ?? "Location";
        LocationSnapshot = location;
        Resolution = resolution;
    }

    public EntityNodeModel(Point position, NPCSnapshot npc, EntityResolutionMetadata resolution = null) : base(position)
    {
        EntityType = SpawnGraphEntityType.NPC;
        EntityName = npc?.Name ?? "NPC";
        NPCSnapshot = npc;
        Resolution = resolution;
    }

    public EntityNodeModel(Point position, RouteSnapshot route, EntityResolutionMetadata resolution = null) : base(position)
    {
        EntityType = SpawnGraphEntityType.Route;
        EntityName = route != null ? route.OriginLocationName + " -> " + route.DestinationLocationName : "Route";
        RouteSnapshot = route;
        Resolution = resolution;
    }
}
