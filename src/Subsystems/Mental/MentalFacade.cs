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

    private MentalSession _currentSession;
    private MentalSessionDeck _sessionDeck;

    public MentalFacade(
        GameWorld gameWorld,
        MentalEffectResolver effectResolver,
        MentalNarrativeService narrativeService,
        MentalDeckBuilder deckBuilder,
        TimeManager timeManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    }

    public MentalSession GetCurrentSession() => _currentSession;
    public bool IsSessionActive() => _currentSession != null;
    public List<CardInstance> GetHand() => _sessionDeck?.Hand.ToList() ?? new List<CardInstance>();
    public MentalDeckBuilder GetDeckBuilder() => _deckBuilder;

    public MentalSession StartSession(MentalEngagementType engagement, List<CardInstance> deck, List<CardInstance> startingHand, string locationId)
    {
        if (IsSessionActive())
        {
            EndSession();
        }

        Player player = _gameWorld.GetPlayer();
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);

        // Get location's persisted exposure (Mental debt system)
        int baseExposure = location?.Exposure ?? 0;

        _currentSession = new MentalSession
        {
            InvestigationId = engagement.Id,
            CurrentAttention = 10,
            MaxAttention = 10,
            CurrentUnderstanding = 0,
            CurrentProgress = 0,
            CurrentExposure = baseExposure, // Start with persisted exposure from location
            MaxExposure = engagement.DangerThreshold,
            VictoryThreshold = engagement.VictoryThreshold,
            ObserveActBalance = 0
        };

        // Use MentalSessionDeck with Pile abstraction
        _sessionDeck = MentalSessionDeck.CreateFromInstances(deck, startingHand);

        // Draw remaining cards to reach InitialHandSize
        int cardsToDrawStartingSized = engagement.InitialHandSize - startingHand.Count;
        if (cardsToDrawStartingSized > 0)
        {
            _sessionDeck.DrawToHand(cardsToDrawStartingSized);
        }

        Console.WriteLine($"[MentalFacade] Started session with {baseExposure} base exposure from location, {startingHand.Count} knowledge cards in starting hand");

        return _currentSession;
    }

    public async Task<MentalTurnResult> ExecuteObserve(CardInstance card)
    {
        // Advance time by 1 segment per action (per documentation)
        _timeManager.AdvanceSegments(1);

        return await ExecuteCard(card, MentalActionType.Observe);
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

        if (card.MentalCardTemplate == null)
        {
            throw new InvalidOperationException("Card has no Mental template");
        }

        Player player = _gameWorld.GetPlayer();

        // PROJECTION PRINCIPLE: Get projection from resolver (single source of truth)
        // DUAL BALANCE: Pass action type to combine with card Method
        MentalCardEffectResult projection = _effectResolver.ProjectCardEffects(card, _currentSession, player, actionType);

        // Check Attention cost BEFORE applying
        if (_currentSession.CurrentAttention < card.MentalCardTemplate.AttentionCost)
        {
            throw new InvalidOperationException($"Insufficient Attention. Need {card.MentalCardTemplate.AttentionCost}, have {_currentSession.CurrentAttention}");
        }

        // Apply projection to session state
        ApplyProjectionToSession(projection, _currentSession, player);

        // Check and unlock goal cards if Progress threshold met
        List<CardInstance> unlockedGoals = _sessionDeck.CheckGoalThresholds(_currentSession.CurrentProgress);
        foreach (CardInstance goalCard in unlockedGoals)
        {
            Console.WriteLine($"[MentalFacade] Goal card unlocked: {goalCard.MentalCardTemplate?.Id} (Progress threshold met)");
        }

        // Track categories for investigation depth
        MentalCategory category = card.MentalCardTemplate.Category;
        if (!_currentSession.CategoryCounts.ContainsKey(category))
        {
            _currentSession.CategoryCounts[category] = 0;
        }
        _currentSession.CategoryCounts[category]++;

        _sessionDeck.PlayCard(card);
        _sessionDeck.DrawToHand(projection.CardsToDraw);

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

        // Balance resource
        if (projection.BalanceChange != 0)
        {
            session.ObserveActBalance += projection.BalanceChange;
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

        MentalOutcome outcome = new MentalOutcome
        {
            Success = _currentSession.CurrentProgress >= 20,
            FinalProgress = _currentSession.CurrentProgress,
            FinalExposure = _currentSession.CurrentExposure,
            SessionSaved = false
        };

        // Persist exposure to location (Mental debt system)
        // Exposure accumulates - next Mental engagement at this location starts with elevated baseline
        Location location = _gameWorld.Locations.FirstOrDefault(l =>
            l.Goals != null && l.Goals.Any(g => g.EngagementTypeId == _currentSession.InvestigationId));

        if (location != null)
        {
            location.Exposure = _currentSession.CurrentExposure;
            Console.WriteLine($"[MentalFacade] Persisted {location.Exposure} exposure to location {location.Id}");
        }

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
}
