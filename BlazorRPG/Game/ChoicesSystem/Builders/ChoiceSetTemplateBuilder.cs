public class ChoiceSetTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private CompositionPattern compositionPattern;
    private List<LocationPropertyCondition> availabilityConditions = new();
    private List<EncounterStateCondition> stateConditions = new();
    private LocationArchetypes locationArchetypeRequired;

    public ChoiceSetTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ChoiceSetTemplateBuilder ForLocationArchetype(LocationArchetypes locationArchetype)
    {
        this.locationArchetypeRequired = locationArchetype;
        return this;
    }

    public ChoiceSetTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        compositionPattern = GameRules.SetDefaultCompositionForActionType(actionType);
        return this;
    }

    public ChoiceSetTemplateBuilder AddAvailabilityCondition(Action<LocationPropertiesBuilder> buildProperties)
    {
        LocationPropertiesBuilder builder = new();
        buildProperties(builder);
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

    public ChoiceSetTemplateBuilder AddStateCondition(Action<EncounterStateConditionBuilder> buildCondition)
    {
        EncounterStateConditionBuilder builder = new();
        buildCondition(builder);
        stateConditions.Add(builder.Build());
        return this;
    }

    public ChoiceSetTemplate Build()
    {
        return new ChoiceSetTemplate(
            name,
            compositionPattern,
            actionType,
            availabilityConditions,
            stateConditions);
    }
}
