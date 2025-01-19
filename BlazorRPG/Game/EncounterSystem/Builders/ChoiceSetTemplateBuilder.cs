public class ChoiceSetTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private CompositionPattern compositionPattern;
    private List<LocationPropertyCondition> availabilityConditions = new();
    private List<EncounterStateCondition> stateConditions = new();

    public ChoiceSetTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ChoiceSetTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        SetDefaultCompositionForActionType(actionType);
        return this;
    }

    public ChoiceSetTemplateBuilder AddAvailabilityCondition(Action<LocationPropertiesBuilder> buildProperties)
    {
        LocationPropertiesBuilder builder = new();
        buildProperties(builder);
        LocationProperties properties = builder.Build();

        // Create LocationPropertyCondition instances for each property defined in the builder
        AddConditionIfSet<LocationArchetypes>(properties, LocationPropertyTypes.Archetype);
        AddConditionIfSet<ResourceTypes>(properties, LocationPropertyTypes.Resource);

        AddConditionIfSet<ActivityLevelTypes>(properties, LocationPropertyTypes.ActivityLevel);
        AddConditionIfSet<AccessibilityTypes>(properties, LocationPropertyTypes.Accessibility);
        AddConditionIfSet<SupervisionTypes>(properties, LocationPropertyTypes.Supervision);

        AddConditionIfSet<AtmosphereTypes>(properties, LocationPropertyTypes.Atmosphere);
        AddConditionIfSet<SpaceTypes>(properties, LocationPropertyTypes.Space);
        AddConditionIfSet<LightingTypes>(properties, LocationPropertyTypes.Lighting);
        AddConditionIfSet<ExposureTypes>(properties, LocationPropertyTypes.Exposure);

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

    public ChoiceSetTemplate Build()
    {
        return new ChoiceSetTemplate(
            name,
            compositionPattern,
            actionType,
            availabilityConditions,
            stateConditions);
    }

    private void SetDefaultCompositionForActionType(BasicActionTypes actionType)
    {
        switch (actionType)
        {
            case BasicActionTypes.Labor:
            case BasicActionTypes.Gather:
            case BasicActionTypes.Travel:
                // Physical-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Physical,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                    PrimaryCount = 2,
                    SecondaryCount = 1
                };
                break;

            case BasicActionTypes.Investigate:
            case BasicActionTypes.Study:
            case BasicActionTypes.Reflect:
                // Focus-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Social,
                    PrimaryCount = 2,
                    SecondaryCount = 1
                };
                break;

            case BasicActionTypes.Mingle:
            case BasicActionTypes.Persuade:
            case BasicActionTypes.Perform:
                // Social-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Physical,
                    PrimaryCount = 2,
                    SecondaryCount = 1
                };
                break;
        }
    }

}
