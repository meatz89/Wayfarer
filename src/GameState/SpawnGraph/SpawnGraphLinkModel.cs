using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

public enum SpawnGraphLinkType
{
    Hierarchy,
    SpawnScene,
    SpawnSituation,
    EntityLocation,
    EntityNpc,
    EntityRoute
}

public class SpawnGraphLinkModel : LinkModel
{
    public SpawnGraphLinkType LinkType { get; }
    public string CssClass { get; }
    public string Label { get; }

    public SpawnGraphLinkModel(NodeModel source, NodeModel target, SpawnGraphLinkType linkType)
        : base(source, target)
    {
        LinkType = linkType;
        Color = GetColorForLinkType(linkType);
        CssClass = GetCssClassForLinkType(linkType);
        Label = GetLabelForLinkType(linkType);
    }

    private string GetColorForLinkType(SpawnGraphLinkType linkType)
    {
        return linkType switch
        {
            SpawnGraphLinkType.Hierarchy => "#888888",
            SpawnGraphLinkType.SpawnScene => "#22c55e",
            SpawnGraphLinkType.SpawnSituation => "#3b82f6",
            SpawnGraphLinkType.EntityLocation => "#f97316",
            SpawnGraphLinkType.EntityNpc => "#ef4444",
            SpawnGraphLinkType.EntityRoute => "#b45309",
            _ => "#888888"
        };
    }

    private string GetCssClassForLinkType(SpawnGraphLinkType linkType)
    {
        return linkType switch
        {
            SpawnGraphLinkType.Hierarchy => "link-hierarchy",
            SpawnGraphLinkType.SpawnScene => "link-spawn-scene",
            SpawnGraphLinkType.SpawnSituation => "link-spawn-situation",
            SpawnGraphLinkType.EntityLocation => "link-entity-location",
            SpawnGraphLinkType.EntityNpc => "link-entity-npc",
            SpawnGraphLinkType.EntityRoute => "link-entity-route",
            _ => "link-hierarchy"
        };
    }

    private string GetLabelForLinkType(SpawnGraphLinkType linkType)
    {
        return linkType switch
        {
            SpawnGraphLinkType.SpawnScene => "spawns",
            SpawnGraphLinkType.SpawnSituation => "cascades",
            SpawnGraphLinkType.EntityLocation => "at",
            SpawnGraphLinkType.EntityNpc => "with",
            SpawnGraphLinkType.EntityRoute => "via",
            _ => null
        };
    }
}
