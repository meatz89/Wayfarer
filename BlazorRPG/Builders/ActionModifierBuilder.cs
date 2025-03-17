public class ActionModifierBuilder
{
    private string description;

    private BasicActionTypes applicableActionType;
    private ItemTypes requiredResourceReward;

    private ItemTypes resourceReward;
    private TimeWindows timeWindowToAdd;
    private EnergyTypes reducedEnergyType;
    private int reducedEnergyAmount;
    private ItemTypes additionalResourceReward;
    private int additionalResourceRewardAmount;
    private int additionalCoinReward;

    private string source;

    public ActionModifierBuilder WithSource(string source)
    {
        this.source = source;
        return this;
    }

    public ActionModifierBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ActionModifierBuilder ForActionType(BasicActionTypes actionType)
    {
        this.applicableActionType = actionType;
        return this;
    }

    public ActionModifierBuilder WhenResourceRewardHas(ItemTypes resourceType)
    {
        this.requiredResourceReward = resourceType;
        return this;
    }

    public ActionModifierBuilder AdditionalResourceReward(ItemTypes resourceType, int amount)
    {
        this.additionalResourceReward = resourceType;
        this.additionalResourceRewardAmount = amount;
        return this;
    }

    public ActionModifierBuilder AdditionalCoinReward(int amount)
    {
        this.additionalCoinReward = amount;
        return this;
    }

    public ActionModifierBuilder ForTimeWindow(TimeWindows timeWindow)
    {
        this.timeWindowToAdd = timeWindow;
        return this;
    }

    public ActionModifierBuilder ReduceActionCost(EnergyTypes energyType, int amount)
    {
        this.reducedEnergyType = energyType;
        this.reducedEnergyAmount = amount;
        return this;
    }

    public List<ActionModifier> Build()
    {
        // Create configuration from builder state
        ModifierConfiguration config = new ModifierConfiguration
        {
            Description = description,
            Source = source,
            ActionType = applicableActionType,
            TimeWindow = timeWindowToAdd,
            EnergyType = reducedEnergyType,
            EnergyReduction = reducedEnergyAmount,
            AdditionalCoins = additionalCoinReward,
            RequiredResourceReward = requiredResourceReward,
            AdditionalResource = additionalResourceReward,
            AdditionalResourceAmount = additionalResourceRewardAmount
        };

        // Let the factory create all applicable modifiers
        return ActionModifierFactory.CreateModifier(config);
    }
}
