using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MentalFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MentalEffectResolver _effectResolver;
    private readonly MentalNarrativeService _narrativeService;
    private readonly MentalDeckBuilder _deckBuilder;
    private readonly TimeManager _timeManager;
    private readonly GoalCompletionHandler _goalCompletionHandler;

    public MentalFacade(
        GameWorld gameWorld,
        MentalEffectResolver effectResolver,
        MentalNarrativeService narrativeService,
        MentalDeckBuilder deckBuilder,
        TimeManager timeManager,
        GoalCompletionHandler goalCompletionHandler)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _goalCompletionHandler = goalCompletionHandler ?? throw new ArgumentNullException(nameof(goalCompletionHandler));
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
        string goalId, string obligationId, int effectiveExposure)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // Get goal for costs and goal cards
        Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
        if (goal == null)
        {
            return null;
        }

        // Track obligation context
        _gameWorld.CurrentMentalGoalId = goalId;
        _gameWorld.CurrentMentalObligationId = obligationId;

        Player player = _gameWorld.GetPlayer();
        Location location = player.CurrentLocation;

        // PROGRESSION SYSTEM: Focus cost from Goal (single source of truth from JSON)
        int focusCost = goal.Costs.Focus;
        if (player.Focus < focusCost)
        {
            return null;
        }
        player.Focus -= focusCost;
        // effectiveExposure passed from GameFacade (already reduced by XXXOBLIGATIONCUBESXXX)

        _gameWorld.CurrentMentalSession = new MentalSession
        {
            ObligationId = engagement.Id,
            VenueId = location.VenueId,
            CurrentAttention = 10,
            MaxAttention = 10,
            CurrentUnderstanding = 0,
            CurrentProgress = 0,
            CurrentExposure = effectiveExposure, // GameFacade orchestrated cube reduction
            MaxExposure = engagement.DangerThreshold
            // VictoryThreshold removed - GoalCard play determines success, not Progress threshold
        };

        // Use MentalSessionDeck with Pile abstraction
        _gameWorld.CurrentMentalSession.Deck = MentalSessionDeck.CreateFromInstances(deck, startingHand);
        _gameWorld.CurrentMentalSession.Deck = _gameWorld.CurrentMentalSession.Deck;

        // SYMMETRY RESTORATION: Extract GoalCards from Goal and add to session deck (MATCH SOCIAL PATTERN)
        if (goal.GoalCards.Any())
        {
            foreach (GoalCard goalCard in goal.GoalCards)
            {
                // Create CardInstance from GoalCard (constructor sets CardType automatically)
                CardInstance goalCardInstance = new CardInstance(goalCard)
                {
                    Context = new CardContext { threshold = goalCard.threshold },
                    IsPlayable = false // Unlocked when Progress reaches threshold
                };

                // Add to session deck's requestPile
                _gameWorld.CurrentMentalSession.Deck.AddGoalCard(goalCardInstance);
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

        // Check and unlock goal cards if Progress threshold met
        _gameWorld.CurrentMentalSession.Deck.CheckGoalThresholds(_gameWorld.CurrentMentalSession.CurrentProgress);

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

        // SYMMETRY RESTORATION: Check goal card type BEFORE template check
        // Goal cards have no MentalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Goal)
        {// Apply obstacle effects via containment pattern (THREE PARALLEL SYSTEMS symmetry)
            // DISTRIBUTED INTERACTION: Find parent obstacle from Goal.ParentObstacle
            Player currentPlayer = _gameWorld.GetPlayer();
            Location location = currentPlayer.CurrentLocation;

            // Get goal and its parent obstacle via object references
            Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == _gameWorld.CurrentMentalGoalId);
            if (goal == null)
                return null; // Goal not found

            Obstacle parentObstacle = goal.ParentObstacle;

            if (parentObstacle != null)
            {// PHASE 2: Five Consequence Types
                switch (goal.ConsequenceType)
                {
                    case Wayfarer.GameState.Enums.ConsequenceType.Resolution:
                        // Permanently overcome - mark as Resolved, remove from play
                        parentObstacle.State = Wayfarer.GameState.Enums.ObstacleState.Resolved;
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;

                        if (!parentObstacle.IsPermanent)
                        {
                            location.ObstacleIds.Remove(parentObstacle.Id);
                        }
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Bypass:
                        // Player passes, obstacle persists
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome; break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Transform:
                        // Fundamentally changed - intensity to 0, new description
                        parentObstacle.State = Wayfarer.GameState.Enums.ObstacleState.Transformed;
                        parentObstacle.Intensity = 0;

                        if (!string.IsNullOrEmpty(goal.TransformDescription))
                        {
                            parentObstacle.TransformedDescription = goal.TransformDescription;
                        }

                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome; break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Modify:
                        // Intensity reduced - other goals may unlock
                        parentObstacle.Intensity = Math.Max(0,
                            parentObstacle.Intensity - goal.PropertyReduction.ReduceIntensity);

                        parentObstacle.ResolutionMethod = Wayfarer.GameState.Enums.ResolutionMethod.Preparation;

                        // Check if intensity now at 0 (auto-transform)
                        if (parentObstacle.Intensity == 0)
                        {
                            parentObstacle.State = Wayfarer.GameState.Enums.ObstacleState.Transformed;
                        }
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Grant:
                        // Grant items/knowledge, no obstacle change
                        // (Knowledge cards handled in Phase 3, items already handled by reward system)
                        break;
                }
            }
            // Else: ambient goal with no obstacle parent

            // Complete goal through GoalCompletionHandler (handles obligation progress)
            Goal completedGoal = _gameWorld.Goals.FirstOrDefault(g => g.Id == _gameWorld.CurrentMentalGoalId);
            if (completedGoal != null)
            {
                _goalCompletionHandler.CompleteGoal(completedGoal);
            }

            _gameWorld.CurrentMentalSession.Deck.PlayCard(card); // Mark card as played
            EndSession(); // Immediate end on GoalCard play

            return new MentalTurnResult
            {
                Success = true,
                Narrative = $"You completed the obligation: {card.GoalCardTemplate.Name}",
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

        // Check and unlock goal cards if Progress threshold met
        List<CardInstance> unlockedGoals = _gameWorld.CurrentMentalSession.Deck.CheckGoalThresholds(_gameWorld.CurrentMentalSession.CurrentProgress);
        foreach (CardInstance goalCard in unlockedGoals)
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

    public MentalOutcome EndSession()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // SYMMETRY RESTORATION: Success determined by GoalCard play (match Social pattern)
        bool success = _gameWorld.CurrentMentalSession.Deck.PlayedCards.Any(c => c.CardType == CardTypes.Goal);

        MentalOutcome outcome = new MentalOutcome
        {
            Success = success,
            FinalProgress = _gameWorld.CurrentMentalSession.CurrentProgress,
            FinalExposure = _gameWorld.CurrentMentalSession.CurrentExposure,
            SessionSaved = false
        };

        // Obligation progress now handled by GoalCompletionHandler (system-agnostic)

        Player player = _gameWorld.GetPlayer();

        // PROGRESSION SYSTEM: Award Venue familiarity on success
        if (success && !string.IsNullOrEmpty(_gameWorld.CurrentMentalSession.VenueId))
        {
            int currentFamiliarity = player.LocationFamiliarity.GetFamiliarity(_gameWorld.CurrentMentalSession.VenueId);
            int newFamiliarity = Math.Min(3, currentFamiliarity + 1); // Max familiarity is 3
            player.LocationFamiliarity.SetFamiliarity(_gameWorld.CurrentMentalSession.VenueId, newFamiliarity);
        }

        // Persist exposure to Location (Mental debt system)
        // Exposure accumulates - next Mental engagement at this location starts with elevated baseline
        Location currentSpot = player.CurrentLocation;
        currentSpot.Exposure = _gameWorld.CurrentMentalSession.CurrentExposure;

        // Clear obligation context
        _gameWorld.CurrentMentalGoalId = null;
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
