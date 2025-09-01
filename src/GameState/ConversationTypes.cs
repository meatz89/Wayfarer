using System;
using System.Collections.Generic;
using System.Linq;

// Core conversation enums and types

public enum ResourceType
{
    Coins,
    Health,
    Hunger,
    Food,
    Attention,
    TrustToken,
    CommerceToken,
    StatusToken,
    ShadowToken,
    Item
}

public enum ConversationType
{
    FriendlyChat,
    Commerce,
    Promise,
    Resolution,
    Delivery
}

// Five core emotional states only
public enum EmotionalState
{
    DESPERATE,
    TENSE,
    NEUTRAL,
    OPEN,
    CONNECTED
}

// New enums for target system
public enum Difficulty
{
    Easy,     // 70% base success
    Medium,   // 60% base success  
    Hard,     // 50% base success
    VeryHard  // 40% base success
}

public enum ConversationAtmosphere
{
    Neutral,      // No effect (default)
    Prepared,     // +1 weight capacity
    Receptive,    // +1 card on LISTEN
    Focused,      // +20% success
    Patient,      // 0 patience cost
    Volatile,     // ±1 comfort changes
    Final,        // Failure ends conversation
    Informed,     // Next card auto-succeeds (observation only)
    Exposed,      // Double comfort changes (observation only)
    Synchronized, // Next effect happens twice (observation only)
    Pressured     // -1 card on LISTEN (observation only)
}

public enum CardEffectType
{
    FixedComfort,
    ScaledComfort,
    DrawCards,
    AddWeight,
    SetConversationAtmosphere,
    ResetComfort,     // Observation only
    MaxWeight,        // Observation only
    FreeAction        // Observation only
}

public enum CardType
{
    Normal,       // Standard conversation cards
    Observation,  // Special observation cards
    Goal          // Goal cards with Final Word
}

public enum PersistenceType
{
    Fleeting,   // Removed after SPEAK
    Persistent  // Stays in deck
}

public enum ActionType
{
    None,
    Listen,
    Speak
}

public enum CardCategory
{
    Standard,
    Exchange,
    Promise,
    StateChange,
    Comfort,
    State,
    Burden,
    Token,
    Patience
}

public enum CardMechanicsType
{
    Standard,
    Exchange,
    Promise,
    StateChange
}

public enum ObservationDecayState
{
    Fresh,
    Aging,
    Stale,
    Expired
}

// Core conversation card definition
public class ConversationCard
{
    public string Id { get; init; }
    public string TemplateId { get; set; }
    public string Name { get; init; }
    public CardType Type { get; init; }
    public int Weight { get; init; }
    public int BaseSuccessChance { get; init; }
    public int BaseComfortReward { get; init; }
    public string DialogueFragment { get; init; }
    public bool IsSpecial { get; init; }
    public bool IsSingleUse { get; init; }
    public PersistenceType Persistence { get; init; } = PersistenceType.Persistent;
    public string VerbPhrase { get; init; }
    public Dictionary<EmotionalState, int> StateModifiers { get; init; } = new();
    public Dictionary<EmotionalState, int> WeightModifiers { get; init; } = new();
    public EmotionalState? TransitionToState { get; init; }
    public int TransitionChance { get; init; }
    public string ConversationType { get; init; }
    public bool IsObservation { get; init; }
    public string ObservationType { get; init; }
    public string SourceItem { get; init; }
    
    // New properties for target system
    public Difficulty Difficulty { get; init; } = Difficulty.Medium;
    public CardEffectType EffectType { get; init; } = CardEffectType.FixedComfort;
    public string EffectValue { get; init; }
    public string EffectFormula { get; init; }
    public ConversationAtmosphere? ConversationAtmosphereChange { get; init; }
    public CardMechanicsType Mechanics { get; set; }
    public string Category { get; set; }
    public CardContext Context { get; set; }
    public int BaseComfort { get; set; }
    public string GoalCardType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int SuccessRate { get; set; }
    public EmotionalState? SuccessState { get; set; }
    public EmotionalState? FailureState { get; set; }
    public List<EmotionalState> DrawableStates { get; set; }
    public int PatienceBonus { get; set; }
    
