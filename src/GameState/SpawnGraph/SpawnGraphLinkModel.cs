using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

public enum SpawnGraphLinkType
{
    Hierarchy,
    SpawnScene,
    SpawnSituation,
    EntityLocation,
    EntityNpc,
    EntityRoute,
    ChoiceFlow  // Choice → Situation via Consequence.NextSituationTemplateId (arc42 §8.30)
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

    public SpawnGraphLinkModel(NodeModel source, NodeModel target, SpawnGraphLinkType linkType, EntityResolutionMetadata metadata)
        : base(source, target)
    {
        LinkType = linkType;
        Color = GetColorForLinkType(linkType);
        CssClass = GetCssClassForLinkType(linkType);
        Label = BuildLabelFromMetadata(metadata) ?? GetLabelForLinkType(linkType);
    }

    /// <summary>
    /// Constructor for ChoiceFlow links with custom label (arc42 §8.30)
    /// </summary>
    public SpawnGraphLinkModel(NodeModel source, NodeModel target, SpawnGraphLinkType linkType, string customLabel)
        : base(source, target)
    {
        LinkType = linkType;
        Color = GetColorForLinkType(linkType);
        CssClass = GetCssClassForLinkType(linkType);
        Label = customLabel ?? GetLabelForLinkType(linkType);
    }

    private string BuildLabelFromMetadata(EntityResolutionMetadata metadata)
    {
        if (metadata == null || metadata.Filter == null)
        {
            return null;
        }

        string outcome = metadata.Outcome switch
        {
            EntityResolutionOutcome.Discovered => "DISCOVERED",
            EntityResolutionOutcome.Created => "CREATED",
            EntityResolutionOutcome.RouteDestination => "ROUTE",
            _ => null
        };

        if (outcome == null)
        {
            return null;
        }

        List<string> properties = BuildPropertyList(metadata);
        if (properties.Count == 0)
        {
            return outcome;
        }

        return outcome + ": " + string.Join(", ", properties);
    }

    private List<string> BuildPropertyList(EntityResolutionMetadata metadata)
    {
        List<string> properties = new List<string>();
        PlacementFilterSnapshot filter = metadata.Filter;

        if (filter.Purpose != null)
        {
            properties.Add("Purpose=" + filter.Purpose);
        }
        if (filter.Privacy != null)
        {
            properties.Add("Privacy=" + filter.Privacy);
        }
        if (filter.Safety != null)
        {
            properties.Add("Safety=" + filter.Safety);
        }
        if (filter.Activity != null)
        {
            properties.Add("Activity=" + filter.Activity);
        }
        if (filter.LocationRole != null)
        {
            properties.Add("Role=" + filter.LocationRole);
        }
        if (filter.PersonalityType != null)
        {
            properties.Add("Personality=" + filter.PersonalityType);
        }
        if (filter.Profession != null)
        {
            properties.Add("Profession=" + filter.Profession);
        }
        if (filter.SocialStanding != null)
        {
            properties.Add("Standing=" + filter.SocialStanding);
        }
        if (filter.StoryRole != null)
        {
            properties.Add("StoryRole=" + filter.StoryRole);
        }
        if (filter.Terrain != null)
        {
            properties.Add("Terrain=" + filter.Terrain);
        }
        if (filter.Structure != null)
        {
            properties.Add("Structure=" + filter.Structure);
        }

        return properties;
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
            SpawnGraphLinkType.ChoiceFlow => "#a855f7",  // Purple for choice-driven flow (arc42 §8.30)
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
            SpawnGraphLinkType.ChoiceFlow => "link-choice-flow",  // Choice-driven flow style (arc42 §8.30)
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
            SpawnGraphLinkType.ChoiceFlow => "→",  // Arrow for choice-driven flow (arc42 §8.30)
            _ => null
        };
    }
}
