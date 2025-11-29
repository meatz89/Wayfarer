
public class TravelManager
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly RewardApplicationService _rewardApplicationService;

    public TravelManager(
        GameWorld gameWorld,
        TimeManager timeManager,
        MessageSystem messageSystem,
        SceneInstantiator sceneInstantiator,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _sceneInstantiator = sceneInstantiator;
        _rewardApplicationService = rewardApplicationService;
    }

    // ========== TRAVEL SESSION METHODS ==========

    /// <summary>
    /// Start a journey on a specific route, initializing a travel session
    /// INTERNAL: Called by TravelFacade, not directly by UI
    /// </summary>
    internal TravelSession StartJourney(RouteOption route)
    {
        // No lookup needed!
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
            Route = route,  // HIGHLANDER: Object reference, not RouteId
            CurrentSegment = 1,
            StaminaRemaining = startingStamina,
            StaminaCapacity = startingStamina,
            CurrentState = DetermineInitialTravelState(player),
            SegmentsElapsed = 0,
            CompletedSegments = new List<string>(),
            // ADR-007: SelectedPath initialized to null (no empty string ID)
            SelectedPath = null
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

        RouteOption route = session.Route;  // HIGHLANDER: Object reference
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
        else if (segment.Type == SegmentType.Encounter)
        {
            // Encounter segments use Scene-Situation system, not PathCards
            // Spawn Scene from MandatorySceneTemplate if not already spawned
            SpawnEncounterScene(segment, session);
            return new List<PathCardDTO>(); // No cards - UI shows Scene instead
        }

        return new List<PathCardDTO>();
    }

    /// <summary>
    /// Spawn Scene from Encounter segment's MandatorySceneTemplate
    /// Scene is spawned once when player reaches segment, stored in PendingScene
    /// Uses SceneInstantiator.ActivateScene() for proper entity resolution
    /// </summary>
    private void SpawnEncounterScene(RouteSegment segment, TravelSession session)
    {
        // Already spawned - don't respawn
        if (session.PendingScene != null)
            return;

        SceneTemplate template = segment.MandatorySceneTemplate;
        if (template == null)
        {
            _messageSystem.AddSystemMessage("Encounter segment has no scene template", SystemMessageTypes.Warning);
            return;
        }

        // Check if Scene already exists for this template (avoid duplicate spawning)
        Scene existingScene = _gameWorld.Scenes.FirstOrDefault(s =>
            s.Template == template && s.State == SceneState.Active);
        if (existingScene != null)
        {
            session.PendingScene = existingScene;
            return;
        }

        // Create new Scene from template in Deferred state
        Scene scene = new Scene
        {
            TemplateId = template.Id,
            Template = template,
            State = SceneState.Deferred,
            Archetype = template.Archetype,
            DisplayName = template.DisplayNameTemplate,
            IntroNarrative = template.IntroNarrativeTemplate,
            Category = template.Category,
            SpawnRules = template.SpawnRules
        };

        // Add to GameWorld before activation
        _gameWorld.Scenes.Add(scene);

        // Build activation context for route encounter
        Player player = _gameWorld.GetPlayer();
        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = null, // Route encounters have no specific location
            CurrentVenue = null,
            CurrentNPC = null,
            CurrentRoute = session.Route,
            CurrentSituation = null
        };

        // Activate scene - creates Situations and resolves entities
        _sceneInstantiator.ActivateScene(scene, activationContext);

        // Set as pending scene for this segment
        session.PendingScene = scene;
        _messageSystem.AddSystemMessage($"Encounter: {scene.DisplayName}", SystemMessageTypes.Info);
    }

    /// <summary>
    /// Get path cards for FixedPath segments - direct access via object reference
    /// HIGHLANDER: NO lookups - segment has PathCollection object reference from parse-time
    /// </summary>
    private List<PathCardDTO> GetPathCardsForFixedPathSegment(RouteSegment segment)
    {
        // HIGHLANDER: Use object reference directly, NO ID lookup
        PathCardCollectionDTO collection = segment.PathCollection;

        if (collection == null)
        {
            return new List<PathCardDTO>();
        }

        // Return embedded cards directly - no lookup needed
        return collection.PathCards;
    }

    /// <summary>
    /// Get path cards for Event segments - direct access via object reference
    /// HIGHLANDER: NO lookups - segment has EventCollection object reference from parse-time
    /// </summary>
    private List<PathCardDTO> GetPathCardsForEventSegment(RouteSegment segment, TravelSession session)
    {
        // HIGHLANDER: Use object reference directly, NO ID lookup
        PathCardCollectionDTO eventCollection = segment.EventCollection;

        if (eventCollection == null)
        {
            return new List<PathCardDTO>();
        }

        return HandleNormalizedEventSegment(segment, session, eventCollection);
    }

    /// <summary>
    /// Handle normalized event structure: EventCollection → Event → EventCards
    /// HIGHLANDER: Accept EventCollection object, not eventCollectionId string
    /// </summary>
    private List<PathCardDTO> HandleNormalizedEventSegment(RouteSegment segment, TravelSession session, PathCardCollectionDTO eventCollection)
    {
        if (eventCollection.EventIds == null || eventCollection.EventIds.Count == 0)
        {
            return new List<PathCardDTO>();
        }

        // Step 1: Get or draw event for this segment
        string eventId = GetOrDrawEventForSegment(segment, session, eventCollection.EventIds);

        if (!_gameWorld.AllTravelEvents.Any(e => e.TravelEvent.Id == eventId))
        {
            return new List<PathCardDTO>();
        }

        TravelEventDTO travelEvent = _gameWorld.GetTravelEvent(eventId);

        // Step 3: Set narrative for UI
        session.CurrentEventNarrative = travelEvent.NarrativeText;

        // Step 4: Return embedded event cards directly - no lookup needed
        return travelEvent.EventCards;
    }

    /// <summary>
    /// Get or draw an event for a segment (ensures deterministic behavior)
    /// DDR-007: Event selection is deterministic based on segment properties
    /// </summary>
    private string GetOrDrawEventForSegment(RouteSegment segment, TravelSession session, List<string> eventIds)
    {
        // Check if we already drew an event for this segment
        string key = $"seg_{segment.SegmentNumber}_event";
        if (session.SegmentEventDraws.Any(kvp => kvp.Key == key))
        {
            return session.SegmentEventDraws[key];
        }

        // DDR-007: Deterministic event selection based on segment number
        // Same segment number always produces same event (predictable)
        int deterministicIndex = segment.SegmentNumber % eventIds.Count;
        string eventId = eventIds[deterministicIndex];
        session.SegmentEventDraws[key] = eventId;

        return eventId;
    }

    /// <summary>
    /// Reveal a face-down path card without playing it
    /// HIGHLANDER: Accept PathCardDTO object, not string ID
    /// </summary>
    public bool RevealPathCard(PathCardDTO card)
    {
        if (card == null)
        {
            return false;
        }

        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        // Check if card is already discovered (face-up)
        if (_gameWorld.IsPathCardDiscovered(card))
        {
            return false; // Card already revealed
        }

        // Basic affordability checks (same as SelectPathCard)
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        if (card.CoinRequirement > 0)
        {
            Player player = _gameWorld.GetPlayer();
            Consequence cost = new Consequence { Coins = -card.CoinRequirement };
            CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
            if (!resourceReq.IsAnySatisfied(player, _gameWorld))
            {
                return false;
            }
        }

        // Check one-time card usage
        if (card.IsOneTime && _gameWorld.IsPathCardDiscovered(card))
        {
            return false;
        }

        // Mark card as discovered (face-up)
        _gameWorld.SetPathCardDiscovered(card, true);

        // ADR-007: Set reveal state with PathCardDTO object (not ID)
        session.IsRevealingCard = true;
        session.RevealedCard = card;

        return true;
    }

    /// <summary>
    /// Confirm the revealed card and apply its effects, then advance to next segment
    /// ADR-007: Use RevealedCard object (not RevealedCardId)
    /// TWO PILLARS: Delegates mutations to RewardApplicationService
    /// </summary>
    public async Task<bool> ConfirmRevealedCard()
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        // ADR-007: Check if RevealedCard object is null (not string empty check)
        if (session == null || !session.IsRevealingCard || session.RevealedCard == null)
        {
            return false;
        }

        // ADR-007: Get card object and ID from RevealedCard (no ID lookup needed)
        PathCardDTO card = session.RevealedCard;
        string pathCardId = card.Id;

        // Clear reveal state
        session.IsRevealingCard = false;
        session.RevealedCard = null;

        // Apply selection effects (shared logic with discovered cards)
        return await ApplyPathCardSelectionEffects(card, pathCardId);
    }

    /// <summary>
    /// Apply path card selection effects - shared logic for both revealed and already-discovered cards
    /// TWO PILLARS: Delegates mutations to RewardApplicationService
    /// </summary>
    private async Task<bool> ApplyPathCardSelectionEffects(PathCardDTO card, string pathCardId)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        Player player = _gameWorld.GetPlayer();

        // Affordability checks
        if (session.StaminaRemaining < card.StaminaCost)
        {
            return false;
        }

        // HIGHLANDER: Use CompoundRequirement for coin affordability check
        if (card.CoinRequirement > 0)
        {
            Consequence cost = new Consequence { Coins = -card.CoinRequirement };
            CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
            if (!resourceReq.IsAnySatisfied(player, _gameWorld))
            {
                return false;
            }
        }

        // Deduct stamina (travel-specific resource, not player resource)
        if (card.StaminaCost > 0)
        {
            session.StaminaRemaining -= card.StaminaCost;
            _messageSystem.AddSystemMessage($"Spent {card.StaminaCost} stamina for path choice", SystemMessageTypes.Info);
        }

        // TWO PILLARS: Apply coin cost via Consequence + ApplyConsequence
        if (card.CoinRequirement > 0)
        {
            Consequence passageCost = new Consequence { Coins = -card.CoinRequirement };
            await _rewardApplicationService.ApplyConsequence(passageCost, null);
            _messageSystem.AddSystemMessage($"Paid {card.CoinRequirement} coins for passage", SystemMessageTypes.Info);
        }

        // Apply effects with messages
        await ApplyPathCardEffects(card);

        // ADR-007: Record path selection with PathCardDTO object (not ID)
        session.SelectedPath = card;
        if (card.TravelTimeSegments > 0)
        {
            session.SegmentsElapsed += card.TravelTimeSegments;
            _messageSystem.AddSystemMessage($"Journey time increased by {card.TravelTimeSegments} segments", SystemMessageTypes.Info);
        }

        // Update travel state based on stamina
        UpdateTravelState(session);

        // Check if we're on the last segment
        RouteOption route = session.Route;  // HIGHLANDER: Object reference
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
    /// HIGHLANDER: Accept PathCardDTO object, not string ID
    /// TWO PILLARS: Delegates mutations to RewardApplicationService
    /// </summary>
    public async Task<bool> SelectPathCard(PathCardDTO card)
    {
        if (card == null)
        {
            return false;
        }

        TravelSession session = _gameWorld.CurrentTravelSession;
        if (session == null)
        {
            return false;
        }

        // Check if this is an event response card (from Event segment)
        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route != null && session.CurrentSegment <= route.Segments.Count)
        {
            RouteSegment segment = route.Segments[session.CurrentSegment - 1];
            if (segment.Type == SegmentType.Event)
            {
                // Set reveal state with object reference (not ID)
                session.IsRevealingCard = true;
                session.RevealedCard = card;
                return true;
            }
        }

        // Check if card is already discovered (face-up)
        bool isDiscovered = _gameWorld.IsPathCardDiscovered(card);

        // For already discovered cards, apply effects immediately (no reveal screen needed)
        if (isDiscovered)
        {
            return await ApplyPathCardSelectionEffects(card, card.Id);
        }

        // For undiscovered cards, use the reveal mechanic
        return RevealPathCard(card);
    }

    /// <summary>
    /// Resolve pending scene after player completes scene situations
    /// Called by GameFacade after scene intensity reaches 0
    /// ADR-007: Accept Scene object (not sceneId string)
    /// </summary>
    public bool ResolveScene(Scene scene)
    {
        TravelSession session = _gameWorld.CurrentTravelSession;
        // ADR-007: Check if PendingScene matches (object reference, not ID comparison)
        if (session == null || session.PendingScene != scene)
        {
            return false;
        }

        if (scene == null || scene.State != SceneState.Completed)
        {
            return false;
        }

        // ADR-007: Clear pending scene (null object, not null ID)
        session.PendingScene = null;
        _messageSystem.AddSystemMessage($"Scene resolved: {scene.DisplayName}", SystemMessageTypes.Success);

        // Now advance segment or complete route
        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route != null && session.CurrentSegment == route.Segments.Count)
        {
            session.IsReadyToComplete = true;
        }
        else
        {
            AdvanceSegment(session);
        }

        return true;
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

        // Player returns to origin Venue - no actual movement needed
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
        if (session == null)
            return null;
        return session.CurrentEventNarrative;
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

        RouteOption route = session.Route;  // HIGHLANDER: Object reference
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
    /// HIGHLANDER: Use PathCollection object reference directly, NO lookup
    /// </summary>
    private PathCardDTO GetCardFromFixedPathSegment(RouteSegment segment, string cardId)
    {
        // HIGHLANDER: Use object reference directly, NO ID lookup
        PathCardCollectionDTO collection = segment.PathCollection;

        if (collection == null)
        {
            return null;
        }

        // Look in embedded path cards
        return collection.PathCards.FirstOrDefault(c => c.Id == cardId);
    }

    /// <summary>
    /// Get a specific card from an Event segment
    /// ADR-007: Use CurrentEvent object (not CurrentEventId)
    /// </summary>
    private PathCardDTO GetCardFromEventSegment(RouteSegment segment, TravelSession session, string cardId)
    {
        // ADR-007: Get CurrentEvent object (no null/empty check on ID)
        if (session.CurrentEvent == null)
            return null;

        TravelEventDTO travelEvent = session.CurrentEvent;

        // Find the card in the embedded event cards
        return travelEvent.EventCards.FirstOrDefault(c => c.Id == cardId);
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
    /// TWO PILLARS: Delegates mutations to RewardApplicationService
    /// </summary>
    private async Task ApplyPathCardEffects(PathCardDTO card)
    {
        // TWO PILLARS: Build Consequence for all path card effects
        Consequence pathEffects = new Consequence
        {
            Hunger = card.HungerEffect,
            Coins = card.CoinReward
        };

        // Apply via RewardApplicationService
        await _rewardApplicationService.ApplyConsequence(pathEffects, null);

        // Generate messages based on effects
        if (card.HungerEffect != 0)
        {
            if (card.HungerEffect > 0)
            {
                _messageSystem.AddSystemMessage($"Hunger increased by {card.HungerEffect}", SystemMessageTypes.Warning);
            }
            else
            {
                _messageSystem.AddSystemMessage($"Hunger decreased by {Math.Abs(card.HungerEffect)}", SystemMessageTypes.Success);
            }
        }

        if (card.CoinReward > 0)
        {
            _messageSystem.AddSystemMessage($"Gained {card.CoinReward} coins from path", SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Update travel state based on current stamina.
    /// States checked from HIGH to LOW using >= to ensure correct tier assignment.
    /// </summary>
    private void UpdateTravelState(TravelSession session)
    {
        if (session.StaminaRemaining <= 0)
        {
            session.CurrentState = TravelState.Exhausted;
            session.StaminaCapacity = 0;
        }
        else if (session.StaminaRemaining >= 6)
        {
            session.CurrentState = TravelState.Steady;
            session.StaminaCapacity = 6;
        }
        else if (session.StaminaRemaining >= 5)
        {
            session.CurrentState = TravelState.Fresh;
            session.StaminaCapacity = 5;
        }
        else if (session.StaminaRemaining >= 4)
        {
            session.CurrentState = TravelState.Tired;
            session.StaminaCapacity = 4;
        }
        else if (session.StaminaRemaining >= 3)
        {
            session.CurrentState = TravelState.Weary;
            session.StaminaCapacity = 3;
        }
        else
        {
            // 1-2 stamina remaining
            session.CurrentState = TravelState.Exhausted;
            session.StaminaCapacity = 0;
        }
    }

    /// <summary>
    /// Advance to next segment or mark journey as ready to complete
    /// </summary>
    private void AdvanceSegment(TravelSession session)
    {
        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route == null) return;

        // Mark current segment as completed
        session.CompletedSegments.Add($"{route.Name}_{session.CurrentSegment}");

        // Check if there are more segments
        if (session.CurrentSegment < route.Segments.Count)
        {
            session.CurrentSegment++;
            // ADR-007: Clear event state for new segment (null object, not empty string ID)
            session.CurrentEvent = null;
            session.CurrentEventNarrative = "";

            // Pre-load cards for the new segment (works for both FixedPath and Event segments)
            // For Event segments, this triggers event selection and sets CurrentEvent
            // For FixedPath segments, this just ensures cards are ready
            GetSegmentCards();
        }
        // Note: IsReadyToComplete is set in SelectPathCard when on the last segment
        // This ensures The Single Player must select a card even on the final segment
    }

    /// <summary>
    /// Complete the journey and update player location
    /// Grants +1 ExplorationCube per completion (max 10 cubes total)
    /// </summary>
    private void CompleteJourney(TravelSession session)
    {
        RouteOption route = session.Route;  // HIGHLANDER: Object reference
        if (route == null) return;

        Player player = _gameWorld.GetPlayer();

        // Move player to destination
        // HEX-FIRST ARCHITECTURE: Set player position via hex coordinates
        Location targetSpot = route.DestinationLocation;

        if (targetSpot != null)
        {
            if (!targetSpot.HexPosition.HasValue)
                throw new InvalidOperationException($"Destination location '{targetSpot.Name}' has no HexPosition - cannot complete journey");

            player.CurrentPosition = targetSpot.HexPosition.Value;

            // Increment Location familiarity (max 3)
            // HIGHLANDER: Pass Location object directly to Player API
            int currentFamiliarity = player.GetLocationFamiliarity(targetSpot);
            player.SetLocationFamiliarity(targetSpot, Math.Min(3, currentFamiliarity + 1));
        }

        // Increase route familiarity (max 5)
        // HIGHLANDER: Pass RouteOption object directly to Player API
        player.IncreaseRouteFamiliarity(route, 1);

        // Grant ExplorationCubes for route mastery (max 10)
        // Each completion grants +1 cube, revealing more hidden paths
        int currentCubes = route.ExplorationCubes;
        if (currentCubes < 10)
        {
            route.ExplorationCubes = currentCubes + 1;
            _messageSystem.AddSystemMessage($"Route mastery increased: {route.ExplorationCubes}/10 exploration cubes", SystemMessageTypes.Success);
        }

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