    // Missing properties from old system
    public bool IsStateCard { get; set; }
    public bool GrantsToken { get; set; }
    public string ObservationSource { get; set; }
    public string DeliveryObligationId { get; set; }
    public bool ManipulatesObligations { get; set; }
    
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
    
    // New methods for target system
    public int GetBaseSuccessPercentage()
    {
        return Difficulty switch
        {
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }
    
    public string GetEffectValueOrFormula()
    {
        return string.IsNullOrEmpty(EffectFormula) ? EffectValue : EffectFormula;
    }
    
    public bool IsFleeting => Persistence == PersistenceType.Fleeting;
    public bool IsGoalCard { get; set; }
    public bool IsObservationCard => Type == CardType.Observation;
    
    // Legacy method compatibility - will be removed
    public int GetEffectiveWeight(EmotionalState state)
    {
        if (WeightModifiers?.ContainsKey(state) == true)
        {
            return WeightModifiers[state];
        }
        return Weight;
    }
    
    // Legacy method compatibility - will be removed
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
    
    // Additional properties needed by other classes
    public string Category { get; init; }
    public bool IsGoalCard { get; init; }
    public int BaseComfort { get; init; }
    public string Description { get; init; }
    public EmotionalState? SuccessState { get; init; }
    public EmotionalState? FailureState { get; init; }
    public CardMechanicsType Mechanics { get; init; }
    public string ObservationSource { get; init; }
    public string DisplayName { get; init; }
    public List<EmotionalState> DrawableStates { get; init; } = new List<EmotionalState>();
    
    // Convenience property for fleeting cards
    public bool IsFleeting => Persistence == PersistenceType.Fleeting;
    
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
        Category = template.Category;
        IsGoalCard = template.IsGoalCard;
        BaseComfort = template.BaseComfort;
        Description = template.Description;
        SuccessState = template.SuccessState;
        FailureState = template.FailureState;
        Mechanics = template.Mechanics;
        ObservationSource = template.ObservationSource;
        DisplayName = template.DisplayName ?? template.Name;
        DrawableStates = template.DrawableStates ?? new List<EmotionalState>();
        
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
    
    public string GetCategoryClass()
    {
        return $"card-{Category?.ToLower() ?? "standard"}";
    }
    
    public int CalculateSuccessChance(EmotionalState currentState = EmotionalState.NEUTRAL)
    {
        return GetEffectiveSuccessChance(currentState);
    }
    
    public int CalculateSuccessChance(TokenMechanicsManager tokenManager)
    {
        // Token-based calculation if needed
        return BaseSuccessChance;
    }
    
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens)
    {
        // Dictionary-based calculation for UI compatibility
        return BaseSuccessChance;
    }
    
    public int CalculateSuccessChance()
    {
        // Parameterless version for simple calls
        return BaseSuccessChance;
    }
    
    public ConnectionType GetConnectionType()
    {
        // For the new simplified system, determine connection type based on card properties
        // This method is kept for compatibility but should not be used in the new system
        return ConnectionType.Trust; // Default - token type now comes from explicit mechanics
    }
}

// Card mechanics
public class CardMechanics
{
    public int SuccessChance { get; set; }
    public int ComfortReward { get; set; }
    public Dictionary<EmotionalState, int> StateModifiers { get; set; } = new();
}

// Card context for special behaviors
public class CardContext
{
    public ExchangeData ExchangeData { get; set; }
    public PromiseCardData PromiseData { get; set; }
    public bool GeneratesLetterOnSuccess { get; set; }
    
