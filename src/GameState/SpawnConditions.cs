
/// <summary>
/// SpawnConditions - temporal eligibility criteria for conditional SceneTemplate spawning
/// THREE EVALUATION DIMENSIONS: PlayerState (progress, history), WorldState (time, weather), EntityState (relationships, reputation)
/// COMBINATION LOGIC: AND (all conditions must pass) or OR (any condition passes)
/// SENTINEL VALUE: Use SpawnConditions.AlwaysEligible for unconditional spawning (not null)
/// SCOPE: Scene-level spawning (when does SceneTemplate become eligible)
/// DISTINCTION: SituationSpawnConditions is for recursive situation spawning, this is for scene eligibility
/// ARCHITECTURE: Parse-time data structure (no runtime generation), evaluated by SpawnConditionsEvaluator service
/// </summary>
public record SpawnConditions
{
    /// <summary>
    /// Sentinel value indicating scene is always eligible (no temporal filtering)
    /// Use this instead of null to explicitly represent unconditional spawning
    /// DDD pattern: Explicit domain concept, not implicit null check
    /// </summary>
    public static readonly SpawnConditions AlwaysEligible = new SpawnConditions
    {
        IsAlwaysEligible = true,
        PlayerState = new PlayerStateConditions(),
        WorldState = new WorldStateConditions(),
        EntityState = new EntityStateConditions(),
        CombinationLogic = CombinationLogic.AND
    };

    /// <summary>
    /// Flag indicating this is the AlwaysEligible sentinel value
    /// Internal use only - evaluator checks this flag first
    /// </summary>
    internal bool IsAlwaysEligible { get; init; } = false;
    /// <summary>
    /// Player state conditions - progression and history requirements
    /// Checks player's completed content, choices made, stats, items, location visits
    /// </summary>
    public PlayerStateConditions PlayerState { get; init; } = new PlayerStateConditions();

    /// <summary>
    /// World state conditions - temporal and environmental requirements
    /// Checks current weather, time of day, day number, location states
    /// </summary>
    public WorldStateConditions WorldState { get; init; } = new WorldStateConditions();

    /// <summary>
    /// Entity state conditions - relationship and reputation requirements
    /// Checks NPC bonds, location reputation, route travel counts, entity properties
    /// </summary>
    public EntityStateConditions EntityState { get; init; } = new EntityStateConditions();

    /// <summary>
    /// Combination logic for multi-dimension conditions
    /// AND: ALL three dimensions must pass (strict gating)
    /// OR: ANY dimension passing allows spawn (flexible eligibility)
    /// </summary>
    public CombinationLogic CombinationLogic { get; init; } = CombinationLogic.AND;
}

/// <summary>
/// Player state conditions - progression and history
/// Validates player's completed content, choices made, stats, inventory, exploration
/// </summary>
public record PlayerStateConditions
{
    /// <summary>
    /// Scene IDs that player must have completed
    /// Enables consequence chains: "This scene spawns only after player completed intro scene"
    /// Empty list = no scene completion requirements
    /// </summary>
    public List<string> CompletedScenes { get; init; } = new List<string>();

    /// <summary>
    /// Choice IDs that player must have selected
    /// Enables branching narrative: "This scene spawns only if player chose to help Marcus"
    /// Empty list = no choice history requirements
    /// </summary>
    public List<string> ChoiceHistory { get; init; } = new List<string>();

    /// <summary>
    /// Minimum stat requirements (scale thresholds)
    /// Key = ScaleType, Value = minimum threshold
    /// Example: {Morality: 5} = spawn only if player Morality >= 5
    /// Empty dictionary = no stat requirements
    /// </summary>
    public Dictionary<ScaleType, int> MinStats { get; init; } = new Dictionary<ScaleType, int>();

    /// <summary>
    /// Required inventory items
    /// Player must possess these item IDs
    /// Example: ["investigation_notes"] = spawn only if player has notes
    /// Empty list = no item requirements
    /// </summary>
    public List<string> RequiredItems { get; init; } = new List<string>();

