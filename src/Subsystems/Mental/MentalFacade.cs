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
    private readonly InvestigationActivity _investigationActivity;

    private MentalSession _currentSession;
    private MentalSessionDeck _sessionDeck;
    private string _currentGoalId; // Track which investigation goal this session is for
    private string _currentInvestigationId; // Track which investigation this goal belongs to

    public MentalFacade(
        GameWorld gameWorld,
        MentalEffectResolver effectResolver,
        MentalNarrativeService narrativeService,
        MentalDeckBuilder deckBuilder,
        TimeManager timeManager,
        InvestigationActivity investigationActivity)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _investigationActivity = investigationActivity ?? throw new ArgumentNullException(nameof(investigationActivity));
    }

    public MentalSession GetCurrentSession()
    {
        return _currentSession;
    }

    public bool IsSessionActive()
    {
        return _currentSession != null;
    }

    public List<CardInstance> GetHand()
    {
        return _sessionDeck?.Hand.ToList() ?? new List<CardInstance>();
    }

    public MentalDeckBuilder GetDeckBuilder()
    {
        return _deckBuilder;
    }

    public int GetDeckCount()
    {
        return _sessionDeck?.RemainingDeckCards ?? 0;
    }

    public int GetDiscardCount()
    {
        return _sessionDeck?.PlayedCards.Count ?? 0;
    }

    public MentalSession StartSession(MentalChallengeDeck engagement, List<CardInstance> deck, List<CardInstance> startingHand,
        string goalId, string investigationId)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // Track investigation context
        _currentGoalId = goalId;
        _currentInvestigationId = investigationId;

        Player player = _gameWorld.GetPlayer();
        Location location = player.CurrentLocation;

        // PROGRESSION SYSTEM: Focus cost for Mental investigations
        int focusCost = engagement.FocusCost ?? 10;
        if (player.Focus < focusCost)
        {
            throw new InvalidOperationException($"Insufficient Focus. Need {focusCost}, have {player.Focus}");
        }
        player.Focus -= focusCost;
        Console.WriteLine($"[MentalFacade] Paid {focusCost} Focus to start investigation (remaining: {player.Focus})");

        // Get location's persisted exposure (Mental debt system)
        int baseExposure = location?.Exposure ?? 0;

        _currentSession = new MentalSession
        {
            InvestigationId = engagement.Id,
            VenueId = location?.VenueId,
            CurrentAttention = 10,
            MaxAttention = 10,
            CurrentUnderstanding = 0,
            CurrentProgress = 0,
            CurrentExposure = baseExposure, // Start with persisted exposure from venue
            MaxExposure = engagement.DangerThreshold
            // VictoryThreshold removed - GoalCard play determines success, not Progress threshold
        };

        // Use MentalSessionDeck with Pile abstraction
        _sessionDeck = MentalSessionDeck.CreateFromInstances(deck, startingHand);
        _currentSession.Deck = _sessionDeck;

        // SYMMETRY RESTORATION: Extract GoalCards from Goal and add to session deck (MATCH SOCIAL PATTERN)
        if (!string.IsNullOrEmpty(goalId) && _gameWorld.Goals.TryGetValue(goalId, out Goal goal))
        {
            if (goal.GoalCards != null && goal.GoalCards.Any())
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
                    _sessionDeck.AddGoalCard(goalCardInstance);
                    Console.WriteLine($"[MentalFacade] Added GoalCard '{goalCard.Name}' with threshold {goalCard.threshold}");
                }
            }
        }

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _sessionDeck.DrawToHand(cardsToDrawStartingSized);
        }

        Console.WriteLine($"[MentalFacade] Started session with {baseExposure} base exposure from venue, {startingHand.Count} knowledge cards in starting hand");

        return _currentSession;
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
        int cardsToDraw = _currentSession.GetDrawCount();

        // Draw cards to hand
        _sessionDeck.DrawToHand(cardsToDraw);
        Console.WriteLine($"[MentalFacade] OBSERVE: Drew {cardsToDraw} cards (consumed {_currentSession.CurrentLeads} Leads)");

        // Consume Leads after drawing
        _currentSession.CurrentLeads = 0;

        // Check and unlock goal cards if Progress threshold met
        _sessionDeck.CheckGoalThresholds(_currentSession.CurrentProgress);

        return new MentalTurnResult
        {
            Success = true,
            Narrative = "You observe carefully, gathering information.",
            CurrentProgress = _currentSession.CurrentProgress,
            CurrentExposure = _currentSession.CurrentExposure,
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

        if (!_sessionDeck.Hand.Contains(card))
        {
            throw new InvalidOperationException("Card not in hand");
        }

        // SYMMETRY RESTORATION: Check goal card type BEFORE template check
        // Goal cards have no MentalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Goal)
        {
            Console.WriteLine($"[MentalFacade] GoalCard played - ending session with success");

            // Apply obstacle effects via containment pattern (THREE PARALLEL SYSTEMS symmetry)
            // DISTRIBUTED INTERACTION: Find parent obstacle from GameWorld.Obstacles
            Player currentPlayer = _gameWorld.GetPlayer();
            Location location = currentPlayer.CurrentLocation;

            // Find obstacle containing this goal (from GameWorld.Obstacles list)
            Obstacle parentObstacle = _gameWorld.Obstacles
                .FirstOrDefault(o => o.Goals.Any(g => g.Id == _currentGoalId));

            if (parentObstacle != null && _gameWorld.Goals.TryGetValue(_currentGoalId, out Goal goal))
            {
                Console.WriteLine($"[MentalFacade] Applying consequence '{goal.ConsequenceType}' for goal '{goal.Name}' on obstacle '{parentObstacle.Name}'");

                // PHASE 2: Five Consequence Types
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
                            Console.WriteLine($"[MentalFacade] RESOLUTION: Obstacle '{parentObstacle.Name}' permanently overcome and removed");
                        }
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Bypass:
                        // Player passes, obstacle persists
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
                        Console.WriteLine($"[MentalFacade] BYPASS: Passed obstacle '{parentObstacle.Name}', persists for world");
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Transform:
                        // Fundamentally changed - properties to 0, new description
                        parentObstacle.State = Wayfarer.GameState.Enums.ObstacleState.Transformed;
                        parentObstacle.PhysicalDanger = 0;
                        parentObstacle.MentalComplexity = 0;
                        parentObstacle.SocialDifficulty = 0;

                        if (!string.IsNullOrEmpty(goal.TransformDescription))
                        {
                            parentObstacle.TransformedDescription = goal.TransformDescription;
                        }

                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
                        Console.WriteLine($"[MentalFacade] TRANSFORM: Obstacle '{parentObstacle.Name}' fundamentally changed");
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Modify:
                        // Properties reduced - other goals may unlock
                        if (goal.PropertyReduction != null)
                        {
                            parentObstacle.PhysicalDanger = Math.Max(0,
                                parentObstacle.PhysicalDanger - goal.PropertyReduction.ReducePhysicalDanger);
                            parentObstacle.MentalComplexity = Math.Max(0,
                                parentObstacle.MentalComplexity - goal.PropertyReduction.ReduceMentalComplexity);
                            parentObstacle.SocialDifficulty = Math.Max(0,
                                parentObstacle.SocialDifficulty - goal.PropertyReduction.ReduceSocialDifficulty);

                            Console.WriteLine($"[MentalFacade] MODIFY: Reduced properties to P:{parentObstacle.PhysicalDanger} M:{parentObstacle.MentalComplexity} S:{parentObstacle.SocialDifficulty}");
                        }

                        parentObstacle.ResolutionMethod = Wayfarer.GameState.Enums.ResolutionMethod.Preparation;

                        // Check if all properties now at 0 (auto-transform)
                        if (parentObstacle.PhysicalDanger == 0 &&
                            parentObstacle.MentalComplexity == 0 &&
                            parentObstacle.SocialDifficulty == 0)
                        {
                            parentObstacle.State = Wayfarer.GameState.Enums.ObstacleState.Transformed;
                            Console.WriteLine($"[MentalFacade] MODIFY: All properties at 0, auto-transformed");
                        }
                        break;

                    case Wayfarer.GameState.Enums.ConsequenceType.Grant:
                        // Grant items/knowledge, no obstacle change
                        // (Knowledge cards handled in Phase 3, items already handled by reward system)
                        Console.WriteLine($"[MentalFacade] GRANT: Granted knowledge/items");
                        break;
                }
            }
            // Else: ambient goal with no obstacle parent

            _sessionDeck.PlayCard(card); // Mark card as played
            EndSession(); // Immediate end on GoalCard play

            return new MentalTurnResult
            {
                Success = true,
                Narrative = $"You completed the investigation: {card.GoalCardTemplate?.Name}",
                CurrentProgress = _currentSession?.CurrentProgress ?? 0,
                CurrentExposure = _currentSession?.CurrentExposure ?? 0,
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
        MentalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _currentSession, player, actionType);

        // Check Attention cost from projection (includes modifiers if any exist)
        if (_currentSession.CurrentAttention + projection.AttentionChange < 0)
        {
            int actualCost = -projection.AttentionChange;
            throw new InvalidOperationException($"Insufficient Attention. Need {actualCost}, have {_currentSession.CurrentAttention}");
        }

        // Apply projection to session state
        ApplyProjectionToSession(projection, _currentSession, player);

        // Check and unlock goal cards if Progress threshold met
        List<CardInstance> unlockedGoals = _sessionDeck.CheckGoalThresholds(_currentSession.CurrentProgress);
        foreach (CardInstance goalCard in unlockedGoals)
        {
            Console.WriteLine($"[MentalFacade] Goal card unlocked: {goalCard.GoalCardTemplate?.Id} (Progress threshold met)");
        }

        // Track categories for investigation depth
        MentalCategory category = card.MentalCardTemplate.Category;
        if (!_currentSession.CategoryCounts.ContainsKey(category))
        {
            _currentSession.CategoryCounts[category] = 0;
        }
        _currentSession.CategoryCounts[category]++;

        // ACT: Generate Leads based on card depth
        // Leads determine how many cards OBSERVE will draw
        int leadsGenerated = card.MentalCardTemplate.Depth switch
        {
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            _ => 0
        };
        _currentSession.CurrentLeads += leadsGenerated;
        Console.WriteLine($"[MentalFacade] ACT: Generated {leadsGenerated} Leads from depth {card.MentalCardTemplate.Depth} card (total Leads: {_currentSession.CurrentLeads})");

        // PROGRESSION SYSTEM: Award XP to bound stat (XP pre-calculated at parse time)
        if (card.MentalCardTemplate.BoundStat != PlayerStatType.None)
        {
            player.Stats.AddXP(card.MentalCardTemplate.BoundStat, card.MentalCardTemplate.XPReward);
            Console.WriteLine($"[MentalFacade] Awarded {card.MentalCardTemplate.XPReward} XP to {card.MentalCardTemplate.BoundStat} (Depth {card.MentalCardTemplate.Depth})");
        }

        _sessionDeck.PlayCard(card);
        // NO DRAWING - Methods move to Applied pile, only OBSERVE draws cards

        string narrative = _narrativeService.GenerateActionNarrative(card, _currentSession);

        // Check victory/consequence thresholds
        bool sessionEnded = false;
        if (_currentSession.ShouldEnd())
        {
            // Apply consequences if Exposure threshold reached
            if (_currentSession.CurrentExposure >= 10)
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
            CurrentProgress = _currentSession?.CurrentProgress ?? 0,
            CurrentExposure = _currentSession?.CurrentExposure ?? 0,
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
    /// Leave investigation - saves state for later return
    /// Mental challenges allow leaving and resuming
    /// </summary>
    public MentalOutcome LeaveInvestigation()
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
            FinalProgress = _currentSession.CurrentProgress,
            FinalExposure = _currentSession.CurrentExposure,
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
        bool success = _sessionDeck.PlayedCards.Any(c => c.CardType == CardTypes.Goal);

        MentalOutcome outcome = new MentalOutcome
        {
            Success = success,
            FinalProgress = _currentSession.CurrentProgress,
            FinalExposure = _currentSession.CurrentExposure,
            SessionSaved = false
        };

        // Check for investigation progress if this was an investigation goal
        if (success && !string.IsNullOrEmpty(_currentGoalId) && !string.IsNullOrEmpty(_currentInvestigationId))
        {
            CheckInvestigationProgress(_currentGoalId, _currentInvestigationId);
        }

        Player player = _gameWorld.GetPlayer();

        // PROGRESSION SYSTEM: Award Venue familiarity on success
        if (success && !string.IsNullOrEmpty(_currentSession.VenueId))
        {
            int currentFamiliarity = player.LocationFamiliarity.GetFamiliarity(_currentSession.VenueId);
            int newFamiliarity = Math.Min(3, currentFamiliarity + 1); // Max familiarity is 3
            player.LocationFamiliarity.SetFamiliarity(_currentSession.VenueId, newFamiliarity);
            Console.WriteLine($"[MentalFacade] Increased familiarity with Venue '{_currentSession.VenueId}' to {newFamiliarity}");
        }

        // Persist exposure to Location (Mental debt system)
        // Exposure accumulates - next Mental engagement at this location starts with elevated baseline
        Location currentSpot = player.CurrentLocation;

        if (currentSpot != null)
        {
            currentSpot.Exposure = _currentSession.CurrentExposure;
            Console.WriteLine($"[MentalFacade] Persisted {currentSpot.Exposure} exposure to location {currentSpot.Id}");
        }

        // Clear investigation context
        _currentGoalId = null;
        _currentInvestigationId = null;

        _currentSession = null;
        _sessionDeck?.Clear();

        return outcome;
    }

    /// <summary>
    /// Apply consequences when Exposure threshold reached
    /// Structure becomes dangerous, investigation discovered
    /// </summary>
    private void ApplyExposureConsequences(Player player)
    {
        // Health damage from dangerous structure or being caught
        int healthDamage = 5 + (_currentSession.CurrentExposure - 10);
        player.Health -= healthDamage;

        Console.WriteLine($"[MentalFacade] EXPOSURE THRESHOLD: Took {healthDamage} health damage from investigation consequences");
    }

    /// <summary>
    /// Check for investigation progress when Mental goal completes
    /// </summary>
    private void CheckInvestigationProgress(string goalId, string investigationId)
    {
        // Check if this is an intro action (Discovered → Active transition)
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation != null && goalId == "notice_waterwheel")
        {
            // This is intro completion - activate investigation
            // CompleteIntroAction spawns goals directly to ActiveGoals
            _investigationActivity.CompleteIntroAction(investigationId);

            Console.WriteLine($"[MentalFacade] Investigation '{investigation.Name}' ACTIVATED");
            return;
        }

        // Regular goal completion
        InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(goalId, investigationId);

        // Log progress for UI modal display (UI will handle modal)
        Console.WriteLine($"[MentalFacade] Investigation progress: {progressResult.CompletedGoalCount}/{progressResult.TotalGoalCount} goals complete");

        // Check if investigation is now complete
        InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigationId);
        if (completeResult != null)
        {
            // Investigation complete - UI will display completion modal
            Console.WriteLine($"[MentalFacade] Investigation '{completeResult.InvestigationName}' COMPLETE!");
        }
    }
}
