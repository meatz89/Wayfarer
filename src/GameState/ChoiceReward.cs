/// <summary>
/// Rewards granted when player selects a Choice
/// Defines all outcomes: resources, relationships, states, and Scene spawning
/// Sir Brante pattern: Rewards are HIDDEN until Choice selected
/// </summary>
public class ChoiceReward
{
    /// <summary>
    /// Coin amount granted (positive) or deducted (negative)
    /// </summary>
    public int Coins { get; set; } = 0;

    /// <summary>
    /// Resolve amount granted (positive) or deducted (negative)
    /// </summary>
    public int Resolve { get; set; } = 0;

    /// <summary>
    /// Time advancement in segments (4 segments per time block)
    /// </summary>
    public int TimeSegments { get; set; } = 0;

    /// <summary>
    /// Bond changes with NPCs
    /// Can strengthen or weaken relationships
    /// </summary>
    public List<BondChange> BondChanges { get; set; } = new List<BondChange>();

    /// <summary>
    /// Scale shifts for player behavioral spectrum
    /// Tracks reputation across moral/behavioral axes
    /// </summary>
    public List<ScaleShift> ScaleShifts { get; set; } = new List<ScaleShift>();

    /// <summary>
    /// States granted or removed
    /// Temporary conditions affecting player capabilities
    /// </summary>
    public List<StateApplication> StateApplications { get; set; } = new List<StateApplication>();

    /// <summary>
    /// Achievements granted
    /// Permanent player accomplishments
    /// </summary>
    public List<string> AchievementIds { get; set; } = new List<string>();

    /// <summary>
    /// Items granted to player inventory
    /// Equipment, consumables, quest items
    /// </summary>
    public List<string> ItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Scenes to spawn as consequences of this Choice
    /// CRITICAL FEATURE: Choices spawn new narrative chains
    /// Creates dynamic cascading storylines
    /// </summary>
    public List<SceneSpawnReward> ScenesToSpawn { get; set; } = new List<SceneSpawnReward>();
}
