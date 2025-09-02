using System;
using System.Collections.Generic;

public class ConversationCard
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton
    public TokenType TokenType { get; set; }
    public int Weight { get; set; }
    public Difficulty Difficulty { get; set; }
    public bool IsFleeting { get; set; }
    public CardEffectType EffectType { get; set; }
    public int EffectValue { get; set; } // For fixed effects
    public string ScalingFormula { get; set; } // For scaled effects
    public AtmosphereType? AtmosphereChange { get; set; }
    public bool HasFinalWord { get; set; } // For goal cards

    // Display properties
    public string DialogueFragment { get; set; }
    public string VerbPhrase { get; set; }

    // Compatibility properties (will be removed later)
    public CardType Type { get; set; } = CardType.Normal;
    public PersistenceType Persistence { get; set; } = PersistenceType.Persistent;
    public ConnectionType ConnectionType { get; set; } = ConnectionType.None;
    public int BaseSuccessChance { get; set; }
    public int BaseComfortReward { get; set; }
    public bool IsSpecial { get; set; }
    public bool IsSingleUse { get; set; }
    public Dictionary<EmotionalState, int> StateModifiers { get; set; } = new();
    public Dictionary<EmotionalState, int> WeightModifiers { get; set; } = new();
    public EmotionalState? TransitionToState { get; set; }
    public int TransitionChance { get; set; }
    public string ConversationType { get; set; }
    public bool IsObservation { get; set; }
    public string ObservationType { get; set; }
    public string SourceItem { get; set; }
    public CardMechanicsType Mechanics { get; set; }
    public string Category { get; set; }
    public CardContext Context { get; set; }
    public int BaseComfort { get; set; }
    public string GoalCardType { get; set; }
    public string DisplayName { get; set; }
    public int SuccessRate { get; set; }
    public EmotionalState? SuccessState { get; set; }
    public EmotionalState? FailureState { get; set; }
    public int PatienceBonus { get; set; }
    public bool IsStateCard { get; set; }
    public bool GrantsToken { get; set; }
    public string ObservationSource { get; set; }
    public string DeliveryObligationId { get; set; }
    public bool ManipulatesObligations { get; set; }
    public bool IsExchange { get; set; }
    public ExchangeData ExchangeDetails { get; set; }
    public bool CanDeliverLetter { get; set; }
    public bool IsPromise { get; set; }
    public PromiseCardData PromiseDetails { get; set; }
    public bool IsBurden { get; set; }
    public bool IsGoal { get; set; }
    public string GoalContext { get; set; }
    public string DialogueText { get; set; }
    public bool IsGoalCard { get; set; }
    public bool IsObservationCard => Type == CardType.Observation;
    public Difficulty Difficulty_Legacy { get; set; } = Difficulty.Medium;
    public string EffectFormula { get; set; }
    public AtmosphereType? ConversationAtmosphereChange => AtmosphereChange;

    // Helper method to check single effect
    public bool HasSingleEffect()
    {
        int effectCount = 0;
        if (EffectValue != 0) effectCount++;
        if (!string.IsNullOrEmpty(ScalingFormula)) effectCount++;
        if (AtmosphereChange.HasValue) effectCount++;
        return effectCount <= 1;
    }

    // Deep clone for deck instances
    public ConversationCard DeepClone()
    {
        return new ConversationCard
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            TokenType = this.TokenType,
            Weight = this.Weight,
            Difficulty = this.Difficulty,
            IsFleeting = this.IsFleeting,
            EffectType = this.EffectType,
            EffectValue = this.EffectValue,
            ScalingFormula = this.ScalingFormula,
            AtmosphereChange = this.AtmosphereChange,
            HasFinalWord = this.HasFinalWord,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase,
            Type = this.Type,
            Persistence = this.Persistence,
            ConnectionType = this.ConnectionType,
            BaseSuccessChance = this.BaseSuccessChance,
            BaseComfortReward = this.BaseComfortReward
        };
    }

    // Get base success percentage from difficulty tier
    public int GetBaseSuccessPercentage()
    {
        // Use DifficultyTier if set, otherwise convert from old Difficulty
        if (Difficulty != default(Difficulty))
            return (int)Difficulty;

        // Fallback to old Difficulty_Legacy
        return Difficulty_Legacy switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }

    // Legacy compatibility methods
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
        int baseChance = BaseSuccessChance;
        if (StateModifiers?.ContainsKey(state) == true)
        {
            baseChance += StateModifiers[state];
        }
        return Math.Clamp(baseChance, 0, 100);
    }

    public string GetEffectValueOrFormula()
    {
        return string.IsNullOrEmpty(EffectFormula) ? EffectValue.ToString() : EffectFormula;
    }

    // Helper methods for TokenType/ConnectionType conversion
    public static TokenType ConvertConnectionToToken(ConnectionType connection)
    {
        return connection switch
        {
            ConnectionType.Trust => TokenType.Trust,
            ConnectionType.Commerce => TokenType.Commerce,
            ConnectionType.Status => TokenType.Status,
            ConnectionType.Shadow => TokenType.Shadow,
            _ => TokenType.Trust
        };
    }

    public static ConnectionType ConvertTokenToConnection(TokenType token)
    {
        return token switch
        {
            TokenType.Trust => ConnectionType.Trust,
            TokenType.Commerce => ConnectionType.Commerce,
            TokenType.Status => ConnectionType.Status,
            TokenType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.None
        };
    }

    // Helper method for Difficulty conversion
    public static Difficulty ConvertDifficulty(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.VeryEasy => Difficulty.VeryEasy,
            Difficulty.Easy => Difficulty.Easy,
            Difficulty.Medium => Difficulty.Medium,
            Difficulty.Hard => Difficulty.Hard,
            Difficulty.VeryHard => Difficulty.VeryHard,
            _ => Difficulty.Medium
        };
    }
}