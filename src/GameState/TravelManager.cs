using System;
using System.Collections.Generic;
using System.Linq;

public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly Random _random = new Random();

    public TravelManager(GameWorld gameWorld, TimeManager timeManager, MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
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
            return GetPathCardsForFixedPathSegment(segment);
        }
        else if (segment.Type == SegmentType.Event)
        {
            return GetPathCardsForEventSegment(segment, session);
        }
        
        return new List<PathCardDTO>();
    }

    /// <summary>
    /// Get path cards for FixedPath segments - direct resolution: Collection → Cards
    /// </summary>
    private List<PathCardDTO> GetPathCardsForFixedPathSegment(RouteSegment segment)
    {
        // Use new normalized property first, fall back to legacy
        string collectionId = segment.PathCollectionId ?? segment.CollectionId;
        
        if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.ContainsKey(collectionId))
        {
            return new List<PathCardDTO>();
        }
        
        PathCardCollectionDTO collection = _gameWorld.AllPathCollections[collectionId];
        
        // Resolve PathCardIds to actual PathCards
        if (collection.PathCardIds != null && collection.PathCardIds.Count > 0)
        {
            List<PathCardDTO> resolvedCards = new List<PathCardDTO>();
            foreach (string cardId in collection.PathCardIds)
            {
                if (_gameWorld.AllPathCards.ContainsKey(cardId))
                {
                    resolvedCards.Add(_gameWorld.AllPathCards[cardId]);
                }
            }
            return resolvedCards;
        }
        
        // Fall back to legacy inline cards
        return collection.PathCards ?? new List<PathCardDTO>();
    }
    
    /// <summary>
    /// Get path cards for Event segments - two-step resolution: Collection → Event → Cards
    /// </summary>
    private List<PathCardDTO> GetPathCardsForEventSegment(RouteSegment segment, TravelSession session)
    {
        // Step 1: Get event collection ID (new normalized or legacy)
        string eventCollectionId = segment.EventCollectionId ?? segment.CollectionId;
        
        if (string.IsNullOrEmpty(eventCollectionId))
        {
            return HandleLegacyEventSegment(segment, session);
        }
        
        // Check for normalized structure first
        if (_gameWorld.AllEventCollections.ContainsKey(eventCollectionId))
        {
            return HandleNormalizedEventSegment(segment, session, eventCollectionId);
        }
        
        // Fall back to legacy structure
        return HandleLegacyEventSegment(segment, session);
    }
    
    /// <summary>
    /// Handle normalized event structure: EventCollection → Event → EventCards
    /// </summary>
    private List<PathCardDTO> HandleNormalizedEventSegment(RouteSegment segment, TravelSession session, string eventCollectionId)
    {
        // Step 1: Get event collection
        PathCardCollectionDTO eventCollection = _gameWorld.AllEventCollections[eventCollectionId];
        
        if (eventCollection.EventIds == null || eventCollection.EventIds.Count == 0)
        {
            return new List<PathCardDTO>();
        }
        
        // Step 2: Get or draw event for this segment  
        string eventId = GetOrDrawEventForSegment(segment, session, eventCollection.EventIds);
        
        // Step 3: Get the event
        if (!_gameWorld.AllTravelEvents.ContainsKey(eventId))
        {
            return new List<PathCardDTO>();
        }
        
        TravelEventDTO travelEvent = _gameWorld.AllTravelEvents[eventId];
        
        // Step 4: Set narrative for UI
        session.CurrentEventNarrative = travelEvent.NarrativeText;
        
        // Step 5: Resolve event cards (from AllEventCards, not AllPathCards)
        List<PathCardDTO> eventCards = new List<PathCardDTO>();
        foreach (string cardId in travelEvent.EventCardIds)
        {
            if (_gameWorld.AllEventCards.ContainsKey(cardId))
            {
                eventCards.Add(_gameWorld.AllEventCards[cardId]);
            }
        }
        
        return eventCards;
    }
    
    /// <summary>
    /// Handle legacy event structure for backwards compatibility
    /// </summary>
    private List<PathCardDTO> HandleLegacyEventSegment(RouteSegment segment, TravelSession session)
    {
        // Legacy: Event segments select randomly from a pool of collections
        string collectionId = session.CurrentEventId;
        
        // Only draw a new collection if we don't have one yet
        if (string.IsNullOrEmpty(collectionId))
        {
            collectionId = DrawRandomCollection(segment);
            session.CurrentEventId = collectionId; // Track which collection was drawn
        }
        
        // Return the cards from the collection
        if (!string.IsNullOrEmpty(collectionId) && _gameWorld.AllPathCollections.ContainsKey(collectionId))
        {
            PathCardCollectionDTO collection = _gameWorld.AllPathCollections[collectionId];
            
            // If collection has PathCardIds, resolve them to actual cards
            if (collection.PathCardIds != null && collection.PathCardIds.Count > 0)
            {
                List<PathCardDTO> resolvedCards = new List<PathCardDTO>();
                foreach (string cardId in collection.PathCardIds)
                {
                    if (_gameWorld.AllPathCards.ContainsKey(cardId))
                    {
                        resolvedCards.Add(_gameWorld.AllPathCards[cardId]);
                    }
                }
                return resolvedCards;
            }
            
            // Otherwise use inline PathCards (for event collections)
            return collection.PathCards ?? new List<PathCardDTO>();
        }

        return new List<PathCardDTO>();
    }
    
    /// <summary>
    /// Get or draw an event for a segment (ensures deterministic behavior)
    /// </summary>
    private string GetOrDrawEventForSegment(RouteSegment segment, TravelSession session, List<string> eventIds)
    {
        // Check if we already drew an event for this segment
        string key = $"seg_{segment.SegmentNumber}_event";
        if (session.SegmentEventDraws.ContainsKey(key))
        {
            return session.SegmentEventDraws[key];
        }
        
        // Draw random event from collection
        string eventId = eventIds[_random.Next(eventIds.Count)];
        session.SegmentEventDraws[key] = eventId;
        
        return eventId;
    }
    
    /// <summary>
    /// Draw a random collection from the segment's collection pool for legacy Event-type segments
    /// </summary>
    private string DrawRandomCollection(RouteSegment segment)
    {
        // Use segment's collection pool if available
        if (segment.CollectionPool != null && segment.CollectionPool.Count > 0)
        {
            int index = _random.Next(segment.CollectionPool.Count);
            return segment.CollectionPool[index];
        }

        // Fall back to single collection ID if no pool
        return segment.CollectionId;
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

        // Get the card from the current segment's collection
        PathCardDTO card = GetCardFromCurrentSegment(pathCardId);
        if (card == null)
        {
            return false;
        }

        // Check if card is already discovered (face-up)
        if (_gameWorld.PathCardDiscoveries.ContainsKey(pathCardId) && _gameWorld.PathCardDiscoveries[pathCardId])
        {
            return false; // Card already revealed
        }
        
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
        
        // Get the card from the current segment's collection
        PathCardDTO card = GetCardFromCurrentSegment(pathCardId);
        if (card == null)
        {
            return false;
        }
        
        // Final affordability check (in case something changed)
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        if (card.CoinRequirement > 0 && _gameWorld.GetPlayer().Coins < card.CoinRequirement)
        {
            return false;
        }

        // Deduct costs and add system messages
        if (card.StaminaCost > 0)
        {
            session.StaminaRemaining -= card.StaminaCost;
            _messageSystem.AddSystemMessage($"Spent {card.StaminaCost} stamina for path choice", SystemMessageTypes.Info);
        }
        
        if (card.CoinRequirement > 0)
        {
            _gameWorld.GetPlayer().ModifyCoins(-card.CoinRequirement);
            _messageSystem.AddSystemMessage($"Paid {card.CoinRequirement} coins for passage", SystemMessageTypes.Info);
        }

        // Apply effects with messages
        ApplyPathCardEffects(card);

        // Record path selection
        session.SelectedPathId = pathCardId;
        if (card.TravelTimeMinutes > 0)
        {
            session.TimeElapsed += card.TravelTimeMinutes;
            _messageSystem.AddSystemMessage($"Journey time increased by {card.TravelTimeMinutes} minutes", SystemMessageTypes.Info);
        }

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
    /// Select and play a path card from the current segment - ALL cards now use reveal mechanic
    /// </summary>
    public bool SelectPathCard(string pathCardId)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        // Check if this is an event response card (from Event segment)
        RouteOption route = GetRoute(session.RouteId);
        if (route != null && session.CurrentSegment <= route.Segments.Count)
        {
            RouteSegment segment = route.Segments[session.CurrentSegment - 1];
            if (segment.Type == SegmentType.Event)
            {
                // For event response cards, they're always face-up
                // Just set the reveal state so player can confirm
                session.IsRevealingCard = true;
                session.RevealedCardId = pathCardId;
                return true;
            }
        }

        // Check if card exists in current segment's collection
        PathCardDTO card = GetCardFromCurrentSegment(pathCardId);
        if (card == null)
        {
            return false;
        }

        // Check if card is already discovered (face-up)
        bool isDiscovered = _gameWorld.PathCardDiscoveries.ContainsKey(pathCardId) && 
                           _gameWorld.PathCardDiscoveries[pathCardId];
        
        // For already discovered cards, set them as revealed immediately so player can confirm
        if (isDiscovered)
        {
            // Set reveal state for already discovered card
            session.IsRevealingCard = true;
            session.RevealedCardId = pathCardId;
            return true;
        }
        
        // For undiscovered cards, use the reveal mechanic
        return RevealPathCard(pathCardId);
    }

    /// <summary>
    /// Rest to recover stamina during travel
    /// - Add 30 minutes to travel time
    /// - Restore stamina to current capacity
    /// - Does NOT skip segments (segments are challenges that must be completed)
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

        // Player remains on current segment - must still select a path card to progress

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

    /// <summary>
    /// Finish the route when journey is ready to complete (after last segment)
    /// </summary>
    public bool FinishRoute()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null || !session.IsReadyToComplete)
        {
            return false;
        }

        // Complete the journey
        CompleteJourney(session);
        return true;
    }
    
    /// <summary>
    /// Get current event narrative for UI display (null if not an event or no event selected)
    /// </summary>
    public string GetCurrentEventNarrative()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        return session?.CurrentEventNarrative;
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Get a card from the current segment's collection
    /// </summary>
    private PathCardDTO GetCardFromCurrentSegment(string cardId)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return null;
        }

        RouteOption route = GetRoute(session.RouteId);
        if (route == null || session.CurrentSegment > route.Segments.Count)
        {
            return null;
        }

        RouteSegment segment = route.Segments[session.CurrentSegment - 1];
        
        if (segment.Type == SegmentType.FixedPath)
        {
            return GetCardFromFixedPathSegment(segment, cardId);
        }
        else if (segment.Type == SegmentType.Event)
        {
            return GetCardFromEventSegment(segment, session, cardId);
        }
        
        return null;
    }
    
    /// <summary>
    /// Get a specific card from a FixedPath segment
    /// </summary>
    private PathCardDTO GetCardFromFixedPathSegment(RouteSegment segment, string cardId)
    {
        // Use new normalized property first, fall back to legacy
        string collectionId = segment.PathCollectionId ?? segment.CollectionId;
        
        if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.ContainsKey(collectionId))
        {
            return null;
        }
        
        PathCardCollectionDTO collection = _gameWorld.AllPathCollections[collectionId];
        
        // Check if this collection uses ID references
        if (collection.PathCardIds != null && collection.PathCardIds.Contains(cardId))
        {
            // Resolve from main path cards dictionary
            return _gameWorld.AllPathCards.ContainsKey(cardId) ? _gameWorld.AllPathCards[cardId] : null;
        }
        
        // Otherwise check inline cards (legacy)
        return collection.PathCards?.FirstOrDefault(c => c.Id == cardId);
    }
    
    /// <summary>
    /// Get a specific card from an Event segment
    /// </summary>
    private PathCardDTO GetCardFromEventSegment(RouteSegment segment, TravelSession session, string cardId)
    {
        // Try normalized structure first
        string eventCollectionId = segment.EventCollectionId ?? segment.CollectionId;
        
        if (!string.IsNullOrEmpty(eventCollectionId) && _gameWorld.AllEventCollections.ContainsKey(eventCollectionId))
        {
            // Normalized structure: look in event cards
            return _gameWorld.AllEventCards.ContainsKey(cardId) ? _gameWorld.AllEventCards[cardId] : null;
        }
        
        // Legacy structure: look in path collections
        string collectionId = session.CurrentEventId ?? segment.CollectionId;
        
        if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.ContainsKey(collectionId))
        {
            return null;
        }
        
        PathCardCollectionDTO collection = _gameWorld.AllPathCollections[collectionId];
        
        // Check if this collection uses ID references
        if (collection.PathCardIds != null && collection.PathCardIds.Contains(cardId))
        {
            // Resolve from main path cards dictionary
            return _gameWorld.AllPathCards.ContainsKey(cardId) ? _gameWorld.AllPathCards[cardId] : null;
        }
        
        // Otherwise check inline cards (for event collections)
        return collection.PathCards?.FirstOrDefault(c => c.Id == cardId);
    }

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
            TravelState.Fresh => 5,
            TravelState.Steady => 6,
            TravelState.Tired => 4,
            TravelState.Weary => 3,
            TravelState.Exhausted => 0,
            _ => 5
        };
    }

    /// <summary>
    /// Apply path card effects to player with system messages
    /// </summary>
    private void ApplyPathCardEffects(PathCardDTO card)
    {
        Player player = _gameWorld.GetPlayer();

        // Apply hunger effect with message
        if (card.HungerEffect != 0)
        {
            player.ModifyHunger(card.HungerEffect);
            if (card.HungerEffect > 0)
            {
                _messageSystem.AddSystemMessage($"Hunger increased by {card.HungerEffect}", SystemMessageTypes.Warning);
            }
            else
            {
                _messageSystem.AddSystemMessage($"Hunger decreased by {Math.Abs(card.HungerEffect)}", SystemMessageTypes.Success);
            }
        }

        // Apply one-time rewards with messages
        if (card.IsOneTime && !string.IsNullOrEmpty(card.OneTimeReward))
        {
            ApplyOneTimeReward(card.OneTimeReward, card.Id);
        }
    }

    /// <summary>
    /// Apply one-time reward effects with system messages
    /// </summary>
    private void ApplyOneTimeReward(string reward, string cardId)
    {
        Player player = _gameWorld.GetPlayer();

        // Parse reward string and apply effects
        if (reward.StartsWith("observation_"))
        {
            // Add observation card to player deck
            // TODO: Implement observation card system
            _messageSystem.AddSystemMessage($"One-time reward claimed: {reward}", SystemMessageTypes.Success);
        }
        else if (reward.EndsWith("_coins"))
        {
            // Extract coin amount and add to player
            if (reward.StartsWith("3_"))
            {
                player.ModifyCoins(3);
                _messageSystem.AddSystemMessage($"One-time reward claimed: 3 coins", SystemMessageTypes.Success);
            }
            // Add more coin parsing as needed
        }
        else
        {
            _messageSystem.AddSystemMessage($"One-time reward claimed: {reward}", SystemMessageTypes.Success);
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
        else if (session.StaminaRemaining <= 3)
        {
            session.CurrentState = TravelState.Weary;
            session.StaminaCapacity = 3;
        }
        else if (session.StaminaRemaining <= 4)
        {
            session.CurrentState = TravelState.Tired;
            session.StaminaCapacity = 4;
        }
        else if (session.StaminaRemaining >= 6)
        {
            session.CurrentState = TravelState.Steady;
            session.StaminaCapacity = 6;
        }
        else
        {
            session.CurrentState = TravelState.Fresh;
            session.StaminaCapacity = 5;
        }
    }

    /// <summary>
    /// Advance to next segment or mark journey as ready to complete
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
            // Clear event state for the new segment
            session.CurrentEventId = null;
            session.CurrentEventNarrative = null;
        }
        else
        {
            // Journey is ready to complete but NOT auto-completed
            // Player must explicitly click "Finish Route" button
            session.IsReadyToComplete = true;
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
        
        int baseStamina = 5; // Default Fresh state
        
        // Health affects maximum stamina capacity
        if (player.Health >= 80)
        {
            baseStamina = 6; // Steady state when healthy
        }
        else if (player.Health <= 30)
        {
            baseStamina = 3; // Weary when unhealthy
        }

        // Hunger affects current stamina
        if (player.Hunger >= 80)
        {
            baseStamina = Math.Max(3, baseStamina - 2); // Very hungry = low stamina
        }
        else if (player.Hunger >= 60)
        {
            baseStamina = Math.Max(3, baseStamina - 1); // Hungry = reduced stamina
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
            >= 6 => TravelState.Steady,
            5 => TravelState.Fresh,
            4 => TravelState.Tired,
            3 => TravelState.Weary,
            _ => TravelState.Exhausted
        };
    }
}


