using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.TravelSubsystem
{
    /// <summary>
    /// Public facade for all travel-related operations.
    /// Single entry point for travel, routes, and exploration.
    /// </summary>
    public class TravelFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly RouteManager _routeManager;
        private readonly RouteDiscoveryManager _routeDiscoveryManager;
        private readonly PermitValidator _permitValidator;
        private readonly TravelTimeCalculator _travelTimeCalculator;
        private readonly TravelManager _travelManager;
        private readonly MessageSystem _messageSystem;

        public TravelFacade(
            GameWorld gameWorld,
            RouteManager routeManager,
            RouteDiscoveryManager routeDiscoveryManager,
            PermitValidator permitValidator,
            TravelTimeCalculator travelTimeCalculator,
            TravelManager travelManager,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _routeManager = routeManager;
            _routeDiscoveryManager = routeDiscoveryManager;
            _permitValidator = permitValidator;
            _travelTimeCalculator = travelTimeCalculator;
            _travelManager = travelManager;
            _messageSystem = messageSystem;
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
                // Extract location ID from destination spot (format: locationId.spotName)
                string locationId = route.DestinationLocationSpot.Split('.')[0];
                Location? destination = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
                if (destination != null)
                {
                    bool canTravel = IsRouteDiscovered(route.Id);

                    destinations.Add(new TravelDestinationViewModel
                    {
                        LocationId = destination.Id,
                        LocationName = destination.Name,
                        Description = destination.Description ?? "",
                        CanTravel = canTravel,
                        CannotTravelReason = !canTravel ? "Route not discovered" : null,
                        MinimumCost = CalculateTravelCost(route, TravelMethods.Walking),
                        MinimumTime = route.TravelTimeSegments,
                        IsCurrent = false,
                        Routes = new List<TravelRouteViewModel>() // This would be populated by a more detailed method
                    });
                }
            }

            return destinations;
        }

        public List<RouteOption> GetDiscoveredRoutes()
        {
            return _routeManager.GetDiscoveredRoutes();
        }

        public RouteOption GetRouteBetweenLocations(string fromLocationId, string toLocationId)
        {
            return _routeManager.GetRouteBetweenLocations(fromLocationId, toLocationId);
        }

        public bool IsRouteDiscovered(string routeId)
        {
            return _routeManager.IsRouteDiscovered(routeId);
        }

        // ========== TRAVEL OPERATIONS ==========

        public bool CanTravelTo(string locationId)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return false;
            }

            // Check if route exists
            RouteOption route = GetRouteBetweenLocations(currentLocationId, locationId);
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

        public TravelResult TravelTo(string locationId, TravelMethods transportMethod)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = "Current location is unknown"
                };
            }

            // Get route
            RouteOption route = GetRouteBetweenLocations(currentLocationId, locationId);
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

            // Calculate time and cost
            int travelTime = _travelTimeCalculator.CalculateTravelTime(currentLocationId, locationId, transportMethod);
            int coinCost = _travelTimeCalculator.CalculateTravelCost(route, transportMethod);

            // Check if player can afford
            if (coinCost > 0 && _gameWorld.GetPlayer().Coins < coinCost)
            {
                return new TravelResult
                {
                    Success = false,
                    Reason = $"Not enough coins. Need {coinCost}, have {_gameWorld.GetPlayer().Coins}"
                };
            }

            // Return travel information for GameFacade to execute
            // GameFacade will handle coin deduction and location update
            return new TravelResult
            {
                Success = true,
                TravelTimeSegments = travelTime,
                SegmentCost = travelTime, // Direct segments usage
                CoinCost = coinCost,
                RouteId = route.Id,
                DestinationId = locationId,
                TransportMethod = transportMethod
            };
        }

        // ========== DISCOVERY OPERATIONS ==========

        public bool AttemptRouteDiscovery(string toLocationId)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return false;
            }
            return _routeDiscoveryManager.AttemptRouteDiscovery(currentLocationId, toLocationId);
        }

        public List<RouteOption> GetUndiscoveredRoutes()
        {
            return _routeDiscoveryManager.GetUndiscoveredRoutesFromCurrentLocation();
        }

        public DiscoveryProgressInfo GetDiscoveryProgress()
        {
            return _routeDiscoveryManager.GetDiscoveryProgress();
        }

        public bool CanExploreFromCurrentLocation()
        {
            return _routeDiscoveryManager.CanExploreFromCurrentLocation();
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

        public int CalculateTravelTime(string toLocationId, TravelMethods transportMethod)
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return 0;
            }
            return _travelTimeCalculator.CalculateTravelTime(currentLocationId, toLocationId, transportMethod);
        }

        public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
        {
            return _travelTimeCalculator.CalculateTravelCost(route, transportMethod);
        }

        public Dictionary<string, int> GetTravelTimesFromCurrentLocation()
        {
            Player player = _gameWorld.GetPlayer();
            string currentLocationId = player.CurrentLocationSpot?.LocationId;
            if (currentLocationId == null)
            {
                return new Dictionary<string, int>();
            }
            return _travelTimeCalculator.GetTravelTimesFrom(currentLocationId);
        }

        // ========== TRANSPORT METHODS ==========

        public List<TravelMethods> GetAvailableTransportMethods()
        {
            Player player = _gameWorld.GetPlayer();
            List<TravelMethods> methods = new List<TravelMethods> { TravelMethods.Walking }; // Always can walk

            // Check for unlocked transport methods
            if (player.UnlockedTravelMethods != null)
            {
                foreach (string method in player.UnlockedTravelMethods)
                {
                    if (Enum.TryParse<TravelMethods>(method, out TravelMethods travelMethod))
                    {
                        methods.Add(travelMethod);
                    }
                }
            }

            return methods.Distinct().ToList();
        }

        public void UnlockTransportMethod(TravelMethods method)
        {
            Player player = _gameWorld.GetPlayer();
            if (player.UnlockedTravelMethods == null)
            {
                player.UnlockedTravelMethods = new List<string>();
            }

            string methodName = method.ToString();
            if (!player.UnlockedTravelMethods.Contains(methodName))
            {
                player.UnlockedTravelMethods.Add(methodName);
                _messageSystem.AddSystemMessage(
                    $"🎯 Unlocked new transport method: {method}",
                    SystemMessageTypes.Success);
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

            RouteOption route = GetRouteById(session.RouteId);
            if (route == null)
            {
                return null;
            }

            // Get current segment cards - delegate to TravelManager which handles both FixedPath and Event segments
            List<PathCardDTO> currentSegmentCards = _travelManager.GetSegmentCards();

            // Check if player must turn back (exhausted with no paths available)
            bool mustTurnBack = session.CurrentState == TravelState.Exhausted && 
                               !currentSegmentCards.Any(card => CanPlayPathCard(card.Id));

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
        /// </summary>
        public PathCardAvailability GetPathCardAvailability(string pathCardId)
        {
            TravelSession session = _gameWorld.CurrentTravelSession;
            if (session == null)
            {
                return new PathCardAvailability { CanPlay = false, Reason = "No active travel session" };
            }

            // Get the card from the current segment's collection
            PathCardDTO card = GetCardFromCurrentSegmentCollection(pathCardId);
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

            // Check coin requirement
            if (card.CoinRequirement > 0 && player.Coins < card.CoinRequirement)
            {
                return new PathCardAvailability { CanPlay = false, Reason = $"Need {card.CoinRequirement} coins, have {player.Coins}" };
            }

            // Check permit requirement
            if (!string.IsNullOrEmpty(card.PermitRequirement))
            {
                return new PathCardAvailability { CanPlay = false, Reason = $"Requires {card.PermitRequirement} permit" };
            }

            // Check one-time card usage
            if (card.IsOneTime && _gameWorld.PathCardRewardsClaimed.ContainsKey(pathCardId)
                && _gameWorld.PathCardRewardsClaimed[pathCardId])
            {
                return new PathCardAvailability { CanPlay = false, Reason = "Already used this one-time path" };
            }

            // Check stat requirements
            if (card.StatRequirements != null && card.StatRequirements.Count > 0)
            {
                foreach (var statRequirement in card.StatRequirements)
                {
                    // Convert string stat name to PlayerStatType enum
                    if (Enum.TryParse<PlayerStatType>(statRequirement.Key, true, out PlayerStatType statType))
                    {
                        if (player.Stats.GetLevel(statType) < statRequirement.Value)
                        {
                            return new PathCardAvailability
                            {
                                CanPlay = false,
                                Reason = $"Requires {statRequirement.Key} level {statRequirement.Value}, have {player.Stats.GetLevel(statType)}"
                            };
                        }
                    }
                    else
                    {
                        return new PathCardAvailability { CanPlay = false, Reason = $"Invalid stat requirement: {statRequirement.Key}" };
                    }
                }
            }

            return new PathCardAvailability { CanPlay = true, Reason = null };
        }

        /// <summary>
        /// Check if a specific path card can be played
        /// </summary>
        public bool CanPlayPathCard(string pathCardId)
        {
            return GetPathCardAvailability(pathCardId).CanPlay;
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
                                  (_gameWorld.PathCardDiscoveries.ContainsKey(card.Id) && _gameWorld.PathCardDiscoveries[card.Id]);
                
                bool canPlay = CanPlayPathCard(card.Id);

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
        public bool StartPathCardJourney(string routeId)
        {
            // Check if player can travel to the route
            RouteOption route = GetRouteById(routeId);
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
            return true;
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
        /// </summary>
        public string GetRevealedCardId()
        {
            TravelSession session = _gameWorld.CurrentTravelSession;
            return session?.RevealedCardId;
        }

        /// <summary>
        /// Confirm the revealed card selection
        /// </summary>
        public bool ConfirmRevealedCard()
        {
            return _travelManager.ConfirmRevealedCard();
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

            RouteOption route = GetRouteById(session.RouteId);
            if (route == null || session.CurrentSegment > route.Segments.Count)
            {
                return false;
            }

            RouteSegment segment = route.Segments[session.CurrentSegment - 1];
            return segment.Type == SegmentType.Event;
        }

        /// <summary>
        /// Get narrative text from current event if in Event segment
        /// </summary>
        public string GetCurrentEventNarrative()
        {
            TravelSession session = _gameWorld.CurrentTravelSession;
            if (session == null || !IsCurrentSegmentEventType())
            {
                return null;
            }

            // Check if we have a current event ID from the drawn event
            if (!string.IsNullOrEmpty(session.CurrentEventId) && 
                _gameWorld.AllPathCollections.ContainsKey(session.CurrentEventId))
            {
                PathCardCollectionDTO collection = _gameWorld.AllPathCollections[session.CurrentEventId];
                return collection.NarrativeText;
            }

            return null;
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

            RouteOption route = GetRouteById(session.RouteId);
            if (route == null || session.CurrentSegment > route.Segments.Count)
            {
                return null;
            }

            RouteSegment segment = route.Segments[session.CurrentSegment - 1];
            
            // Handle different segment types
            if (segment.Type == SegmentType.Event)
            {
                // For Event segments: get card from current event
                if (!string.IsNullOrEmpty(session.CurrentEventId) && 
                    _gameWorld.AllTravelEvents.ContainsKey(session.CurrentEventId))
                {
                    TravelEventDTO travelEvent = _gameWorld.AllTravelEvents[session.CurrentEventId];
                    return travelEvent.EventCards?.FirstOrDefault(c => c.Id == cardId);
                }
            }
            else if (segment.Type == SegmentType.FixedPath)
            {
                // For FixedPath segments: use PathCollectionId
                string collectionId = segment.PathCollectionId;
                
                if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.ContainsKey(collectionId))
                {
                    return null;
                }
                
                PathCardCollectionDTO collection = _gameWorld.AllPathCollections[collectionId];
                
                // Look in embedded path cards
                return collection.PathCards?.FirstOrDefault(c => c.Id == cardId);
            }
            
            return null;
        }

        /// <summary>
        /// Get route by ID - search through all location connections
        /// </summary>
        private RouteOption GetRouteById(string routeId)
        {
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
            var session = _gameWorld.CurrentTravelSession;
            return session != null && session.IsReadyToComplete;
        }
        
        /// <summary>
        /// Complete the journey after last segment
        /// </summary>
        public bool FinishRoute()
        {
            return _travelManager.FinishRoute();
        }
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
    /// </summary>
    public class TravelResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; }
        public int TravelTimeSegments { get; set; }
        public int SegmentCost { get; set; }
        public int CoinCost { get; set; }
        public string RouteId { get; set; }
        public string DestinationId { get; set; }
        public TravelMethods TransportMethod { get; set; }
    }
}