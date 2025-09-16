using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Manages all card deck operations for the new conversation system.
/// Handles single-card SPEAK mechanics, focus management, and dice roll success.
/// </summary>
public class CardDeckManager
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random;
    private readonly CategoricalEffectResolver _effectResolver;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly TokenMechanicsManager _tokenManager;
    // Removed exhausted pile - now using SessionCardDeck's discard pile

    public CardDeckManager(GameWorld gameWorld, CategoricalEffectResolver effectResolver,
        FocusManager focusManager, AtmosphereManager atmosphereManager, TokenMechanicsManager tokenManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _random = new Random();
    }

    /// <summary>
    /// Create a conversation deck from NPC templates (no filtering by type)
    /// Returns both the deck and any request cards that should start in hand
    /// </summary>
    public (SessionCardDeck deck, List<CardInstance> requestCards) CreateConversationDeck(NPC npc, ConversationType conversationType, string goalCardId = null, List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Start with player's conversation deck (persistent CardInstances with XP)
        Player player = _gameWorld.GetPlayer();
        List<CardInstance> playerInstances = new List<CardInstance>();

        // Get all player's card instances (these have XP)
        if (player.ConversationDeck != null && player.ConversationDeck.Count > 0)
        {
            playerInstances.AddRange(player.ConversationDeck.GetAllInstances());
        }
        else
        {
            // Critical error - player has no conversation abilities!
            Console.WriteLine("[CardDeckManager] ERROR: Player has no conversation deck! Check PackageLoader initialization.");
            // Continue anyway to avoid crash, but conversation will be unplayable
        }

        // Create session deck from player's instances (preserves XP)
        SessionCardDeck deck = SessionCardDeck.CreateFromInstances(playerInstances, sessionId);

        // Add unlocked NPC progression cards as new instances
        List<ConversationCard> unlockedProgressionCards = GetUnlockedProgressionCards(npc);
        foreach (var progressionCard in unlockedProgressionCards)
        {
            CardInstance progressionInstance = new CardInstance(progressionCard, npc.ID);
            deck.AddCard(progressionInstance);
        }

        // Safety check - ensure we have at least some cards
        if (playerInstances.Count == 0 && unlockedProgressionCards.Count == 0)
        {
            Console.WriteLine($"[CardDeckManager] WARNING: Creating conversation with {npc.Name} but deck has NO cards!");
        }

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Get request cards based on conversation type from JSON data
        // For Request conversations, this loads ALL cards from the Request bundle
        List<CardInstance> requestCards = SelectGoalCardsForConversationType(npc, conversationType, goalCardId, deck);

        // HIGHLANDER: Add request cards directly to deck's request pile
        foreach (var requestCard in requestCards)
        {
            deck.AddRequestCard(requestCard);
        }

        // Now shuffle the deck after all cards (including promise cards) have been added
        deck.ShuffleDrawPile();

        // HIGHLANDER: Return only deck, not separate request cards
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Draw cards based on connection state (no type filtering)
    /// </summary>
    public List<CardInstance> DrawCards(SessionCardDeck deck, int count)
    {
        // HIGHLANDER: Draw cards one by one since DrawCards was removed
        var drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            var card = deck.DrawCard();
            if (card != null)
                drawn.Add(card);
        }
        return drawn;
    }

    /// <summary>
    /// Play a single card with dice roll and focus management
    /// </summary>
    public CardPlayResult PlayCard(ConversationSession session, CardInstance selectedCard)
    {
        // Check if card is unplayable (but skip this check for promise cards which handle rapport separately)
        if (!selectedCard.IsPlayable &&
            !(selectedCard.CardType == CardType.Letter || selectedCard.CardType == CardType.Promise || selectedCard.CardType == CardType.BurdenGoal))
        {
            return new CardPlayResult
            {
                Results = new List<SingleCardResult>
                {
                    new SingleCardResult
                    {
                        Card = selectedCard,
                        Success = false,
                        Flow = 0,
                        Roll = 0,
                        SuccessChance = 0
                    }
                },
                FinalFlow = 0
            };
        }

        // Check for free focus from observation effect
        int focusCost = _atmosphereManager.IsNextSpeakFree() ? 0 : selectedCard.Focus;

        // Validate focus availability
        if (!_focusManager.CanAffordCard(focusCost))
        {
            return new CardPlayResult
            {
                Results = new List<SingleCardResult>
                {
                    new SingleCardResult
                    {
                        Card = selectedCard,
                        Success = false,
                        Flow = 0,
                        Roll = 0,
                        SuccessChance = 0
                    }
                },
                FinalFlow = 0
            };
        }

        // Calculate success percentage - use modified rate if personality rules applied it
        int successPercentage = selectedCard.Context?.ModifiedSuccessRate
            ?? _effectResolver.CalculateSuccessPercentage(selectedCard, session);

        // Promise/request cards (GoalCard) ALWAYS succeed
        bool success;
        int roll;

        if (selectedCard.CardType == CardType.Letter || selectedCard.CardType == CardType.Promise || selectedCard.CardType == CardType.BurdenGoal)
        {
            // Promise/request cards always succeed (100% success rate)
            success = true;
            roll = 100; // For display purposes
            successPercentage = 100; // Override to show 100% in UI
            
            // Mark request as completed if this is a BurdenGoal (request) card
            if (selectedCard.CardType == CardType.BurdenGoal && selectedCard.Context?.RequestId != null)
            {
                // Find and complete the request
                var request = session.NPC.GetRequestById(selectedCard.Context.RequestId);
                if (request != null)
                {
                    request.Complete();
                    // The conversation will end after this card is played
                }
            }
        }
        else
        {
            // Use pre-rolled value if available, otherwise generate one (shouldn't happen normally)
            roll = selectedCard.Context?.PreRolledValue ?? _random.Next(1, 101);

            // Check success using the pre-rolled value with momentum system
            success = _effectResolver.CheckSuccessWithPreRoll(roll, successPercentage, session);
        }

        // Spend focus (possibly 0 if free) - focus represents effort of speaking
        _focusManager.SpendFocus(focusCost);

        CardEffectResult effectResult = null;
        int flowChange = 0;

        if (success)
        {
            // Success always gives +1 to flow
            flowChange = 1;

            // Reset hidden momentum on success (bad luck protection resets)
            session.HiddenMomentum = 0;

            // Process card's success effect
            // PROJECTION PRINCIPLE: Get projection from resolver and apply it
            effectResult = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply personality modifier to rapport change
            int rapportChange = effectResult.RapportChange;
            if (session.PersonalityEnforcer != null && rapportChange != 0)
            {
                rapportChange = session.PersonalityEnforcer.ModifyRapportChange(selectedCard, rapportChange);
            }

            // Apply rapport changes to RapportManager
            if (rapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(rapportChange, session.CurrentAtmosphere);
            }

            // Apply focus restoration (for Focusing success effect)
            if (effectResult.FocusAdded > 0)
            {
                _focusManager.AddFocus(effectResult.FocusAdded);
            }

            // Apply flow battery change (for Advancing success effect)
            if (effectResult.FlowChange != 0)
            {
                flowChange += effectResult.FlowChange; // Add to the base flow change
            }

            // Add drawn cards to active cards (for Threading success effect)
            if (effectResult.CardsToAdd.Any())
            {
                session.Deck.Hand.AddRange(effectResult.CardsToAdd);
            }

            // Handle atmosphere change (for Atmospheric success effect)
            if (effectResult.AtmosphereTypeChange.HasValue)
            {
                _atmosphereManager.SetAtmosphere(effectResult.AtmosphereTypeChange.Value);
            }

            // Consume one-time atmosphere effects after successful card play
            _atmosphereManager.OnCardSuccess();
        }
        else
        {
            // Failure always gives -1 to flow
            flowChange = -1;

            // Increment hidden momentum for bad luck protection (invisible to player)
            session.HiddenMomentum = Math.Min(session.HiddenMomentum + 1, 4); // Cap at 4 failures

            // PROJECTION PRINCIPLE: Get projection from resolver
            effectResult = _effectResolver.ProcessFailureEffect(selectedCard, session);

            // Apply personality modifier to rapport change (for failure effects)
            int failureRapportChange = effectResult.RapportChange;
            if (session.PersonalityEnforcer != null && failureRapportChange != 0)
            {
                failureRapportChange = session.PersonalityEnforcer.ModifyRapportChange(selectedCard, failureRapportChange);
            }

            // Apply rapport changes to RapportManager (if any failure effects modify rapport)
            if (failureRapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(failureRapportChange, session.CurrentAtmosphere);
            }

            // Apply focus penalty (for ForceListen effect)
            if (effectResult.FocusAdded < 0)
            {
                _focusManager.AddFocus(effectResult.FocusAdded); // Adding negative = removing
            }

            // Apply card removal effects (Overreach/Disrupting)
            if (effectResult.CardsToAdd.Any())
            {
                // For failure effects, CardsToAdd represents cards to REMOVE
                // Overreach: all cards marked for removal
                // Disrupting: specific high-focus cards marked for removal
                foreach (var cardToRemove in effectResult.CardsToAdd)
                {
                    if (session.Deck.Hand.Contains(cardToRemove))
                    {
                        session.Deck.Hand.Remove(cardToRemove);
                        session.Deck.DiscardCard(cardToRemove); // HIGHLANDER: Use deck's discard
                    }
                }
            }

            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();
        }

        // HIGHLANDER: Use deck's PlayCard method which handles all transitions
        session.Deck.PlayCard(selectedCard);

        // Remove impulse cards from hand after SPEAK, executing exhaust effects
        bool conversationContinues = RemoveImpulseCardsFromHand(session);

        CardPlayResult result = new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Flow = flowChange,
                    Roll = roll,
                    SuccessChance = successPercentage
                }
            },
            FinalFlow = flowChange
        };

        // Handle exhaust ending conversation
        if (!conversationContinues)
        {
            result.Success = false; // Override to mark conversation as failed
        }

        return result;
    }

    /// <summary>
    /// Execute LISTEN action - refresh focus, draw cards, and exhaust opening cards
    /// </summary>
    public List<CardInstance> ExecuteListen(ConversationSession session)
    {
        // First, exhaust all Opening cards in hand
        if (!ExhaustOpeningCards(session))
        {
            // Exhaust effect ended conversation
            return new List<CardInstance>();
        }

        // Refresh focus
        _focusManager.RefreshPool();

        // Calculate draw count based on state and atmosphere
        int drawCount = session.GetDrawCount();

        // HIGHLANDER: Draw directly to hand
        session.Deck.DrawToHand(drawCount);

        // Get the drawn cards for return value
        var drawnCards = session.Deck.Hand.Cards.TakeLast(drawCount).ToList();

        // Check if any goal cards should become playable based on rapport
        UpdateGoalCardPlayabilityAfterListen(session);

        return drawnCards;
    }

    /// <summary>
    /// Check if goal cards should become playable after LISTEN based on rapport threshold
    /// </summary>
    public void UpdateGoalCardPlayabilityAfterListen(ConversationSession session)
    {
        // Get current rapport
        int currentRapport = session.RapportManager?.CurrentRapport ?? 0;
        
        // Check all goal cards in active hand
        foreach (CardInstance card in session.Deck.Hand.Cards)
        {
            // Only process goal cards that are currently Unplayable
            if ((card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
                && !card.IsPlayable)
            {
                // Check if rapport threshold is met
                int rapportThreshold = card.Context?.RapportThreshold ?? 0;
                
                if (currentRapport >= rapportThreshold)
                {
                    // Make card playable
                    card.IsPlayable = true;
                    // Request cards already have Impulse + Opening persistence set
                    
                    // Mark that a request card is now playable
                    session.RequestCardDrawn = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Legacy method for compatibility - now just marks request card presence
    /// </summary>
    public void UpdateRequestCardPlayability(ConversationSession session)
    {
        // This is called at conversation start - just check for goal card presence
        bool hasRequestCard = session.Deck.Hand.Cards
            .Any(c => c.CardType == CardType.Letter || c.CardType == CardType.Promise || c.CardType == CardType.BurdenGoal);

        if (hasRequestCard)
        {
            session.RequestCardDrawn = true;
        }
    }

    /// <summary>
    /// Update all cards' playability based on current focus availability
    /// Cards that cost more focus than available are marked Unplayable
    /// </summary>
    public void UpdateCardPlayabilityBasedOnFocus(ConversationSession session)
    {
        int availableFocus = _focusManager.AvailableFocus;

        // Check if next speak is free (from observation effect)
        bool isNextSpeakFree = _atmosphereManager.IsNextSpeakFree();

        foreach (CardInstance card in session.Deck.Hand.Cards)
        {
            // Skip request/promise cards - their playability is based on rapport, not focus
            if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
            {
                continue; // Don't modify request card playability here
            }

            // Calculate effective focus cost for this card
            int effectiveFocusCost = isNextSpeakFree ? 0 : card.Focus;

            // Check if we can afford this card
            bool canAfford = _focusManager.CanAffordCard(effectiveFocusCost);

            // Update playability based on focus availability
            card.IsPlayable = canAfford;
        }
    }

    /// <summary>
    /// Remove all impulse cards from hand (happens after every SPEAK)
    /// Executes exhaust effects before removing cards
    /// </summary>
    private bool RemoveImpulseCardsFromHand(ConversationSession session)
    {
        // Get all impulse cards
        List<CardInstance> impulseCards = session.Deck.Hand.Cards.Where(c => c.Persistence == PersistenceType.Impulse).ToList();

        foreach (CardInstance card in impulseCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustType != ExhaustEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.Deck.ExhaustFromHand(card); // HIGHLANDER: Use deck method
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Exhaust all opening cards in hand (happens on LISTEN)
    /// </summary>
    private bool ExhaustOpeningCards(ConversationSession session)
    {
        // Get all opening cards
        List<CardInstance> openingCards = session.Deck.Hand.Cards
            .Where(c => c.Persistence == PersistenceType.Opening)
            .ToList();

        foreach (CardInstance card in openingCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustType != ExhaustEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.Deck.ExhaustFromHand(card); // HIGHLANDER: Use deck method
        }

        return true; // Conversation continues
    }


    /// <summary>
    /// PROJECTION PRINCIPLE: Execute exhaust effect using projection from resolver.
    /// Exhaust effects are ALWAYS penalties (negative effects).
    /// </summary>
    private bool ExecuteExhaustEffect(CardInstance card, ConversationSession session)
    {
        if (card.ExhaustType == ExhaustEffectType.None)
            return true; // No exhaust effect, conversation continues

        // PROJECTION PRINCIPLE: Get projection from resolver
        var projection = _effectResolver.ProcessExhaustEffect(card, session);

        // Apply rapport penalty
        if (projection.RapportChange < 0 && session.RapportManager != null)
        {
            session.RapportManager.ApplyRapportChange(projection.RapportChange, session.CurrentAtmosphere);
        }

        // Apply focus penalty (negative focus)
        if (projection.FocusAdded < 0)
        {
            _focusManager.AddFocus(projection.FocusAdded); // Adding negative = removing
        }

        // Apply card removal penalty (Threading exhaust loses cards from hand)
        if (projection.CardsToAdd?.Count > 0)
        {
            // For exhaust Threading, CardsToAdd represents cards to LOSE from hand
            int cardsToLose = Math.Min(projection.CardsToAdd.Count, session.Deck.Hand.Count);
            for (int i = 0; i < cardsToLose; i++)
            {
                if (session.Deck.Hand.Cards.Any())
                {
                    var cardToRemove = session.Deck.Hand.Cards.First();
                    session.Deck.ExhaustFromHand(cardToRemove); // HIGHLANDER: Use deck method
                }
            }
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Get all exhausted cards (now in discard pile)
    /// </summary>
    public IReadOnlyList<CardInstance> GetExhaustedCards()
    {
        // Exhausted cards are now in the discard pile
        return new List<CardInstance>().AsReadOnly();
    }

    /// <summary>
    /// Validate if a card can be played (focus check and rapport for goal cards)
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        // Check if card is marked as Unplayable
        if (!card.IsPlayable)
            return false;

        // Check focus availability
        if (!_focusManager.CanAffordCard(card.Focus))
            return false;

        // Only check rapport threshold for goal cards that are still in RequestPile
        // Cards that have been moved to ActiveCards have already met their threshold
        if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
        {
            // If card is in ActiveCards, it's already playable (threshold was met)
            if (session.Deck?.Hand?.Cards?.Contains(card) == true)
            {
                return true;  // Card already in active hand, no need for rapport check
            }

            // If card is in RequestPile, check rapport threshold
            if (session.Deck?.RequestCards?.Contains(card) == true)
            {
                int rapportThreshold = card.Context?.RapportThreshold ?? 0;
                int currentRapport = session.RapportManager?.CurrentRapport ?? 0;
                return currentRapport >= rapportThreshold;
            }
        }

        return true;
    }

    /// <summary>
    /// Select appropriate goal card from JSON data based on conversation type
    /// </summary>
    private List<CardInstance> SelectGoalCardsForConversationType(NPC npc, ConversationType conversationType, string goalCardId, SessionCardDeck deck)
    {
        List<CardInstance> requestCards = new List<CardInstance>();
        
        // If specific card ID provided, this might be a request ID - find that request
        if (!string.IsNullOrEmpty(goalCardId) && npc.Requests != null)
        {
            // First check if it's a request ID
            var request = npc.GetRequestById(goalCardId);
            if (request != null && request.IsAvailable())
            {
                // Load ALL cards from the Request bundle
                
                // Add ALL request cards to be returned for active pile
                foreach (var requestCardId in request.RequestCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(requestCardId, out var requestCard))
                    {
                        Console.WriteLine($"[CardDeckManager] Warning: Request card ID '{requestCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }
                    
                    // Create a new template with BurdenGoal type based on the original
                    ConversationCard burdenGoalTemplate = new ConversationCard
                    {
                        Id = requestCard.Id,
                        Description = requestCard.Description,
                        Focus = requestCard.Focus,
                        Difficulty = requestCard.Difficulty,
                        TokenType = requestCard.TokenType,
                        Persistence = requestCard.Persistence,
                        SuccessType = requestCard.SuccessType,
                        FailureType = requestCard.FailureType,
                        ExhaustType = requestCard.ExhaustType,
                        DialogueFragment = requestCard.DialogueFragment,
                        VerbPhrase = requestCard.VerbPhrase,
                        PersonalityTypes = requestCard.PersonalityTypes,
                        LevelBonuses = requestCard.LevelBonuses,
                        MinimumTokensRequired = requestCard.MinimumTokensRequired,
                        RapportThreshold = requestCard.RapportThreshold,
                        QueuePosition = requestCard.QueuePosition,
                        InstantRapport = requestCard.InstantRapport,
                        RequestId = requestCard.RequestId,
                        IsSkeleton = requestCard.IsSkeleton,
                        SkeletonSource = requestCard.SkeletonSource,
                        RequiredTokenType = requestCard.RequiredTokenType,
                        CardType = CardType.BurdenGoal // Override to mark as BurdenGoal
                    };

                    // Create a new card instance with BurdenGoal template
                    CardInstance instance = new CardInstance(burdenGoalTemplate, npc.ID);
                    
                    // Store the rapport threshold and request ID in the card context
                    instance.Context = new CardContext
                    {
                        RapportThreshold = requestCard.RapportThreshold,
                        RequestId = request.Id
                    };
                    
                    // Request cards start as Unplayable until rapport threshold is met
                    instance.IsPlayable = false;
                    
                    requestCards.Add(instance);
                }
                
                // Add promise cards to the deck for shuffling (not returned)
                foreach (var promiseCardId in request.PromiseCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(promiseCardId, out var promiseCard))
                    {
                        Console.WriteLine($"[CardDeckManager] Warning: Promise card ID '{promiseCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }
                    
                    CardInstance promiseInstance = new CardInstance(promiseCard, npc.ID);
                    deck.AddCard(promiseInstance); // Add to deck for shuffling into draw pile
                }
                
                return requestCards; // Return all request cards for active pile
            }
        }
        
        // Fallback to existing logic if no specific card ID provided
        switch (conversationType)
        {
            // Promise is no longer a ConversationType - promise cards are part of Request bundles
                
            case ConversationType.FriendlyChat:
                // For FriendlyChat, select from NPC's connection token goal cards
                var goalCard = SelectConnectionTokenGoalCard(npc);
                return goalCard != null ? new List<CardInstance> { goalCard } : new List<CardInstance>();
                
            case ConversationType.Delivery:
                // For Delivery, the goal card is generated based on the letter being delivered
                // This is handled by the obligation system when the delivery conversation starts
                return new List<CardInstance>();
                
            case ConversationType.Resolution:
                // For Resolution, select from burden resolution cards
                var burdenCard = SelectBurdenResolutionCard(npc);
                return burdenCard != null ? new List<CardInstance> { burdenCard } : new List<CardInstance>();
                
            default:
                return new List<CardInstance>();
        }
    }

    /// <summary>
    /// Select a connection token goal card from NPC's goal deck
    /// </summary>
    private CardInstance SelectConnectionTokenGoalCard(NPC npc)
    {
        // Connection token goal cards should be in the NPC's one-time requests
        // These are cards that grant connection tokens when played at rapport threshold
        if (npc.Requests == null || !npc.Requests.Any())
            return null;

        // Look for cards with CardType Promise in available requests
        var availableRequests = npc.GetAvailableRequests();
        if (!availableRequests.Any())
            return null;
            
        // Get all promise cards from all available requests
        List<ConversationCard> goalCards = new List<ConversationCard>();
        foreach (var request in availableRequests)
        {
            // Retrieve promise cards from GameWorld using IDs
            var promiseCards = request.GetPromiseCards(_gameWorld);
            goalCards.AddRange(promiseCards.Where(card => card.CardType == CardType.Promise));
        }

        if (!goalCards.Any())
            return null;

        ConversationCard selectedGoal = goalCards[_random.Next(goalCards.Count)];
        CardInstance goalInstance = new CardInstance(selectedGoal, npc.ID);

        // Store the rapport threshold in the card context (same as Elena's letter)
        if (goalInstance.Context == null)
            goalInstance.Context = new CardContext();
        
        // Use the rapport threshold from the card itself (from JSON)
        goalInstance.Context.RapportThreshold = selectedGoal.RapportThreshold;
        
        // Goal cards start as Unplayable until rapport threshold is met
        goalInstance.IsPlayable = false;
        
        return goalInstance;
    }

    /// <summary>
    /// Select a burden resolution card from NPC's goal deck
    /// </summary>
    private CardInstance SelectBurdenResolutionCard(NPC npc)
    {
        // Burden resolution cards should be in the NPC's one-time requests
        if (npc.Requests == null || !npc.Requests.Any())
            return null;

        // Look for cards with CardType BurdenGoal in available requests
        var availableRequests = npc.GetAvailableRequests();
        if (!availableRequests.Any())
            return null;
            
        List<ConversationCard> resolutionCards = new List<ConversationCard>();
        foreach (var request in availableRequests)
        {
            // Retrieve request cards from GameWorld using IDs
            var requestCards = request.GetRequestCards(_gameWorld);
            resolutionCards.AddRange(requestCards.Where(card => card.CardType == CardType.BurdenGoal));
        }

        if (!resolutionCards.Any())
            return null;

        ConversationCard selectedResolution = resolutionCards[_random.Next(resolutionCards.Count)];
        CardInstance resolutionInstance = new CardInstance(selectedResolution, npc.ID);

        if (resolutionInstance.Context == null)
            resolutionInstance.Context = new CardContext();
        
        // Use the rapport threshold from the card itself (from JSON)
        resolutionInstance.Context.RapportThreshold = selectedResolution.RapportThreshold;
        
        // Goal cards start as Unplayable until rapport threshold is met
        resolutionInstance.IsPlayable = false;
        
        return resolutionInstance;
    }

    /// <summary>
    /// Select a valid request card for conversation type (legacy method for Promise conversations)
    /// </summary>
    private CardInstance SelectValidRequestCard(NPC npc, ConversationType conversationType)
    {
        if (npc.Requests == null || !npc.Requests.Any())
            return null;

        var availableRequests = npc.GetAvailableRequests();
        if (!availableRequests.Any())
            return null;
            
        List<ConversationCard> requestCards = new List<ConversationCard>();
        foreach (var request in availableRequests)
        {
            // Retrieve cards from GameWorld using IDs
            var reqCards = request.GetRequestCards(_gameWorld);
            var promCards = request.GetPromiseCards(_gameWorld);
            requestCards.AddRange(reqCards.Where(card => IsRequestCardValidForConversation(card, conversationType)));
            requestCards.AddRange(promCards.Where(card => IsRequestCardValidForConversation(card, conversationType)));
        }

        if (!requestCards.Any())
            return null;

        ConversationCard selectedRequest = requestCards[_random.Next(requestCards.Count)];
        CardInstance requestInstance = new CardInstance(selectedRequest, npc.ID);

        // Store the rapport threshold in the card context for goal cards
        if (selectedRequest.CardType == CardType.Letter || selectedRequest.CardType == CardType.Promise || selectedRequest.CardType == CardType.BurdenGoal)
        {
            if (requestInstance.Context == null)
                requestInstance.Context = new CardContext();

            // Store the rapport threshold from the card
            requestInstance.Context.RapportThreshold = selectedRequest.RapportThreshold;
            
            // Goal cards start as Unplayable until rapport threshold is met
            requestInstance.IsPlayable = false;
        }

        return requestInstance;
    }

    /// <summary>
    /// Check if request card is valid for conversation type
    /// </summary>
    private bool IsRequestCardValidForConversation(ConversationCard card, ConversationType type)
    {
        // Check if this is a promise/goal card (has GoalCard property)
        return card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal;
    }


    /// <summary>
    /// Convert legacy success chance to new Difficulty enum
    /// </summary>
    private Difficulty ConvertToDifficulty(int successChance)
    {
        return successChance switch
        {
            >= 70 => Difficulty.Easy,
            >= 60 => Difficulty.Medium,
            >= 50 => Difficulty.Hard,
            _ => Difficulty.VeryHard
        };
    }




    /// <summary>
    /// Restore cards to a session from saved IDs
    /// </summary>
    public void RestoreSessionCards(ConversationSession session, List<string> handCardIds, List<string> deckCardIds)
    {
        // Clear existing cards
        session.Deck.Hand.Clear();
        session.Deck.Clear();

        // Restore active cards
        foreach (string cardId in handCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC?.ID ?? "unknown");
                session.Deck.Hand.Add(cardInstance);
            }
        }

        // Restore deck cards
        foreach (string cardId in deckCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC?.ID ?? "unknown");
                session.Deck.AddCard(cardInstance);
            }
        }
    }

    /// <summary>
    /// Get NPC progression cards that are unlocked based on player's tokens
    /// </summary>
    private List<ConversationCard> GetUnlockedProgressionCards(NPC npc)
    {
        List<ConversationCard> unlockedCards = new List<ConversationCard>();

        // Get player's tokens with this NPC
        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);

        // Check each card in NPC's progression deck
        if (npc.ProgressionDeck != null)
        {
            foreach (ConversationCard card in npc.ProgressionDeck.GetAllCards())
            {
                // Check if player has enough tokens to unlock this card
                // Cards unlock at 1, 3, 6, 10, 15 token thresholds
                int requiredTokens = card.MinimumTokensRequired;
                ConnectionType tokenType = card.RequiredTokenType ?? card.TokenType;

                if (npcTokens.ContainsKey(tokenType) && npcTokens[tokenType] >= requiredTokens)
                {
                    unlockedCards.Add(card);
                }
            }
        }

        return unlockedCards;
    }

    /// <summary>
    /// Find a card template by ID from game world data
    /// </summary>
    private ConversationCard FindCardTemplateById(string cardId)
    {
        // Search in all card collections in GameWorld
        if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? template))
        {
            return template;
        }

        // If not found in main templates, create a basic one
        return new ConversationCard
        {
            Id = cardId,
            Description = "Placeholder card",
            Focus = 1,
            Difficulty = Difficulty.Medium,
            TokenType = ConnectionType.Trust,
            Persistence = PersistenceType.Thought,
            SuccessType = SuccessEffectType.Rapport,
            FailureType = FailureEffectType.None,
            ExhaustType = ExhaustEffectType.None
        };
    }
}