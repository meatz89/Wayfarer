
using System.Collections.Generic;

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    
    // Properties list - copied from template
    public List<CardProperty> Properties { get; init; } = new List<CardProperty>();
    
    // Core mechanics
    public TokenType TokenType { get; init; }
    public int Weight { get; init; }
    public Difficulty Difficulty { get; init; }
    
    // Effects - copied from template
    public CardEffect SuccessEffect { get; init; }
    public CardEffect FailureEffect { get; init; }
    public CardEffect ExhaustEffect { get; init; }
    
    // Display properties
    public string DialogueFragment { get; init; }
    public string VerbPhrase { get; init; }
    
    // Runtime context
    public string SourceContext { get; init; }
    
    // Helper properties that use Properties list
    public bool IsFleeting => Properties.Contains(CardProperty.Fleeting);
    public bool IsOpportunity => Properties.Contains(CardProperty.Opportunity);
    public bool IsPersistent => !Properties.Contains(CardProperty.Fleeting) 
                                && !Properties.Contains(CardProperty.Opportunity);
    public bool IsGoal => Properties.Contains(CardProperty.Fleeting) 
                          && Properties.Contains(CardProperty.Opportunity);
    public bool IsBurden => Properties.Contains(CardProperty.Burden);
    public bool IsObservation => Properties.Contains(CardProperty.Observable);
    
    // Legacy compatibility properties - kept temporarily for UI
    public PersistenceType Persistence => 
        IsFleeting ? PersistenceType.Fleeting :
        IsOpportunity ? PersistenceType.Opportunity :
        PersistenceType.Persistent;
    public bool IsGoalCard => IsGoal;
    public ConnectionType TokenType_Legacy => TokenType switch
    {
        TokenType.Trust => ConnectionType.Trust,
        TokenType.Commerce => ConnectionType.Commerce,
        TokenType.Status => ConnectionType.Status,
        TokenType.Shadow => ConnectionType.Shadow,
        _ => ConnectionType.None
    };
    
    // Legacy compatibility properties for UI that still needs them
    public bool IsExchange => Properties.Contains(CardProperty.Exchange);
    public bool CanDeliverLetter => Properties.Contains(CardProperty.DeliveryEligible);
    public string DeliveryObligationId => ""; // No longer used
    public string Category => GetCategoryClass().Replace("card-", "");
    public CardType Type => IsGoal ? CardType.Goal : 
                           IsObservation ? CardType.Observation : 
                           CardType.Normal;
    public EmotionalState? SuccessState => null; // No longer used
    public EmotionalState? FailureState => null; // No longer used
    public CardContext Context => null; // No longer used
    public CardMechanicsType Mechanics => CardMechanicsType.Standard; // No longer used
    public string ObservationSource => ""; // No longer used
    public string DisplayName => Name; // Alias for Name

    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        Id = template.Id;
        Name = template.Name;
        Description = template.Description;
        Properties = new List<CardProperty>(template.Properties); // Copy properties
        TokenType = template.TokenType;
        Weight = template.Weight;
        Difficulty = template.Difficulty;
        SuccessEffect = template.SuccessEffect;
        FailureEffect = template.FailureEffect;
        ExhaustEffect = template.ExhaustEffect;
        DialogueFragment = template.DialogueFragment;
        VerbPhrase = template.VerbPhrase;
        SourceContext = sourceContext;
    }

    // Legacy compatibility methods for UI
    public int GetBaseSuccessPercentage()
    {
        return Difficulty switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }
    
    public int BaseSuccessChance => GetBaseSuccessPercentage();
    public int BaseComfort => 1; // Default for legacy UI
    public int BaseComfortReward => 1; // Default for legacy UI
    
    public string GetCategoryClass()
    {
        // Determine category from properties
        if (IsGoal) return "card-goal";
        if (IsObservation) return "card-observation";
        if (IsBurden) return "card-burden";
        if (IsExchange) return "card-exchange";
        return "card-standard";
    }
    
    // Legacy weight calculation methods
    public int GetEffectiveWeight(EmotionalState state)
    {
        return Weight; // No state modifiers in new system
    }
    
    // Legacy success chance calculation
    public int CalculateSuccessChance()
    {
        return GetBaseSuccessPercentage();
    }
    
    public int CalculateSuccessChance(EmotionalState state)
    {
        return GetBaseSuccessPercentage();
    }
    
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens)
    {
        return GetBaseSuccessPercentage();
    }
    
    // Legacy method for getting connection type
    public ConnectionType GetConnectionType()
    {
        return TokenType_Legacy;
    }
}
