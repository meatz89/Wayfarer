/// <summary>
/// Request to spawn a scene from a template
/// HIGHLANDER: Object references only, no string IDs
/// </summary>
public class SpawnRequest
{
    public SceneTemplate Template { get; set; }
    public Location Location { get; set; }
    public NPC Npc { get; set; }
    public Player Player { get; set; }
}

public class SpawnResult
{
    public bool Success { get; set; }
    public string FailureReason { get; set; }
    public string SceneId { get; set; }
    public List<string> CreatedSituationIds { get; set; } = new();
    // Tracking properties deleted in 5-system architecture
    // Use query-based discovery: gameWorld.Locations.Where(loc => template.DependentLocations.Any(...))

    public static SpawnResult Failure(string reason)
    {
        return new SpawnResult
        {
            Success = false,
            FailureReason = reason
        };
    }
}

/// <summary>
/// Query interface for game state existence checks
/// HIGHLANDER: Object references only, no string ID lookups
/// </summary>
public interface IGameStateQuery
{
    bool HasScene(string templateId);  // Template IDs allowed (immutable archetypes)
    bool HasLocation(Location location);
    bool HasNPC(NPC npc);
}
