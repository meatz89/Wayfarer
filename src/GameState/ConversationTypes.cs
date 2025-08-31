using System;
using System.Collections.Generic;
using System.Linq;

// Core conversation enums and types

public enum ResourceType
{
    Coins,
    Health,
    Hunger,
    Food
}

public enum ConversationType
{
    FriendlyChat,
    Commerce,
    Promise,
    Resolution,
    Delivery
}

public enum EmotionalState
{
    EAGER,
    CONNECTED,
    GUARDED,
    TENSE,
    DEFENSIVE
}

public enum CardType
{
    Trust,
    Commerce,
    Status,
    Shadow,
    Comfort,
    Observation,
    Patience,
    Burden,
    Letter,
    Exchange,
    Promise,
    Goal,
    Token
}

public enum PersistenceType
{
    Fleeting,
    Sticky
}

public enum ActionType
{
    None,
    Listen,
    Speak
}

// Core conversation card definition
public class ConversationCard
{
    public string Id { get; init; }
    public string Name { get; init; }
    public CardType Type { get; init; }
    public int Weight { get; init; }
    public int BaseSuccessChance { get; init; }
    public int BaseComfortReward { get; init; }
    public string DialogueFragment { get; init; }
    public bool IsSpecial { get; init; }
    public bool IsSingleUse { get; init; }
    public PersistenceType Persistence { get; init; } = PersistenceType.Sticky;
    public string VerbPhrase { get; init; }
    public Dictionary<EmotionalState, int> StateModifiers { get; init; } = new();
    public Dictionary<EmotionalState, int> WeightModifiers { get; init; } = new();
    public EmotionalState? TransitionToState { get; init; }
    public int TransitionChance { get; init; }
    public string ConversationType { get; init; }
    public bool IsObservation { get; init; }
    public string ObservationType { get; init; }
    public string SourceItem { get; init; }
    
    // Exchange properties
    public bool IsExchange { get; init; }
    public ExchangeData ExchangeDetails { get; init; }
    
    // Letter delivery properties
    public bool CanDeliverLetter { get; init; }
    
    // Promise card properties
    public bool IsPromise { get; init; }
    public PromiseCardData PromiseDetails { get; init; }
    
    // Burden card properties
    public bool IsBurden { get; init; }
    
    // Goal card properties
    public bool IsGoal { get; init; }
    public string GoalContext { get; init; }
    
    public int GetEffectiveWeight(EmotionalState state)
    {
        if (WeightModifiers?.ContainsKey(state) == true)
        {
            return WeightModifiers[state];
        }
        return Weight;
    }
    
    public int GetEffectiveSuccessChance(EmotionalState state)
    {
        var baseChance = BaseSuccessChance;
        if (StateModifiers?.ContainsKey(state) == true)
        {
            baseChance += StateModifiers[state];
        }
        return Math.Clamp(baseChance, 0, 100);
    }
}

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();
    public string TemplateId { get; init; }
    public string Name { get; init; }
    public CardType Type { get; init; }
    public int Weight { get; init; }
    public int BaseSuccessChance { get; init; }
    public int BaseComfortReward { get; init; }
    public string DialogueFragment { get; init; }
    public bool IsSpecial { get; init; }
    public bool IsSingleUse { get; init; }
    public PersistenceType Persistence { get; init; }
    public string VerbPhrase { get; init; }
    public Dictionary<EmotionalState, int> StateModifiers { get; init; }
    public Dictionary<EmotionalState, int> WeightModifiers { get; init; }
    public EmotionalState? TransitionToState { get; init; }
    public int TransitionChance { get; init; }
    public bool IsObservation { get; init; }
    public string ObservationType { get; init; }
    public string SourceItem { get; init; }
    public string SourceContext { get; init; }
    
    // Exchange properties
    public bool IsExchange { get; init; }
    
    // Letter delivery properties
    public bool CanDeliverLetter { get; init; }
    public string DeliveryObligationId { get; init; }
    
    // Special context for runtime behavior
    public CardContext Context { get; set; }
    
    // Burden properties
    public bool IsBurden { get; init; }
    
    // Promise properties
    public bool IsPromise { get; init; }
    
    // Goal properties
    public bool IsGoal { get; init; }
    public string GoalContext { get; init; }
    
    public CardInstance() { }
    
    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        TemplateId = template.Id;
        Name = template.Name;
        Type = template.Type;
        Weight = template.Weight;
        BaseSuccessChance = template.BaseSuccessChance;
        BaseComfortReward = template.BaseComfortReward;
        DialogueFragment = template.DialogueFragment;
        IsSpecial = template.IsSpecial;
        IsSingleUse = template.IsSingleUse;
        Persistence = template.Persistence;
        VerbPhrase = template.VerbPhrase;
        StateModifiers = template.StateModifiers ?? new Dictionary<EmotionalState, int>();
        WeightModifiers = template.WeightModifiers ?? new Dictionary<EmotionalState, int>();
        TransitionToState = template.TransitionToState;
        TransitionChance = template.TransitionChance;
        IsObservation = template.IsObservation;
        ObservationType = template.ObservationType;
        SourceItem = template.SourceItem;
        SourceContext = sourceContext;
        IsExchange = template.IsExchange;
        CanDeliverLetter = template.CanDeliverLetter;
        IsBurden = template.IsBurden;
        IsPromise = template.IsPromise;
        IsGoal = template.IsGoal;
        GoalContext = template.GoalContext;
        
        // Set context for special cards
        if (template.IsExchange && template.ExchangeDetails != null)
        {
            Context = new CardContext
            {
                ExchangeData = template.ExchangeDetails
            };
        }
        
        if (template.IsPromise && template.PromiseDetails != null)
        {
            Context = new CardContext
            {
                PromiseData = template.PromiseDetails
            };
        }
    }
    
    public int GetEffectiveWeight(EmotionalState state)
    {
        if (WeightModifiers?.ContainsKey(state) == true)
        {
            return WeightModifiers[state];
        }
        return Weight;
    }
    
    public int GetEffectiveSuccessChance(EmotionalState state)
    {
        var baseChance = BaseSuccessChance;
        if (StateModifiers?.ContainsKey(state) == true)
        {
            baseChance += StateModifiers[state];
        }
        return Math.Clamp(baseChance, 0, 100);
    }
}

