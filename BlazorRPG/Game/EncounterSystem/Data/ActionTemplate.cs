public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<TimeSlots> TimeSlots { get; set; }
    public LocationArchetypes LocationArchetype { get; set; }
    public CrowdDensity CrowdDensity { get; set; }
    public LocationScale LocationScale { get; set; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; set; }

    // Make the constructor public so that only the builder can access it
    public ActionTemplate(
        string name,
        BasicActionTypes actionType,
        List<TimeSlots> timeSlots,
        LocationArchetypes locationArchetype,
        CrowdDensity crowdDensity,
        LocationScale locationScale,
        List<LocationPropertyCondition> availabilityConditions)
    {
        Name = name;
        ActionType = actionType;
        TimeSlots = timeSlots;
        LocationArchetype = locationArchetype;
        CrowdDensity = crowdDensity;
        LocationScale = locationScale;
        AvailabilityConditions = availabilityConditions;
    }


}