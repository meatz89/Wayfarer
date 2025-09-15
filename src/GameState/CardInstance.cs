
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Template reference - single source of truth for card properties
    public ConversationCard Template { get; init; }

    // Delegating properties for API compatibility
    public string Id => Template.Id;
    public string Description => Template.Description;
    public SuccessEffectType SuccessType => Template.SuccessType;
    public FailureEffectType FailureType => Template.FailureType;
    public ExhaustEffectType ExhaustType => Template.ExhaustType;
    public CardType CardType => Template.CardType;
    public ConnectionType TokenType => Template.TokenType;
    public int Focus => Template.Focus;
    public Difficulty Difficulty => Template.Difficulty;
    public int RapportThreshold => Template.RapportThreshold;
    public string RequestId => Template.RequestId;
    public int MinimumTokensRequired => Template.MinimumTokensRequired;
    public string DialogueFragment => Template.DialogueFragment;
    public string VerbPhrase => Template.VerbPhrase;

    // Categorical properties that define behavior through context
    // Cards with "gains_thought_persistence" effect gain Thought persistence
    public PersistenceType Persistence
    {
        get
        {
            var bonus = GameRules.StandardRuleset.CardProgression.GetBonusForLevel(Level);
            bool gainsThought = bonus?.Effects?.Contains("gains_thought_persistence") ?? false;

            if (gainsThought && _basePersistence != PersistenceType.Thought)
            {
                return PersistenceType.Thought;
            }
            return _basePersistence;
        }
    }
    private PersistenceType _basePersistence => Template.Persistence;

    // Runtime context
    public string SourceContext { get; init; }
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;

    // XP and Leveling System
    public int XP { get; set; } = 0;
    public int Level => GameRules.StandardRuleset.CardProgression.GetLevelFromXp(XP);
    // Cards with "ignores_failure_listen" effect ignore forced LISTEN on failure
    public bool IgnoresFailureListen
    {
        get
        {
            var bonus = GameRules.StandardRuleset.CardProgression.GetBonusForLevel(Level);
            return bonus?.Effects?.Contains("ignores_failure_listen") ?? false;
        }
    }

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
        int baseChance = Template.GetBaseSuccessPercentage();

        // Add level bonus from JSON configuration
        int levelBonus = GameRules.StandardRuleset.CardProgression.GetTotalSuccessBonusForLevel(Level);
        return baseChance + levelBonus;
    }

    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        Template = template;
        SourceContext = sourceContext;
    }

    // Removed - now using GameRules.CardProgression.GetLevelFromXp()

}