// Card context for special behaviors
public class CardContext
{
    public ExchangeData ExchangeData { get; set; }
    public PromiseCardData PromiseData { get; set; }
    public bool GeneratesLetterOnSuccess { get; set; }
}

// Exchange data for commerce cards
public class ExchangeData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, int> PlayerGives { get; set; } = new();
    public Dictionary<string, int> PlayerReceives { get; set; } = new();
    public int TrustRequirement { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool SingleUse { get; set; }
}

// Promise card data
public class PromiseCardData
{
    public string CardId { get; set; }
    public TermDetails SuccessTerms { get; set; }
    public TermDetails FailureTerms { get; set; }
    public int NegotiationDifficulty { get; set; }
    public ConnectionType TokenType { get; set; }
    public string Destination { get; set; }
    public string RecipientName { get; set; }
}

// TermDetails is defined in LetterDeckRepository.cs

// Observation card
public class ObservationCard : ConversationCard
{
    public string ObservationId { get; init; }
    public string ItemName { get; init; }
    public string LocationDiscovered { get; init; }
    public string TimeDiscovered { get; init; }
    
    public ObservationCard()
    {
        Type = CardType.Observation;
        IsObservation = true;
        Persistence = PersistenceType.Fleeting;
        IsSingleUse = true;
    }
}

// Card play results
public class CardPlayResult
{
    public int TotalComfort { get; init; }
    public EmotionalState? NewState { get; init; }
    public List<SingleCardResult> Results { get; init; }
    public int SetBonus { get; init; }
    public int ConnectedBonus { get; init; }
    public int EagerBonus { get; init; }
    public bool DeliveredLetter { get; init; }
    public bool ManipulatedObligations { get; init; }
    public List<LetterNegotiationResult> LetterNegotiations { get; init; } = new List<LetterNegotiationResult>();
    public bool Success => Results?.Any(r => r.Success) ?? false;
}

public class SingleCardResult
{
    public CardInstance Card { get; init; }
    public bool Success { get; init; }
    public int Comfort { get; init; }
    public int Roll { get; init; }
    public int SuccessChance { get; init; }
    public int PatienceAdded { get; init; }
}

public class LetterNegotiationResult
{
    public string PromiseCardId { get; init; }
    public bool NegotiationSuccess { get; init; }
    public TermDetails FinalTerms { get; init; }
    public CardInstance SourcePromiseCard { get; init; }
    public DeliveryObligation CreatedObligation { get; init; }
}

// Conversation session
public class ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public NPC NPC { get; set; }
    public ConversationType ConversationType { get; set; }
    public EmotionalState CurrentState { get; set; }
    public EmotionalState InitialState { get; set; }
    public int CurrentComfort { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool GoalCardDrawn { get; set; }
    public int GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public SessionCardDeck Deck { get; set; }
    public HandDeck Hand { get; set; }
    public HashSet<CardInstance> HandCards => Hand?.Cards ?? new HashSet<CardInstance>();
    public List<CardInstance> PlayedCards { get; set; } = new();
    public List<CardInstance> DiscardedCards { get; set; } = new();
    public TokenMechanicsManager TokenManager { get; set; }
    
    public bool ShouldEnd()
    {
        return CurrentPatience <= 0 || CurrentComfort >= 100;
    }
    
    public ConversationOutcome CheckThresholds()
    {
        if (CurrentComfort >= 100)
        {
            return new ConversationOutcome
            {
                Success = true,
                FinalComfort = CurrentComfort,
                FinalState = CurrentState,
                TokensEarned = CalculateTokenReward(),
                Reason = "Comfort threshold reached"
            };
        }
        
        if (CurrentPatience <= 0)
        {
            return new ConversationOutcome
            {
                Success = false,
                FinalComfort = CurrentComfort,
                FinalState = CurrentState,
                TokensEarned = 0,
                Reason = "Patience exhausted"
            };
        }
        
        return null;
    }
    
    private int CalculateTokenReward()
    {
        if (CurrentComfort >= 100) return 3;
        if (CurrentComfort >= 75) return 2;
        if (CurrentComfort >= 50) return 1;
        return 0;
    }
    
    public void ExecuteListen(TokenMechanicsManager tokenManager, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Implementation handled by ConversationOrchestrator
    }
    
    public CardPlayResult ExecuteSpeak(HashSet<CardInstance> selectedCards)
    {
        // Implementation handled by ConversationOrchestrator
        return new CardPlayResult
        {
            TotalComfort = 0,
            Results = new List<SingleCardResult>()
        };
    }
}

