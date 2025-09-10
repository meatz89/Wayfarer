using System;
using System.Collections.Generic;
using System.Linq;

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly Random _random = new Random();

    public TravelManager(GameWorld gameWorld, TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
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
    /// Get available cards for the current segment (works for both FixedPath and Event segments)
    /// </summary>
    public List<PathCardDTO> GetSegmentCards()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return new List<PathCardDTO>();
        }

        RouteOption route = GetRoute(session.RouteId);
        if (route == null || session.CurrentSegment > route.Segments.Count)
        {
            return new List<PathCardDTO>();
        }

        RouteSegment segment = route.Segments[session.CurrentSegment - 1];

        if (segment.Type == SegmentType.FixedPath)
        {
            // Return fixed path cards from PathCardIds
            List<PathCardDTO> pathCards = new List<PathCardDTO>();
            foreach (string cardId in segment.PathCardIds)
            {
                if (_gameWorld.AllPathCards.ContainsKey(cardId))
                {
                    pathCards.Add(_gameWorld.AllPathCards[cardId]);
                }
            }
            return pathCards;
        }
        else if (segment.Type == SegmentType.Event)
        {
            // Draw random event and return its response cards
            string eventId = DrawRandomEvent(route, segment);
            if (!string.IsNullOrEmpty(eventId) && _gameWorld.AllEventCollections.ContainsKey(eventId))
            {
                EventCollectionDTO eventCollection = _gameWorld.AllEventCollections[eventId];
                session.CurrentEventId = eventId; // Track which event was drawn
                return eventCollection.ResponseCards;
            }
        }

        return new List<PathCardDTO>();
    }

    /// <summary>
    /// Draw a random event from the route's event pool for Event-type segments
    /// </summary>
    private string DrawRandomEvent(RouteOption route, RouteSegment segment)
    {
        // Use event collection ID if specified directly on segment
        if (!string.IsNullOrEmpty(segment.EventCollectionId))
        {
            return segment.EventCollectionId;
        }

        // Use segment's event pool if available
        if (segment.EventPool != null && segment.EventPool.Count > 0)
        {
            int index = _random.Next(segment.EventPool.Count);
            return segment.EventPool[index];
        }

        // Fall back to route event pools
        if (_gameWorld.RouteEventPools.ContainsKey(route.Id))
        {
            List<string> eventPool = _gameWorld.RouteEventPools[route.Id];
            if (eventPool != null && eventPool.Count > 0)
            {
                int index = _random.Next(eventPool.Count);
                return eventPool[index];
            }
        }

        return null;
    }

    /// <summary>
    /// Reveal a face-down path card without playing it
    /// </summary>
    public bool RevealPathCard(string pathCardId)
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

        // Check if card is already discovered (face-up)
        if (_gameWorld.PathCardDiscoveries.ContainsKey(pathCardId) && _gameWorld.PathCardDiscoveries[pathCardId])
        {
            return false; // Card already revealed
        }

        PathCardDTO card = _gameWorld.AllPathCards[pathCardId];
        
        // Basic affordability checks (same as SelectPathCard)
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        if (card.CoinRequirement > 0 && _gameWorld.GetPlayer().Coins < card.CoinRequirement)
        {
            return false;
        }

        // Check one-time card usage
        if (card.IsOneTime && _gameWorld.PathCardRewardsClaimed.ContainsKey(pathCardId) 
            && _gameWorld.PathCardRewardsClaimed[pathCardId])
        {
            return false;
        }

        // Mark card as discovered (face-up)
        _gameWorld.PathCardDiscoveries[pathCardId] = true;
        
        // Set reveal state
        session.IsRevealingCard = true;
        session.RevealedCardId = pathCardId;

        return true;
    }

    /// <summary>
    /// Confirm the revealed card and apply its effects, then advance to next segment
    /// </summary>
    public bool ConfirmRevealedCard()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null || !session.IsRevealingCard || string.IsNullOrEmpty(session.RevealedCardId))
        {
            return false;
        }

        string pathCardId = session.RevealedCardId;
        if (!_gameWorld.AllPathCards.ContainsKey(pathCardId))
        {
            return false;
        }

        PathCardDTO card = _gameWorld.AllPathCards[pathCardId];
        
        // Final affordability check (in case something changed)
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        if (card.CoinRequirement > 0 && _gameWorld.GetPlayer().Coins < card.CoinRequirement)
        {
            return false;
        }

        // Deduct costs
        session.StaminaRemaining -= card.StaminaCost;
        if (card.CoinRequirement > 0)
        {
            _gameWorld.GetPlayer().ModifyCoins(-card.CoinRequirement);
        }

        // Apply effects
        ApplyPathCardEffects(card);

        // Record path selection
        session.SelectedPathId = pathCardId;
        session.TimeElapsed += card.TravelTimeMinutes;

        // Clear reveal state
        session.IsRevealingCard = false;
        session.RevealedCardId = null;

        // Update travel state based on stamina
        UpdateTravelState(session);

        // Move to next segment or complete journey
        AdvanceSegment(session);

        return true;
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
        
        // Check if card is face-down and needs to be revealed first
        bool isCardDiscovered = _gameWorld.PathCardDiscoveries.ContainsKey(pathCardId) && _gameWorld.PathCardDiscoveries[pathCardId];
        
        // For face-down cards in FixedPath segments, use reveal mechanic
        RouteOption route = GetRoute(session.RouteId);
        bool isFixedPathSegment = false;
        if (route != null && session.CurrentSegment <= route.Segments.Count)
        {
            RouteSegment segment = route.Segments[session.CurrentSegment - 1];
            isFixedPathSegment = segment.Type == SegmentType.FixedPath;
        }
        
        if (!isCardDiscovered && isFixedPathSegment)
        {
            // Face-down card in FixedPath segment - use reveal mechanic
            return RevealPathCard(pathCardId);
        }
        
        // Face-up card or Event segment - proceed with normal selection
        
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

        // Reveal if face-down (for Event segments)
        if (!_gameWorld.PathCardDiscoveries.ContainsKey(pathCardId))
        {
            _gameWorld.PathCardDiscoveries[pathCardId] = true;
        }

        // Apply effects
        ApplyPathCardEffects(card);

        // Note: Encounters are now handled through Event-type segments, not as side-effects
        // The card selection itself handles both fixed path cards and event response cards uniformly

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
    /// - Skip the current segment (advance to next)
    /// - Add 30 minutes to travel time
    /// - Restore stamina to current capacity
    /// </summary>
    public bool RestAction()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        // Resting takes time and restores stamina
        session.TimeElapsed += 30; // 30 minutes to rest
        session.StaminaRemaining = session.StaminaCapacity;
        session.CurrentState = TravelState.Fresh;

        // Skip the current segment (advance to next)
        RouteOption route = GetRoute(session.RouteId);
        if (route != null && session.CurrentSegment <= route.Segments.Count)
        {
            // Mark current segment as completed (skipped via rest)
            session.CompletedSegments.Add($"{session.RouteId}_{session.CurrentSegment}_rested");
            
            // Move to next segment or complete journey
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


