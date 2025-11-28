/// <summary>
/// Unified representation of all costs and rewards for any action.
/// Used by both atmospheric actions (Travel, Work, Rest) and scene-based actions (ChoiceTemplates).
///
/// DESIGN: Single property per resource type (negative = cost, positive = reward)
/// Example: Coins = -5 means pay 5 coins, Coins = 10 means gain 10 coins
///
/// HYBRID PATTERN: Provides projection methods (pure functions), mutation delegated to ConsequenceApplicationService
/// </summary>
public class Consequence
{
    // ============================================
    // RESOURCE CHANGES (negative = cost, positive = reward)
    // ============================================

    /// <summary>
    /// Currency change (negative = pay, positive = earn)
    /// </summary>
    public int Coins { get; init; } = 0;

    /// <summary>
    /// Resolve/willpower change (negative = spend, positive = restore)
    /// </summary>
    public int Resolve { get; init; } = 0;

    /// <summary>
    /// Time advancement in segments (4 segments per time block)
    /// Always positive (time only moves forward)
    /// </summary>
    public int TimeSegments { get; init; } = 0;

    /// <summary>
    /// Health change (negative = damage, positive = healing)
    /// </summary>
    public int Health { get; init; } = 0;

    /// <summary>
    /// Hunger change (positive = more hungry, negative = less hungry)
    /// Note: Positive is BAD for hunger (increases hunger level)
    /// </summary>
    public int Hunger { get; init; } = 0;

    /// <summary>
    /// Stamina change (negative = exhaust, positive = recover)
    /// </summary>
    public int Stamina { get; init; } = 0;

    /// <summary>
    /// Focus/mental energy change (negative = deplete, positive = restore)
    /// </summary>
    public int Focus { get; init; } = 0;

    // ============================================
    // FIVE STATS (Sir Brante pattern: direct grants)
    // ============================================

    /// <summary>Five Stat: Insight change (investigation, observation, puzzle-solving)</summary>
    public int Insight { get; init; } = 0;

    /// <summary>Five Stat: Rapport change (building trust, friendly persuasion)</summary>
    public int Rapport { get; init; } = 0;

    /// <summary>Five Stat: Authority change (intimidation, command presence)</summary>
    public int Authority { get; init; } = 0;

    /// <summary>Five Stat: Diplomacy change (formal negotiation, authority appeal)</summary>
    public int Diplomacy { get; init; } = 0;

    /// <summary>Five Stat: Cunning change (deception, misdirection, reading situations)</summary>
    public int Cunning { get; init; } = 0;

    // ============================================
    // TIME ADVANCEMENT (overrides TimeSegments)
    // ============================================

    /// <summary>
    /// Advance to specific time block (overrides TimeSegments)
    /// Used for major time jumps (e.g., sleep until morning)
    /// </summary>
    public TimeBlocks? AdvanceToBlock { get; init; }

    /// <summary>
    /// Advance to next day (or stay on current day)
    /// Used with AdvanceToBlock for overnight transitions
    /// </summary>
    public DayAdvancement? AdvanceToDay { get; init; }

    /// <summary>
    /// Full recovery of all resources to maximum
    /// Used for securing room at inn - restores Health/Stamina/Focus to max, Hunger to 0
    /// </summary>
    public bool FullRecovery { get; init; } = false;

    // ============================================
    // RELATIONSHIP CONSEQUENCES
    // ============================================

    /// <summary>
    /// Bond changes with NPCs (can strengthen or weaken relationships)
    /// </summary>
    public List<BondChange> BondChanges { get; init; } = new List<BondChange>();

    /// <summary>
    /// Scale shifts for player behavioral spectrum (tracks reputation across moral/behavioral axes)
    /// </summary>
    public List<ScaleShift> ScaleShifts { get; init; } = new List<ScaleShift>();

    /// <summary>
    /// States granted or removed (temporary conditions affecting player capabilities)
    /// </summary>
    public List<StateApplication> StateApplications { get; init; } = new List<StateApplication>();

    // ============================================
    // PROGRESSION CONSEQUENCES
    // ============================================

    /// <summary>
    /// Achievements granted (permanent player accomplishments)
    /// HIGHLANDER: Object references only, no string IDs
    /// </summary>
    public List<Achievement> Achievements { get; init; } = new List<Achievement>();

