public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<TimeSlots> TimeSlots { get; set; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; set; }

    // Make the constructor public so that only the builder can access it
    public ActionTemplate(string name, BasicActionTypes actionType, List<TimeSlots> timeSlots,
                         List<LocationPropertyCondition> availabilityConditions)
    {
        Name = name;
        ActionType = actionType;
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
            TimeSlots = new List<TimeSlots>(this.TimeSlots),
            AvailabilityConditions = this.AvailabilityConditions
        };
    }
}