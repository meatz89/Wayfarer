
public class LocationPropertiesBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private List<ActivityTypes> activityTypes = new();
    private AccessTypes accessType;
    private ShelterStates shelterState;
    private DangerLevels dangerLevel;

    private List<BasicAction> actions = new();

    internal LocationPropertiesBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    internal LocationPropertiesBuilder SetLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    internal LocationPropertiesBuilder AddActivityType(ActivityTypes activityType)
    {
        this.activityTypes.Add(activityType);
        return this;
    }

    internal LocationPropertiesBuilder SetAccessType(AccessTypes accessType)
    {
        this.accessType = accessType;

        return this;
    }

    internal LocationPropertiesBuilder SetShelterStatus(ShelterStates shelterState)
    {
        this.shelterState = shelterState;

        return this;
    }

    internal LocationPropertiesBuilder SetDangerLevel(DangerLevels dangerLevel)
    {
        this.dangerLevel = dangerLevel;

        return this;
    }

    internal LocationPropertiesBuilder AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public LocationProperties Build()
    {
        List<BasicAction> locActions = LocationActionsFactory.Create(
            this.locationType,
            this.activityTypes,
            this.accessType,
            this.shelterState,
            this.dangerLevel
        );

        locActions.AddRange(actions);

        return new LocationProperties
        {
            Location = location,
            LocationType = locationType,
            ActivityTypes = activityTypes,
            Actions = locActions
        };
    }
}