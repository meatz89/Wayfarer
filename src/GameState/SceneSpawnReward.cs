
/// <summary>
/// Defines a Scene to be spawned as a Choice reward
/// Part of Scene-Situation architecture where Choices can spawn new Scenes dynamically
/// Spawned scene uses SceneTemplate.PlacementFilter (categorical specification)
/// Supports parametric spawning: parent scene passes parameters to spawned scene
/// </summary>
public class SceneSpawnReward
{
    /// <summary>
    /// Template identifier of Scene to spawn
    /// References SceneTemplate in GameWorld.SceneTemplates
    /// Spawned scene inherits SceneTemplate.PlacementFilter for entity resolution
    /// </summary>
    public string SceneTemplateId { get; set; }

    /// <summary>
    /// Parameters passed to spawned Scene for dynamic configuration
    /// Enables parametric spawning: parent scene choice determines spawned scene behavior
    /// Example: A2 negotiation sets ContractPayment parameter, A3 reads it for reward calculation
    /// Keys: Parameter names (e.g., "ContractPayment", "Difficulty", "TimeLimit")
    /// Values: Integer values (coins, thresholds, segments, etc.)
    /// Empty dictionary = no parameters (standard spawning)
    /// </summary>
    public Dictionary<string, int> Parameters { get; set; } = new Dictionary<string, int>();
}
