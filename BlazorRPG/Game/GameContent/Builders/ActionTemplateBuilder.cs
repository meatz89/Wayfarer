public class ActionTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
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

    public ActionTemplateBuilder AddTimeSlot(TimeSlots timeSlot)
    {
        this.timeSlots.Add(timeSlot);
        return this;
    }

    public ActionTemplateBuilder AddAvailabilityCondition(Action<LocationPropertiesBuilder> buildLocationProperties)
    {
        LocationPropertiesBuilder builder = new LocationPropertiesBuilder();
        buildLocationProperties(builder);
        LocationProperties properties = builder.Build();

        // Create LocationPropertyCondition instances for each property defined in the builder
        AddConditionIfSet<LocationArchetypes>(properties, LocationPropertyTypes.Archetype);
        AddConditionIfSet<ActivityLevelTypes>(properties, LocationPropertyTypes.ActivityLevel);
        AddConditionIfSet<ResourceTypes>(properties, LocationPropertyTypes.Resource);
        AddConditionIfSet<AccessibilityTypes>(properties, LocationPropertyTypes.Exposure);
        AddConditionIfSet<ExposureConditionTypes>(properties, LocationPropertyTypes.Accessibility);
        AddConditionIfSet<SupervisionTypes>(properties, LocationPropertyTypes.Supervision);
        AddConditionIfSet<SupervisionTypes>(properties, LocationPropertyTypes.Atmosphere);
        AddConditionIfSet<AtmosphereTypes>(properties, LocationPropertyTypes.Space);

        return this;
    }

    private void AddConditionIfSet<T>(LocationProperties properties, LocationPropertyTypes propertyType)
    {
        object propertyValue = properties.GetProperty(propertyType);
        if (propertyValue != null)
        {
            availabilityConditions.Add(new LocationPropertyCondition(propertyType, (T)propertyValue));
        }
    }

    public ActionTemplate Build()
    {
        // Add validation to ensure required properties are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("ActionTemplate must have a name.");
        }
        return new ActionTemplate(name, actionType, timeSlots, availabilityConditions);
    }
}