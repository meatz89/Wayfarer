using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks conversation state and history for analytics and debugging.
/// Maintains turn history and state transitions.
/// </summary>
public class ConversationStateTracker
{
    private ConversationSession _currentSession;
    private List<ConversationTurn> _turnHistory;
    private List<StateTransition> _stateTransitions;
    private DateTime _conversationStartTime;
    private int _totalCardsPlayed;
    private int _totalCardsDrawn;

    public ConversationStateTracker()
    {
        _turnHistory = new List<ConversationTurn>();
        _stateTransitions = new List<StateTransition>();
    }

    /// <summary>
    /// Begin tracking a new conversation
    /// </summary>
    public void BeginTracking(ConversationSession session)
    {
        _currentSession = session;
        _turnHistory.Clear();
        _stateTransitions.Clear();
        _conversationStartTime = DateTime.Now;
        _totalCardsPlayed = 0;
        _totalCardsDrawn = 0;
        
        // Record initial state
        RecordStateTransition(null, session.CurrentState, "Conversation started");
    }

    /// <summary>
    /// End tracking current conversation
    /// </summary>
    public void EndTracking()
    {
        if (_currentSession != null)
        {
            var duration = DateTime.Now - _conversationStartTime;
            Console.WriteLine($"[ConversationStateTracker] Conversation ended after {duration.TotalSeconds:F1} seconds");
            Console.WriteLine($"[ConversationStateTracker] Total turns: {_turnHistory.Count}");
            Console.WriteLine($"[ConversationStateTracker] Cards played: {_totalCardsPlayed}, Cards drawn: {_totalCardsDrawn}");
        }
        
        _currentSession = null;
    }

    /// <summary>
    /// Record a conversation turn
    /// </summary>
    public void RecordTurn(ConversationTurnResult result)
    {
        if (_currentSession == null)
            return;
        
        var turn = new ConversationTurn
        {
            TurnNumber = _turnHistory.Count + 1,
            Timestamp = DateTime.Now,
            ActionType = result.PlayedCards != null ? ActionType.Speak : ActionType.Listen,
            EmotionalState = result.NewState,
            ComfortBefore = result.OldComfort ?? _currentSession.CurrentComfort,
            ComfortAfter = result.NewComfort ?? _currentSession.CurrentComfort,
            ComfortChange = result.ComfortChange ?? 0,
            PatienceRemaining = result.PatienceRemaining ?? _currentSession.CurrentPatience
        };
        
        // Track cards
        if (result.PlayedCards != null)
        {
            turn.CardsPlayed = result.PlayedCards.Select(c => c.TemplateId).ToList();
            _totalCardsPlayed += result.PlayedCards.Count;
        }
        
        if (result.DrawnCards != null)
        {
            turn.CardsDrawn = result.DrawnCards.Select(c => c.TemplateId).ToList();
            _totalCardsDrawn += result.DrawnCards.Count;
        }
        
        _turnHistory.Add(turn);
        
        // Check for state transition
        if (result.NewState != _currentSession.CurrentState)
        {
            RecordStateTransition(_currentSession.CurrentState, result.NewState, "Comfort threshold reached");
        }
    }

    /// <summary>
    /// Record a state transition
    /// </summary>
    public void RecordStateTransition(EmotionalState? fromState, EmotionalState toState, string reason)
    {
        _stateTransitions.Add(new StateTransition
        {
            FromState = fromState,
            ToState = toState,
            TurnNumber = _turnHistory.Count,
            Timestamp = DateTime.Now,
            Reason = reason
        });
    }

    /// <summary>
    /// Get conversation statistics
    /// </summary>
    public ConversationStatistics GetStatistics()
    {
        if (_currentSession == null)
            return new ConversationStatistics();
        
        return new ConversationStatistics
        {
            TotalTurns = _turnHistory.Count,
            TotalCardsPlayed = _totalCardsPlayed,
            TotalCardsDrawn = _totalCardsDrawn,
            StateTransitions = _stateTransitions.Count,
            Duration = DateTime.Now - _conversationStartTime,
            AverageComfortPerTurn = _turnHistory.Any() ? 
                _turnHistory.Average(t => Math.Abs(t.ComfortChange)) : 0,
            MostCommonCardType = GetMostCommonCardType(),
            FinalState = _currentSession.CurrentState,
            GoalAchieved = _currentSession.GoalCardPlayed
        };
    }

    /// <summary>
    /// Get turn history for debugging
    /// </summary>
    public List<ConversationTurn> GetTurnHistory()
    {
        return new List<ConversationTurn>(_turnHistory);
    }

    /// <summary>
    /// Get state transition history
    /// </summary>
    public List<StateTransition> GetStateTransitions()
    {
        return new List<StateTransition>(_stateTransitions);
    }

    /// <summary>
    /// Check if conversation is progressing well
    /// </summary>
    public ConversationHealth CheckHealth()
    {
        if (_currentSession == null)
            return ConversationHealth.Unknown;
        
        // Check if stuck in negative states
        if (_turnHistory.Count > 5)
        {
            var recentStates = _turnHistory.TakeLast(5).Select(t => t.EmotionalState);
            if (recentStates.All(s => s == EmotionalState.HOSTILE || s == EmotionalState.DESPERATE))
            {
                return ConversationHealth.Critical;
            }
        }
        
        // Check if making progress
        if (_stateTransitions.Count == 0 && _turnHistory.Count > 10)
        {
            return ConversationHealth.Stagnant;
        }
        
        // Check comfort trend
        if (_turnHistory.Count >= 3)
        {
            var recentComfort = _turnHistory.TakeLast(3).Sum(t => t.ComfortChange);
            if (recentComfort > 5)
                return ConversationHealth.Excellent;
            if (recentComfort < -5)
                return ConversationHealth.Poor;
        }
        
        return ConversationHealth.Good;
    }

    /// <summary>
    /// Get most common card type played
    /// </summary>
    private CardType? GetMostCommonCardType()
    {
        if (!_turnHistory.Any() || _totalCardsPlayed == 0)
            return null;
        
        // This would need access to card templates to determine types
        // For now, return null
        return null;
    }
}

/// <summary>
/// Represents a single conversation turn
/// </summary>
public class ConversationTurn
{
    public int TurnNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public ActionType ActionType { get; set; }
    public EmotionalState EmotionalState { get; set; }
    public int ComfortBefore { get; set; }
    public int ComfortAfter { get; set; }
    public int ComfortChange { get; set; }
    public int PatienceRemaining { get; set; }
    public List<string> CardsPlayed { get; set; }
    public List<string> CardsDrawn { get; set; }
}

/// <summary>
/// Represents a state transition
/// </summary>
public class StateTransition
{
    public EmotionalState? FromState { get; set; }
    public EmotionalState ToState { get; set; }
    public int TurnNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Reason { get; set; }
}

/// <summary>
/// Conversation statistics
/// </summary>
public class ConversationStatistics
{
    public int TotalTurns { get; set; }
    public int TotalCardsPlayed { get; set; }
    public int TotalCardsDrawn { get; set; }
    public int StateTransitions { get; set; }
    public TimeSpan Duration { get; set; }
    public double AverageComfortPerTurn { get; set; }
    public CardType? MostCommonCardType { get; set; }
    public EmotionalState FinalState { get; set; }
    public bool GoalAchieved { get; set; }
}

/// <summary>
/// Health status of conversation
/// </summary>
public enum ConversationHealth
{
    Unknown,
    Excellent,
    Good,
    Stagnant,
    Poor,
    Critical
}