/// <summary>
/// Public facade for all travel-related operations.
/// Single entry point for travel, routes, and exploration.
/// </summary>
public class TravelFacade
{
    private readonly GameWorld _gameWorld;
    private readonly RouteManager _routeManager;
    private readonly PermitValidator _permitValidator;
    private readonly TravelTimeCalculator _travelTimeCalculator;
    private readonly TravelManager _travelManager;
    private readonly MessageSystem _messageSystem;
    private readonly ItemRepository _itemRepository;

    public TravelFacade(
        GameWorld gameWorld,
        RouteManager routeManager,
        PermitValidator permitValidator,
        TravelTimeCalculator travelTimeCalculator,
        TravelManager travelManager,
        MessageSystem messageSystem,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _routeManager = routeManager;
        _permitValidator = permitValidator;
        _travelTimeCalculator = travelTimeCalculator;
        _travelManager = travelManager;
        _messageSystem = messageSystem;
        _itemRepository = itemRepository;
    }

    // ========== ROUTE OPERATIONS ==========

    public List<RouteOption> GetAvailableRoutesFromCurrentLocation()
    {
        return _routeManager.GetAvailableRoutesFromCurrentLocation();
    }

    /// <summary>
    /// Get travel destinations with full view model data for UI
    /// </summary>
    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        List<RouteOption> routes = GetAvailableRoutesFromCurrentLocation();
        List<TravelDestinationViewModel> destinations = new List<TravelDestinationViewModel>();

        foreach (RouteOption route in routes)
        {
            // Get destination Location directly from object reference
            Location destination = route.DestinationLocation;
            if (destination != null)
            {
                // Core Loop: All routes physically exist and are visible from game start
                // Can travel unless missing permits
                bool hasPermit = _permitValidator.HasRequiredPermit(route);

                destinations.Add(new TravelDestinationViewModel
                {
                    Location = destination,  // HIGHLANDER: Object reference
                    LocationName = destination.Name,  // For display
                    Description = destination.Description,
                    CanTravel = hasPermit,
                    CannotTravelReason = !hasPermit ? "Missing required permits" : null,
                    MinimumCost = CalculateTravelCost(route, TravelMethods.Walking),
                    MinimumTime = route.TravelTimeSegments,
                    IsCurrent = false,
                    Routes = new List<TravelRouteViewModel>() // This would be populated by a more detailed method
                });
            }
        }

