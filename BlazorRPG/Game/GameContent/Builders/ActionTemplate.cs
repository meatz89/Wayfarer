public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<Requirement> BaseRequirements { get; set; }
    public List<Outcome> BaseCosts { get; set; }
    public List<Outcome> BaseRewards { get; set; }
    public List<TimeSlots> TimeSlots { get; set; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; set; }

    // Make the constructor internal so that only the builder can access it
    internal ActionTemplate(string name, BasicActionTypes actionType, List<Requirement> baseRequirements,
                         List<Outcome> baseCosts, List<Outcome> baseRewards, List<TimeSlots> timeSlots,
                         List<LocationPropertyCondition> availabilityConditions)
    {
        Name = name;
        ActionType = actionType;
        BaseRequirements = baseRequirements;
        BaseCosts = baseCosts;
        BaseRewards = baseRewards;
        TimeSlots = timeSlots;
        AvailabilityConditions = availabilityConditions;
    }

    // Method to create an ActionImplementation instance from the template
    public ActionImplementation CreateActionImplementation()
    {
        return new ActionImplementation
        {
            Name = this.Name,
            ActionType = this.ActionType,
            Requirements = new List<Requirement>(this.BaseRequirements), // Clone the list
            Costs = new List<Outcome>(this.BaseCosts), // Clone the list
            Rewards = new List<Outcome>(this.BaseRewards), // Clone the list
            TimeSlots = new List<TimeSlots>(this.TimeSlots), // Clone the list
            AvailabilityConditions = this.AvailabilityConditions // No need to clone this
        };
    }
}
