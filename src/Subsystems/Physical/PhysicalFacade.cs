using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public PhysicalSession GetCurrentSession() => _currentSession;
    public bool IsSessionActive() => _currentSession != null;
    public List<CardInstance> GetHand() => _sessionDeck?.Hand.ToList() ?? new List<CardInstance>();
    public PhysicalDeckBuilder GetDeckBuilder() => _deckBuilder;
    public int GetDeckCount() => _sessionDeck?.RemainingDeckCards ?? 0;
    public int GetDiscardCount() => _sessionDeck?.PlayedCards.Count ?? 0;

    public PhysicalSession StartSession(PhysicalChallengeType engagement, List<CardInstance> deck, List<CardInstance> startingHand, string locationId, string goalId = null, string investigationId = null)
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
            CurrentPosition = 0,
            MaxPosition = 10,
            CurrentUnderstanding = 0,
            CurrentBreakthrough = 0,
            CurrentDanger = 0,
            MaxDanger = engagement.DangerThreshold,
            VictoryThreshold = engagement.VictoryThreshold,
            Commitment = 0
        };

        // Use PhysicalSessionDeck with Pile abstraction
        _sessionDeck = PhysicalSessionDeck.CreateFromInstances(deck, startingHand);

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _sessionDeck.DrawToHand(cardsToDrawStartingSized);
        }

        Console.WriteLine($"[PhysicalFacade] Started session with {startingHand.Count} cards in starting hand");

        return _currentSession;
    }

    public async Task<PhysicalTurnResult> ExecuteAssess(CardInstance card)
    {
        // Advance time by 1 segment per action (per documentation)
        _timeManager.AdvanceSegments(1);

        return await ExecuteCard(card, PhysicalActionType.Assess);
    }

    public async Task<PhysicalTurnResult> ExecuteExecute(CardInstance card)
    {
        // Advance time by 1 segment per action (per documentation)
        _timeManager.AdvanceSegments(1);

        return await ExecuteCard(card, PhysicalActionType.Execute);
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: Execute card using projection pattern
    /// 1. Get projection from resolver (single source of truth)
    /// 2. Apply projection to session
    /// DUAL BALANCE: Action type (Assess/Execute) combined with card Approach
    /// Parallel to ConversationFacade.SelectCard() and MentalFacade.ExecuteCard()
    /// </summary>
    private async Task<PhysicalTurnResult> ExecuteCard(CardInstance card, PhysicalActionType actionType)
    {
        if (!IsSessionActive())
        {
            throw new InvalidOperationException("No active physical session");
        }

        if (!_sessionDeck.Hand.Contains(card))
        {
            throw new InvalidOperationException("Card not in hand");
        }

        if (card.PhysicalCardTemplate == null)
        {
            throw new InvalidOperationException("Card has no Physical template");
        }

        Player player = _gameWorld.GetPlayer();

        // PROJECTION PRINCIPLE: Get projection from resolver (single source of truth)
        // DUAL BALANCE: Pass action type to combine with card Approach
        PhysicalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _currentSession, player, actionType);

        // Check Position cost BEFORE applying
        if (_currentSession.CurrentPosition < card.PhysicalCardTemplate.PositionCost)
        {
            throw new InvalidOperationException($"Insufficient Position. Need {card.PhysicalCardTemplate.PositionCost}, have {_currentSession.CurrentPosition}");
        }

        // Apply projection to session state
        ApplyProjectionToSession(projection, _currentSession, player);

        // Check and unlock goal cards if Breakthrough threshold met
        List<CardInstance> unlockedGoals = _sessionDeck.CheckGoalThresholds(_currentSession.CurrentBreakthrough);
        foreach (CardInstance goalCard in unlockedGoals)
        {
            Console.WriteLine($"[PhysicalFacade] Goal card unlocked: {goalCard.PhysicalCardTemplate?.Id} (Breakthrough threshold met)");
        }

        // Track approach history for Decisive card requirements
        if (actionType == PhysicalActionType.Execute)
        {
            _currentSession.ApproachHistory++;
        }

        // PROGRESSION SYSTEM: Award XP to bound stat (XP pre-calculated at parse time)
        if (card.PhysicalCardTemplate.BoundStat != PlayerStatType.None)
        {
            player.Stats.AddXP(card.PhysicalCardTemplate.BoundStat, card.PhysicalCardTemplate.XPReward);
            Console.WriteLine($"[PhysicalFacade] Awarded {card.PhysicalCardTemplate.XPReward} XP to {card.PhysicalCardTemplate.BoundStat} (Depth {card.PhysicalCardTemplate.Depth})");
        }

        _sessionDeck.PlayCard(card);
        _sessionDeck.DrawToHand(projection.CardsToDraw);

        string narrative = _narrativeService.GenerateActionNarrative(card, _currentSession);

        // Check victory/consequence thresholds
        bool sessionEnded = false;
        if (_currentSession.ShouldEnd())
        {
            // Apply consequences if Danger threshold reached
            if (_currentSession.CurrentDanger >= _currentSession.MaxDanger)
            {
                ApplyDangerConsequences(player);
            }

            EndSession();
            sessionEnded = true;
        }

        return new PhysicalTurnResult
        {
            Success = true,
            Narrative = narrative,
            CurrentBreakthrough = _currentSession?.CurrentBreakthrough ?? 0,
            CurrentDanger = _currentSession?.CurrentDanger ?? 0,
            SessionEnded = sessionEnded
        };
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: ONLY place where projections become reality
    /// Parallel to ConversationFacade.ApplyProjectionToSession() and MentalFacade.ApplyProjectionToSession()
    /// </summary>
    private void ApplyProjectionToSession(PhysicalCardEffectResult projection, PhysicalSession session, Player player)
    {
        // Builder resource: Position
        session.CurrentPosition += projection.PositionChange;
        if (session.CurrentPosition > session.MaxPosition)
        {
            session.CurrentPosition = session.MaxPosition;
        }
        if (session.CurrentPosition < 0)
        {
            session.CurrentPosition = 0;
        }

        // Victory resource: Breakthrough (stored as Progress in session)
        if (projection.BreakthroughChange != 0)
        {
            session.CurrentBreakthrough += projection.BreakthroughChange;
        }

        // Consequence resource: Danger
        if (projection.DangerChange != 0)
        {
            session.CurrentDanger += projection.DangerChange;
        }

        // Balance resource
        if (projection.BalanceChange != 0)
        {
            session.Commitment += projection.BalanceChange;
        }

        // Persistent progress: Readiness (stored as Understanding in session)
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

        bool success = _currentSession.CurrentBreakthrough >= 20;

        PhysicalOutcome outcome = new PhysicalOutcome
        {
            Success = success,
            FinalProgress = _currentSession.CurrentBreakthrough,
            FinalDanger = _currentSession.CurrentDanger,
            EscapeCost = null
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
        if (investigation != null && goalId == $"{investigationId}_intro")
        {
            // This is intro completion - activate investigation
            List<LocationGoal> firstGoals = _investigationActivity.CompleteIntroAction(investigationId);

            // Add first goals to their respective spots (Spots are the only entity that matters)
            if (firstGoals.Count > 0)
            {
                foreach (LocationGoal goal in firstGoals)
                {
                    LocationSpotEntry spotEntry = _gameWorld.Spots.FirstOrDefault(s => s.Spot.SpotID == goal.SpotId);
                    if (spotEntry != null)
                    {
                        if (spotEntry.Spot.Goals == null)
                            spotEntry.Spot.Goals = new List<LocationGoal>();
                        spotEntry.Spot.Goals.Add(goal);
                    }
                }
            }

            Console.WriteLine($"[PhysicalFacade] Investigation '{investigation.Name}' ACTIVATED - {firstGoals.Count} goals spawned");
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
