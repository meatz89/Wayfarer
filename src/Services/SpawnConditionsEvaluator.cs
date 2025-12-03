
/// <summary>
/// SpawnConditionsEvaluator - evaluates SpawnConditions against current game state
/// Determines if SceneTemplate is eligible to spawn based on temporal/state-based conditions
/// Three evaluation dimensions: PlayerState, WorldState, EntityState
/// Called at instantiation time (Scene/Situation spawn), NOT at parse time
/// NOT a catalogue - evaluates runtime state, doesn't translate categorical values
/// </summary>
public class SpawnConditionsEvaluator
{
    private readonly GameWorld _gameWorld;

    public SpawnConditionsEvaluator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Evaluate all spawn conditions for a SceneTemplate
    /// SpawnConditions.AlwaysEligible = always eligible (no temporal filtering)
    /// Other SpawnConditions = evaluate three dimensions and combine via CombinationLogic
    /// DDD pattern: Explicit sentinel value check, not implicit null check
    /// </summary>
    public bool EvaluateAll(SpawnConditions conditions)
    {
        if (conditions == null)
            throw new ArgumentNullException(nameof(conditions), "SpawnConditions cannot be null. Use SpawnConditions.AlwaysEligible for unconditional spawning.");

        if (conditions.IsAlwaysEligible)
            return true; // AlwaysEligible sentinel = unconditional spawn

        Player player = _gameWorld.GetPlayer();

        // Evaluate each dimension
        bool playerStatePass = EvaluatePlayerStateConditions(conditions.PlayerState, player);
        bool worldStatePass = EvaluateWorldStateConditions(conditions.WorldState);
        bool entityStatePass = EvaluateEntityStateConditions(conditions.EntityState);

        // Combine results via CombinationLogic
        return conditions.CombinationLogic switch
        {
            CombinationLogic.AND => playerStatePass && worldStatePass && entityStatePass,
            CombinationLogic.OR => playerStatePass || worldStatePass || entityStatePass,
            _ => throw new InvalidOperationException($"Unknown CombinationLogic: {conditions.CombinationLogic}")
        };
    }

    /// <summary>
    /// Evaluate PlayerStateConditions - progression and history requirements
    /// Empty lists/dictionaries = no restrictions in that category
    /// </summary>
    private bool EvaluatePlayerStateConditions(PlayerStateConditions conditions, Player player)
    {
        if (conditions == null)
            return true; // No player state conditions = pass

        // Check MinStats (scale thresholds) - DOMAIN COLLECTION PRINCIPLE: Explicit properties
        if (conditions.MinMorality.HasValue && GetPlayerScale(player, ScaleType.Morality) < conditions.MinMorality.Value)
            return false;
        if (conditions.MinLawfulness.HasValue && GetPlayerScale(player, ScaleType.Lawfulness) < conditions.MinLawfulness.Value)
            return false;
        if (conditions.MinMethod.HasValue && GetPlayerScale(player, ScaleType.Method) < conditions.MinMethod.Value)
            return false;
        if (conditions.MinCaution.HasValue && GetPlayerScale(player, ScaleType.Caution) < conditions.MinCaution.Value)
            return false;
        if (conditions.MinTransparency.HasValue && GetPlayerScale(player, ScaleType.Transparency) < conditions.MinTransparency.Value)
            return false;
        if (conditions.MinFame.HasValue && GetPlayerScale(player, ScaleType.Fame) < conditions.MinFame.Value)
            return false;

        // Check RequiredItems
        if (conditions.RequiredItems != null && conditions.RequiredItems.Count > 0)
        {
            List<Item> items = player.Inventory.GetAllItems();
            foreach (string itemId in conditions.RequiredItems)
            {
                if (!items.Any(item => item.Name == itemId))
                {
                    return false; // Required item not possessed
                }
            }
        }

        // LocationVisits DELETED - §8.30: SpawnConditions must reference existing entities via object refs

        return true; // All player state conditions met
    }

    /// <summary>
    /// Get player scale value by ScaleType enum
    /// </summary>
    private int GetPlayerScale(Player player, ScaleType scaleType)
    {
        return scaleType switch
        {
            ScaleType.Morality => player.Scales.Morality,
            ScaleType.Lawfulness => player.Scales.Lawfulness,
            ScaleType.Method => player.Scales.Method,
            ScaleType.Caution => player.Scales.Caution,
            ScaleType.Transparency => player.Scales.Transparency,
            ScaleType.Fame => player.Scales.Fame,
            _ => 0
        };
    }

    /// <summary>
    /// Evaluate WorldStateConditions - temporal and environmental requirements
    /// null properties = no restrictions in that category
    /// </summary>
    private bool EvaluateWorldStateConditions(WorldStateConditions conditions)
    {
        if (conditions == null)
            return true; // No world state conditions = pass

        // Check Weather
        if (conditions.Weather.HasValue)
        {
            if (_gameWorld.CurrentWeather != conditions.Weather.Value)
            {
                return false; // Weather doesn't match
            }
        }

        // Check TimeBlock
        if (conditions.TimeBlock.HasValue)
        {
            if (_gameWorld.CurrentTimeBlock != conditions.TimeBlock.Value)
            {
                return false; // TimeBlock doesn't match
            }
        }

        // Check CurrentDay (min/max range)
        if (conditions.MinDay.HasValue)
        {
            if (_gameWorld.CurrentDay < conditions.MinDay.Value)
            {
                return false; // Too early
            }
        }

        if (conditions.MaxDay.HasValue)
        {
            if (_gameWorld.CurrentDay > conditions.MaxDay.Value)
            {
                return false; // Too late
            }
        }

        return true; // All world state conditions met
    }

    /// <summary>
    /// Evaluate EntityStateConditions - entity property requirements
    /// §8.30: NPCBond, LocationReputation, RouteTravelCount DELETED - must use object refs not string IDs
    /// </summary>
    private bool EvaluateEntityStateConditions(EntityStateConditions conditions)
    {
        if (conditions == null)
            return true; // No entity state conditions = pass

        return true; // All entity state conditions met
    }
}
