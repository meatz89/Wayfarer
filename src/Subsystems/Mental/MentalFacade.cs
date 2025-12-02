public class MentalFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MentalEffectResolver _effectResolver;
    private readonly MentalNarrativeService _narrativeService;
    private readonly MentalDeckBuilder _deckBuilder;
    private readonly TimeManager _timeManager;
    private readonly SituationCompletionHandler _situationCompletionHandler;
    private readonly RewardApplicationService _rewardApplicationService;

    public MentalFacade(
        GameWorld gameWorld,
        MentalEffectResolver effectResolver,
        MentalNarrativeService narrativeService,
        MentalDeckBuilder deckBuilder,
        TimeManager timeManager,
        SituationCompletionHandler situationCompletionHandler,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    public MentalSession GetCurrentSession()
    {
        return _gameWorld.CurrentMentalSession;
    }

    public bool IsSessionActive()
    {
        return _gameWorld.CurrentMentalSession != null;
    }

    public List<CardInstance> GetHand()
    {
        return _gameWorld.CurrentMentalSession.Deck.Hand.ToList();
    }

    public MentalDeckBuilder GetDeckBuilder()
    {
        return _deckBuilder;
    }

    public int GetDeckCount()
    {
        return _gameWorld.CurrentMentalSession.Deck.RemainingDeckCards;
    }

    public int GetDiscardCount()
    {
        return _gameWorld.CurrentMentalSession.Deck.PlayedCards.Count;
    }

    public async Task<MentalSession> StartSession(MentalChallengeDeck engagement, List<CardInstance> deck, List<CardInstance> startingHand,
        string situationId, string obligationId)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // Get situation for costs and situation cards
        // HIGHLANDER: Query by TemplateId (natural key), not deleted .Id property
        // situationId parameter is actually the TemplateId
        Situation situation = _gameWorld.Scenes
            .SelectMany(s => s.Situations)
            .FirstOrDefault(sit => sit.TemplateId == situationId);
        if (situation == null)
        {
            return null;
        }

        // Track obligation context
        // HIGHLANDER: Find Obligation object by Id (template property), assign object to session
        Obligation obligation = _gameWorld.GetPlayer().ActiveObligations
            .FirstOrDefault(ob => ob.Id == obligationId);

        Player player = _gameWorld.GetPlayer();
        Location location = _gameWorld.GetPlayerCurrentLocation();

        // HIGHLANDER: Extract focus cost from EntryCost (negative values = costs)
        int focusCost = situation.EntryCost.Focus < 0 ? -situation.EntryCost.Focus : 0;
        CompoundRequirement focusRequirement = new CompoundRequirement();
        OrPath focusPath = new OrPath { FocusRequired = focusCost };
        focusRequirement.OrPaths.Add(focusPath);
        if (!focusRequirement.IsAnySatisfied(player, _gameWorld))
        {
            return null;
        }

        // HIGHLANDER: Apply focus cost via EntryCost directly (already negative)
        if (focusCost > 0)
        {
            Consequence focusCostConsequence = new Consequence { Focus = situation.EntryCost.Focus };
            await _rewardApplicationService.ApplyConsequence(focusCostConsequence, situation);
        }

        _gameWorld.CurrentMentalSession = new MentalSession
        {
            Obligation = obligation, // HIGHLANDER: Store Obligation object, not string Id
            Location = location,
            CurrentAttention = 10,
            MaxAttention = 10,
            CurrentUnderstanding = 0,
            CurrentProgress = 0,
            CurrentExposure = 0, // Starts at 0, accumulates during play
            MaxExposure = engagement.DangerThreshold // Max from deck (varies by difficulty)
                                                     // VictoryThreshold removed - SituationCard play determines success, not Progress threshold
        };

        // Use MentalSessionDeck with Pile abstraction
        _gameWorld.CurrentMentalSession.Deck = MentalSessionDeck.CreateFromInstances(deck, startingHand);
        _gameWorld.CurrentMentalSession.Deck = _gameWorld.CurrentMentalSession.Deck;

        // SYMMETRY RESTORATION: Extract SituationCards from Situation and add to session deck (MATCH SOCIAL PATTERN)
        if (situation.SituationCards.Any())
        {
            foreach (SituationCard situationCard in situation.SituationCards)
            {
                // Create CardInstance from SituationCard (constructor sets CardType automatically)
                CardInstance situationCardInstance = new CardInstance(situationCard)
                {
                    Context = new CardContext { threshold = situationCard.threshold },
                    IsPlayable = false // Unlocked when Progress reaches threshold
                };

                // Add to session deck's requestPile
                _gameWorld.CurrentMentalSession.Deck.AddSituationCard(situationCardInstance);
            }
        }

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _gameWorld.CurrentMentalSession.Deck.DrawToHand(cardsToDrawStartingSized);
        }
        return _gameWorld.CurrentMentalSession;
    }

    public async Task<MentalTurnResult> ExecuteObserve()
    {
        if (!IsSessionActive())
        {
            throw new InvalidOperationException("No active mental session");
        }

        // Advance time by 1 segment per action (per documentation)
        _timeManager.AdvanceSegments(1);

        // Calculate cards to draw (based on CurrentLeads generated by ACT)
        int cardsToDraw = _gameWorld.CurrentMentalSession.GetDrawCount();

        // Draw cards to hand
        _gameWorld.CurrentMentalSession.Deck.DrawToHand(cardsToDraw);// Consume Leads after drawing
        _gameWorld.CurrentMentalSession.CurrentLeads = 0;

        // Check and unlock situation cards if Progress threshold met
        _gameWorld.CurrentMentalSession.Deck.CheckSituationThresholds(_gameWorld.CurrentMentalSession.CurrentProgress);

        return new MentalTurnResult
        {
            Success = true,
            Narrative = "You observe carefully, gathering information.",
            CurrentProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            CurrentExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionEnded = false
        };
    }

    public async Task<MentalTurnResult> ExecuteAct(CardInstance card)
    {
        // Advance time by 1 segment per action (per documentation)
        _timeManager.AdvanceSegments(1);

        return await ExecuteCard(card, MentalActionType.Act);
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: Execute card using projection pattern
    /// 1. Get projection from resolver (single source of truth)
    /// 2. Apply projection to session
    /// DUAL BALANCE: Action type (Observe/Act) combined with card Method
    /// Parallel to ConversationFacade.SelectCard()
    /// </summary>
    private async Task<MentalTurnResult> ExecuteCard(CardInstance card, MentalActionType actionType)
    {
        if (!IsSessionActive())
        {
            throw new InvalidOperationException("No active mental session");
        }

        if (!_gameWorld.CurrentMentalSession.Deck.Hand.Contains(card))
        {
            throw new InvalidOperationException("Card not in hand");
        }

        // SYMMETRY RESTORATION: Check situation card type BEFORE template check
        // Situation cards have no MentalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Situation)
        {

            // ADR-007: Complete situation through SituationCompletionHandler (handles obligation progress)
            // Use PendingMentalContext.Situation (object reference), no ID lookup needed
            // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
            Situation completedSituation = _gameWorld.PendingMentalContext!.Situation;
            await _situationCompletionHandler.CompleteSituation(completedSituation);

            _gameWorld.CurrentMentalSession.Deck.PlayCard(card); // Mark card as played
            EndSession(); // Immediate end on SituationCard play

            return new MentalTurnResult
            {
                Success = true,
                Narrative = $"You completed the obligation: {card.SituationCardTemplate.Name}",
                CurrentProgress = 0,
                CurrentExposure = 0,
                SessionEnded = true
            };
        }

        if (card.MentalCardTemplate == null)
        {
            throw new InvalidOperationException("Card has no Mental template");
        }

        Player player = _gameWorld.GetPlayer();

        // PROJECTION PRINCIPLE: Get projection from resolver (single source of truth)
        // DUAL BALANCE: Pass action type to combine with card Method
        MentalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _gameWorld.CurrentMentalSession, player, actionType);

        // Check Attention cost from projection (includes modifiers if any exist)
        if (_gameWorld.CurrentMentalSession.CurrentAttention + projection.AttentionChange < 0)
        {
            int actualCost = -projection.AttentionChange;
            throw new InvalidOperationException($"Insufficient Attention. Need {actualCost}, have {_gameWorld.CurrentMentalSession.CurrentAttention}");
        }

        // Apply projection to session state
        await ApplyProjectionToSession(projection, _gameWorld.CurrentMentalSession, player);

        // Check and unlock situation cards if Progress threshold met
        List<CardInstance> unlockedSituations = _gameWorld.CurrentMentalSession.Deck.CheckSituationThresholds(_gameWorld.CurrentMentalSession.CurrentProgress);
        foreach (CardInstance situationCard in unlockedSituations)
        { }

        // Track categories for obligation depth
        MentalCategory category = card.MentalCardTemplate.Category;
        _gameWorld.CurrentMentalSession.CategoryCounts[category] =
            _gameWorld.CurrentMentalSession.CategoryCounts.GetValueOrDefault(category, 0) + 1;

        // ACT: Generate Leads based on card depth
        // Leads determine how many cards OBSERVE will draw
        int leadsGenerated = card.MentalCardTemplate.Depth switch
        {
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            _ => 0
        };
        _gameWorld.CurrentMentalSession.CurrentLeads += leadsGenerated;
        // Stats are now simple integers - no XP system
        // XP granting deleted as part of XP system removal

        _gameWorld.CurrentMentalSession.Deck.PlayCard(card);
        // NO DRAWING - Methods move to Applied pile, only OBSERVE draws cards

        string narrative = _narrativeService.GenerateActionNarrative(card, _gameWorld.CurrentMentalSession);

        // Check victory/consequence thresholds
        bool sessionEnded = false;
        if (_gameWorld.CurrentMentalSession.ShouldEnd())
        {
            // Apply consequences if Exposure threshold reached
            if (_gameWorld.CurrentMentalSession.CurrentExposure >= 10)
            {
                await ApplyExposureConsequences(player);
            }

            EndSession();
            sessionEnded = true;
        }

        return new MentalTurnResult
        {
            Success = true,
            Narrative = narrative,
            CurrentProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            CurrentExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionEnded = sessionEnded
        };
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: ONLY place where projections become reality
    /// Parallel to ConversationFacade.ApplyProjectionToSession()
    /// </summary>
    private async Task ApplyProjectionToSession(MentalCardEffectResult projection, MentalSession session, Player player)
    {
        // Builder resource: Attention
        session.CurrentAttention += projection.AttentionChange;
        if (session.CurrentAttention > session.MaxAttention)
        {
            session.CurrentAttention = session.MaxAttention;
        }
        if (session.CurrentAttention < 0)
        {
            session.CurrentAttention = 0;
        }

        // Victory resource: Progress
        if (projection.ProgressChange != 0)
        {
            session.CurrentProgress += projection.ProgressChange;
        }

        // Consequence resource: Exposure
        if (projection.ExposureChange != 0)
        {
            session.CurrentExposure += projection.ExposureChange;
        }

        // Persistent progress: Understanding
        if (projection.UnderstandingChange != 0)
        {
            session.CurrentUnderstanding += projection.UnderstandingChange;
        }

        // Strategic resource costs (HIGHLANDER: single source of truth via Consequence class)
        // TWO PILLARS: All cost/reward application uses Consequence class
        Consequence tacticalCosts = new Consequence
        {
            Coins = -projection.CoinsCost,
            Health = -projection.HealthCost,
            Stamina = -projection.StaminaCost
        };
        await _rewardApplicationService.ApplyConsequence(tacticalCosts, _gameWorld.PendingMentalContext?.Situation);
    }

    /// <summary>
    /// Leave obligation - saves state for later return
    /// Mental challenges allow leaving and resuming
    /// </summary>
    public MentalOutcome LeaveObligation()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Save session state - NOT clearing session, hand, deck
        // Player can return later and continue
        MentalOutcome outcome = new MentalOutcome
        {
            Success = false, // Didn't complete
            FinalProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            FinalExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionSaved = true
        };

        return outcome;
    }

    /// <summary>
    /// Abandon mental challenge - marks as failed and triggers OnFailure transitions
    /// Unlike LeaveObligation, this is permanent failure
    /// TRANSITION TRACKING: Calls FailSituation to enable OnFailure transitions
    /// </summary>
    public async Task<MentalOutcome> AbandonChallenge()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Apply Exposure consequences if threshold reached
        Player player = _gameWorld.GetPlayer();
        if (_gameWorld.CurrentMentalSession.CurrentExposure >= 10)
        {
            await ApplyExposureConsequences(player);
        }

        // TRANSITION TRACKING: Find situation and call FailSituation for OnFailure transitions
        // ADR-007: Use PendingMentalContext.Situation (object reference), no ID lookup
        // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
        {
            Situation situation = _gameWorld.PendingMentalContext!.Situation;
            _situationCompletionHandler.FailSituation(situation);
        }

        MentalOutcome outcome = new MentalOutcome
        {
            Success = false,
            FinalProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            FinalExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionSaved = false
        };

        // ADR-007: Clear session and context (CurrentMentalSituationId/ObligationId deleted)
        _gameWorld.CurrentMentalSession.Deck.Clear();
        _gameWorld.CurrentMentalSession = null;
        _gameWorld.PendingMentalContext = null;

        return outcome;
    }

    public MentalOutcome EndSession()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // SYMMETRY RESTORATION: Success determined by SituationCard play (match Social pattern)
        bool success = _gameWorld.CurrentMentalSession.Deck.PlayedCards.Any(c => c.CardType == CardTypes.Situation);

        MentalOutcome outcome = new MentalOutcome
        {
            Success = success,
            FinalProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            FinalExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionSaved = false
        };

        // TRANSITION TRACKING: If challenge failed, call FailSituation for OnFailure transitions
        // Mirrors Social EndConversation pattern
        // HIGHLANDER: Use Situation object reference, not SituationId string
        // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
        if (!success)
        {
            _situationCompletionHandler.FailSituation(_gameWorld.PendingMentalContext!.Situation);
        }

        // Obligation progress now handled by SituationCompletionHandler (system-agnostic)

        Player player = _gameWorld.GetPlayer();

        // PROGRESSION SYSTEM: Award Location familiarity on success
        // HIGHLANDER: Pass Location object to familiarity methods (not Location.Id)
        if (success && _gameWorld.CurrentMentalSession.Location != null)
        {
            int currentFamiliarity = player.GetLocationFamiliarity(_gameWorld.CurrentMentalSession.Location);
            int newFamiliarity = Math.Min(3, currentFamiliarity + 1); // Max familiarity is 3
            player.SetLocationFamiliarity(_gameWorld.CurrentMentalSession.Location, newFamiliarity);
        }

        // TACTICAL LAYER: Do NOT apply CompletionReward here
        // Rewards are strategic layer concern - GameOrchestrator applies them after receiving outcome
        // PendingContext stays alive for GameOrchestrator to process

        // ADR-007: Clear session and context (CurrentMentalSituationId/ObligationId deleted)
        _gameWorld.CurrentMentalSession.Deck.Clear();
        _gameWorld.CurrentMentalSession = null;
        _gameWorld.PendingMentalContext = null;

        return outcome;
    }

    /// <summary>
    /// Apply consequences when Exposure threshold reached
    /// Structure becomes dangerous, obligation discovered
    /// TWO PILLARS: Uses Consequence + ApplyConsequence for all mutations
    /// </summary>
    private async Task ApplyExposureConsequences(Player player)
    {
        // Health damage from dangerous structure or being caught
        int healthDamage = 5 + (_gameWorld.CurrentMentalSession.CurrentExposure - 10);
        Consequence damageConsequence = new Consequence { Health = -healthDamage };
        await _rewardApplicationService.ApplyConsequence(damageConsequence, _gameWorld.PendingMentalContext?.Situation);
    }

}
