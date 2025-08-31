using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a conversation action (LISTEN, SPEAK, LEAVE)
/// </summary>
public class ConversationAction
{
    public ActionType ActionType { get; set; }
    public HashSet<CardInstance> SelectedCards { get; set; }
    public bool IsAvailable { get; set; }
    public List<CardInstance> AvailableCards { get; set; }
    
    public ConversationAction()
    {
        SelectedCards = new HashSet<CardInstance>();
        AvailableCards = new List<CardInstance>();
    }
}

/// <summary>
/// Result of processing a conversation turn
/// </summary>
public class ConversationTurnResult
{
    public bool Success { get; set; }
    public EmotionalState NewState { get; set; }
    public string NPCResponse { get; set; }
    public int? ComfortChange { get; set; }
    public int? OldComfort { get; set; }
    public int? NewComfort { get; set; }
    public int? PatienceRemaining { get; set; }
    public List<CardInstance> DrawnCards { get; set; }
    public List<CardInstance> RemovedCards { get; set; }
    public List<CardInstance> PlayedCards { get; set; }
    public CardPlayResult CardPlayResult { get; set; }
    public bool ExchangeAccepted { get; set; }
    
    public ConversationTurnResult()
    {
        DrawnCards = new List<CardInstance>();
        RemovedCards = new List<CardInstance>();
        PlayedCards = new List<CardInstance>();
    }
}

/// <summary>
/// Memento for saving/restoring conversation state
/// </summary>
public class ConversationMemento
{
    public string NpcId { get; set; }
    public ConversationType ConversationType { get; set; }
    public EmotionalState CurrentState { get; set; }
    public int CurrentComfort { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool GoalCardDrawn { get; set; }
    public int? GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public List<string> HandCardIds { get; set; }
    public List<string> DeckCardIds { get; set; }
    
    public ConversationMemento()
    {
        HandCardIds = new List<string>();
        DeckCardIds = new List<string>();
    }
}

/// <summary>
/// Player's current resource state
/// </summary>
public class ResourceState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Food { get; set; }
    public int Attention { get; set; }
    public Dictionary<ConnectionType, int> Tokens { get; set; }
    
    public ResourceState()
    {
        Tokens = new Dictionary<ConnectionType, int>();
    }
}