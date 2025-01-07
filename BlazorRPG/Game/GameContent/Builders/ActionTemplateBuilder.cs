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
        AddConditionIfSet<LocationArchetype>(properties, LocationPropertyTypes.Archetype);
        AddConditionIfSet<CrowdLevelTypes>(properties, LocationPropertyTypes.CrowdLevel);
        AddConditionIfSet<ResourceTypes>(properties, LocationPropertyTypes.Resource);
        AddConditionIfSet<ScaleVariationTypes>(properties, LocationPropertyTypes.Scale);
        AddConditionIfSet<ExposureConditionTypes>(properties, LocationPropertyTypes.Exposure);
        AddConditionIfSet<LegalityTypes>(properties, LocationPropertyTypes.Legality);
        AddConditionIfSet<TensionStateTypes>(properties, LocationPropertyTypes.Tension);
        AddConditionIfSet<ComplexityTypes>(properties, LocationPropertyTypes.Complexity);

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