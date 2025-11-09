public class MentalFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MentalEffectResolver _effectResolver;
    private readonly MentalNarrativeService _narrativeService;
    private readonly MentalDeckBuilder _deckBuilder;
    private readonly TimeManager _timeManager;
    private readonly SituationCompletionHandler _situationCompletionHandler;

    public MentalFacade(
        GameWorld gameWorld,
        MentalEffectResolver effectResolver,
        MentalNarrativeService narrativeService,
        MentalDeckBuilder deckBuilder,
        TimeManager timeManager,
        SituationCompletionHandler situationCompletionHandler)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
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

    public MentalSession StartSession(MentalChallengeDeck engagement, List<CardInstance> deck, List<CardInstance> startingHand,
        string situationId, string obligationId)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // Get situation for costs and situation cards
        Situation situation = _gameWorld.Scenes
            .SelectMany(s => s.Situations)
            .FirstOrDefault(sit => sit.Id == situationId);
        if (situation == null)
        {
            return null;
        }

        // Track obligation context
        _gameWorld.CurrentMentalSituationId = situationId;
        _gameWorld.CurrentMentalObligationId = obligationId;

        Player player = _gameWorld.GetPlayer();
        Location location = _gameWorld.GetPlayerCurrentLocation();

        // PROGRESSION SYSTEM: Focus cost from Situation (single source of truth from JSON)
        int focusCost = situation.Costs.Focus;
        if (player.Focus < focusCost)
        {
            return null;
        }
        player.Focus -= focusCost;

        _gameWorld.CurrentMentalSession = new MentalSession
        {
            ObligationId = engagement.Id,
            LocationId = location.Id,
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

            // Complete situation through SituationCompletionHandler (handles obligation progress)
            Situation completedSituation = _gameWorld.Scenes
                .SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.CurrentMentalSituationId);
            if (completedSituation != null)
            {
                _situationCompletionHandler.CompleteSituation(completedSituation);
            }

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
        ApplyProjectionToSession(projection, _gameWorld.CurrentMentalSession, player);

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
        _gameWorld.CurrentMentalSession.CurrentLeads += leadsGenerated;// PROGRESSION SYSTEM: Award XP to bound stat (XP pre-calculated at parse time)
        if (card.MentalCardTemplate.BoundStat != PlayerStatType.None)
        {
            player.Stats.AddXP(card.MentalCardTemplate.BoundStat, card.MentalCardTemplate.XPReward);
        }

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
                ApplyExposureConsequences(player);
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
    private void ApplyProjectionToSession(MentalCardEffectResult projection, MentalSession session, Player player)
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

        // Strategic resource costs
        if (projection.HealthCost > 0)
        {
            player.Health -= projection.HealthCost;
        }
        if (projection.StaminaCost > 0)
        {
            player.Stamina -= projection.StaminaCost;
        }
        if (projection.CoinsCost > 0)
        {
            player.Coins -= projection.CoinsCost;
        }
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
    public MentalOutcome AbandonChallenge()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Apply Exposure consequences if threshold reached
        Player player = _gameWorld.GetPlayer();
        if (_gameWorld.CurrentMentalSession.CurrentExposure >= 10)
        {
            ApplyExposureConsequences(player);
        }

        // TRANSITION TRACKING: Find situation and call FailSituation for OnFailure transitions
        if (!string.IsNullOrEmpty(_gameWorld.CurrentMentalSituationId))
        {
            Situation situation = _gameWorld.Scenes
                .SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.CurrentMentalSituationId);

            if (situation != null)
            {
                // Call FailSituation to set LastChallengeSucceeded = false and trigger OnFailure
                _situationCompletionHandler.FailSituation(situation);
            }
        }

        MentalOutcome outcome = new MentalOutcome
        {
            Success = false,
            FinalProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            FinalExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionSaved = false
        };

        // Clear obligation context
        _gameWorld.CurrentMentalSituationId = null;
        _gameWorld.CurrentMentalObligationId = null;

        _gameWorld.CurrentMentalSession.Deck.Clear();
        _gameWorld.CurrentMentalSession = null;

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
        // Mirrors Social EndConversation pattern (lines 157-169)
        if (!success && _gameWorld.PendingMentalContext?.SituationId != null)
        {
            Situation situation = _gameWorld.Scenes
                .SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.PendingMentalContext.SituationId);

            if (situation != null)
            {
                _situationCompletionHandler.FailSituation(situation);
            }
        }

        // Obligation progress now handled by SituationCompletionHandler (system-agnostic)

        Player player = _gameWorld.GetPlayer();

        // PROGRESSION SYSTEM: Award Location familiarity on success
        if (success && !string.IsNullOrEmpty(_gameWorld.CurrentMentalSession.LocationId))
        {
            int currentFamiliarity = player.GetLocationFamiliarity(_gameWorld.CurrentMentalSession.LocationId);
            int newFamiliarity = Math.Min(3, currentFamiliarity + 1); // Max familiarity is 3
            player.SetLocationFamiliarity(_gameWorld.CurrentMentalSession.LocationId, newFamiliarity);
        }

        // TACTICAL LAYER: Do NOT apply CompletionReward here
        // Rewards are strategic layer concern - GameFacade applies them after receiving outcome
        // PendingContext stays alive for GameFacade to process

        // Clear obligation context
        _gameWorld.CurrentMentalSituationId = null;
        _gameWorld.CurrentMentalObligationId = null;

        _gameWorld.CurrentMentalSession.Deck.Clear();
        _gameWorld.CurrentMentalSession = null;

        return outcome;
    }

    /// <summary>
    /// Apply consequences when Exposure threshold reached
    /// Structure becomes dangerous, obligation discovered
    /// </summary>
    private void ApplyExposureConsequences(Player player)
    {
        // Health damage from dangerous structure or being caught
        int healthDamage = 5 + (_gameWorld.CurrentMentalSession.CurrentExposure - 10);
        player.Health -= healthDamage;
    }

}
