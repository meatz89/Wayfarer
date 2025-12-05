/// <summary>
/// DTO for unified Consequence - all costs and rewards in single structure.
/// DESIGN: Negative values = costs, positive values = rewards
/// Example: Coins = -5 means pay 5 coins, Coins = 10 means gain 10 coins
///
/// Perfect Information: All consequences visible BEFORE selection (Sir Brante pattern)
/// </summary>
public class ConsequenceDTO
{
    // ============================================
    // RESOURCE CHANGES (negative = cost, positive = reward)
    // ============================================

    /// <summary>
    /// Currency change (negative = pay, positive = earn)
    /// </summary>
    public int Coins { get; set; } = 0;

    /// <summary>
    /// Resolve/willpower change (negative = spend, positive = restore)
    /// </summary>
    public int Resolve { get; set; } = 0;

    /// <summary>
    /// Time advancement in segments (4 segments per time block)
    /// Always positive (time only moves forward)
    /// </summary>
    public int TimeSegments { get; set; } = 0;

    /// <summary>
    /// Health change (negative = damage, positive = healing)
    /// </summary>
    public int Health { get; set; } = 0;

    /// <summary>
    /// Hunger change (positive = more hungry, negative = less hungry)
    /// Note: Positive is BAD for hunger
    /// </summary>
    public int Hunger { get; set; } = 0;

    /// <summary>
    /// Stamina change (negative = exhaust, positive = recover)
    /// </summary>
    public int Stamina { get; set; } = 0;

    /// <summary>
    /// Focus/mental energy change (negative = deplete, positive = restore)
    /// </summary>
    public int Focus { get; set; } = 0;

    // ============================================
    // FIVE STATS (Sir Brante pattern: direct grants)
    // ============================================

    /// <summary>Five Stat: Insight change</summary>
    public int Insight { get; set; } = 0;

    /// <summary>Five Stat: Rapport change</summary>
    public int Rapport { get; set; } = 0;

    /// <summary>Five Stat: Authority change</summary>
    public int Authority { get; set; } = 0;

    /// <summary>Five Stat: Diplomacy change</summary>
    public int Diplomacy { get; set; } = 0;

    /// <summary>Five Stat: Cunning change</summary>
    public int Cunning { get; set; } = 0;

    // ============================================
    // TIME ADVANCEMENT (overrides TimeSegments)
    // ============================================

    /// <summary>
    /// Advance to specific time block (overrides TimeSegments)
    /// Values: "Morning", "Midday", "Afternoon", "Evening"
    /// null = use TimeSegments instead
    /// </summary>
    public string AdvanceToBlock { get; set; }

    /// <summary>
    /// Advance to next day
    /// Values: "CurrentDay", "NextDay"
    /// null = stay on current day
    /// </summary>
    public string AdvanceToDay { get; set; }

    /// <summary>
    /// Full recovery of all resources to maximum
    /// Used for securing room at inn - restores Health/Stamina/Focus to max, Hunger to 0
    /// </summary>
    public bool FullRecovery { get; set; } = false;

    // ============================================
    // RELATIONSHIP CONSEQUENCES
    // ============================================

    /// <summary>
    /// Bond changes with NPCs (can strengthen or weaken relationships)
    /// </summary>
    public List<BondChangeDTO> BondChanges { get; set; } = new List<BondChangeDTO>();

    /// <summary>
    /// Scale shifts for player behavioral spectrum
    /// </summary>
    public List<ScaleShiftDTO> ScaleShifts { get; set; } = new List<ScaleShiftDTO>();

    /// <summary>
    /// States granted or removed
    /// </summary>
    public List<StateApplicationDTO> StateApplications { get; set; } = new List<StateApplicationDTO>();

    // ============================================
    // PROGRESSION CONSEQUENCES
    // ============================================

    /// <summary>
    /// Achievement IDs to grant (resolved to objects at parse-time)
    /// </summary>
    public List<string> AchievementIds { get; set; } = new List<string>();

    /// <summary>
    /// Item IDs to grant (resolved to objects at parse-time)
    /// </summary>
    public List<string> ItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Item IDs to remove from player inventory
    /// </summary>
    public List<string> ItemsToRemove { get; set; } = new List<string>();

    /// <summary>
    /// Scenes to spawn as consequences (cascading narrative)
    /// </summary>
    public List<SceneSpawnRewardDTO> ScenesToSpawn { get; set; } = new List<SceneSpawnRewardDTO>();

    // ============================================
    // FLOW CONTROL (HIGHLANDER: all flow through choices)
    // ============================================

    /// <summary>
    /// Template ID of next situation to activate within same scene.
    /// Mutually exclusive with IsTerminal.
    /// </summary>
    public string NextSituationTemplateId { get; set; }

    /// <summary>
    /// When true, this choice ends the current scene.
    /// </summary>
    public bool IsTerminal { get; set; }
}
