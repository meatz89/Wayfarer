
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();
    public string Id { get; init; }
    public string Description { get; init; }

    // Categorical properties that define behavior through context
    // Note: Level 3+ cards gain Thought persistence regardless of base persistence
    public PersistenceType Persistence
    {
        get
        {
            // Level 3 cards gain Thought persistence if they don't already have it
            if (Level >= 3 && _basePersistence != PersistenceType.Thought)
            {
                return PersistenceType.Thought;
            }
            return _basePersistence;
        }
        init { _basePersistence = value; }
    }
    private PersistenceType _basePersistence = PersistenceType.Thought;
    public SuccessEffectType SuccessType { get; init; } = SuccessEffectType.None;
    public FailureEffectType FailureType { get; init; } = FailureEffectType.None;
    public ExhaustEffectType ExhaustType { get; init; } = ExhaustEffectType.None;

    // Single source of truth for card type
    public CardType CardType { get; init; } = CardType.Conversation;

    // Core mechanics
    public ConnectionType TokenType { get; init; }
    public int Focus { get; init; }
    public Difficulty Difficulty { get; init; }

    // Request card specific properties
    public int RapportThreshold { get; init; } = 0;
    public string RequestId { get; init; }

    // Display properties
    public string DialogueFragment { get; init; }
    public string VerbPhrase { get; init; }

    // Runtime context
    public string SourceContext { get; init; }
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;

    // XP and Leveling System
    public int XP { get; set; } = 0;
    public int Level => CalculateLevel(XP);
    // Level 5 cards ignore forced LISTEN on failure
    public bool IgnoresFailureListen => Level >= 5;

    public string GetCategoryClass()
    {
        List<string> classes = new List<string>();

        // Add persistence class
        classes.Add($"card-{Persistence.ToString().ToLower()}");

        // Add success type class if not None
        if (SuccessType != SuccessEffectType.None)
        {
            classes.Add($"success-{SuccessType.ToString().ToLower()}");
        }

        // Add failure type class if not None
        if (FailureType != FailureEffectType.None)
        {
            classes.Add($"failure-{FailureType.ToString().ToLower()}");
        }

        // Add special classes for request cards
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
        {
            classes.Add("card-request");
            if (!IsPlayable)
            {
                classes.Add("card-unplayable");
            }
        }

        return string.Join(" ", classes);
    }

    // Removed misleading Category and Type properties - use Properties list directly

    public ConnectionState? SuccessState => null;
    public ConnectionState? FailureState => null;
    public int BaseFlow => 1;
    public bool CanDeliverLetter => CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal;
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    // Exchange detection - exchanges are now separate ExchangeCard entities, not ConversationCards
    public bool IsExchange => false;  // CardInstances are only for ConversationCards now
    public CardMechanicsType Mechanics => CardMechanicsType.Standard;

    public int GetEffectiveFocus(ConnectionState state)
    {
        return Focus;
    }

    public int CalculateSuccessChance()
    {
        // Request/Promise cards always succeed (100%)
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
            return 100;

        return GetBaseSuccessPercentage();
    }

    public int CalculateSuccessChance(ConnectionState state)
    {
        // Request/Promise cards always succeed (100%)
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
            return 100;

        return GetBaseSuccessPercentage();
    }
    public ConnectionType GetConnectionType()
    {
        return TokenType;
    }

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

    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        Id = template.Id;
        Description = template.Description;
        _basePersistence = template.Persistence;
        SuccessType = template.SuccessType;
        FailureType = template.FailureType;
        ExhaustType = template.ExhaustType;
        CardType = template.CardType;
        TokenType = template.TokenType;
        Focus = template.Focus;
        Difficulty = template.Difficulty;
        RapportThreshold = template.RapportThreshold;
        RequestId = template.RequestId;
        DialogueFragment = template.DialogueFragment;
        VerbPhrase = template.VerbPhrase;
        SourceContext = sourceContext;
    }

    private int CalculateLevel(int xp)
    {
        // Level progression with infinite scaling
        if (xp < 3) return 1;
        if (xp < 7) return 2;
        if (xp < 15) return 3;
        if (xp < 30) return 4;
        if (xp < 50) return 5;
        if (xp < 75) return 6;
        if (xp < 100) return 7;
        if (xp < 150) return 8;
        if (xp < 200) return 9;

        // For levels 10+, continue exponential growth pattern
        // Level 10: 250 XP, Level 11: 325 XP, Level 12: 400 XP, etc.
        int level = 9;
        int threshold = 200;
        int increment = 50;

        while (xp >= threshold)
        {
            level++;
            increment += 25; // Increases by 25 each level (50, 75, 100, 125...)
            threshold += increment;
        }

        return level;
    }

}