    // Additional context properties  
    public string ExchangeRequest { get; set; }
    public string ObservationLocation { get; set; }
    public string ObservationSpot { get; set; }
    public string NPCName { get; set; }
    public PersonalityType Personality { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public EmotionalState EmotionalState { get; set; }
    public string UrgencyLevel { get; set; }
    public bool HasDeadline { get; set; }
    public int MinutesUntilDeadline { get; set; }
    public string ObservationType { get; set; }
    public string LetterId { get; set; }
    public string TargetNpcId { get; set; }
    public string ObservationId { get; set; }
    public string ObservationText { get; set; }
    public string ObservationDescription { get; set; }
    public string ExchangeName { get; set; }
    public Dictionary<ResourceType, int> ExchangeCost { get; set; }
    public Dictionary<ResourceType, int> ExchangeReward { get; set; }
    public string ObservationDecayState { get; set; }
    public string ObservationDecayDescription { get; set; }
    public bool IsOfferCard { get; set; }
    public bool GrantsToken { get; set; }
    public bool IsAcceptCard { get; set; }
    public bool IsDeclineCard { get; set; }
    public string OfferCardId { get; set; }
    public string CustomText { get; set; }
    public LetterDetails LetterDetails { get; set; }
    public List<EmotionalState> ValidStates { get; set; }
    public ExchangeOffer ExchangeOffer { get; set; }
}

// Exchange data for commerce cards
public class ExchangeData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ExchangeName { get; set; }
    public string Description { get; set; }
    public Dictionary<string, int> PlayerGives { get; set; } = new();
    public Dictionary<string, int> PlayerReceives { get; set; } = new();
    public Dictionary<ResourceType, int> Cost { get; set; } = new();
    public Dictionary<ResourceType, int> Reward { get; set; } = new();
    public int TrustRequirement { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool SingleUse { get; set; }
    
    // Additional properties used in CardDeckLoader
    public PersonalityType NPCPersonality { get; set; }
    public int BaseSuccessRate { get; set; }
    public bool CanBarter { get; set; }
    public string TemplateId { get; set; }
    
    public bool CanAfford(Player player)
    {
        foreach (var cost in Cost)
        {
            switch (cost.Key)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Value) return false;
                    break;
                case ResourceType.Health:
                    if (player.Health < cost.Value) return false;
                    break;
                case ResourceType.Food:
                    if (player.Food < cost.Value) return false;
                    break;
            }
        }
        return true;
    }
    
    public bool CanAfford(PlayerResourceState playerResources, TokenMechanicsManager tokenManager, int currentAttention)
    {
        // Check if player can afford the exchange
        if (playerResources == null) return false;
        
        foreach (var cost in Cost)
        {
            switch (cost.Key)
            {
                case ResourceType.Coins:
                    if (playerResources.Coins < cost.Value) return false;
                    break;
                case ResourceType.Health:
                    if (playerResources.Health < cost.Value) return false;
                    break;
                case ResourceType.Food:
                    if (playerResources.Stamina < cost.Value) return false;
                    break;
                case ResourceType.Attention:
                    if (currentAttention < cost.Value) return false;
                    break;
            }
        }
        return true;
    }
    
    public string GetNarrativeContext()
    {
        var givesList = new List<string>();
        foreach (var kv in PlayerGives)
        {
            givesList.Add($"{kv.Value} {kv.Key}");
        }
        
        var receivesList = new List<string>();
        foreach (var kv in PlayerReceives)
        {
            receivesList.Add($"{kv.Value} {kv.Key}");
        }
        
        return $"Trading {string.Join(", ", givesList)} for {string.Join(", ", receivesList)}";
    }
    
    public List<ResourceExchange> GetCostAsList()
    {
        var list = new List<ResourceExchange>();
        foreach (var kv in Cost)
        {
            list.Add(new ResourceExchange { ResourceType = kv.Key, Amount = kv.Value });
        }
        return list;
    }
    
    public List<ResourceExchange> GetRewardAsList()
    {
        var list = new List<ResourceExchange>();
        foreach (var kv in Reward)
        {
            list.Add(new ResourceExchange { ResourceType = kv.Key, Amount = kv.Value });
        }
        return list;
    }
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
    
    // Additional properties needed by PlayerObservationDeck
    public DateTime CreatedAt { get; set; }
    public bool IsPlayable { get; set; } = true;
    public ConversationCard ConversationCard { get; set; }
    
    public ObservationCard()
    {
        Type = CardType.Observation;
        IsObservation = true;
        Persistence = PersistenceType.Fleeting;
        IsSingleUse = true;
    }
    
    public static ObservationCard FromConversationCard(ConversationCard card)
    {
        return new ObservationCard
        {
            Id = card.Id,
            Name = card.Name,
            ObservationId = card.ObservationSource ?? card.Id,
            ItemName = card.SourceItem,
            LocationDiscovered = card.Context?.ObservationLocation,
            TimeDiscovered = DateTime.Now.ToString(),
            DialogueFragment = card.DialogueFragment,
            Weight = card.Weight,
            BaseSuccessChance = card.BaseSuccessChance,
            BaseComfortReward = card.BaseComfortReward,
            CreatedAt = DateTime.Now,
            ConversationCard = card
        };
    }
    
    public void UpdateDecayState(DateTime currentGameTime)
    {
        // Simple decay based on creation time
        var age = currentGameTime - CreatedAt;
        if (age.TotalHours > 24)
        {
            IsPlayable = false;
        }
    }
    
    public void UpdateDecayState()
    {
        // Overload with current time
        UpdateDecayState(DateTime.Now);
    }
    
    public string GetDecayStateDescription()
    {
        var age = DateTime.Now - CreatedAt;
        if (age.TotalHours < 1) return "Fresh";
        if (age.TotalHours < 6) return "Recent";
        if (age.TotalHours < 24) return "Aging";
        return "Stale";
    }
    
    public string GetDecayStateDescription(DateTime currentTime)
    {
        var age = currentTime - CreatedAt;
        if (age.TotalHours < 1) return "Fresh";
        if (age.TotalHours < 6) return "Recent";
        if (age.TotalHours < 24) return "Aging";
        return "Stale";
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
    public bool Success 
    { 
        get 
        { 
            if (Results == null) return false;
            foreach (var r in Results)
            {
                if (r.Success) return true;
            }
            return false;
        } 
    }
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
    public int? GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public SessionCardDeck Deck { get; set; }
    public HandDeck Hand { get; set; }
    public HashSet<CardInstance> HandCards 
    { 
        get 
        { 
            if (Hand?.Cards != null) return Hand.Cards;
            return new HashSet<CardInstance>();
        } 
    }
    public List<CardInstance> PlayedCards { get; set; } = new();
    public List<CardInstance> DiscardedCards { get; set; } = new();
    public TokenMechanicsManager TokenManager { get; set; }
    
    // New weight pool and atmosphere system
    public int ComfortBattery { get; set; } = 0; // -3 to +3
    public int CurrentWeightPool { get; set; } = 0; // Current spent weight
    public int WeightCapacity { get; set; } = 5; // Based on state
    public ConversationAtmosphere CurrentAtmosphere { get; set; } = ConversationAtmosphere.Neutral;
    
    // Legacy properties for compatibility
    public CardInstance GoalCard { get; set; }
    public List<CardInstance> ObservationCards { get; set; } = new();
    
    // New helper methods
    public int GetAvailableWeight() => Math.Max(0, GetEffectiveWeightCapacity() - CurrentWeightPool);
    
    public int GetEffectiveWeightCapacity()
    {
        int baseCapacity = CurrentState switch
        {
            EmotionalState.DESPERATE => 3,
            EmotionalState.TENSE => 4,
            EmotionalState.NEUTRAL => 5,
            EmotionalState.OPEN => 5,
            EmotionalState.CONNECTED => 6,
            _ => 5
        };
        
        // Prepared atmosphere adds +1 capacity
        if (CurrentAtmosphere == ConversationAtmosphere.Prepared)
            baseCapacity += 1;
            
        return baseCapacity;
    }
    
    public int GetDrawCount()
    {
        int baseCount = CurrentState switch
        {
            EmotionalState.DESPERATE => 1,
            EmotionalState.TENSE => 2,
            EmotionalState.NEUTRAL => 2,
            EmotionalState.OPEN => 3,
            EmotionalState.CONNECTED => 3,
            _ => 2
        };
        
        // ConversationAtmosphere modifiers
        if (CurrentAtmosphere == ConversationAtmosphere.Receptive)
            baseCount += 1;
        else if (CurrentAtmosphere == ConversationAtmosphere.Pressured)
            baseCount = Math.Max(1, baseCount - 1);
            
        return baseCount;
    }
    
    public void RefreshWeightPool()
    {
        CurrentWeightPool = 0;
        WeightCapacity = GetEffectiveWeightCapacity();
    }
    
    public bool IsHandOverflowing()
    {
        return HandCards.Count > 10; // Simplified overflow check
    }
    
    public bool ShouldEnd()
    {
        // End if patience exhausted or at desperate with -3 comfort
        return CurrentPatience <= 0 || (CurrentState == EmotionalState.DESPERATE && ComfortBattery <= -3);
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
        
        // Conversation ended normally without hitting thresholds
        return new ConversationOutcome
        {
            Success = true,
            FinalComfort = CurrentComfort,
            FinalState = CurrentState,
            TokensEarned = CalculateTokenReward(),
            Reason = "Conversation ended"
        };
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
    
    public static ConversationSession StartConversation(NPC npc, ObligationQueueManager queueManager, TokenMechanicsManager tokenManager, 
        List<CardInstance> observationCards, ConversationType conversationType, PlayerResourceState playerResourceState, GameWorld gameWorld)
    {
        // Use properly typed parameters
        var obsCards = observationCards ?? new List<CardInstance>();
        var convType = conversationType;
        var world = gameWorld;
        
        // Determine initial state
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        
        // Create session deck from NPC's conversation cards
        var sessionDeck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck?.GetAllCards() ?? new List<ConversationCard>(), npc.ID);
        
        // Add observation cards if provided
        foreach (var obsCard in obsCards)
        {
            sessionDeck.AddCard(obsCard);
        }
        
        // Create session with proper initialization
        var session = new ConversationSession
        {
            NPC = npc,
            ConversationType = convType,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentComfort = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager,
            ObservationCards = obsCards
        };
        
        return session;
    }
    
    public static ConversationSession StartExchange(NPC npc, PlayerResourceState playerResourceState, TokenMechanicsManager tokenManager,
        List<string> spotDomainTags, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Create session deck from NPC's exchange cards
        var exchangeCards = npc.ExchangeDeck?.GetAllCards() ?? new List<ConversationCard>();
        var sessionDeck = SessionCardDeck.CreateFromTemplates(exchangeCards, npc.ID);
        
        // Determine initial state  
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        
        var session = new ConversationSession
        {
            NPC = npc,
            ConversationType = ConversationType.Commerce,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentComfort = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager as TokenMechanicsManager
        };
        
        return session;
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
    public bool GoalAchieved { get; set; }
    public int TotalComfort { get { return FinalComfort; } } // Alias for compatibility
}

// Card decks
public class CardDeck
{
    protected List<ConversationCard> cards = new();
    protected HashSet<string> drawnCardIds = new();
    
    public int RemainingCards 
    { 
        get 
        {
            int count = 0;
            foreach (var c in cards)
            {
                if (!drawnCardIds.Contains(c.Id)) count++;
            }
            return count;
        }
    }
    public int Count { get { return cards.Count; } }
    
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
        var available = new List<ConversationCard>();
        foreach (var c in cards)
        {
            if (!drawnCardIds.Contains(c.Id)) available.Add(c);
        }
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
    
    public bool Any()
    {
        return cards.Any();
    }
    
    public bool HasCardsAvailable()
    {
        return HasCards();
    }
    
    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        // Card decks are initialized from JSON
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
        var available = new List<CardInstance>();
        foreach (var c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                available.Add(c);
        }
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
    
    public void Discard(CardInstance card)
    {
        discardedCardIds.Add(card.InstanceId);
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
    
    public void Shuffle()
    {
        // Shuffle is handled by randomized drawing
    }
    
    public List<CardInstance> DrawFilteredByCategory(string category, int count)
    {
        var available = new List<CardInstance>();
        foreach (var c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && 
                !discardedCardIds.Contains(c.InstanceId) &&
                c.Category == category)
            {
                available.Add(c);
            }
        }
        
        var drawn = new List<CardInstance>();
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            var card = available[new Random().Next(available.Count)];
            drawnCardIds.Add(card.InstanceId);
            drawn.Add(card);
        }
        return drawn;
    }
    
    public int RemainingCards 
    { 
        get 
        {
            int count = 0;
            foreach (var c in allCards)
            {
                if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId)) 
                    count++;
            }
            return count;
        }
    }
    
    public bool HasCardsAvailable()
    {
        return RemainingCards > 0;
    }
    
    public void Clear()
    {
        allCards.Clear();
        drawnCardIds.Clear();
        discardedCardIds.Clear();
    }
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
    
    public void RemoveCards(IEnumerable<CardInstance> cardsToRemove)
    {
        foreach (var card in cardsToRemove)
        {
            Cards.Remove(card);
        }
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
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
    public List<DeliveryObligation> LettersCarriedForNpc { get; set; }
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
    
    public static ResourceState FromPlayerResourceState(PlayerResourceState playerState)
    {
        return new ResourceState
        {
            Coins = playerState.Coins,
            Health = playerState.Health,
            Hunger = 10 - playerState.Stamina, // Map stamina to hunger inversely
            Food = playerState.Stamina, // Map stamina to food
            Attention = playerState.Concentration, // Map concentration to attention
            Tokens = new Dictionary<ConnectionType, int>()
        };
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
    
    // Additional properties used in CardDeckLoader
    public ResourceType ResourceType { get; set; }
    public int Amount { get; set; }
    public bool IsAbsolute { get; set; }
}

// LetterDetails is defined in LetterDeckRepository.cs

// Exchange offer
public class ExchangeOffer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<ResourceType, int> Cost { get; set; }
    public Dictionary<ResourceType, int> Reward { get; set; }
}

