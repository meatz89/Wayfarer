public class LocationBuilder
{
    private LocationNames locationName;
    private LocationTypes locationType;
    private List<LocationNames> travelConnections = new();

    private List<LocationSpot> locationSpots = new();
    private List<ActionImplementation> actions = new();

    public LocationBuilder ForLocation(LocationNames location)
    {
        this.locationName = location;
        return this;
    }

    public LocationBuilder AddLocationSpot(Action<LocationSpotBuilder> buildLocationSpot)
    {
        var locationSpotBuilder = new LocationSpotBuilder(this.locationName, this.locationType);
        buildLocationSpot(locationSpotBuilder);
        
        LocationSpot item = locationSpotBuilder.Build();
        this.locationSpots.Add(item);
        
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
            Name = locationName,
            TravelConnections = travelConnections,
            LocationType = locationType,
            LocationSpots = locationSpots
        };
    }
}