    /// <summary>
    /// Location visit count requirements
    /// Key = LocationId, Value = minimum visit count
    /// Example: {"tavern": 3} = spawn only if player visited tavern 3+ times
    /// Empty dictionary = no visit requirements
    /// </summary>
    public Dictionary<string, int> LocationVisits { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Required tags for scene eligibility (TAG-BASED PROGRESSION SYSTEM)
    /// Player must have ALL these tags granted through prior scene completions
    /// Enables flexible branching: A1 grants ['tutorial_complete'] → A2 OR B1 both require ['tutorial_complete']
    /// Replaces rigid CompletedScenes chains with flexible tag unlock graphs
    /// Example: ["met_innkeeper", "knows_location"] = spawn only if player has both tags
    /// Empty list = no tag requirements
    /// </summary>
    public List<string> RequiresTags { get; init; } = new List<string>();
}

/// <summary>
/// World state conditions - temporal and environmental
/// Validates current time, weather, day progression, location states
/// </summary>
public record WorldStateConditions
{
    /// <summary>
    /// Required weather state
    /// null = any weather allowed
    /// Specific value = spawn only in this weather
    /// Example: WeatherCondition.Rain = spawn only during rain
    /// </summary>
    public WeatherCondition? Weather { get; init; }

    /// <summary>
    /// Required time block
    /// null = any time allowed
    /// Specific value = spawn only in this time block
    /// Example: TimeBlocks.Night = spawn only at night
    /// </summary>
    public TimeBlocks? TimeBlock { get; init; }

    /// <summary>
    /// Minimum day requirement
    /// null = no minimum day
    /// Specific value = spawn only on/after this day
    /// Example: 7 = spawn only on day 7 or later
    /// </summary>
    public int? MinDay { get; init; }

    /// <summary>
    /// Maximum day requirement
    /// null = no maximum day
    /// Specific value = spawn only before/on this day
    /// Example: 14 = spawn only on day 14 or earlier
    /// </summary>
    public int? MaxDay { get; init; }

    /// <summary>
    /// Required location states
    /// Location must have ALL these states active
    /// Example: ["flooded", "evacuated"] = spawn only at flooded, evacuated locations
    /// Empty list = no location state requirements
    /// </summary>
    public List<StateType> LocationStates { get; init; } = new List<StateType>();
}

/// <summary>
/// Entity state conditions - relationships and reputation
/// Validates NPC bonds, location reputation, route familiarity, entity properties
/// </summary>
public record EntityStateConditions
{
    /// <summary>
    /// NPC bond requirements
    /// Key = NpcId, Value = minimum bond strength
    /// Example: {"elena": 10} = spawn only if Elena bond >= 10
    /// Empty dictionary = no bond requirements
    /// </summary>
    public Dictionary<string, int> NPCBond { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Location reputation requirements
    /// Key = LocationId, Value = minimum reputation score
    /// Example: {"market_square": 5} = spawn only if market reputation >= 5
    /// Empty dictionary = no reputation requirements
    /// </summary>
    public Dictionary<string, int> LocationReputation { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Route travel count requirements
    /// Key = RouteId, Value = minimum travel count
    /// Example: {"mountain_pass": 2} = spawn only if traveled mountain pass 2+ times
    /// Empty dictionary = no route travel requirements
    /// </summary>
    public Dictionary<string, int> RouteTravelCount { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Required entity properties
    /// Entity must have ALL these properties
    /// Property interpretation varies by entity type (NPC, Location, Route)
    /// Example: ["Dangerous", "Remote"] = spawn only at dangerous, remote entities
    /// Empty list = no property requirements
    /// </summary>
    public List<string> Properties { get; init; } = new List<string>();
}

/// <summary>
/// Combination logic for multi-condition evaluation
/// Determines how multiple condition dimensions are combined
/// </summary>
public enum CombinationLogic
{
    /// <summary>
    /// ALL conditions must pass (strict gating)
    /// PlayerState AND WorldState AND EntityState must all be satisfied
    /// </summary>
    AND,

    /// <summary>
    /// ANY condition passing allows spawn (flexible eligibility)
    /// PlayerState OR WorldState OR EntityState satisfying is sufficient
    /// </summary>
    OR
}
