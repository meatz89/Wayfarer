public class ChoiceSetTemplateBuilder
{
    private string name;
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private BasicActionTypes actionType;
    private List<LocationPropertyCondition> availabilityConditions = new();
    private List<EncounterStateCondition> stateConditions = new();
    private List<ChoiceTemplate> choicePatterns = new();

    public ChoiceSetTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ChoiceSetTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ChoiceSetTemplateBuilder AddAvailabilityCondition(Action<LocationPropertiesBuilder> buildProperties)
    {
        LocationPropertiesBuilder builder = new();
        buildProperties(builder);
        LocationProperties properties = builder.Build();

        // Create LocationPropertyCondition instances for each property defined in the builder
        AddConditionIfSet<LocationArchetypes>(properties, LocationPropertyTypes.Archetype);
        AddConditionIfSet<ActivityLevelTypes>(properties, LocationPropertyTypes.ActivityLevel);
        AddConditionIfSet<ResourceTypes>(properties, LocationPropertyTypes.Resource);
        AddConditionIfSet<AccessibilityTypes>(properties, LocationPropertyTypes.Exposure);
        AddConditionIfSet<ExposureTypes>(properties, LocationPropertyTypes.Accessibility);
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

    public ChoiceSetTemplateBuilder AddStateCondition(Action<EncounterStateConditionBuilder> buildCondition)
    {
        EncounterStateConditionBuilder builder = new();
        buildCondition(builder);
        stateConditions.Add(builder.Build());
        return this;
    }

    public ChoiceSetTemplateBuilder AddChoice(Action<ChoiceTemplateBuilder> buildChoice)
    {
        ChoiceTemplateBuilder builder = new();
        buildChoice(builder);
        choicePatterns.Add(builder.Build());
        return this;
    }

    public ChoiceSetTemplate Build()
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Choice Set Template must have a name");
        if (choicePatterns.Count == 0)
            throw new InvalidOperationException("Choice Set Template must have at least one choice pattern");

        return new ChoiceSetTemplate(
            name,
            actionType,
            availabilityConditions,
            stateConditions,
            choicePatterns
        );
    }

}
