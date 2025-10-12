using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

public class PhysicalFacade
{
    private readonly GameWorld _gameWorld;
    private readonly PhysicalEffectResolver _effectResolver;
    private readonly PhysicalNarrativeService _narrativeService;
    private readonly PhysicalDeckBuilder _deckBuilder;
    private readonly TimeManager _timeManager;
    private readonly InvestigationActivity _investigationActivity;

    private PhysicalSession _currentSession;
    private PhysicalSessionDeck _sessionDeck;
    private string _currentGoalId; // Track which investigation goal this session is for
    private string _currentInvestigationId; // Track which investigation this goal belongs to

    public PhysicalFacade(
        GameWorld gameWorld,
        PhysicalEffectResolver effectResolver,
        PhysicalNarrativeService narrativeService,
        PhysicalDeckBuilder deckBuilder,
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

    public PhysicalSession GetCurrentSession()
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

    public PhysicalDeckBuilder GetDeckBuilder()
    {
        return _deckBuilder;
    }

    public int GetDeckCount()
    {
        return _sessionDeck?.RemainingDeckCards ?? 0;
    }

    public int GetExhaustCount()
    {
        return _sessionDeck?.LockedCards.Count ?? 0;
    }

    public List<CardInstance> GetLockedCards()
    {
        return _sessionDeck?.LockedCards.ToList() ?? new List<CardInstance>();
    }

    public PhysicalSession StartSession(PhysicalChallengeDeck engagement, List<CardInstance> deck, List<CardInstance> startingHand,
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

        _currentSession = new PhysicalSession
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
        _sessionDeck = PhysicalSessionDeck.CreateFromInstances(deck, startingHand);
        _currentSession.Deck = _sessionDeck;

        // Extract GoalCards from Goal and add to session deck (MATCH SOCIAL PATTERN)
        if (!string.IsNullOrEmpty(goalId) && _gameWorld.Goals.TryGetValue(goalId, out Goal goal))
        {
            if (goal.GoalCards != null && goal.GoalCards.Any())
            {
                foreach (GoalCard goalCard in goal.GoalCards)
                {
                    // Create CardInstance from GoalCard (constructor sets CardType automatically)
                    CardInstance goalCardInstance = new CardInstance(goalCard)
                    {
                        Context = new CardContext { threshold = goalCard.threshold }
                    };

                    // Add to session deck's requestPile
                    _sessionDeck.AddGoalCard(goalCardInstance);
                    Console.WriteLine($"[PhysicalFacade] Added GoalCard '{goalCard.Name}' with threshold {goalCard.threshold}");
                }
            }
        }

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _sessionDeck.DrawToHand(cardsToDrawStartingSized);
        }

        Console.WriteLine($"[PhysicalFacade] Started session with {startingHand.Count} cards in starting hand");

        return _currentSession;
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
        List<CardInstance> lockedCards = _sessionDeck.LockedCards.ToList();

        // TRIGGER COMBO: Apply full projection for each locked card
        foreach (CardInstance card in lockedCards)
        {
            if (card.PhysicalCardTemplate != null)
            {
                // PROJECTION PRINCIPLE: Get projection for this card with Assess action
                PhysicalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _currentSession, player, PhysicalActionType.Assess);

                // Apply FULL projection: Breakthrough, Danger, Aggression (includes action -2 + card approach)
                // NOTE: Exertion was already spent on EXECUTE, projection.ExertionChange should be 0 here
                _currentSession.CurrentBreakthrough += projection.BreakthroughChange;
                _currentSession.CurrentDanger += projection.DangerChange;
                _currentSession.Aggression += projection.BalanceChange; // Includes action -2 AND card approach modifier

                // Award XP for this card
                if (card.PhysicalCardTemplate.BoundStat != PlayerStatType.None)
                {
                    player.Stats.AddXP(card.PhysicalCardTemplate.BoundStat, card.PhysicalCardTemplate.XPReward);
                }

                Console.WriteLine($"[PhysicalFacade] COMBO: {card.PhysicalCardTemplate.Id} -> Breakthrough +{projection.BreakthroughChange}, Danger +{projection.DangerChange}, Aggression {projection.BalanceChange:+#;-#;0}");
            }
        }

        Console.WriteLine($"[PhysicalFacade] ASSESS: All {lockedCards.Count} locked cards triggered, Aggression now {_currentSession.Aggression}");

        // Shuffle exhaust (locked cards) + hand back to deck, draw fresh
        _sessionDeck.ShuffleExhaustAndHandBackToDeck();

        int cardsToDraw = _currentSession.GetDrawCount();
        _sessionDeck.DrawToHand(cardsToDraw);

        // Check and unlock goal cards if Breakthrough threshold met
        _sessionDeck.CheckGoalThresholds(_currentSession.CurrentBreakthrough);

        // Check if danger threshold reached
        bool sessionEnded = false;
        if (_currentSession.ShouldEnd())
        {
            ApplyDangerConsequences(player);
            EndSession();
            sessionEnded = true;
        }

