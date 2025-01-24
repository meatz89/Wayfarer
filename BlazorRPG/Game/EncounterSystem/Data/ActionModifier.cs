public abstract class ActionModifier
{
    public string Source;
    public string Description;
    public abstract void ApplyModification(ActionImplementation action);
    public List<BasicActionTypes> ApplicableActions = new();
}

public class EnergyCostReducer : ActionModifier
{
    private readonly EnergyTypes energyType;


    private readonly int reduction;

    public EnergyCostReducer(string description, string source, int reduction, EnergyTypes type, BasicActionTypes actionType)
    {
        this.Source = source;
        this.Description = description;
        this.reduction = reduction;
        this.energyType = type;
        this.ApplicableActions.Add(actionType);
    }

    public override void ApplyModification(ActionImplementation action)
    {
        foreach (Outcome cost in action.FailureOutcomes)
        {
            if (cost is EnergyOutcome energyCost &&
                energyCost.EnergyType == energyType)
            {
                energyCost.Amount = Math.Max(0, energyCost.Amount - reduction);
            }
        }
    }
}

public class TimeSlotModifier : ActionModifier
{
    private readonly TimeSlots timeWindowToAdd;

    public TimeSlotModifier(string description, string source, TimeSlots timeWindow, BasicActionTypes actionType)
    {
        this.Source = source;
        this.Description = description;
        this.timeWindowToAdd = timeWindow;
        this.ApplicableActions.Add(actionType);
    }

    public override void ApplyModification(ActionImplementation action)
    {
        // Only add the time window if it's not already present
        if (!action.TimeSlots.Contains(timeWindowToAdd))
        {
            action.TimeSlots.Add(timeWindowToAdd);
        }
    }
}

public class GATHERingBonusModifier : ActionModifier
{
    private readonly int bonusAmount;
    private readonly ResourceTypes resourceType;

    public GATHERingBonusModifier(string description, string source, int bonusAmount, ResourceTypes resourceType, BasicActionTypes actionType)
    {
        this.Source = source;
        this.Description = description;
        this.bonusAmount = bonusAmount;
        this.resourceType = resourceType;
        this.ApplicableActions.Add(actionType);
    }

    public override void ApplyModification(ActionImplementation action)
    {
        foreach (Outcome reward in action.SuccessOutcomes)
        {
            if (reward is ResourceOutcome resourceReward &&
                resourceReward.ResourceType == resourceType)
            {
                resourceReward.Amount += bonusAmount;
            }
        }
    }
}

public class RequirementRemover : ActionModifier
{
    private readonly Type requirementTypeToRemove;

    public RequirementRemover(string description, string source, Type requirementType, BasicActionTypes actionType)
    {
        this.Source = source;
        this.Description = description;
        this.requirementTypeToRemove = requirementType;
        this.ApplicableActions.Add(actionType);
    }

    public override void ApplyModification(ActionImplementation action)
    {
        // Remove all requirements of the specified type
        action.Requirements.RemoveAll(req => req.GetType() == requirementTypeToRemove);
    }
}

public class CoinsRewardModifier : ActionModifier
{
    private readonly int bonusAmount;

    public CoinsRewardModifier(string description, string source, int amount, BasicActionTypes actionType)
    {
        this.Source = source;
        this.Description = description;
        this.bonusAmount = amount;
        this.ApplicableActions.Add(actionType);
    }

    public override void ApplyModification(ActionImplementation action)
    {
        action.SuccessOutcomes.Add(new CoinsOutcome(bonusAmount));
    }
}

public class ConditionalResourceBonusModifier : ActionModifier
{
    private readonly ResourceTypes requiredResourceType;
    private readonly ResourceTypes bonusResourceType;
    private readonly int bonusAmount;

    public ConditionalResourceBonusModifier(
        string description,
        string source,
        BasicActionTypes actionType,
        ResourceTypes requiredResource,
        ResourceTypes bonusResource,
        int bonusAmount)
    {
        this.Source = source;
        this.Description = description;
        this.ApplicableActions.Add(actionType);
        this.requiredResourceType = requiredResource;
        this.bonusResourceType = bonusResource;
        this.bonusAmount = bonusAmount;
    }

    public override void ApplyModification(ActionImplementation action)
    {
        // First check if the action normally gives the required resource
        bool hasRequiredResource = action.SuccessOutcomes.Any(reward =>
            reward is ResourceOutcome resourceReward &&
            resourceReward.ResourceType == requiredResourceType);

        // Only add the bonus if the condition is met
        if (hasRequiredResource)
        {
            // Add a new resource outcome for the bonus
            action.SuccessOutcomes.Add(new ResourceOutcome(bonusResourceType, bonusAmount));
        }
    }
}