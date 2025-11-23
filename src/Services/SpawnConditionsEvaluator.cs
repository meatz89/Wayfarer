
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
    public bool EvaluateAll(SpawnConditions conditions, Player player, string placementId = null)
    {
        if (conditions == null)
            throw new ArgumentNullException(nameof(conditions), "SpawnConditions cannot be null. Use SpawnConditions.AlwaysEligible for unconditional spawning.");

        if (conditions.IsAlwaysEligible)
            return true; // AlwaysEligible sentinel = unconditional spawn

        // Evaluate each dimension
        bool playerStatePass = EvaluatePlayerStateConditions(conditions.PlayerState, player);
        bool worldStatePass = EvaluateWorldStateConditions(conditions.WorldState, placementId);
        bool entityStatePass = EvaluateEntityStateConditions(conditions.EntityState, player, placementId);

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

        // Check MinStats (scale thresholds)
        if (conditions.MinStats != null && conditions.MinStats.Count > 0)
        {
            foreach (KeyValuePair<ScaleType, int> kvp in conditions.MinStats)
            {
                int currentValue = GetPlayerScale(player, kvp.Key);
                if (currentValue < kvp.Value)
                {
                    return false; // Stat below threshold
                }
            }
        }

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

        // Check LocationVisits
        // NOTE: Player has LocationFamiliarity but not LocationVisits visit count tracking
        // TODO: Add Player.LocationVisits dictionary tracking when needed
        if (conditions.LocationVisits != null && conditions.LocationVisits.Count > 0)
        {
            foreach (KeyValuePair<string, int> kvp in conditions.LocationVisits)
            {
                // Resolve location name to Location object
                Location location = _gameWorld.Locations.FirstOrDefault(loc => loc.Name == kvp.Key);
                if (location == null) continue; // Skip if location not found

                // For now, use LocationFamiliarity as proxy (familiarity increases with visits)
                int familiarityLevel = player.GetLocationFamiliarity(location);
                // Rough mapping: 0 visits = 0 familiarity, 3+ visits = 3 familiarity
                if (familiarityLevel < Math.Min(kvp.Value, 3))
                {
                    return false; // Location visit count/familiarity below threshold
                }
            }
        }

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
    private bool EvaluateWorldStateConditions(WorldStateConditions conditions, string placementId)
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

        // Check LocationStates (if placement is a Location)
        if (conditions.LocationStates != null && conditions.LocationStates.Count > 0 && !string.IsNullOrEmpty(placementId))
        {
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Name == placementId);
            if (location != null)
            {
                foreach (StateType requiredState in conditions.LocationStates)
                {
                    // StateType validation removed - time-specific properties eliminated
                    // Location capabilities are now static flags, not temporal states
                }
            }
        }

        return true; // All world state conditions met
    }

    /// <summary>
    /// Evaluate EntityStateConditions - relationship and reputation requirements
    /// Empty dictionaries/lists = no restrictions in that category
    /// </summary>
    private bool EvaluateEntityStateConditions(EntityStateConditions conditions, Player player, string placementId)
    {
        if (conditions == null)
            return true; // No entity state conditions = pass

        // Check NPCBond requirements
        if (conditions.NPCBond != null && conditions.NPCBond.Count > 0)
        {
            foreach (KeyValuePair<string, int> kvp in conditions.NPCBond)
            {
                int currentBond = player.Relationships.GetLevel(kvp.Key);
                if (currentBond < kvp.Value)
                {
                    return false; // NPC bond below threshold
                }
            }
        }

        // Check LocationReputation requirements
        if (conditions.LocationReputation != null && conditions.LocationReputation.Count > 0)
        {
            foreach (KeyValuePair<string, int> kvp in conditions.LocationReputation)
            {
                // NOTE: Location reputation system not yet implemented in Player
                // When implemented, query player.GetLocationReputation(locationId)
                // For now, skip this check (treat as passed)
                // TODO: Implement when Player.LocationReputation tracking added
            }
        }

        // Check RouteTravelCount requirements
        if (conditions.RouteTravelCount != null && conditions.RouteTravelCount.Count > 0)
        {
            foreach (KeyValuePair<string, int> kvp in conditions.RouteTravelCount)
            {
                // HIGHLANDER: Resolve route name to RouteOption object
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Name == kvp.Key);
                if (route == null)
                {
                    return false; // Route not found
                }

                // NOTE: Player has RouteFamiliarity but not RouteTravelCount
                // Use familiarity as proxy for travel count (familiarity correlates with travel frequency)
                // HIGHLANDER: Pass RouteOption object to Player API
                int routeFamiliarity = player.GetRouteFamiliarity(route);
                // Rough mapping: 0 travels = 0 familiarity, 5+ travels = 5 familiarity
                if (routeFamiliarity < Math.Min(kvp.Value, 5))
                {
                    return false; // Route travel count/familiarity below threshold
                }
            }
        }

        // Check Properties requirements (entity-specific properties)
        if (conditions.Properties != null && conditions.Properties.Count > 0 && !string.IsNullOrEmpty(placementId))
        {
            // Properties vary by entity type - determine type from placement
            // NOTE: This is a simplified check. Full implementation would need to determine entity type
            // and check appropriate properties (NPC personality, Location properties, Route terrain, etc.)
            // For now, treat as passed (properties system fully functional but not queried here)
            // TODO: Implement full property checking when needed
        }

        return true; // All entity state conditions met
    }
}
