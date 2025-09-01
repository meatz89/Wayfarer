
// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();
    public string TemplateId { get; init; }
    public string Name { get; init; }
    public CardType Type { get; init; }
    public ConnectionType TokenType { get; init; }
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
        int baseChance = BaseSuccessChance;
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
