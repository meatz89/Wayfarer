using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

public class ChoiceNodeModel : NodeModel
{
    public ChoiceExecutionNode SpawnNode { get; }

    public string ActionText => SpawnNode?.ActionText ?? "Choice";
    public string TruncatedText => ActionText.Length > 30 ? ActionText.Substring(0, 27) + "..." : ActionText;
    public ChoicePathType PathType => SpawnNode?.PathType ?? ChoicePathType.Fallback;
    public ChoiceActionType ActionType => SpawnNode?.ActionType ?? ChoiceActionType.Instant;
    public bool? ChallengeSucceeded => SpawnNode?.ChallengeSucceeded;
    public bool WasExecuted => SpawnNode != null;

    public ChoiceNodeModel(Point position, ChoiceExecutionNode spawnNode) : base(position)
    {
        SpawnNode = spawnNode;
    }
}