// Goal card factory
public static class GoalCardFactory
{
    public static void Initialize(GameWorld gameWorld)
    {
        // Goal cards are loaded from JSON
    }
    
    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        // Goal cards are loaded from JSON
    }
}

// Conversation state rules
public class ConversationStateRules
{
    public string Description { get; set; }
    public int CardsOnListen { get; set; }
    public int MaxWeight { get; set; }
    public bool ChecksGoalDeck { get; set; }
    public int ComfortThreshold { get; set; }
    public int PatienceReduction { get; set; }
    public int AutoAdvanceDepth { get; set; }
    public EmotionalState? ListenTransition { get; set; }
    public bool ListenEndsConversation { get; set; }
    
    public ConversationStateRules(string description, int cardsOnListen, int maxWeight, bool checksGoalDeck, int comfortThreshold, int patienceReduction, int autoAdvanceDepth, EmotionalState? listenTransition, bool listenEndsConversation)
    {
        Description = description;
        CardsOnListen = cardsOnListen;
        MaxWeight = maxWeight;
        ChecksGoalDeck = checksGoalDeck;
        ComfortThreshold = comfortThreshold;
        PatienceReduction = patienceReduction;
        AutoAdvanceDepth = autoAdvanceDepth;
        ListenTransition = listenTransition;
        ListenEndsConversation = listenEndsConversation;
    }
}

