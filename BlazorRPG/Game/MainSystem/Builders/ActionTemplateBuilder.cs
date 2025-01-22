public class ActionTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private List<TimeSlots> timeSlots = new List<TimeSlots>();

    private LocationArchetypes locationArchetype;
    private CrowdDensity crowdDensity;
    private LocationScale locationScale;
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

    public ActionTemplateBuilder SetLocationArchetype(LocationArchetypes archetype)
    {
        this.locationArchetype = archetype;
        return this;
    }

    public ActionTemplateBuilder SetCrowdDensity(CrowdDensity crowdDensity)
    {
        this.crowdDensity = crowdDensity;
        return this;
    }

    public ActionTemplateBuilder SetLocationScale(LocationScale locationScale)
    {
        this.locationScale = locationScale;
        return this;
    }

    public ActionTemplateBuilder AddAvailabilityCondition(Action<LocationPropertiesBuilder> buildLocationProperties)
    {
        LocationPropertiesBuilder builder = new LocationPropertiesBuilder();
        buildLocationProperties(builder);
        LocationSpotProperties properties = builder.Build();

        // Create LocationPropertyCondition instances for each property defined in the builder
        AddConditionIfSet<Accessability>(properties, LocationPropertyTypes.Accessibility);
        AddConditionIfSet<Engagement>(properties, LocationPropertyTypes.Engagement);
        AddConditionIfSet<Atmosphere>(properties, LocationPropertyTypes.Atmosphere);
        AddConditionIfSet<RoomLayout>(properties, LocationPropertyTypes.RoomLayout);
        AddConditionIfSet<Temperature>(properties, LocationPropertyTypes.Temperature);

        return this;
    }

    private void AddConditionIfSet<T>(LocationSpotProperties properties, LocationPropertyTypes propertyType)
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
        return new ActionTemplate(name, actionType, timeSlots,
            locationArchetype, crowdDensity, locationScale,
            availabilityConditions);
    }
}