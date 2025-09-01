using System.Collections.Generic;

/// <summary>
/// Context for the current scene, used by literary UI system
/// </summary>
public class SceneContext
{
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public NPC TargetNPC { get; set; }

    // Tags for literary UI
    public List<PressureTag> PressureTags { get; set; } = new List<PressureTag>();
    public List<RelationshipTag> RelationshipTags { get; set; } = new List<RelationshipTag>();
    public List<DiscoveryTag> DiscoveryTags { get; set; } = new List<DiscoveryTag>();
    public List<ResourceTag> ResourceTags { get; set; } = new List<ResourceTag>();
    public List<FeelingTag> FeelingTags { get; set; } = new List<FeelingTag>();

    // Queue and deadline info
    public int MinutesUntilDeadline { get; set; }
    public int ObligationQueueSize { get; set; }

    // Attention management
    public AttentionManager AttentionManager { get; set; }
}