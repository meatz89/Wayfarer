public class ActionModifierBuilder
{
    private string description;

    private BasicActionTypes applicableActionType;
    private LocationTypes applicableLocationType;
    private PlayerStatusTypes applicablePlayerStatusType;

    private ResourceTypes requiredResourceReward;

    private ResourceTypes resourceReward;
    private TimeWindows timeWindowToAdd;
    private EnergyTypes reducedEnergyType;
    private int reducedEnergyAmount;
    private ResourceTypes additionalResourceReward;
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

    public ActionModifierBuilder ForLocationType(LocationTypes locationType)
    {
        this.applicableLocationType = locationType;
        return this;
    }

    public ActionModifierBuilder ForPlayerStatus(PlayerStatusTypes playerStatusType)
    {
        this.applicablePlayerStatusType = playerStatusType;
        return this;
    }

    public ActionModifierBuilder WhenResourceRewardHas(ResourceTypes resourceType)
    {
        this.requiredResourceReward = resourceType;
        return this;
    }

    public ActionModifierBuilder AdditionalResourceReward(ResourceTypes resourceType, int amount)
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
            LocationType = applicableLocationType,
            PlayerStatus = applicablePlayerStatusType,
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

public class ModifierConfiguration
{
    public string Description { get; set; }
    public string Source { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public LocationTypes LocationType { get; set; }
    public PlayerStatusTypes PlayerStatus { get; set; }
    public TimeWindows TimeWindow { get; set; }
    public EnergyTypes EnergyType { get; set; }
    public int EnergyReduction { get; set; }

    public ResourceTypes RequiredResourceReward { get; set; } // Condition: action must reward this
    public ResourceTypes AdditionalResource { get; set; }     // The extra resource to give
    public int AdditionalResourceAmount { get; set; }         // How much extra to give
    public int AdditionalCoins { get; set; }
}