using System;
using System.Collections.Generic;
using System.Linq;

// Core conversation card definition
public class ConversationCard
{
    public string Id { get; init; }
    public string TemplateId { get; set; }
    public string Name { get; init; }
    public CardType Type { get; init; }
    public ConnectionType TokenType { get; init; }
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
    
    // Missing properties needed by the system
    public bool HasFinalWord { get; init; } = false;
    public string DialogueText { get; init; }
}