        return destinations;
    }

    /// <summary>
    /// PHASE 4: Accept Location objects instead of IDs
    /// </summary>
    public RouteOption GetRouteBetweenLocations(Location fromLocation, Location toLocation)
    {
        if (fromLocation == null || toLocation == null)
            return null;

        return _routeManager.GetRouteBetweenLocations(fromLocation, toLocation);
    }

    // ========== TRAVEL OPERATIONS ==========

    /// <summary>
    /// PHASE 4: Accept Location object instead of ID
    /// </summary>
    public bool CanTravelTo(Location location)
    {
        if (location == null)
            return false;

        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
        {
            return false;
        }

        // Check if route exists
        RouteOption route = GetRouteBetweenLocations(currentLocation, location);
        if (route == null)
        {
            return false;
        }

        // Routes are always available - no discovery mechanic

        // Check permits
        if (!_permitValidator.HasRequiredPermit(route))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// PHASE 4: Accept Location object instead of ID
    /// </summary>
    public TravelResult TravelTo(Location location, TravelMethods transportMethod)
    {
        if (location == null)
        {
            return new TravelResult
            {
                Success = false,
                Reason = "Destination location is null"
            };
        }

        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
        {
            return new TravelResult
            {
                Success = false,
                Reason = "Current Location is unknown"
            };
        }

        // Get route
        RouteOption route = GetRouteBetweenLocations(currentLocation, location);
        if (route == null)
        {
            return new TravelResult
            {
                Success = false,
                Reason = "No route exists to that location"
            };
        }

        // Routes are always available - no discovery mechanic

        // Check permits
        if (!_permitValidator.HasRequiredPermit(route))
        {
            List<string> missingPermits = _permitValidator.GetMissingPermits(route);
            return new TravelResult
            {
                Success = false,
                Reason = $"Missing required permits: {string.Join(", ", missingPermits)}"
            };
        }

        // Check transport compatibility
        if (!_permitValidator.IsTransportCompatible(route, transportMethod))
        {
            return new TravelResult
            {
                Success = false,
                Reason = $"{transportMethod} cannot be used on this route"
            };
        }

        // Calculate time and cost (pass route object for improvement lookup)
        int travelTime = _travelTimeCalculator.CalculateTravelTime(route, transportMethod);
        int coinCost = _travelTimeCalculator.CalculateTravelCost(route, transportMethod);

        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        if (coinCost > 0)
        {
            Consequence cost = new Consequence { Coins = -coinCost };
            CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
            if (!resourceReq.IsAnySatisfied(player, _gameWorld))
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = $"Not enough coins. Need {coinCost}, have {player.Coins}"
                };
            }
        }

        // Return travel information for GameFacade to execute
        // GameFacade will handle coin deduction and Location update
        return new TravelResult
        {
            Success = true,
            TravelTimeSegments = travelTime,
            SegmentCost = travelTime, // Direct segments usage
            CoinCost = coinCost,
            Route = route,  // PHASE 4: Object reference
            Destination = location,  // PHASE 4: Object reference
            TransportMethod = transportMethod
        };
    }

    // ========== PERMIT OPERATIONS ==========

    public bool HasRequiredPermit(RouteOption route)
    {
        return _permitValidator.HasRequiredPermit(route);
    }

    public List<string> GetMissingPermits(RouteOption route)
    {
        return _permitValidator.GetMissingPermits(route);
    }

    public string GetAccessRequirementDescription(RouteOption route)
    {
        return _permitValidator.GetAccessRequirementDescription(route);
    }

    public bool IsTransportCompatible(RouteOption route, TravelMethods transportMethod)
    {
        return _permitValidator.IsTransportCompatible(route, transportMethod);
    }

    // ========== TIME CALCULATIONS ==========

    /// <summary>
    /// PHASE 4: Accept Location object instead of ID
    /// </summary>
    public int CalculateTravelTime(Location toLocation, TravelMethods transportMethod)
    {
        if (toLocation == null)
            return 0;

        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
        {
            return 0;
        }

        // Find route between current location and destination
        RouteOption route = GetRouteBetweenLocations(currentLocation, toLocation);

        if (route == null)
        {
            // No route found - fallback to hex distance calculation
            return _travelTimeCalculator.GetBaseTravelTime(currentLocation, toLocation);
        }

        return _travelTimeCalculator.CalculateTravelTime(route, transportMethod);
    }

    public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
    {
        return _travelTimeCalculator.CalculateTravelCost(route, transportMethod);
    }

    // ========== TRANSPORT METHODS ==========

    public List<TravelMethods> GetAvailableTransportMethods()
    {
        Player player = _gameWorld.GetPlayer();
        List<TravelMethods> methods = new List<TravelMethods> { TravelMethods.Walking }; // Always can walk

        // Check for unlocked transport methods
        foreach (string method in player.UnlockedTravelMethods)
        {
            if (Enum.TryParse<TravelMethods>(method, out TravelMethods travelMethod))
            {
                methods.Add(travelMethod);
            }
        }

        return methods.Distinct().ToList();
    }

    public void UnlockTransportMethod(TravelMethods method)
    {
        Player player = _gameWorld.GetPlayer();
        string methodName = method.ToString();

        if (!player.UnlockedTravelMethods.Contains(methodName))
        {
            player.UnlockedTravelMethods.Add(methodName);
            _messageSystem.AddSystemMessage(
                $"Unlocked new transport method: {method}",
                SystemMessageTypes.Success,
                MessageCategory.Achievement);
        }
    }

    // ========== PATH CARD SYSTEM OPERATIONS ==========

    /// <summary>
    /// Get current travel context for active path card session
    /// </summary>
    public TravelContext GetCurrentTravelContext()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return null;
        }

        // HIGHLANDER: TravelSession.Route is object reference (no ID lookup needed)
        RouteOption route = session.Route;
        if (route == null)
        {
            return null;
        }

        // Get current segment cards - delegate to TravelManager which handles both FixedPath and Event segments
        List<PathCardDTO> currentSegmentCards = _travelManager.GetSegmentCards();

        // Check if player must turn back (exhausted with no paths available)
        bool mustTurnBack = session.CurrentState == TravelState.Exhausted &&
                           !currentSegmentCards.Any(card => CanPlayPathCard(card));

        return new TravelContext
        {
            CurrentRoute = route,
            Session = session,
            CurrentSegmentCards = currentSegmentCards,
            CardDiscoveries = _gameWorld.PathCardDiscoveries,
            Player = _gameWorld.GetPlayer(),
            MustTurnBack = mustTurnBack
        };
    }

    /// <summary>
    /// Get availability information for a path card including reasons why it can't be used
    /// PHASE 6D: Accept PathCardDTO object instead of ID
    /// </summary>
    public PathCardAvailability GetPathCardAvailability(PathCardDTO card)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return new PathCardAvailability { CanPlay = false, Reason = "No active travel session" };
        }

        if (card == null)
        {
            return new PathCardAvailability { CanPlay = false, Reason = "Card not found" };
        }

        Player player = _gameWorld.GetPlayer();

        // Check stamina requirement
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return new PathCardAvailability { CanPlay = false, Reason = $"Need {card.StaminaCost} stamina, have {session.StaminaRemaining}" };
        }

        // HIGHLANDER: Use CompoundRequirement for coin requirement check
        if (card.CoinRequirement > 0)
        {
            Consequence cost = new Consequence { Coins = -card.CoinRequirement };
            CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
            if (!resourceReq.IsAnySatisfied(player, _gameWorld))
            {
                return new PathCardAvailability { CanPlay = false, Reason = $"Need {card.CoinRequirement} coins, have {player.Coins}" };
            }
        }

        // Check permit requirement
        if (!string.IsNullOrEmpty(card.PermitRequirement))
        {
            return new PathCardAvailability { CanPlay = false, Reason = $"Requires {card.PermitRequirement} permit" };
        }

        // Check one-time card usage
        if (card.IsOneTime && _gameWorld.IsPathCardDiscovered(card))
        {
            return new PathCardAvailability { CanPlay = false, Reason = "Already used this one-time path" };
        }

        // Check stat requirements
        if (card.StatRequirements != null && card.StatRequirements.Count > 0)
        {
            foreach (KeyValuePair<string, int> statRequirement in card.StatRequirements)
            {
                // Convert string stat name to PlayerStatType enum
                if (Enum.TryParse<PlayerStatType>(statRequirement.Key, true, out PlayerStatType statType))
                {
                    int currentLevel = statType switch
                    {
                        PlayerStatType.Insight => player.Insight,
                        PlayerStatType.Rapport => player.Rapport,
                        PlayerStatType.Authority => player.Authority,
                        PlayerStatType.Diplomacy => player.Diplomacy,
                        PlayerStatType.Cunning => player.Cunning,
                        _ => 0
                    };
                    if (currentLevel < statRequirement.Value)
                    {
                        return new PathCardAvailability
                        {
                            CanPlay = false,
                            Reason = $"Requires {statRequirement.Key} level {statRequirement.Value}, have {currentLevel}"
                        };
                    }
                }
                else
                {
                    return new PathCardAvailability { CanPlay = false, Reason = $"Invalid stat requirement: {statRequirement.Key}" };
                }
            }
        }

        return new PathCardAvailability { CanPlay = true, Reason = "" };
    }

    /// <summary>
    /// Check if a specific path card can be played
    /// PHASE 6D: Accept PathCardDTO object instead of ID
    /// </summary>
    public bool CanPlayPathCard(PathCardDTO card)
    {
        return GetPathCardAvailability(card).CanPlay;
    }

    /// <summary>
    /// Get available path cards for the current segment with discovery states
    /// Delegates to TravelManager for card data, handles discovery logic based on segment type
    /// </summary>
    public List<PathCardInfo> GetAvailablePathCards()
    {
        // Delegate card retrieval to TravelManager
        List<PathCardDTO> cards = _travelManager.GetSegmentCards();
        if (cards == null || cards.Count == 0)
        {
            return new List<PathCardInfo>();
        }

        List<PathCardInfo> pathCardInfos = new List<PathCardInfo>();
        bool isEventSegment = IsCurrentSegmentEventType();

        foreach (PathCardDTO card in cards)
        {
            // Event segments: cards are ALWAYS face-up (IsDiscovered = true)
            // FixedPath segments: check PathCardDiscoveries dictionary
            bool isDiscovered = isEventSegment ||
                              _gameWorld.IsPathCardDiscovered(card);

            bool canPlay = CanPlayPathCard(card);

            pathCardInfos.Add(new PathCardInfo
            {
                Card = card,
                IsDiscovered = isDiscovered,
                CanPlay = canPlay,
                IsHidden = card.IsHidden && !isDiscovered
            });
        }

        return pathCardInfos;
    }

    /// <summary>
    /// Start a new path card journey
    /// </summary>
    public bool StartPathCardJourney(RouteOption route)
    {
        // No lookup needed - route passed as object
        if (route == null)
        {
            return false;
        }

        // Check access requirements (permits, etc.)
        if (!_permitValidator.HasRequiredPermit(route))
        {
            return false;
        }

        // Check if already in a travel session
        if (_gameWorld.CurrentTravelSession != null)
        {
            return false;
        }

        // Delegate to TravelManager to actually start the journey
        // TravelManager will create the session and set up initial state
        TravelSession session = _travelManager.StartJourney(route);
        return session != null;
    }

    /// <summary>
    /// Get stamina derived from hunger/health as per design requirements
    /// Fresh (3) - default/healthy state
    /// Steady (4) - well-fed and rested
    /// Tired (2) - moderate hunger/fatigue
    /// Weary (1) - significant hunger/fatigue  
    /// Exhausted (0) - critical condition
    /// </summary>
    public int GetDerivedStamina(Player player)
    {
        // Start with base Fresh state (3 stamina)
        int stamina = 3;

        // Well-fed and well-rested gives Steady (4 stamina)
        if (player.Health >= 80 && player.Hunger <= 20)
        {
            stamina = 4; // Steady: well-fed and rested
        }
        // Critical conditions give Exhausted (0 stamina)
        else if (player.Health <= 10 || player.Hunger >= 90)
        {
            stamina = 0; // Exhausted: critical condition
        }
        // Significant problems give Weary (1 stamina)
        else if (player.Health <= 30 || player.Hunger >= 70)
        {
            stamina = 1; // Weary: significant hunger/fatigue
        }
        // Moderate problems give Tired (2 stamina)
        else if (player.Health <= 60 || player.Hunger >= 50)
        {
            stamina = 2; // Tired: moderate hunger/fatigue
        }
        // Otherwise Fresh (3 stamina) - default/healthy state

        return stamina;
    }

    /// <summary>
    /// Check if the session is in card reveal state
    /// </summary>
    public bool IsRevealingCard()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        return session != null && session.IsRevealingCard;
    }

    /// <summary>
    /// Get the ID of the card being revealed
    /// ADR-007: Get ID from RevealedCard object (not RevealedCardId property)
    /// </summary>
    public string GetRevealedCardId()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        return session?.RevealedCard?.Id;
    }

    /// <summary>
    /// Confirm the revealed card selection
    /// TWO PILLARS: Delegates to async TravelManager
    /// </summary>
    public async Task<bool> ConfirmRevealedCard()
    {
        return await _travelManager.ConfirmRevealedCard();
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Check if the current segment is an Event type (caravan segment)
    /// </summary>
    public bool IsCurrentSegmentEventType()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route == null || session.CurrentSegment > route.Segments.Count)
        {
            return false;
        }

        RouteSegment segment = route.Segments[session.CurrentSegment - 1];
        return segment.Type == SegmentType.Event;
    }

    /// <summary>
    /// Get narrative text from current event if in Event segment
    /// HIGHLANDER: Use CurrentEventNarrative property set by TravelManager, NO lookups
    /// </summary>
    public string GetCurrentEventNarrative()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null || !IsCurrentSegmentEventType())
        {
            return null;
        }

        // TravelManager sets CurrentEventNarrative from TravelEventDTO.NarrativeText
        // NO lookups needed - already populated at event selection time
        return session.CurrentEventNarrative;
    }

    /// <summary>
    /// Get a card from the current segment's collection
    /// </summary>
    private PathCardDTO GetCardFromCurrentSegmentCollection(string cardId)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return null;
        }

        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route == null || session.CurrentSegment > route.Segments.Count)
        {
            return null;
        }

        RouteSegment segment = route.Segments[session.CurrentSegment - 1];

        // Handle different segment types
        if (segment.Type == SegmentType.Event)
        {
            // ADR-007: For Event segments, use CurrentEvent object (not CurrentEventId)
            if (session.CurrentEvent != null)
            {
                TravelEventDTO travelEvent = session.CurrentEvent;
                return travelEvent.EventCards?.FirstOrDefault(c => c.Id == cardId);
            }
        }
        else if (segment.Type == SegmentType.FixedPath)
        {
            // HIGHLANDER: Use PathCollection object reference directly, NO lookup
            PathCardCollectionDTO collection = segment.PathCollection;

            if (collection == null)
            {
                return null;
            }

            // Look in embedded path cards
            return collection.PathCards?.FirstOrDefault(c => c.Id == cardId);
        }

        return null;
    }

    // PHASE 4: GetRouteById DELETED - ID lookups forbidden, use object references

    /// <summary>
    /// Turn back and cancel the journey
    /// </summary>
    public bool TurnBack()
    {
        return _travelManager.TurnBack();
    }

    /// <summary>
    /// Check if journey is ready to complete (last segment finished)
    /// </summary>
    public bool IsReadyToComplete()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        return session != null && session.IsReadyToComplete;
    }

    /// <summary>
    /// Complete the journey after last segment
    /// </summary>
    public bool FinishRoute()
    {
        return _travelManager.FinishRoute();
    }

    /// <summary>
    /// Resolve pending scene after player completes scene situations
    /// Called after scene intensity reaches 0
    /// </summary>
    public bool ResolveScene(Scene scene)
    {
        return _travelManager.ResolveScene(scene);
    }

    // ========== CORE LOOP: PATH FILTERING ==========

    /// <summary>
    /// Get available paths for route segment (filtered by exploration cubes)
}

/// <summary>
/// Information about a path card including its discovery and availability state
/// </summary>
public class PathCardInfo
{
    public PathCardDTO Card { get; set; }
    public bool IsDiscovered { get; set; }
    public bool CanPlay { get; set; }
    public bool IsHidden { get; set; }
}

/// <summary>
/// Information about path card availability including reasons for unavailability
/// </summary>
public class PathCardAvailability
{
    public bool CanPlay { get; set; }
    public string Reason { get; set; }
}

/// <summary>
/// Result of a travel attempt.
/// PHASE 4: ID properties replaced with object references
/// </summary>
public class TravelResult
{
    public bool Success { get; set; }
    public string Reason { get; set; }
    public int TravelTimeSegments { get; set; }
    public int SegmentCost { get; set; }
    public int CoinCost { get; set; }
    public RouteOption Route { get; set; }  // PHASE 4: Object reference instead of RouteId
    public Location Destination { get; set; }  // PHASE 4: Object reference instead of DestinationId
    public TravelMethods TransportMethod { get; set; }
}
