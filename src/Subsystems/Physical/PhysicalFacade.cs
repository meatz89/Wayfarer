public class PhysicalFacade
{
    private readonly GameWorld _gameWorld;
    private readonly PhysicalEffectResolver _effectResolver;
    private readonly PhysicalNarrativeService _narrativeService;
    private readonly PhysicalDeckBuilder _deckBuilder;
    private readonly TimeManager _timeManager;
    private readonly ObligationActivity _obligationActivity;
    private readonly SituationCompletionHandler _situationCompletionHandler;

    public PhysicalFacade(
        GameWorld gameWorld,
        PhysicalEffectResolver effectResolver,
        PhysicalNarrativeService narrativeService,
        PhysicalDeckBuilder deckBuilder,
        TimeManager timeManager,
        ObligationActivity obligationActivity,
        SituationCompletionHandler situationCompletionHandler)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _obligationActivity = obligationActivity ?? throw new ArgumentNullException(nameof(obligationActivity));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
    }

    public PhysicalSession GetCurrentSession()
    {
        return _gameWorld.CurrentPhysicalSession;
    }

    public bool IsSessionActive()
    {
        return _gameWorld.CurrentPhysicalSession != null;
    }

    public List<CardInstance> GetHand()
    {
        return _gameWorld.CurrentPhysicalSession.Deck.Hand.ToList();
    }

    public PhysicalDeckBuilder GetDeckBuilder()
    {
        return _deckBuilder;
    }

    public int GetDeckCount()
    {
        return _gameWorld.CurrentPhysicalSession.Deck.RemainingDeckCards;
    }

    public int GetExhaustCount()
    {
        return _gameWorld.CurrentPhysicalSession.Deck.LockedCards.Count;
    }

    public List<CardInstance> GetLockedCards()
    {
        return _gameWorld.CurrentPhysicalSession.Deck.LockedCards.ToList();
    }

    public PhysicalSession StartSession(PhysicalChallengeDeck engagement, List<CardInstance> deck, List<CardInstance> startingHand,
        Situation situation, Obligation obligation)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // ADR-007: PendingPhysicalContext already set upstream (GameFacade/SceneContent)
        // No ID storage needed - context contains object references

        Player player = _gameWorld.GetPlayer();

        _gameWorld.CurrentPhysicalSession = new PhysicalSession
        {
            ChallengeId = engagement.Id,
            CurrentExertion = 0,
            MaxExertion = 10,
            CurrentUnderstanding = 0,
            CurrentBreakthrough = 0,
            CurrentDanger = 0,
            MaxDanger = engagement.DangerThreshold,
            Aggression = 0
        };

        // Use PhysicalSessionDeck with Pile abstraction
        _gameWorld.CurrentPhysicalSession.Deck = PhysicalSessionDeck.CreateFromInstances(deck, startingHand);
        _gameWorld.CurrentPhysicalSession.Deck = _gameWorld.CurrentPhysicalSession.Deck;

        // ADR-007: Extract SituationCards from Situation object (no ID lookup)
        if (situation != null)
        {
            if (situation.SituationCards.Any())
            {
                foreach (SituationCard situationCard in situation.SituationCards)
                {
                    // Create CardInstance from SituationCard (constructor sets CardType automatically)
                    CardInstance situationCardInstance = new CardInstance(situationCard)
                    {
                        Context = new CardContext { threshold = situationCard.threshold }
                    };

                    // Add to session deck's requestPile
                    _gameWorld.CurrentPhysicalSession.Deck.AddSituationCard(situationCardInstance);
                }
            }
        }

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _gameWorld.CurrentPhysicalSession.Deck.DrawToHand(cardsToDrawStartingSized);
        }
        return _gameWorld.CurrentPhysicalSession;
    }

    /// <summary>
    /// ASSESS: Trigger all locked cards as combo, shuffle everything back, draw fresh
    /// CORE PHYSICAL MECHANIC: Locked cards execute together, then all cards cycle back through Situation deck
    /// PROJECTION PRINCIPLE: Apply full projection for each locked card (Breakthrough, Danger, Aggression with approach)
    /// </summary>
    public async Task<PhysicalTurnResult> ExecuteAssess()
    {
        if (!IsSessionActive())
        {
            throw new InvalidOperationException("No active physical session");
        }

        _timeManager.AdvanceSegments(1);
        Player player = _gameWorld.GetPlayer();

        // Get all locked cards for combo execution
        List<CardInstance> lockedCards = _gameWorld.CurrentPhysicalSession.Deck.LockedCards.ToList();

        // TRIGGER COMBO: Apply full projection for each locked card
        foreach (CardInstance card in lockedCards)
        {
            if (card.PhysicalCardTemplate != null)
            {
                // PROJECTION PRINCIPLE: Get projection for this card with Assess action
                PhysicalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _gameWorld.CurrentPhysicalSession, player, PhysicalActionType.Assess);

                // Apply FULL projection: Breakthrough, Danger, Aggression (includes action -2 + card approach)
                // NOTE: Exertion was already spent on EXECUTE, projection.ExertionChange should be 0 here
                _gameWorld.CurrentPhysicalSession.CurrentBreakthrough += projection.BreakthroughChange;
                _gameWorld.CurrentPhysicalSession.CurrentDanger += projection.DangerChange;
                _gameWorld.CurrentPhysicalSession.Aggression += projection.BalanceChange; // Includes action -2 AND card approach modifier

                // Stats are now simple integers - no XP system
                // XP granting deleted as part of XP system removal
            }
        }
        // Shuffle exhaust (locked cards) + hand back to deck, draw fresh
        _gameWorld.CurrentPhysicalSession.Deck.ShuffleExhaustAndHandBackToDeck();

        int cardsToDraw = _gameWorld.CurrentPhysicalSession.GetDrawCount();
        _gameWorld.CurrentPhysicalSession.Deck.DrawToHand(cardsToDraw);

        // Check and unlock situation cards if Breakthrough threshold met
        _gameWorld.CurrentPhysicalSession.Deck.CheckSituationThresholds(_gameWorld.CurrentPhysicalSession.CurrentBreakthrough);

        // Check if danger threshold reached
        bool sessionEnded = false;
        if (_gameWorld.CurrentPhysicalSession.ShouldEnd())
        {
            ApplyDangerConsequences(player);
            EndSession();
            sessionEnded = true;
        }

        return new PhysicalTurnResult
        {
            Success = true,
            Narrative = $"You assess the situation. {lockedCards.Count} prepared actions execute as a combo.",
            CurrentBreakthrough = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            CurrentDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            SessionEnded = sessionEnded
        };
    }

    /// <summary>
    /// EXECUTE: Lock card for combo, spend Exertion, increase Aggression
    /// Card effects (Breakthrough, Danger) DON'T apply yet - they trigger on ASSESS
    /// PROJECTION PRINCIPLE: Use projection to check affordability and apply Exertion + Aggression + costs
    /// </summary>
    public async Task<PhysicalTurnResult> ExecuteExecute(CardInstance card)
    {
        if (!IsSessionActive())
        {
            throw new InvalidOperationException("No active physical session");
        }

        if (!_gameWorld.CurrentPhysicalSession.Deck.Hand.Contains(card))
        {
            throw new InvalidOperationException("Card not in hand");
        }

        // Check situation card type BEFORE template check
        // Situation cards have no PhysicalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Situation)
        {
            // ADR-007: Complete situation through SituationCompletionHandler (applies rewards: coins, StoryCubes, equipment)
            // Use PendingPhysicalContext.Obligation.Situation (object reference), no ID lookup
            // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
            Situation completedSituation = _gameWorld.PendingPhysicalContext!.Obligation!.Situation;
            await _situationCompletionHandler.CompleteSituation(completedSituation);

            // SituationCards execute immediately (not locked for combo)
            // Move to PlayedCards for success detection (matches Mental pattern)
            _gameWorld.CurrentPhysicalSession.Deck.PlayCard(card);
            string narrative = _narrativeService.GenerateActionNarrative(card, _gameWorld.CurrentPhysicalSession);
            EndSession();

            return new PhysicalTurnResult
            {
                Success = true,
                Narrative = narrative,
                CurrentBreakthrough = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
                CurrentDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
                SessionEnded = true
            };
        }

        if (card.PhysicalCardTemplate == null)
        {
            throw new InvalidOperationException("Card has no Physical template");
        }

        _timeManager.AdvanceSegments(1);
        Player player = _gameWorld.GetPlayer();

        // PROJECTION PRINCIPLE: Get projection from resolver (single source of truth)
        PhysicalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _gameWorld.CurrentPhysicalSession, player, PhysicalActionType.Execute);

        // Check Exertion cost from projection (includes modifiers like fatigue penalties, Foundation generation)
        if (_gameWorld.CurrentPhysicalSession.CurrentExertion + projection.ExertionChange < 0)
        {
            int actualCost = -projection.ExertionChange;
            throw new InvalidOperationException($"Insufficient Exertion. Need {actualCost}, have {_gameWorld.CurrentPhysicalSession.CurrentExertion}");
        }

        // EXECUTE: Lock card for combo
        _gameWorld.CurrentPhysicalSession.Deck.LockCard(card);

        // Apply PARTIAL projection: Exertion + Aggression + strategic costs
        // DON'T apply Breakthrough/Danger yet (they trigger on ASSESS)
        _gameWorld.CurrentPhysicalSession.CurrentExertion += projection.ExertionChange;
        if (_gameWorld.CurrentPhysicalSession.CurrentExertion > _gameWorld.CurrentPhysicalSession.MaxExertion)
        {
            _gameWorld.CurrentPhysicalSession.CurrentExertion = _gameWorld.CurrentPhysicalSession.MaxExertion;
        }

        // Apply Aggression from projection (includes action +1 AND card approach modifier)
        _gameWorld.CurrentPhysicalSession.Aggression += projection.BalanceChange;

        // Apply strategic resource costs
        if (projection.HealthCost > 0) player.Health -= projection.HealthCost;
        if (projection.StaminaCost > 0) player.Stamina -= projection.StaminaCost;
        if (projection.CoinsCost > 0) player.Coins -= projection.CoinsCost;

        _gameWorld.CurrentPhysicalSession.ApproachHistory++;

        Console.WriteLine($"[PhysicalFacade] EXECUTE: Locked {card.PhysicalCardTemplate.Id}, Exertion {projection.ExertionChange:+#;-#;0}, Aggression {projection.BalanceChange:+#;-#;0} (now {_gameWorld.CurrentPhysicalSession.Aggression})");

        string executeNarrative = _narrativeService.GenerateActionNarrative(card, _gameWorld.CurrentPhysicalSession);

        return new PhysicalTurnResult
        {
            Success = true,
            Narrative = executeNarrative,
            CurrentBreakthrough = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            CurrentDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            SessionEnded = false
        };
    }

    /// <summary>
    /// Escape physical challenge - costs resources, potentially fails
    /// Physical challenges make retreat difficult
    /// TRANSITION TRACKING: Calls FailSituation to enable OnFailure transitions
    /// </summary>
    public PhysicalOutcome EscapeChallenge(Player player)
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Escape costs - health and stamina damage from retreat
        int healthCost = 5 + (_gameWorld.CurrentPhysicalSession.CurrentDanger / 2);
        int staminaCost = 10;

        player.Health -= healthCost;
        player.Stamina -= staminaCost;

        // TRANSITION TRACKING: Find situation and call FailSituation for OnFailure transitions
        // ADR-007: Use PendingPhysicalContext.Obligation.Situation (object reference), no ID lookup
        // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
        Situation situation = _gameWorld.PendingPhysicalContext!.Obligation!.Situation;
        _situationCompletionHandler.FailSituation(situation);

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = false, // Escape is failure
            FinalProgress = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            FinalDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            EscapeCost = $"{healthCost} Health, {staminaCost} Stamina"
        };

        // ADR-007: Clear session and context (CurrentPhysicalSituationId/ObligationId deleted)
        _gameWorld.CurrentPhysicalSession.Deck.Clear();
        _gameWorld.CurrentPhysicalSession = null;
        _gameWorld.PendingPhysicalContext = null;

        return outcome;
    }

    public async Task<PhysicalOutcome> EndSession()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // SYMMETRY RESTORATION: Success determined by SituationCard play (match Mental pattern)
        bool success = _gameWorld.CurrentPhysicalSession.Deck.PlayedCards.Any(c => c.CardType == CardTypes.Situation);

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = success,
            FinalProgress = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            FinalDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            EscapeCost = ""
        };

        // TRANSITION TRACKING: If challenge failed, call FailSituation for OnFailure transitions
        // Mirrors Social EndConversation pattern
        // HIGHLANDER: Use Situation object reference, not SituationId string
        // NO DEFENSIVE NULLS: Let it crash if context missing (reveals architectural problem)
        if (!success)
        {
            _situationCompletionHandler.FailSituation(_gameWorld.PendingPhysicalContext!.Obligation!.Situation);
        }

        // Check for obligation progress if this was an obligation situation
        if (success)
        {
            await CheckObligationProgress(_gameWorld.PendingPhysicalContext!.Obligation!.Situation, _gameWorld.PendingPhysicalContext!.Obligation);
        }

        // Award Reputation on success (Reputation system)
        // Physical success builds reputation affecting future Social and Physical engagements
        if (success)
        {
            Player player = _gameWorld.GetPlayer();
            int reputationGain = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough >= 20 ? 1 : 0;
            player.Reputation += reputationGain;// PROGRESSION SYSTEM: Award mastery cube for this challenge type
            if (!string.IsNullOrEmpty(_gameWorld.CurrentPhysicalSession.ChallengeId))
            {
                player.MasteryCubes.AddMastery(_gameWorld.CurrentPhysicalSession.ChallengeId, 1);
                int masteryLevel = player.MasteryCubes.GetMastery(_gameWorld.CurrentPhysicalSession.ChallengeId);
            }
        }

        // TACTICAL LAYER: Do NOT apply CompletionReward here
        // Rewards are strategic layer concern - GameFacade applies them after receiving outcome
        // PendingContext stays alive for GameFacade to process

        // Clear obligation context

        // ADR-007: Clear session and context (CurrentPhysicalSituationId/ObligationId deleted)
        _gameWorld.CurrentPhysicalSession.Deck.Clear();
        _gameWorld.CurrentPhysicalSession = null;
        _gameWorld.PendingPhysicalContext = null;

        return outcome;
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: ONLY place where projections become reality
    /// Parallel to MentalFacade.ApplyProjectionToSession() and ConversationFacade.ApplyProjectionToSession()
    /// </summary>
    private void ApplyProjectionToSession(PhysicalCardEffectResult projection, PhysicalSession session, Player player)
    {
        // Builder resource: Exertion (can be negative for cost, positive for generation)
        session.CurrentExertion += projection.ExertionChange;
        if (session.CurrentExertion > session.MaxExertion)
        {
            session.CurrentExertion = session.MaxExertion;
        }
        if (session.CurrentExertion < 0)
        {
            session.CurrentExertion = 0;
        }

        // Victory resource: Breakthrough
        if (projection.BreakthroughChange != 0)
        {
            session.CurrentBreakthrough += projection.BreakthroughChange;
        }

        // Consequence resource: Danger
        if (projection.DangerChange != 0)
        {
            session.CurrentDanger += projection.DangerChange;
        }

        // Balance tracker: Aggression (includes action balance + card approach)
        if (projection.BalanceChange != 0)
        {
            session.Aggression += projection.BalanceChange;
        }

        // Persistent progress: Understanding
        if (projection.ReadinessChange != 0)
        {
            session.CurrentUnderstanding += projection.ReadinessChange;
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
    /// Apply consequences when Danger threshold reached
    /// Health/stamina damage, injury cards, forced defeat
    /// </summary>
    private void ApplyDangerConsequences(Player player)
    {
        // Health damage from physical consequences (6-point scale)
        // Base: 1 point + excess danger (capped at 2 for total max of 3 damage)
        int excessDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger - _gameWorld.CurrentPhysicalSession.MaxDanger;
        int healthDamage = 1 + Math.Min(2, excessDanger);
        player.Health = Math.Max(0, player.Health - healthDamage);

        // Stamina damage from exhaustion (6-point scale)
        // 2 points = 33% of max stamina
        int staminaDamage = 2;
        player.Stamina = Math.Max(0, player.Stamina - staminaDamage);

        // Only add injury card if health drops below 2 (critical state)
        if (player.Health < 2)
        {
            player.InjuryCardIds.Add("injury_physical_moderate");
        }
    }

    /// <summary>
    /// Check for obligation progress when Physical situation completes
    /// </summary>
    private async Task CheckObligationProgress(Situation situation, Obligation obligation)
    {
        if (situation == null || obligation == null)
            return;

        // Check if this is an intro action (Discovered → Active transition)
        if (situation.SituationTemplate?.Id == "notice_waterwheel")
        {
            // This is intro completion - activate obligation
            // CompleteIntroAction spawns situations directly to ActiveSituations
            await _obligationActivity.CompleteIntroAction(obligation);
            return;
        }

        // Regular situation completion
        ObligationProgressResult progressResult = await _obligationActivity.CompleteSituation(situation, obligation);

        // Log progress for UI modal display (UI will handle modal)

        // Check if obligation is now complete
        ObligationCompleteResult completeResult = _obligationActivity.CheckObligationCompletion(obligation);
        if (completeResult != null)
        {
            // Obligation complete - UI will display completion modal
        }
    }
}
