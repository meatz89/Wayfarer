using System.Collections.Generic;
using System.Linq;

public static class TravelTimeMatrix
{
    // Travel times in minutes between locations
    private static readonly Dictionary<(string, string), int> travelTimes = new Dictionary<(string, string), int>
    {
        // Your Room connections (very close to Market Square)
        { ("your_room", "market_square"), 10 },
        { ("market_square", "your_room"), 10 },
        
        // Market Square as central hub
        { ("market_square", "noble_district"), 30 },
        { ("noble_district", "market_square"), 30 },

        { ("market_square", "merchant_row"), 15 },
        { ("merchant_row", "market_square"), 15 },

        { ("market_square", "city_gates"), 30 },
        { ("city_gates", "market_square"), 30 },

        { ("market_square", "riverside"), 45 },
        { ("riverside", "market_square"), 45 },
        
        // Non-hub connections (longer)
        { ("noble_district", "merchant_row"), 45 },
        { ("merchant_row", "noble_district"), 45 },

        { ("noble_district", "city_gates"), 60 },
        { ("city_gates", "noble_district"), 60 },

        { ("noble_district", "riverside"), 60 },
        { ("riverside", "noble_district"), 60 },

        { ("merchant_row", "city_gates"), 30 },
        { ("city_gates", "merchant_row"), 30 },

        { ("merchant_row", "riverside"), 60 },
        { ("riverside", "merchant_row"), 60 },

        { ("city_gates", "riverside"), 30 },
        { ("riverside", "city_gates"), 30 },
        
        // Your Room to other locations (must go through Market Square)
        { ("your_room", "noble_district"), 40 },
        { ("noble_district", "your_room"), 40 },

        { ("your_room", "merchant_row"), 25 },
        { ("merchant_row", "your_room"), 25 },

        { ("your_room", "city_gates"), 40 },
        { ("city_gates", "your_room"), 40 },

        { ("your_room", "riverside"), 55 },
        { ("riverside", "your_room"), 55 },
    };

    public static int GetTravelTime(string fromLocationId, string toLocationId)
    {
        // Same location = no travel time
        if (fromLocationId == toLocationId)
            return 0;

        // Look up travel time
        (string fromLocationId, string toLocationId) key = (fromLocationId, toLocationId);
        if (travelTimes.TryGetValue(key, out int time))
            return time;

        // No fallback - route must be defined
        throw new ArgumentException($"Travel time not defined for route {fromLocationId} -> {toLocationId} - add to travelTimes configuration");
    }

    public static Dictionary<string, int> GetTravelTimesFrom(string locationId)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        foreach (KeyValuePair<(string, string), int> kvp in travelTimes)
        {
            if (kvp.Key.Item1 == locationId)
            {
                result[kvp.Key.Item2] = kvp.Value;
            }
        }

