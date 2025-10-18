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
        string goalId, string investigationId)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        // Track investigation context
        _gameWorld.CurrentPhysicalGoalId = goalId;
        _gameWorld.CurrentPhysicalInvestigationId = investigationId;

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

        // Extract GoalCards from Goal and add to session deck (MATCH SOCIAL PATTERN)
        Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
        if (!string.IsNullOrEmpty(goalId) && goal != null)
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
                    _gameWorld.CurrentPhysicalSession.Deck.AddGoalCard(goalCardInstance);
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

                // Award XP for this card
                if (card.PhysicalCardTemplate.BoundStat != PlayerStatType.None)
                {
                    player.Stats.AddXP(card.PhysicalCardTemplate.BoundStat, card.PhysicalCardTemplate.XPReward);
                }
            }
        }// Shuffle exhaust (locked cards) + hand back to deck, draw fresh
        _gameWorld.CurrentPhysicalSession.Deck.ShuffleExhaustAndHandBackToDeck();

        int cardsToDraw = _gameWorld.CurrentPhysicalSession.GetDrawCount();
        _gameWorld.CurrentPhysicalSession.Deck.DrawToHand(cardsToDraw);

        // Check and unlock goal cards if Breakthrough threshold met
        _gameWorld.CurrentPhysicalSession.Deck.CheckGoalThresholds(_gameWorld.CurrentPhysicalSession.CurrentBreakthrough);

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

        // Check goal card type BEFORE template check
        // Goal cards have no PhysicalCardTemplate, so must be checked first
        if (card.CardType == CardTypes.Goal)
        {
            // Apply obstacle effects via containment pattern (THREE PARALLEL SYSTEMS symmetry)
            // DISTRIBUTED INTERACTION: Find parent obstacle from Goal.ParentObstacle
            Player currentPlayer = _gameWorld.GetPlayer();
            Location location = currentPlayer.CurrentLocation;

            // Get goal and its parent obstacle via object references
            Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == _gameWorld.CurrentPhysicalGoalId);
            if (goal == null)
                return null; // Goal not found

            Obstacle parentObstacle = goal.ParentObstacle;

            if (parentObstacle != null)
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
                        break;

                    case ConsequenceType.Bypass:
                        // Player passes, obstacle persists
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome; break;

                    case ConsequenceType.Transform:
                        // Fundamentally changed
                        parentObstacle.State = ObstacleState.Transformed;
                        parentObstacle.Intensity = 0;
                        if (!string.IsNullOrEmpty(goal.TransformDescription))
                            parentObstacle.TransformedDescription = goal.TransformDescription;
                        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
                        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome; break;

                    case ConsequenceType.Modify:
                        // Intensity reduced
                        if (goal.PropertyReduction != null)
                        {
                            parentObstacle.Intensity = Math.Max(0,
                                parentObstacle.Intensity - goal.PropertyReduction.ReduceIntensity);
                        }
                        parentObstacle.ResolutionMethod = ResolutionMethod.Preparation;
                        // Check if intensity is now 0 (fully modified)
                        if (parentObstacle.Intensity == 0)
                        {
                            parentObstacle.State = ObstacleState.Transformed;
                        }
                        break;

                    case ConsequenceType.Grant:
                        // Grant knowledge/items, no obstacle change
                        // Knowledge cards handled in Phase 3
                        // Items already handled by existing reward system
                        break;
                }
            }
            // Else: ambient goal with no obstacle parent

            // GoalCards execute immediately (not locked for combo)
            _gameWorld.CurrentPhysicalSession.Deck.Hand.ToList().Remove(card); // Remove from hand
            string narrative = _narrativeService.GenerateActionNarrative(card, _gameWorld.CurrentPhysicalSession);
            EndSession();

            return new PhysicalTurnResult
            {
                Success = true,
                Narrative = narrative,
                CurrentBreakthrough = _gameWorld.CurrentPhysicalSession?.CurrentBreakthrough ?? 0,
                CurrentDanger = _gameWorld.CurrentPhysicalSession?.CurrentDanger ?? 0,
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

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = false, // Escape is failure
            FinalProgress = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            FinalDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            EscapeCost = $"{healthCost} Health, {staminaCost} Stamina"
        };

        _gameWorld.CurrentPhysicalSession = null;
        _gameWorld.CurrentPhysicalSession.Deck?.Clear();

        return outcome;
    }

    public PhysicalOutcome EndSession()
    {
        if (!IsSessionActive())
        {
            return null;
        }

        // Success determined by GoalCard play (GoalCards end session immediately in ExecuteExecute)
        bool success = !string.IsNullOrEmpty(_gameWorld.CurrentPhysicalGoalId);

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = success,
            FinalProgress = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough,
            FinalDanger = _gameWorld.CurrentPhysicalSession.CurrentDanger,
            EscapeCost = ""
        };

        // Check for investigation progress if this was an investigation goal
        if (success && !string.IsNullOrEmpty(_gameWorld.CurrentPhysicalGoalId) && !string.IsNullOrEmpty(_gameWorld.CurrentPhysicalInvestigationId))
        {
            CheckInvestigationProgress(_gameWorld.CurrentPhysicalGoalId, _gameWorld.CurrentPhysicalInvestigationId);
        }

        // Award Reputation on success (Reputation system)
        // Physical success builds reputation affecting future Social and Physical engagements
        if (success)
        {
            Player player = _gameWorld.GetPlayer();
            int reputationGain = _gameWorld.CurrentPhysicalSession.CurrentBreakthrough >= 20 ? 1 : 0;
            player.Reputation += reputationGain;// PROGRESSION SYSTEM: Award mastery token for this challenge type
            if (!string.IsNullOrEmpty(_gameWorld.CurrentPhysicalSession.ChallengeId))
            {
                player.MasteryTokens.AddMastery(_gameWorld.CurrentPhysicalSession.ChallengeId, 1);
                int masteryLevel = player.MasteryTokens.GetMastery(_gameWorld.CurrentPhysicalSession.ChallengeId);
            }
        }

        // Clear investigation context
        _gameWorld.CurrentPhysicalGoalId = null;
        _gameWorld.CurrentPhysicalInvestigationId = null;

        _gameWorld.CurrentPhysicalSession = null;
        _gameWorld.CurrentPhysicalSession.Deck?.Clear();

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
        int healthDamage = 5 + (_gameWorld.CurrentPhysicalSession.CurrentDanger - _gameWorld.CurrentPhysicalSession.MaxDanger);
        player.Health -= healthDamage;

        // Stamina damage from exhaustion
        int staminaDamage = 10;
        player.Stamina -= staminaDamage;

        player.InjuryCardIds.Add("injury_physical_moderate");
    }

    /// <summary>
    /// Check for investigation progress when Physical goal completes
    /// </summary>
    private void CheckInvestigationProgress(string goalId, string investigationId)
    {
        // KEEP - investigationId is external input from session
        // Check if this is an intro action (Discovered → Active transition)
        Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation != null && goalId == "notice_waterwheel")
        {
            // This is intro completion - activate investigation
            // CompleteIntroAction spawns goals directly to ActiveGoals
            _investigationActivity.CompleteIntroAction(investigationId);
            return;
        }

        // Regular goal completion
        InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(goalId, investigationId);

        // Log progress for UI modal display (UI will handle modal)

        // Check if investigation is now complete
        InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigationId);
        if (completeResult != null)
        {
            // Investigation complete - UI will display completion modal
        }
    }
}