// Conversation outcome
public class ConversationOutcome
{
    public bool Success { get; set; }
    public int FinalComfort { get; set; }
    public EmotionalState FinalState { get; set; }
    public int TokensEarned { get; set; }
    public string Reason { get; set; }
}

// Card decks
public class CardDeck
{
    protected List<ConversationCard> cards = new();
    protected HashSet<string> drawnCardIds = new();
    
    public int RemainingCards => cards.Count(c => !drawnCardIds.Contains(c.Id));
    
    public void AddCard(ConversationCard card)
    {
        cards.Add(card);
    }
    
    public void AddCards(IEnumerable<ConversationCard> newCards)
    {
        cards.AddRange(newCards);
    }
    
    public ConversationCard DrawCard()
    {
        var available = cards.Where(c => !drawnCardIds.Contains(c.Id)).ToList();
        if (!available.Any()) return null;
        
        var card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.Id);
        return card;
    }
    
    public List<ConversationCard> DrawCards(int count)
    {
        var drawn = new List<ConversationCard>();
        for (int i = 0; i < count; i++)
        {
            var card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }
    
    public void Reset()
    {
        drawnCardIds.Clear();
    }
    
    public List<ConversationCard> GetAllCards()
    {
        return cards.ToList();
    }
    
    public bool HasCards()
    {
        return RemainingCards > 0;
    }
}

public class SessionCardDeck
{
    private readonly List<CardInstance> allCards = new();
    private readonly HashSet<string> drawnCardIds = new();
    private readonly HashSet<string> discardedCardIds = new();
    private readonly string npcId;
    
    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }
    
    public static SessionCardDeck CreateFromTemplates(List<ConversationCard> templates, string npcId)
    {
        var deck = new SessionCardDeck(npcId);
        foreach (var template in templates)
        {
            deck.AddCard(new CardInstance(template));
        }
        return deck;
    }
    
    public void AddCard(CardInstance card)
    {
        allCards.Add(card);
    }
    
    public CardInstance DrawCard()
    {
        var available = allCards.Where(c => !drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId)).ToList();
        if (!available.Any()) return null;
        
        var card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.InstanceId);
        return card;
    }
    
    public List<CardInstance> DrawCards(int count)
    {
        var drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            var card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }
    
    public void DiscardCard(string instanceId)
    {
        discardedCardIds.Add(instanceId);
    }
    
    public void ResetForNewConversation()
    {
        drawnCardIds.Clear();
        // Keep discarded cards discarded (single-use cards)
    }
    
    public List<CardInstance> GetAllCards()
    {
        return allCards.ToList();
    }
    
    public int RemainingCards => allCards.Count(c => !drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId));
}

public class HandDeck
{
    public HashSet<CardInstance> Cards { get; } = new();
    
    public void AddCard(CardInstance card)
    {
        Cards.Add(card);
    }
    
    public void AddCards(IEnumerable<CardInstance> cards)
    {
        foreach (var card in cards)
        {
            Cards.Add(card);
        }
    }
    
    public void RemoveCard(CardInstance card)
    {
        Cards.Remove(card);
    }
    
    public void Clear()
    {
        Cards.Clear();
    }
    
    public bool HasCards()
    {
        return Cards.Any();
    }
}

// Conversation context for UI
public class ConversationContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }
    public ConversationType Type { get; set; }
    public EmotionalState InitialState { get; set; }
    public ConversationSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public int AttentionSpent { get; set; }
}

// Conversation action
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

// Result of processing a conversation turn
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

// Player's current resource state
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

// Resource exchange for trade
public class ResourceExchange
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<ResourceType, int> PlayerGives { get; set; } = new();
    public Dictionary<ResourceType, int> PlayerReceives { get; set; } = new();
    public int RequiredTrust { get; set; }
    public bool SingleUse { get; set; }
}

// Conversation memento for save/load
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
    public int GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public List<string> HandCardIds { get; set; }
    public List<string> DeckCardIds { get; set; }
}