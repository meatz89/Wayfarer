public class LocationPropertiesBuilder
{
    private LocationNames location;
    private List<LocationNames> travelConnections = new();
    private LocationTypes locationType;
    private List<ActivityTypes> activityTypes = new();
    private AccessTypes accessType;
    private ShelterStates shelterState;
    private DangerLevels dangerLevel;

    private List<BasicAction> actions = new();

    public LocationPropertiesBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public LocationPropertiesBuilder AddTravelConnection(LocationNames connectedLocation)
    {
        travelConnections.Add(connectedLocation);
        return this;
    }

    public LocationPropertiesBuilder SetLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    public LocationPropertiesBuilder AddActivityType(ActivityTypes activityType)
    {
        this.activityTypes.Add(activityType);
        return this;
    }

    public LocationPropertiesBuilder SetAccessType(AccessTypes accessType)
    {
        this.accessType = accessType;

        return this;
    }

    public LocationPropertiesBuilder SetShelterStatus(ShelterStates shelterState)
    {
        this.shelterState = shelterState;

        return this;
    }

    public LocationPropertiesBuilder SetDangerLevel(DangerLevels dangerLevel)
    {
        this.dangerLevel = dangerLevel;

        return this;
    }

    public LocationPropertiesBuilder AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public Location Build()
    {
        List<BasicAction> locActions = LocationActionsFactory.Create(
            this.locationType,
            this.activityTypes,
            this.accessType,
            this.shelterState,
            this.dangerLevel
        );

        locActions.AddRange(actions);

        return new Location
        {
            LocationName = location,
            ConnectedLocations = travelConnections,
            LocationType = locationType,
            ActivityTypes = activityTypes,
            Actions = locActions
        };
    }
}