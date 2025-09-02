
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
    public CardContext Context { get; set; } // For exchange data and other context
    
    // Helper properties that use Properties list
    public bool IsFleeting => Properties.Contains(CardProperty.Fleeting);
    public bool IsOpportunity => Properties.Contains(CardProperty.Opportunity);
    public bool IsPersistent => !Properties.Contains(CardProperty.Fleeting) 
                                && !Properties.Contains(CardProperty.Opportunity);
    public bool IsGoal => Properties.Contains(CardProperty.Fleeting) 
                          && Properties.Contains(CardProperty.Opportunity);
    public bool IsBurden => Properties.Contains(CardProperty.Burden);
    public bool IsObservable => Properties.Contains(CardProperty.Observable);

    public string GetCategoryClass()
    {
        // Determine category from properties
        if (IsGoal) return "card-goal";
        if (IsObservable) return "card-observation";
        if (IsBurden) return "card-burden";
        if (Properties.Contains(CardProperty.Exchange)) return "card-exchange";
        return "card-standard";
    }
    
    public string Category
    {
        get
        {
            if (Properties.Contains(CardProperty.Exchange)) return nameof(CardCategory.Exchange);
            if (IsBurden) return nameof(CardCategory.Burden);
            if (IsGoal) return nameof(CardCategory.Promise);
            if (IsObservable) return nameof(CardCategory.Observation);
            return nameof(CardCategory.Comfort);
        }
    }
    
    public CardType Type
    {
        get
        {
            if (IsGoal) return CardType.Goal;
            if (IsObservable) return CardType.Observation;
            return CardType.Normal;
        }
    }
    
    public EmotionalState? SuccessState => null;
    public EmotionalState? FailureState => null;
    public int BaseComfort => 1;
    public string DisplayName => Name;
    public bool CanDeliverLetter => Properties.Contains(CardProperty.DeliveryEligible);
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    public bool IsExchange => Properties.Contains(CardProperty.Exchange);
    public CardMechanicsType Mechanics => Properties.Contains(CardProperty.Exchange) ? CardMechanicsType.Exchange : CardMechanicsType.Standard;
    
    public int GetEffectiveWeight(EmotionalState state) => Weight;
    public int CalculateSuccessChance() => GetBaseSuccessPercentage();
    public int CalculateSuccessChance(EmotionalState state) => GetBaseSuccessPercentage();
    public ConnectionType GetConnectionType() => TokenType switch
    {
        TokenType.Trust => ConnectionType.Trust,
        TokenType.Commerce => ConnectionType.Commerce,
        TokenType.Status => ConnectionType.Status,
        TokenType.Shadow => ConnectionType.Shadow,
        _ => ConnectionType.None
    };
    
    private int GetBaseSuccessPercentage() => Difficulty switch
    {
        Difficulty.VeryEasy => 85,
        Difficulty.Easy => 70,
        Difficulty.Medium => 60,
        Difficulty.Hard => 50,
        Difficulty.VeryHard => 40,
        _ => 60
    };

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

}
