using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

public class SituationNodeModel : NodeModel
{
    public SituationSpawnNode SpawnNode { get; }

    public string Name => SpawnNode?.Name ?? "Situation";
    public SituationType Type => SpawnNode?.Type ?? SituationType.Normal;
    public TacticalSystemType SystemType => SpawnNode?.SystemType ?? TacticalSystemType.Social;
    public SituationInteractionType InteractionType => SpawnNode?.InteractionType ?? SituationInteractionType.Instant;
    public LifecycleStatus LifecycleStatus => SpawnNode?.LifecycleStatus ?? LifecycleStatus.Selectable;
    public bool IsHighlighted { get; set; }

    public SituationNodeModel(Point position, SituationSpawnNode spawnNode) : base(position)
    {
        SpawnNode = spawnNode;
    }
}