// Conversation rules for new 5-state system
public static class ConversationRules
{
    public static Dictionary<EmotionalState, ConversationStateRules> States = new Dictionary<EmotionalState, ConversationStateRules>
    {
        { EmotionalState.DESPERATE, new ConversationStateRules("Desperate - conversation ends at -3 comfort", 1, 3, true, -3, 1, 0, null, true) },
        { EmotionalState.TENSE, new ConversationStateRules("Tense and anxious", 2, 4, false, 0, 1, 0, null, false) },
        { EmotionalState.NEUTRAL, new ConversationStateRules("Balanced starting state", 2, 5, false, 0, 1, 0, null, false) },
        { EmotionalState.OPEN, new ConversationStateRules("Open and receptive", 3, 5, false, 0, 1, 0, null, false) },
        { EmotionalState.CONNECTED, new ConversationStateRules("Maximum positive connection", 3, 6, false, 0, 1, 0, null, false) }
    };
    
    public static EmotionalState DetermineInitialState(NPC npc, ObligationQueueManager queueManager = null)
    {
        // All conversations start in NEUTRAL
        return EmotionalState.NEUTRAL;
    }
    
    public static EmotionalState TransitionState(EmotionalState current, int comfortChange)
    {
        // Linear progression: DESPERATE ← TENSE ← NEUTRAL → OPEN → CONNECTED
        if (comfortChange >= 3)
        {
            return current switch
            {
                EmotionalState.DESPERATE => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.CONNECTED,
                EmotionalState.CONNECTED => EmotionalState.CONNECTED, // Stay at max
                _ => EmotionalState.NEUTRAL
            };
        }
        else if (comfortChange <= -3)
        {
            return current switch
            {
                EmotionalState.CONNECTED => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.DESPERATE,
                EmotionalState.DESPERATE => EmotionalState.DESPERATE, // Stay - ends conversation
                _ => EmotionalState.NEUTRAL
            };
        }
        
        return current; // No transition
    }
    
