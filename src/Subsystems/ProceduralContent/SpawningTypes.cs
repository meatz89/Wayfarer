public class SpawnRequest
{
    public SceneTemplate Template { get; set; }
    public string LocationId { get; set; }
    public string NpcId { get; set; }
    public string PlayerId { get; set; }
}

public class SpawnResult
{
    public bool Success { get; set; }
    public string FailureReason { get; set; }
    public string SceneId { get; set; }
    public List<string> CreatedSituationIds { get; set; } = new();
    public List<string> CreatedLocationIds { get; set; } = new();
    public List<string> CreatedItemIds { get; set; } = new();
    public Dictionary<string, string> MarkerResolutionMap { get; set; } = new();

    public static SpawnResult Failure(string reason)
    {
        return new SpawnResult
        {
            Success = false,
            FailureReason = reason
        };
    }
}

public interface IGameStateQuery
{
    bool HasScene(string templateId);
    bool HasLocation(string locationId);
    bool HasNPC(string npcId);
}
