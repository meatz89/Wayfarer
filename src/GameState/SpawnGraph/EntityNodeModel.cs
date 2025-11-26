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

    public EntityNodeModel(Point position, LocationSnapshot location) : base(position)
    {
        EntityType = SpawnGraphEntityType.Location;
        EntityName = location?.Name ?? "Location";
        LocationSnapshot = location;
    }

    public EntityNodeModel(Point position, NPCSnapshot npc) : base(position)
    {
        EntityType = SpawnGraphEntityType.NPC;
        EntityName = npc?.Name ?? "NPC";
        NPCSnapshot = npc;
    }

    public EntityNodeModel(Point position, RouteSnapshot route) : base(position)
    {
        EntityType = SpawnGraphEntityType.Route;
        EntityName = route != null ? route.OriginLocationName + " -> " + route.DestinationLocationName : "Route";
        RouteSnapshot = route;
    }
}
