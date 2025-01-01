public class LocationBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private List<LocationNames> travelConnections = new();
    private AccessTypes accessType;
    private DangerLevels dangerLevel;

    private List<LocationSpot> locationSpots = new();
    private List<BasicAction> actions = new();

    public LocationBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public LocationBuilder AddLocationSpot(Action<LocationSpotBuilder> buildLocationSpot)
    {
        LocationSpotBuilder builder = new();
        buildLocationSpot(builder);
        locationSpots.Add(builder.Build());
        return this;
    }

    public LocationBuilder AddTravelConnection(LocationNames connectedLocation)
    {
        travelConnections.Add(connectedLocation);
        return this;
    }

    public LocationBuilder SetLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    public LocationBuilder SetAccessType(AccessTypes accessType)
    {
        this.accessType = accessType;

        return this;
    }

    public LocationBuilder SetDangerLevel(DangerLevels dangerLevel)
    {
        this.dangerLevel = dangerLevel;

        return this;
    }

    public LocationBuilder AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public Location Build()
    {
        return new Location
        {
            Name = location,
            ConnectedLocations = travelConnections,
            CoreType = locationType,
            Spots = locationSpots
        };
    }
}
