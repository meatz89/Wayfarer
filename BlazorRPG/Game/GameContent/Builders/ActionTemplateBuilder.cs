public class ActionTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private List<Requirement> baseRequirements = new List<Requirement>();
    private List<Outcome> baseCosts = new List<Outcome>();
    private List<Outcome> baseRewards = new List<Outcome>();
    private List<TimeSlots> timeSlots = new List<TimeSlots>();
    private List<LocationPropertyCondition> availabilityConditions = new List<LocationPropertyCondition>();

    public ActionTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionTemplateBuilder AddRequirement(Requirement requirement)
    {
        this.baseRequirements.Add(requirement);
        return this;
    }

    public ActionTemplateBuilder AddCost(Outcome cost)
    {
        this.baseCosts.Add(cost);
        return this;
    }

    public ActionTemplateBuilder AddReward(Outcome reward)
    {
        this.baseRewards.Add(reward);
        return this;
    }

    public ActionTemplateBuilder AddTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlots.Add(timeSlot);
        return this;
    }

    public ActionTemplateBuilder AddAvailabilityCondition(LocationPropertyCondition condition)
    {
        this.availabilityConditions.Add(condition);
        return this;
    }

    public ActionTemplate Build()
    {
        // Add validation to ensure required properties are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("ActionTemplate must have a name.");
        }
        //if (actionType == BasicActionTypes.Wait || actionType == BasicActionTypes.Rest) // Example check for missing action type
        //{
        //    throw new InvalidOperationException($"ActionTemplate '{name}' must have a valid ActionType.");
        //}

        return new ActionTemplate(name, actionType, baseRequirements, baseCosts, baseRewards, timeSlots, availabilityConditions);
    }
}