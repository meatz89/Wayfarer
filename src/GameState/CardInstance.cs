
using System.Collections.Generic;

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();
    public string Id { get; init; }
    public string Description { get; init; }

    // Properties list - copied from template
    public List<CardProperty> Properties { get; init; } = new List<CardProperty>();

    // Core mechanics
    public TokenType TokenType { get; init; }
    public int Focus { get; init; }
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

    // Use Properties list directly - no legacy boolean helpers

    public string GetCategoryClass()
    {
        // Return ALL properties as CSS classes
        List<string> classes = new List<string>();

        foreach (CardProperty property in Properties)
        {
            classes.Add($"card-{property.ToString().ToLower()}");
        }

        // Add special combined classes - only if playable
        if (Properties.Contains(CardProperty.Impulse) &&
            Properties.Contains(CardProperty.Opening) &&
            !Properties.Contains(CardProperty.Unplayable))
        {
            classes.Add("card-request");
        }

        // If no properties, add default
        if (classes.Count == 0)
        {
            classes.Add("card-standard");
        }

        return string.Join(" ", classes);
    }

    // Removed misleading Category and Type properties - use Properties list directly

    public ConnectionState? SuccessState => null;
    public ConnectionState? FailureState => null;
    public int BaseFlow => 1;
    public bool CanDeliverLetter => Properties.Contains(CardProperty.DeliveryEligible);
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    public bool IsExchange => Properties.Contains(CardProperty.Exchange);
    public CardMechanicsType Mechanics => Properties.Contains(CardProperty.Exchange) ? CardMechanicsType.Exchange : CardMechanicsType.Standard;

    public int GetEffectiveFocus(ConnectionState state)
    {
        return Focus;
    }

    public int CalculateSuccessChance()
    {
        // Request/Promise cards always succeed (100%)
        if (Properties.Contains(CardProperty.DeliveryEligible))
            return 100;

        return GetBaseSuccessPercentage();
    }

    public int CalculateSuccessChance(ConnectionState state)
    {
        // Request/Promise cards always succeed (100%)
        if (Properties.Contains(CardProperty.DeliveryEligible))
            return 100;

        return GetBaseSuccessPercentage();
    }
    public ConnectionType GetConnectionType()
    {
        return TokenType switch
        {
            TokenType.Trust => ConnectionType.Trust,
            TokenType.Commerce => ConnectionType.Commerce,
            TokenType.Status => ConnectionType.Status,
            TokenType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.None
        };
    }

    private int GetBaseSuccessPercentage()
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

    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        Id = template.Id;
        Description = template.Description;
        Properties = new List<CardProperty>(template.Properties); // Copy properties
        TokenType = template.TokenType;
        Focus = template.Focus;
        Difficulty = template.Difficulty;
        SuccessEffect = template.SuccessEffect;
        FailureEffect = template.FailureEffect;
        ExhaustEffect = template.ExhaustEffect;
        DialogueFragment = template.DialogueFragment;
        VerbPhrase = template.VerbPhrase;
        SourceContext = sourceContext;
    }

}
