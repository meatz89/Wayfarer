using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

public class SceneNodeModel : NodeModel
{
    public SceneSpawnNode SpawnNode { get; }

    public string DisplayName => SpawnNode?.DisplayName ?? "Scene";
    public StoryCategory Category => SpawnNode?.Category ?? StoryCategory.Service;
    public SceneState CurrentState => SpawnNode?.CurrentState ?? SceneState.Active;
    public bool IsProcedurallyGenerated => SpawnNode?.IsProcedurallyGenerated ?? false;
    public int SituationCount => SpawnNode?.SituationCount ?? 0;

    public SceneNodeModel(Point position, SceneSpawnNode spawnNode) : base(position)
    {
        SpawnNode = spawnNode;
    }
}
