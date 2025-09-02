using System;
using System.Collections.Generic;

public class ConversationCard
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Properties list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new List<CardProperty>();
    
    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton
    public TokenType TokenType { get; set; }
    public int Weight { get; set; }
    public Difficulty Difficulty { get; set; }
    public CardEffectType EffectType { get; set; }
    public int EffectValue { get; set; } // For fixed effects
    public string ScalingFormula { get; set; } // For scaled effects
    public AtmosphereType? AtmosphereChange { get; set; }

    // Display properties
    public string DialogueFragment { get; set; }
    public string VerbPhrase { get; set; }

    // Helper properties that use Properties list as source of truth
    public bool IsFleeting => Properties.Contains(CardProperty.Fleeting);
    public bool IsOpportunity => Properties.Contains(CardProperty.Opportunity);
    public bool IsPersistent => !Properties.Contains(CardProperty.Fleeting) 
                                && !Properties.Contains(CardProperty.Opportunity);
    public bool IsGoal => Properties.Contains(CardProperty.Fleeting) 
                          && Properties.Contains(CardProperty.Opportunity);
    public bool IsBurden => Properties.Contains(CardProperty.Burden);
    public bool IsObservable => Properties.Contains(CardProperty.Observable);
    public bool IsObservation => IsObservable; // Backward compatibility alias
    public bool HasFinalWord => IsGoal; // Goal cards have "final word" behavior
    public bool IsGoalCard => IsGoal; // Backward compatibility alias

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
    // Removed - now use IsObservable helper property
    // public bool IsObservation { get; set; }  // Now: IsObservable => Properties.Contains(CardProperty.Observable)
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
    // Removed - now use helper properties above
    // public bool IsBurden { get; set; }  // Now: IsBurden => Properties.Contains(CardProperty.Burden)
    // public bool IsGoal { get; set; }    // Now: IsGoal => Properties.Contains(Fleeting) && Properties.Contains(Opportunity)
    public string GoalContext { get; set; }
    public string DialogueText { get; set; }
    // Removed - use IsGoal helper property instead
    // public bool IsGoalCard { get; set; }  // Now: IsGoal => Properties.Contains(Fleeting) && Properties.Contains(Opportunity)
    public bool IsObservationCard => Type == CardType.Observation || IsObservable;
    public Difficulty Difficulty_Legacy { get; set; } = Difficulty.Medium;
    public string EffectFormula { get; set; }
    public AtmosphereType? ConversationAtmosphereChange => AtmosphereChange;

    // Helper method to check single effect
    // Compatibility method to ensure default Persistent property
    public void EnsureDefaultProperties()
    {
        // If no exhaustion properties set, default to Persistent
        if (!Properties.Contains(CardProperty.Fleeting) && 
            !Properties.Contains(CardProperty.Opportunity) && 
            !Properties.Contains(CardProperty.Persistent))
        {
            Properties.Add(CardProperty.Persistent);
        }
    }

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
            Properties = new List<CardProperty>(this.Properties), // Clone the properties list
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            TokenType = this.TokenType,
            Weight = this.Weight,
            Difficulty = this.Difficulty,
            EffectType = this.EffectType,
            EffectValue = this.EffectValue,
            ScalingFormula = this.ScalingFormula,
            AtmosphereChange = this.AtmosphereChange,
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