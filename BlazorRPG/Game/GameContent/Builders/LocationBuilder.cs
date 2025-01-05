public class LocationBuilder
{
    private LocationNames location;
    private LocationTypes locationType;
    private List<LocationNames> travelConnections = new();

    private List<LocationSpot> locationSpots = new();
    private List<ActionImplementation> actions = new();

    public LocationBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public LocationBuilder AddLocationSpot(Action<LocationSpotBuilder> buildLocationSpot)
    {
        LocationSpotBuilder builder = new(location, locationType);
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

    public LocationBuilder AddAction(Action<ActionBuilder> buildBasicAction)
    {
        ActionBuilder builder = new ActionBuilder();
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
