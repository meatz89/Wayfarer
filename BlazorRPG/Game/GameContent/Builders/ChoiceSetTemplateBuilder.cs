public class ChoiceSetTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private List<LocationPropertyCondition> availabilityConditions = new();
    private List<EncounterStateCondition> stateConditions = new();
    private List<ChoicePattern> choicePatterns = new();

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

    public ChoiceSetTemplateBuilder AddStateCondition(Action<EncounterStateConditionBuilder> buildCondition)
    {
        EncounterStateConditionBuilder builder = new();
        buildCondition(builder);
        stateConditions.Add(builder.Build());
        return this;
    }

    public ChoiceSetTemplateBuilder AddChoice(Action<ChoicePatternBuilder> buildChoice)
    {
        ChoicePatternBuilder builder = new();
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