        return new PhysicalTurnResult
        {
            Success = true,
            Narrative = $"You assess the situation. {lockedCards.Count} prepared actions execute as a combo.",
            CurrentBreakthrough = _currentSession.CurrentBreakthrough,
            CurrentDanger = _currentSession.CurrentDanger,
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

        if (!_sessionDeck.Hand.Contains(card))
        {
            throw new InvalidOperationException("Card not in hand");
        }

        // Check goal card type BEFORE template check
        // Goal cards have no PhysicalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Goal)
        {
            // Apply obstacle effects via containment pattern (THREE PARALLEL SYSTEMS symmetry)
            // Find parent obstacle containing this goal (distributed interaction pattern)
            Player currentPlayer = _gameWorld.GetPlayer();
            Location location = currentPlayer.CurrentLocation;

            Obstacle parentObstacle = _gameWorld.Obstacles
                .FirstOrDefault(o => o.Goals.Any(g => g.Id == _currentGoalId));

            if (parentObstacle != null && _gameWorld.Goals.TryGetValue(_currentGoalId, out Goal goal))
            {
                switch (goal.ConsequenceType)
                {
                    case ConsequenceType.Resolution:
                        // Permanently overcome
                        parentObstacle.State = ObstacleState.Resolved;
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
                        // Remove from active play (but keep in GameWorld for history)
                        if (!parentObstacle.IsPermanent)
                        {
                            location.ObstacleIds.Remove(parentObstacle.Id);
                        }
                        Console.WriteLine($"[PhysicalFacade] Obstacle '{parentObstacle.Name}' permanently resolved via {goal.SetsResolutionMethod}");
                        break;

                    case ConsequenceType.Bypass:
                        // Player passes, obstacle persists
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
                        Console.WriteLine($"[PhysicalFacade] Bypassed obstacle '{parentObstacle.Name}', obstacle persists");
                        break;

                    case ConsequenceType.Transform:
                        // Fundamentally changed
                        parentObstacle.State = ObstacleState.Transformed;
                        parentObstacle.PhysicalDanger = 0;
                        parentObstacle.SocialDifficulty = 0;
                        parentObstacle.MentalComplexity = 0;
                        if (!string.IsNullOrEmpty(goal.TransformDescription))
                            parentObstacle.TransformedDescription = goal.TransformDescription;
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
                        Console.WriteLine($"[PhysicalFacade] Transformed obstacle '{parentObstacle.Name}', properties set to 0");
                        break;

                    case ConsequenceType.Modify:
                        // Properties reduced
                        if (goal.PropertyReduction != null)
                        {
                            parentObstacle.PhysicalDanger = Math.Max(0,
                                parentObstacle.PhysicalDanger - goal.PropertyReduction.ReducePhysicalDanger);
                            parentObstacle.SocialDifficulty = Math.Max(0,
                                parentObstacle.SocialDifficulty - goal.PropertyReduction.ReduceSocialDifficulty);
                            parentObstacle.MentalComplexity = Math.Max(0,
                                parentObstacle.MentalComplexity - goal.PropertyReduction.ReduceMentalComplexity);
                        }
                        parentObstacle.ResolutionMethod = ResolutionMethod.Preparation;
                        // Check if all properties are now 0 (fully modified)
                        if (parentObstacle.PhysicalDanger == 0 &&
                            parentObstacle.SocialDifficulty == 0 &&
                            parentObstacle.MentalComplexity == 0)
                        {
                            parentObstacle.State = ObstacleState.Transformed;
                        }
                        Console.WriteLine($"[PhysicalFacade] Modified obstacle '{parentObstacle.Name}', properties reduced");
                        break;

                    case ConsequenceType.Grant:
                        // Grant knowledge/items, no obstacle change
                        // Knowledge cards handled in Phase 3
                        // Items already handled by existing reward system
                        Console.WriteLine($"[PhysicalFacade] Grant consequence - items/knowledge granted");
                        break;
                }
            }
            // Else: ambient goal with no obstacle parent

            // GoalCards execute immediately (not locked for combo)
            _sessionDeck.Hand.ToList().Remove(card); // Remove from hand
            string narrative = _narrativeService.GenerateActionNarrative(card, _currentSession);
            EndSession();

            return new PhysicalTurnResult
            {
                Success = true,
                Narrative = narrative,
                CurrentBreakthrough = _currentSession?.CurrentBreakthrough ?? 0,
                CurrentDanger = _currentSession?.CurrentDanger ?? 0,
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
        PhysicalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _currentSession, player, PhysicalActionType.Execute);

        // Check Exertion cost from projection (includes modifiers like fatigue penalties, Foundation generation)
        if (_currentSession.CurrentExertion + projection.ExertionChange < 0)
        {
            int actualCost = -projection.ExertionChange;
            throw new InvalidOperationException($"Insufficient Exertion. Need {actualCost}, have {_currentSession.CurrentExertion}");
        }

        // EXECUTE: Lock card for combo
        _sessionDeck.LockCard(card);

        // Apply PARTIAL projection: Exertion + Aggression + strategic costs
        // DON'T apply Breakthrough/Danger yet (they trigger on ASSESS)
        _currentSession.CurrentExertion += projection.ExertionChange;
        if (_currentSession.CurrentExertion > _currentSession.MaxExertion)
        {
            _currentSession.CurrentExertion = _currentSession.MaxExertion;
        }

        // Apply Aggression from projection (includes action +1 AND card approach modifier)
        _currentSession.Aggression += projection.BalanceChange;

        // Apply strategic resource costs
        if (projection.HealthCost > 0) player.Health -= projection.HealthCost;
        if (projection.StaminaCost > 0) player.Stamina -= projection.StaminaCost;
        if (projection.CoinsCost > 0) player.Coins -= projection.CoinsCost;

        _currentSession.ApproachHistory++;

        Console.WriteLine($"[PhysicalFacade] EXECUTE: Locked {card.PhysicalCardTemplate.Id}, Exertion {projection.ExertionChange:+#;-#;0}, Aggression {projection.BalanceChange:+#;-#;0} (now {_currentSession.Aggression})");

        string executeNarrative = _narrativeService.GenerateActionNarrative(card, _currentSession);

        return new PhysicalTurnResult
        {
            Success = true,
            Narrative = executeNarrative,
            CurrentBreakthrough = _currentSession.CurrentBreakthrough,
            CurrentDanger = _currentSession.CurrentDanger,
            SessionEnded = false
        };
    }

    /// <summary>
    /// Escape physical challenge - costs resources, potentially fails
    /// Physical challenges make retreat difficult
    /// </summary>
    public PhysicalOutcome EscapeChallenge(Player player)
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Escape costs - health and stamina damage from retreat
        int healthCost = 5 + (_currentSession.CurrentDanger / 2);
        int staminaCost = 10;

        player.Health -= healthCost;
        player.Stamina -= staminaCost;

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = false, // Escape is failure
            FinalProgress = _currentSession.CurrentBreakthrough,
            FinalDanger = _currentSession.CurrentDanger,
            EscapeCost = $"{healthCost} Health, {staminaCost} Stamina"
        };