        return result;
    }
}

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly TransportCompatibilityValidator _transportValidator;
    private readonly RouteRepository _routeRepository;
    private readonly AccessRequirementChecker _accessChecker;
    public ItemRepository ItemRepository { get; }

    public TravelManager(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        TransportCompatibilityValidator transportValidator,
        RouteRepository routeRepository,
        AccessRequirementChecker accessChecker,
        TimeManager timeManager
        )
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _transportValidator = transportValidator;
        _routeRepository = routeRepository;
        _accessChecker = accessChecker;
        ItemRepository = itemRepository;
    }

    public bool CanTravelTo(string locationId)
    {
        Location destination = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
        Location currentLocation = _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId != null ?
            _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == _gameWorld.GetPlayer().CurrentLocationSpot.LocationId) : null;

        if (destination == null || currentLocation == null)
            return false;

        // Check if any route exists and is available
        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);

        // Check if player can travel (hunger check)
        Player player = _gameWorld.GetPlayer();
        if (player.Hunger >= player.MaxHunger - 2) // Can't travel if too hungry
        {
            return false; // Too hungry to travel
        }

        return routes.Any();
    }

    public RouteOption StartLocationTravel(string locationId, TravelMethods method = TravelMethods.Walking)
    {
        Location destination = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
        Location currentLocation = _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId != null ?
            _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == _gameWorld.GetPlayer().CurrentLocationSpot.LocationId) : null;

        // Find the appropriate route

        List<RouteOption> routes = GetAvailableRoutes(currentLocation.Id, destination.Id);
        RouteOption? selectedRoute = routes.FirstOrDefault(r => r.Method == method);

        if (selectedRoute == null) return null;

        return selectedRoute;
    }

    public void TravelToLocation(RouteOption selectedRoute)
    {
        // Calculate actual costs with weather and focus modifications
        int hungerIncrease = CalculateHungerIncrease(selectedRoute);
        int adjustedCoinCost = CalculateCoinCost(selectedRoute);

        // Apply costs
        _gameWorld.GetPlayer().ModifyCoins(-adjustedCoinCost);
        _gameWorld.GetPlayer().ModifyHunger(hungerIncrease); // Travel makes you hungry

        // Record route usage for discovery mechanics
        // Route usage counting removed - violates NO USAGE COUNTERS principle
        // Routes are discovered through NPC relationships and token spending

        // Time advancement handled by GameFacade to ensure letter deadlines are updated

        // Update location
        // Get the spot from the route, then get its location
        LocationSpot targetSpot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == selectedRoute.DestinationLocationSpot);
        if (targetSpot == null)
        {
            return; // Invalid destination spot
        }

        Location targetLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == targetSpot.LocationId);
        if (targetLocation == null)
        {
            return; // Invalid location
        }

        // Use the exact spot from the route - NO FALLBACKS
        _gameWorld.GetPlayer().CurrentLocationSpot = targetSpot;

        string? currentLocation = targetLocation?.Id;

    }


    public List<RouteOption> GetAvailableRoutes(string fromLocationId, string toLocationId)
    {
        List<RouteOption> availableRoutes = new List<RouteOption>();
        Location fromLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == fromLocationId);

        // If location doesn't exist, return empty list
        if (fromLocation == null) return availableRoutes;

        // Find connection to destination
        LocationConnection connection = fromLocation.Connections.Find(c => c.DestinationLocationId == toLocationId);

        // CRITICAL FIX: If no connection exists, create a basic walking route
        // This ensures players can always travel between locations
        if (connection == null)
        {
            // Check if a travel time exists in the matrix
            int travelTime = TravelTimeMatrix.GetTravelTime(fromLocationId, toLocationId);
            if (travelTime > 0 && travelTime < 100) // Valid travel time exists
            {
                // Create a basic walking route
                RouteOption walkingRoute = new RouteOption
                {
                    Id = $"walk_{fromLocationId}_to_{toLocationId}",
                    OriginLocationSpot = fromLocationId,
                    DestinationLocationSpot = toLocationId,
                    Method = TravelMethods.Walking,
                    TravelTimeMinutes = travelTime, // Direct minutes from TravelTimeMatrix
                    BaseStaminaCost = Math.Max(1, travelTime / 30), // 1 stamina per 30 minutes
                    IsDiscovered = true, // All walking routes are discovered
                    Description = "On foot"
                    // Note: CoinCost is calculated, not set directly
                    // Note: TransportNPCId doesn't exist in RouteOption
                };
                availableRoutes.Add(walkingRoute);
                return availableRoutes;
            }
            return availableRoutes;
        }

        foreach (RouteOption route in connection.RouteOptions)
        {
            // Check if route is discovered
            if (!route.IsDiscovered)
                continue;

            // Check departure times and weather-dependent boat schedules
            if (route.DepartureTime != null && route.DepartureTime != _timeManager.GetCurrentTimeBlock())
                continue;

            // Check weather-dependent boat schedules
            if (route.Method == TravelMethods.Boat && !IsBoatScheduleAvailable(route))
                continue;

            // Check if route is temporarily blocked
            if (_routeRepository.IsRouteBlocked(route.Id))
                continue;

            // Weather no longer blocks routes - it affects efficiency instead

            // Check if route is accessible using logical blocking system
            RouteAccessResult accessResult = route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _routeRepository.GetCurrentWeather());
            if (!accessResult.IsAllowed)
                continue;

            // Check additional access requirements (token/equipment based)
            if (route.AccessRequirement != null)
            {
                AccessCheckResult requirementCheck = _accessChecker.CheckRouteAccess(route);
                if (!requirementCheck.IsAllowed)
                    continue;
            }

            availableRoutes.Add(route);
        }

        return availableRoutes;
    }




    public int CalculateCurrentFocus(GameWorld _gameWorld)
    {
        int totalFocus = 0;

        // Calculate item focus
        foreach (string itemName in _gameWorld.GetPlayer().Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    totalFocus += item.Focus;
                }
            }
        }

        // Add coin focus
        totalFocus += _gameWorld.GetPlayer().Coins / GameConstants.Inventory.COINS_PER_FOCUS_UNIT;

        return totalFocus;
    }

    public int CalculateHungerIncrease(RouteOption route)
    {
        int totalFocus = CalculateCurrentFocus(_gameWorld);
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();

        // Travel increases hunger based on route difficulty and conditions
        int baseHunger = 2; // Base hunger increase for any travel

        // Heavy load increases hunger
        if (totalFocus > GameConstants.LoadFocus.MEDIUM_LOAD_MAX)
            baseHunger += 2;
        else if (totalFocus > GameConstants.LoadFocus.LIGHT_LOAD_MAX)
            baseHunger += 1;

        // Bad weather increases hunger
        if (currentWeather == WeatherCondition.Rain || currentWeather == WeatherCondition.Snow)
            baseHunger += 1;

        return baseHunger;
    }

    public int CalculateCoinCost(RouteOption route)
    {
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();
        return route.CalculateCoinCost(currentWeather);
    }

    // Add a helper method for UI display
    public string GetFocusStatusDescription(int totalFocus)
    {
        return totalFocus switch
        {
            _ when totalFocus <= GameConstants.LoadFocus.LIGHT_LOAD_MAX => "Light load",
            _ when totalFocus <= GameConstants.LoadFocus.MEDIUM_LOAD_MAX => "Medium load (+1 hunger)",
            _ => "Heavy load (+2 hunger)"
        };
    }
    public bool CanTravel(RouteOption route)
    {
        Player player = _gameWorld.GetPlayer();
        int hungerAfterTravel = player.Hunger + CalculateHungerIncrease(route);

        bool canTravel = player.Coins >= CalculateCoinCost(route) &&
               hungerAfterTravel < player.MaxHunger; // Can't travel if it would max hunger

        return canTravel;
    }

    /// <summary>
    /// Get route access information for UI display
    /// </summary>
    public RouteAccessResult GetRouteAccessInfo(RouteOption route)
    {
        return route.CheckRouteAccess(ItemRepository, _gameWorld.GetPlayer(), _routeRepository.GetCurrentWeather());
    }

    /// <summary>
    /// Get token-based access information for UI display
    /// </summary>
    public AccessCheckResult GetTokenAccessInfo(RouteOption route)
    {
        if (route.AccessRequirement == null)
            return AccessCheckResult.Allowed();

        return _accessChecker.CheckRouteAccess(route);
    }

    // REMOVED: GetTravelEvents method - TravelEventManager system deleted
    // Travel events should be loaded from JSON templates, not generated in code

    /// <summary>
    /// Check if boat schedule is available based on weather conditions and river state
    /// </summary>
    private bool IsBoatScheduleAvailable(RouteOption route)
    {
        // Boats can't operate in poor weather conditions
        WeatherCondition currentWeather = _routeRepository.GetCurrentWeather();

        // Block boat schedules during dangerous weather
        if (currentWeather == WeatherCondition.Rain || currentWeather == WeatherCondition.Snow)
        {
            return false;
        }

        // Boats need specific departure times (no always-available boat routes)
        if (route.DepartureTime == null)
        {
            return false;
        }

        // Additional river condition logic could be added here
        // For now, boats depend on weather and scheduled departure times
        return true;
    }


    /// <summary>
    /// Get current inventory status with transport information
    /// </summary>
    public string GetInventoryStatusDescription(TravelMethods? transport = null)
    {
        Player player = _gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;

        int usedSlots = inventory.GetUsedSlots(ItemRepository);
        int maxSlots = inventory.GetMaxSlots(ItemRepository, transport);

        string transportInfo = transport.HasValue ? $" with {transport}" : "";
        return $"Inventory: {usedSlots}/{maxSlots} slots used{transportInfo}";
    }

    // ========== TRAVEL SESSION METHODS ==========

    /// <summary>
    /// Start a journey on a specific route, initializing a travel session
    /// </summary>
    public TravelSession StartJourney(string routeId)
    {
        RouteOption route = GetRoute(routeId);
        if (route == null)
        {
            return null;
        }

        // Get derived stamina based on hunger/health state
        Player player = _gameWorld.GetPlayer();
        int startingStamina = GetDerivedStamina(player);
        
        TravelSession session = new TravelSession
        {
            RouteId = routeId,
            CurrentSegment = 1,
            StaminaRemaining = startingStamina,
            StaminaCapacity = startingStamina,
            CurrentState = DetermineInitialTravelState(player),
            TimeElapsed = 0,
            CompletedSegments = new List<string>(),
            SelectedPathId = null
        };

        _gameWorld.CurrentTravelSession = session;
        return session;
    }

    /// <summary>
    /// Select and play a path card from the current segment
    /// </summary>
    public bool SelectPathCard(string pathCardId)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        if (!_gameWorld.AllPathCards.ContainsKey(pathCardId))
        {
            return false;
        }

        PathCardDTO card = _gameWorld.AllPathCards[pathCardId];
        
        // Check if player can afford the stamina cost
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        // Check coin requirement
        if (card.CoinRequirement > 0 && _gameWorld.GetPlayer().Coins < card.CoinRequirement)
        {
            return false;
        }

        // Check permit requirement
        if (!string.IsNullOrEmpty(card.PermitRequirement))
        {
            // TODO: Check player inventory for permit
            // For now, assume player has required permits
        }

        // Check one-time card usage
        if (card.IsOneTime && _gameWorld.PathCardRewardsClaimed.ContainsKey(pathCardId) 
            && _gameWorld.PathCardRewardsClaimed[pathCardId])
        {
            return false;
        }

        // Deduct costs
        session.StaminaRemaining -= card.StaminaCost;
        if (card.CoinRequirement > 0)
        {
            _gameWorld.GetPlayer().ModifyCoins(-card.CoinRequirement);
        }

        // Reveal if face-down
        if (!_gameWorld.PathCardDiscoveries.ContainsKey(pathCardId))
        {
            _gameWorld.PathCardDiscoveries[pathCardId] = true;
        }

        // Apply effects
        ApplyPathCardEffects(card);

        // Check for encounters
        if (card.HasEncounter)
        {
            DrawEncounterCard();
        }

        // Record path selection
        session.SelectedPathId = pathCardId;
        session.TimeElapsed += card.TravelTimeMinutes;

        // Update travel state based on stamina
        UpdateTravelState(session);

        // Move to next segment or complete journey
        AdvanceSegment(session);

        return true;
    }

    /// <summary>
    /// Rest to recover stamina during travel
    /// </summary>
    public bool RestAction()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null || session.CurrentState == TravelState.Exhausted)
        {
            return false;
        }

        // Resting takes time and restores stamina
        session.TimeElapsed += 30; // 30 minutes to rest
        session.StaminaRemaining = session.StaminaCapacity;
        session.CurrentState = TravelState.Fresh;

        return true;
    }

    /// <summary>
    /// Turn back and cancel the journey
    /// </summary>
    public bool TurnBack()
    {
        if (_gameWorld.CurrentTravelSession == null)
        {
            return false;
        }

        // Clear the travel session
        _gameWorld.CurrentTravelSession = null;
        
        // Player returns to origin location - no actual movement needed
        // as they haven't completed the journey
        
        return true;
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Get route by ID from world state
    /// </summary>
    private RouteOption GetRoute(string routeId)
    {
        // Find route in world state - need to search through location connections
        foreach (Location location in _gameWorld.WorldState.locations)
        {
            foreach (LocationConnection connection in location.Connections)
            {
                RouteOption route = connection.RouteOptions.FirstOrDefault(r => r.Id == routeId);
                if (route != null)
                {
                    return route;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Get stamina capacity based on travel state
    /// </summary>
    private int GetStaminaCapacity(TravelState state)
    {
        return state switch
        {
            TravelState.Fresh => 3,
            TravelState.Steady => 4,
            TravelState.Tired => 2,
            TravelState.Weary => 1,
            TravelState.Exhausted => 0,
            _ => 3
        };
    }

    /// <summary>
    /// Apply path card effects to player
    /// </summary>
    private void ApplyPathCardEffects(PathCardDTO card)
    {
        Player player = _gameWorld.GetPlayer();

        // Apply hunger effect
        if (card.HungerEffect != 0)
        {
            player.ModifyHunger(card.HungerEffect);
        }

        // Apply one-time rewards
        if (card.IsOneTime && !string.IsNullOrEmpty(card.OneTimeReward))
        {
            ApplyOneTimeReward(card.OneTimeReward, card.Id);
        }
    }

    /// <summary>
    /// Apply one-time reward effects
    /// </summary>
    private void ApplyOneTimeReward(string reward, string cardId)
    {
        Player player = _gameWorld.GetPlayer();

        // Parse reward string and apply effects
        if (reward.StartsWith("observation_"))
        {
            // Add observation card to player deck
            // TODO: Implement observation card system
        }
        else if (reward.EndsWith("_coins"))
        {
            // Extract coin amount and add to player
            if (reward.StartsWith("3_"))
            {
                player.ModifyCoins(3);
            }
            // Add more coin parsing as needed
        }

        // Mark reward as claimed
        _gameWorld.PathCardRewardsClaimed[cardId] = true;
    }

    /// <summary>
    /// Draw an encounter card and apply its effects
    /// </summary>
    private void DrawEncounterCard()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        RouteOption route = GetRoute(session.RouteId);
        
        if (route != null && route.EncounterDeckIds.Any())
        {
            // Randomly select an encounter card
            Random random = new Random();
            string encounterId = route.EncounterDeckIds[random.Next(route.EncounterDeckIds.Count)];
            
            if (_gameWorld.AllEncounterCards.ContainsKey(encounterId))
            {
                EncounterCardDTO encounter = _gameWorld.AllEncounterCards[encounterId];
                // TODO: Apply encounter effects
                // For now, just log that an encounter occurred
                System.Console.WriteLine($"Encounter: {encounter.Name} - {encounter.Effect}");
            }
        }
    }

    /// <summary>
    /// Update travel state based on current stamina
    /// </summary>
    private void UpdateTravelState(TravelSession session)
    {
        if (session.StaminaRemaining <= 0)
        {
            session.CurrentState = TravelState.Exhausted;
            session.StaminaCapacity = 0;
        }
        else if (session.StaminaRemaining <= 1)
        {
            session.CurrentState = TravelState.Weary;
            session.StaminaCapacity = 1;
        }
        else if (session.StaminaRemaining <= 2)
        {
            session.CurrentState = TravelState.Tired;
            session.StaminaCapacity = 2;
        }
        else if (session.StaminaRemaining >= 4)
        {
            session.CurrentState = TravelState.Steady;
            session.StaminaCapacity = 4;
        }
        else
        {
            session.CurrentState = TravelState.Fresh;
            session.StaminaCapacity = 3;
        }
    }

    /// <summary>
    /// Advance to next segment or complete journey
    /// </summary>
    private void AdvanceSegment(TravelSession session)
    {
        RouteOption route = GetRoute(session.RouteId);
        if (route == null) return;

        // Mark current segment as completed
        session.CompletedSegments.Add($"{session.RouteId}_{session.CurrentSegment}");

        // Check if there are more segments
        if (session.CurrentSegment < route.Segments.Count)
        {
            session.CurrentSegment++;
        }
        else
        {
            // Journey complete - player reaches destination
            CompleteJourney(session);
        }
    }

    /// <summary>
    /// Complete the journey and update player location
    /// </summary>
    private void CompleteJourney(TravelSession session)
    {
        RouteOption route = GetRoute(session.RouteId);
        if (route == null) return;

        // Move player to destination
        LocationSpot targetSpot = _gameWorld.WorldState.locationSpots
            .FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);
        
        if (targetSpot != null)
        {
            _gameWorld.GetPlayer().CurrentLocationSpot = targetSpot;
        }

        // Apply travel time to game world
        _timeManager.AdvanceTimeMinutes(session.TimeElapsed);

        // Clear travel session
        _gameWorld.CurrentTravelSession = null;
    }

    /// <summary>
    /// Get derived stamina based on hunger/health state as per design requirements
    /// </summary>
    private int GetDerivedStamina(Player player)
    {
        // Stamina is derived from hunger and health state
        // Lower hunger = higher stamina capacity
        // Better health = better stamina efficiency
        
        int baseStamina = 3; // Default Fresh state
        
        // Health affects maximum stamina capacity
        if (player.Health >= 80)
        {
            baseStamina = 4; // Steady state when healthy
        }
        else if (player.Health <= 30)
        {
            baseStamina = 1; // Weary when unhealthy
        }

        // Hunger affects current stamina
        if (player.Hunger >= 80)
        {
            baseStamina = Math.Max(1, baseStamina - 2); // Very hungry = low stamina
        }
        else if (player.Hunger >= 60)
        {
            baseStamina = Math.Max(1, baseStamina - 1); // Hungry = reduced stamina
        }

        return baseStamina;
    }

    /// <summary>
    /// Determine initial travel state based on player condition
    /// </summary>
    private TravelState DetermineInitialTravelState(Player player)
    {
        int stamina = GetDerivedStamina(player);
        
        return stamina switch
        {
            4 => TravelState.Steady,
            3 => TravelState.Fresh,
            2 => TravelState.Tired,
            1 => TravelState.Weary,
            0 => TravelState.Exhausted,
            _ => TravelState.Fresh
        };
    }
}