    /// <summary>
    /// Items granted to player inventory (equipment, consumables, quest items)
    /// HIGHLANDER: Object references only, no string IDs
    /// </summary>
    public List<Item> Items { get; init; } = new List<Item>();

    /// <summary>
    /// Items to remove from player inventory (cleanup, consuming keys, etc.)
    /// HIGHLANDER: Object references only, no string IDs
    /// </summary>
    public List<Item> ItemsToRemove { get; init; } = new List<Item>();

    /// <summary>
    /// Scenes to spawn as consequences (creates dynamic cascading storylines)
    /// </summary>
    public List<SceneSpawnReward> ScenesToSpawn { get; init; } = new List<SceneSpawnReward>();

    // ============================================
    // QUERY METHODS (Pure projections, no side effects)
    // ============================================

    /// <summary>
    /// Check if this consequence has any costs (negative resource changes).
    /// Used by UI to determine whether to show COSTS section.
    /// </summary>
    public bool HasAnyCosts()
    {
        return Coins < 0 ||
               Resolve < 0 ||
               Health < 0 ||
               Stamina < 0 ||
               Focus < 0 ||
               Hunger > 0 || // Hunger increase is a cost (bad for player)
               TimeSegments > 0; // Time passing is a cost
    }

    /// <summary>
    /// Check if this consequence has any rewards (positive changes).
    /// Used by UI to determine whether to show REWARDS section.
    /// </summary>
    public bool HasAnyRewards()
    {
        return Coins > 0 ||
               Resolve > 0 ||
               Health > 0 ||
               Stamina > 0 ||
               Focus > 0 ||
               Hunger < 0 || // Hunger decrease is a reward (good for player)
               FullRecovery ||
               Insight != 0 ||
               Rapport != 0 ||
               Authority != 0 ||
               Diplomacy != 0 ||
               Cunning != 0 ||
               BondChanges.Count > 0 ||
               ScaleShifts.Count > 0 ||
               StateApplications.Count > 0 ||
               Achievements.Count > 0 ||
               Items.Count > 0 ||
               ScenesToSpawn.Count > 0;
    }

    /// <summary>
    /// Check if this consequence has any effect at all.
    /// REPLACES the 15+ field sprawl in SceneContent.razor:79-87.
    /// </summary>
    public bool HasAnyEffect()
    {
        return HasAnyCosts() ||
               HasAnyRewards() ||
               AdvanceToBlock.HasValue ||
               ItemsToRemove.Count > 0;
    }

    /// <summary>
    /// Project what player's state would be AFTER applying this consequence.
    /// Used for Perfect Information display (Sir Brante pattern).
    /// Does NOT mutate player - purely a projection.
    /// </summary>
    public PlayerStateProjection GetProjectedState(Player player)
    {
        if (FullRecovery)
        {
            return new PlayerStateProjection
            {
                Coins = player.Coins + Coins,
                Resolve = player.Resolve + Resolve,
                Health = player.MaxHealth,
                Stamina = player.MaxStamina,
                Focus = player.MaxFocus,
                Hunger = 0,
                Insight = player.Insight + Insight,
                Rapport = player.Rapport + Rapport,
                Authority = player.Authority + Authority,
                Diplomacy = player.Diplomacy + Diplomacy,
                Cunning = player.Cunning + Cunning
            };
        }

        return new PlayerStateProjection
        {
            Coins = player.Coins + Coins,
            Resolve = player.Resolve + Resolve,
            Health = Math.Clamp(player.Health + Health, 0, player.MaxHealth),
            Stamina = Math.Clamp(player.Stamina + Stamina, 0, player.MaxStamina),
            Focus = Math.Clamp(player.Focus + Focus, 0, player.MaxFocus),
            Hunger = Math.Clamp(player.Hunger + Hunger, 0, player.MaxHunger),
            Insight = player.Insight + Insight,
            Rapport = player.Rapport + Rapport,
            Authority = player.Authority + Authority,
            Diplomacy = player.Diplomacy + Diplomacy,
            Cunning = player.Cunning + Cunning
        };
    }

    // ============================================
    // FACTORY METHODS
    // ============================================

    /// <summary>Create an empty consequence with no effects</summary>
    public static Consequence None() => new Consequence();
}