        _currentSession = null;
        _sessionDeck?.Clear();

        return outcome;
    }

    public PhysicalOutcome EndSession()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Success determined by GoalCard play (GoalCards end session immediately in ExecuteExecute)
        bool success = !string.IsNullOrEmpty(_currentGoalId);

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = success,
            FinalProgress = _currentSession.CurrentBreakthrough,
            FinalDanger = _currentSession.CurrentDanger,
            EscapeCost = ""
        };

        // Check for investigation progress if this was an investigation goal
        if (success && !string.IsNullOrEmpty(_currentGoalId) && !string.IsNullOrEmpty(_currentInvestigationId))
        {
            CheckInvestigationProgress(_currentGoalId, _currentInvestigationId);
        }

        // Award Reputation on success (Reputation system)
        // Physical success builds reputation affecting future Social and Physical engagements
        if (success)
        {
            Player player = _gameWorld.GetPlayer();
            int reputationGain = _currentSession.CurrentBreakthrough >= 20 ? 1 : 0;
            player.Reputation += reputationGain;
            Console.WriteLine($"[PhysicalFacade] Awarded {reputationGain} reputation for victory (total: {player.Reputation})");

            // PROGRESSION SYSTEM: Award mastery token for this challenge type
            if (!string.IsNullOrEmpty(_currentSession.ChallengeId))
            {
                player.MasteryTokens.AddMastery(_currentSession.ChallengeId, 1);
                int masteryLevel = player.MasteryTokens.GetMastery(_currentSession.ChallengeId);
                Console.WriteLine($"[PhysicalFacade] Earned mastery token for '{_currentSession.ChallengeId}'. Total: {masteryLevel}");
            }
        }

        // Clear investigation context
        _currentGoalId = null;
        _currentInvestigationId = null;

        _currentSession = null;
        _sessionDeck?.Clear();

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
        // Health damage from physical consequences
        int healthDamage = 5 + (_currentSession.CurrentDanger - _currentSession.MaxDanger);
        player.Health -= healthDamage;

        // Stamina damage from exhaustion
        int staminaDamage = 10;
        player.Stamina -= staminaDamage;

        player.InjuryCardIds.Add("injury_physical_moderate");

        Console.WriteLine($"[PhysicalFacade] DANGER THRESHOLD: Took {healthDamage} health and {staminaDamage} stamina damage, gained injury card");
    }

    /// <summary>
    /// Check for investigation progress when Physical goal completes
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

            Console.WriteLine($"[PhysicalFacade] Investigation '{investigation.Name}' ACTIVATED");
            return;
        }

        // Regular goal completion
        InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(goalId, investigationId);

        // Log progress for UI modal display (UI will handle modal)
        Console.WriteLine($"[PhysicalFacade] Investigation progress: {progressResult.CompletedGoalCount}/{progressResult.TotalGoalCount} goals complete");

        // Check if investigation is now complete
        InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigationId);
        if (completeResult != null)
        {
            // Investigation complete - UI will display completion modal
            Console.WriteLine($"[PhysicalFacade] Investigation '{completeResult.InvestigationName}' COMPLETE!");
        }
    }
}
