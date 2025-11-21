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
    /// Advance to specific time block (overrides TimeSegments)
    /// Used for major time jumps (e.g., sleep until morning)
    /// </summary>
    public TimeBlocks? AdvanceToBlock { get; set; }

    /// <summary>
    /// Advance to next day (or stay on current day)
    /// Used with AdvanceToBlock for overnight transitions
    /// </summary>
    public DayAdvancement? AdvanceToDay { get; set; }

    /// <summary>
    /// Health amount granted (positive) or deducted (negative)
    /// Rest and healing restore health, injuries reduce it
    /// </summary>
    public int Health { get; set; } = 0;

    /// <summary>
    /// Hunger amount changed (positive increases hunger, negative decreases hunger)
    /// Eating food reduces hunger, exertion increases it
    /// </summary>
    public int Hunger { get; set; } = 0;

    /// <summary>
    /// Stamina amount granted (positive) or deducted (negative)
    /// Rest restores stamina, exertion depletes it
    /// </summary>
    public int Stamina { get; set; } = 0;

    /// <summary>
    /// Focus amount granted (positive) or deducted (negative)
    /// Rest restores focus, investigation depletes it
    /// </summary>
    public int Focus { get; set; } = 0;

    /// <summary>
    /// Five Stats - Simple integers granted or deducted
    /// Sir Brante pattern: Choices directly grant stat points (no XP system)
    /// </summary>
    public int Insight { get; set; } = 0;
    public int Rapport { get; set; } = 0;
    public int Authority { get; set; } = 0;
    public int Diplomacy { get; set; } = 0;
    public int Cunning { get; set; } = 0;

    /// <summary>
    /// Full recovery of all resources to maximum
    /// Used for securing room at inn - restores Health/Stamina/Focus to max, Hunger to 0
    /// Tutorial: Paying for lodging grants full recovery
    /// </summary>
    public bool FullRecovery { get; set; } = false;

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
    /// HIGHLANDER: Object references only, no string IDs
    /// </summary>
    public List<Achievement> Achievements { get; set; } = new List<Achievement>();

    /// <summary>
    /// Items granted to player inventory (RUNTIME - resolved entities)
    /// Equipment, consumables, quest items
    /// Part of item lifecycle: GRANT phase
    /// HIGHLANDER: Object references only, no string IDs
    /// Used at RUNTIME after scene instantiation
    /// </summary>
    public List<Item> Items { get; set; } = new List<Item>();

    /// <summary>
    /// Items to remove from player inventory (RUNTIME - resolved entities)
    /// Used for: Consuming keys, removing temporary access tokens, cleanup
    /// Part of item lifecycle: REMOVE phase
    /// Multi-Situation Scene Pattern: Situations can clean up items granted earlier in arc
    /// HIGHLANDER: Object references only, no string IDs
    /// Used at RUNTIME after scene instantiation
    /// </summary>
    public List<Item> ItemsToRemove { get; set; } = new List<Item>();

    /// <summary>
    /// Scenes to spawn as consequences of this Choice
    /// CRITICAL FEATURE: Choices spawn new narrative chains
    /// Creates dynamic cascading storylines
    /// </summary>
    public List<SceneSpawnReward> ScenesToSpawn { get; set; } = new List<SceneSpawnReward>();
}
