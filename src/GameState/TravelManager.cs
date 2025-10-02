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

        // Track route as known
        player.AddKnownRoute(route);

        TravelSession session = new TravelSession
        {
            RouteId = routeId,
            CurrentSegment = 1,
            StaminaRemaining = startingStamina,
            StaminaCapacity = startingStamina,
            CurrentState = DetermineInitialTravelState(player),
            SegmentsElapsed = 0,
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
        string collectionId = segment.PathCollectionId;

        if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.Any(p => p.CollectionId == collectionId))
        {
            return new List<PathCardDTO>();
        }

        PathCardCollectionDTO collection = _gameWorld.AllPathCollections.GetCollection(collectionId);

        // Return embedded cards directly - no lookup needed
        return collection.PathCards ?? new List<PathCardDTO>();
    }

    /// <summary>
    /// Get path cards for Event segments - two-step resolution: Collection → Event → Cards
    /// </summary>
    private List<PathCardDTO> GetPathCardsForEventSegment(RouteSegment segment, TravelSession session)
    {
        // Step 1: Get event collection ID
        string eventCollectionId = segment.EventCollectionId;

        if (string.IsNullOrEmpty(eventCollectionId))
        {
            return new List<PathCardDTO>();
        }

        // Check for normalized structure
        if (_gameWorld.AllEventCollections.Any(e => e.CollectionId == eventCollectionId))
        {
            return HandleNormalizedEventSegment(segment, session, eventCollectionId);
        }

        return new List<PathCardDTO>();
    }

    /// <summary>
    /// Handle normalized event structure: EventCollection → Event → EventCards
    /// </summary>
    private List<PathCardDTO> HandleNormalizedEventSegment(RouteSegment segment, TravelSession session, string eventCollectionId)
    {
        // Step 1: Get event collection
        PathCardCollectionDTO eventCollection = _gameWorld.AllEventCollections.GetCollection(eventCollectionId);

        if (eventCollection.EventIds == null || eventCollection.EventIds.Count == 0)
        {
            return new List<PathCardDTO>();
        }

        // Step 2: Get or draw event for this segment  
        string eventId = GetOrDrawEventForSegment(segment, session, eventCollection.EventIds);

        // Step 3: Get the event
        if (!_gameWorld.AllTravelEvents.Any(e => e.EventId == eventId))
        {
            return new List<PathCardDTO>();
        }

        TravelEventDTO travelEvent = _gameWorld.AllTravelEvents.GetEvent(eventId);

        // Step 4: Set narrative for UI
        session.CurrentEventNarrative = travelEvent.NarrativeText;

        // Step 5: Return embedded event cards directly - no lookup needed
        return travelEvent.EventCards ?? new List<PathCardDTO>();
    }


    /// <summary>
    /// Get or draw an event for a segment (ensures deterministic behavior)
    /// </summary>
    private string GetOrDrawEventForSegment(RouteSegment segment, TravelSession session, List<string> eventIds)
    {
        // Check if we already drew an event for this segment
        string key = $"seg_{segment.SegmentNumber}_event";
        if (session.SegmentEventDraws.Any(kvp => kvp.Key == key))
        {
            return session.SegmentEventDraws[key];
        }

        // Draw random event from collection
        string eventId = eventIds[_random.Next(eventIds.Count)];
        session.SegmentEventDraws[key] = eventId;

        return eventId;
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
        if (_gameWorld.PathCardDiscoveries.IsDiscovered(pathCardId))
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
        if (card.IsOneTime && _gameWorld.PathCardRewardsClaimed.IsDiscovered(pathCardId))
        {
            return false;
        }

        // Mark card as discovered (face-up)
        _gameWorld.PathCardDiscoveries.SetDiscovered(pathCardId, true);

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
        if (card.TravelTimeSegments > 0)
        {
            session.SegmentsElapsed += card.TravelTimeSegments;
            _messageSystem.AddSystemMessage($"Journey time increased by {card.TravelTimeSegments} segments", SystemMessageTypes.Info);
        }

        // Clear reveal state
        session.IsRevealingCard = false;
        session.RevealedCardId = null;

        // Update travel state based on stamina
        UpdateTravelState(session);

        // Check if we're on the last segment
        RouteOption route = GetRoute(session.RouteId);
        if (route != null && session.CurrentSegment == route.Segments.Count)
        {
            // This was the last segment - mark journey as ready to complete
            session.IsReadyToComplete = true;
        }
        else
        {
            // Move to next segment
            AdvanceSegment(session);
        }

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
        bool isDiscovered = _gameWorld.PathCardDiscoveries.IsDiscovered(pathCardId);

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
    /// - Add 2 segments to travel time
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
        session.SegmentsElapsed += 1; // 1 segment to rest
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
        string collectionId = segment.PathCollectionId;

        if (string.IsNullOrEmpty(collectionId) || !_gameWorld.AllPathCollections.Any(p => p.CollectionId == collectionId))
        {
            return null;
        }

        PathCardCollectionDTO collection = _gameWorld.AllPathCollections.GetCollection(collectionId);

        // Look in embedded path cards
        return collection.PathCards?.FirstOrDefault(c => c.Id == cardId);
    }

    /// <summary>
    /// Get a specific card from an Event segment
    /// </summary>
    private PathCardDTO GetCardFromEventSegment(RouteSegment segment, TravelSession session, string cardId)
    {
        // Get the current event ID from session state
        if (string.IsNullOrEmpty(session.CurrentEventId))
            return null;

        // Get the travel event
        TravelEventEntry? eventEntry = _gameWorld.AllTravelEvents.FindById(session.CurrentEventId);
        if (eventEntry == null)
            return null;

        TravelEventDTO travelEvent = eventEntry.TravelEvent;

        // Find the card in the embedded event cards
        return travelEvent.EventCards?.FirstOrDefault(c => c.Id == cardId);
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
            // Observation cards are not implemented in current design
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
        _gameWorld.PathCardRewardsClaimed.SetDiscovered(cardId, true);
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

            // Pre-load cards for the new segment (works for both FixedPath and Event segments)
            // For Event segments, this triggers event selection and sets CurrentEventId
            // For FixedPath segments, this just ensures cards are ready
            GetSegmentCards();
        }
        // Note: IsReadyToComplete is set in SelectPathCard when on the last segment
        // This ensures players must select a card even on the final segment
    }

    /// <summary>
    /// Complete the journey and update player location
    /// </summary>
    private void CompleteJourney(TravelSession session)
    {
        RouteOption route = GetRoute(session.RouteId);
        if (route == null) return;

        Player player = _gameWorld.GetPlayer();

        // Move player to destination
        LocationSpot targetSpot = _gameWorld.WorldState.locationSpots
            .FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);

        if (targetSpot != null)
        {
            player.CurrentLocationSpot = targetSpot;

            // Track location discovery
            string locationId = targetSpot.LocationId;
            if (!player.DiscoveredLocationIds.Contains(locationId))
            {
                player.DiscoveredLocationIds.Add(locationId);
            }

            // Increment location familiarity (max 3)
            int currentFamiliarity = player.GetLocationFamiliarity(locationId);
            player.SetLocationFamiliarity(locationId, Math.Min(3, currentFamiliarity + 1));
        }

        // Increase route familiarity (max 5)
        player.IncreaseRouteFamiliarity(session.RouteId, 1);

        // Apply travel time to game world
        _timeManager.AdvanceSegments(session.SegmentsElapsed);

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