    public static string GetStateEffects(EmotionalState state)
    {
        return States.ContainsKey(state) ? States[state].Description : "Unknown state";
    }
    
    public static Dictionary<EmotionalState, string> GetAllStateEffects()
    {
        var dict = new Dictionary<EmotionalState, string>();
        foreach (var kvp in States)
        {
            dict[kvp.Key] = kvp.Value.Description;
        }
        return dict;
    }
}

// Conversation type config
public static class ConversationTypeConfig
{
    public static int GetAttentionCost(ConversationType type)
    {
        if (type == ConversationType.FriendlyChat) return 1;
        if (type == ConversationType.Commerce) return 1;
        if (type == ConversationType.Promise) return 2;
        if (type == ConversationType.Resolution) return 2;
        if (type == ConversationType.Delivery) return 1;
        return 1; // Default
    }
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
    public int? GoalUrgencyCounter { get; set; }
    public bool GoalCardPlayed { get; set; }
    public List<string> HandCardIds { get; set; }
    public List<string> DeckCardIds { get; set; }
}

// Extension methods for display
public static class ResourceExtensions
{
    public static string GetDisplayText(this KeyValuePair<ResourceType, int> resourcePair)
    {
        string resourceName;
        if (resourcePair.Key == ResourceType.Coins) resourceName = "coins";
        else if (resourcePair.Key == ResourceType.Health) resourceName = "health";
        else if (resourcePair.Key == ResourceType.Hunger) resourceName = "hunger";
        else if (resourcePair.Key == ResourceType.Food) resourceName = "food";
        else if (resourcePair.Key == ResourceType.Attention) resourceName = "attention";
        else if (resourcePair.Key == ResourceType.TrustToken) resourceName = "trust tokens";
        else if (resourcePair.Key == ResourceType.CommerceToken) resourceName = "commerce tokens";
        else if (resourcePair.Key == ResourceType.StatusToken) resourceName = "status tokens";
        else if (resourcePair.Key == ResourceType.ShadowToken) resourceName = "shadow tokens";
        else if (resourcePair.Key == ResourceType.Item) resourceName = "items";
        else resourceName = resourcePair.Key.ToString().ToLower();
        
        return $"{resourcePair.Value} {resourceName}";
    }
}

// Card selection manager for UI
public class CardSelectionManager
{
    private readonly EmotionalState _currentState;
    private readonly HashSet<CardInstance> _selectedCards = new();
    
    public CardSelectionManager(EmotionalState currentState)
    {
        _currentState = currentState;
    }
    
    public void ToggleCard(CardInstance card)
    {
        if (_selectedCards.Contains(card))
            _selectedCards.Remove(card);
        else
            _selectedCards.Add(card);
    }
    
    public string GetSelectionDescription()
    {
        if (!_selectedCards.Any())
            return "No cards selected";
            
        int totalWeight = 0;
        foreach (var c in _selectedCards)
        {
            totalWeight += c.GetEffectiveWeight(_currentState);
        }
        
        var cardNames = new List<string>();
        foreach (var c in _selectedCards)
        {
            cardNames.Add(c.Name);
        }
        
        return $"Playing {string.Join(", ", cardNames)} (weight: {totalWeight})";
    }
    
    public HashSet<CardInstance> GetSelectedCards()
    {
        return new HashSet<CardInstance>(_selectedCards);
    }
}