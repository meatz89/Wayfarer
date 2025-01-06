public class LocationBuilder
{
    private LocationTypes locationType;
    private LocationNames locationName;
    private LocationArchetype locationArchetype;
    private LocationProperties locationProperties = new LocationProperties();
    private List<LocationNames> travelConnections = new();
    private List<LocationSpot> locationSpots = new();

    public LocationBuilder SetLocationType(LocationTypes type)
    {
        this.locationType = type;
        return this;
    }

    public LocationBuilder ForLocation(LocationNames name)
    {
        this.locationName = name;
        return this;
    }

    public LocationBuilder SetLocationArchetype(LocationArchetype archetype)
    {
        this.locationArchetype = archetype;
        return this;
    }

    public LocationBuilder AddTravelConnection(LocationNames connection)
    {
        this.travelConnections.Add(connection);
        return this;
    }

    public LocationBuilder AddLocationSpot(Action<LocationSpotBuilder> buildLocationSpot)
    {
        // Create a new LocationSpotBuilder instance here
        LocationSpotBuilder locationSpotBuilder = new LocationSpotBuilder(locationName);

        // Invoke the builder action to configure the LocationSpot
        buildLocationSpot(locationSpotBuilder);

        // Add the built LocationSpot to the list
        this.locationSpots.Add(locationSpotBuilder.Build());

        return this;
    }

    public LocationBuilder SetSpace(Action<SpacePropertiesBuilder> spaceBuilder)
    {
        SpacePropertiesBuilder builder = new SpacePropertiesBuilder();
        spaceBuilder?.Invoke(builder);
        SpaceProperties spaceProperties = builder.Build();

        this.locationProperties.Scale = spaceProperties.Scale;
        this.locationProperties.Exposure = spaceProperties.Exposure;
        this.locationProperties.CrowdLevel = spaceProperties.CrowdLevel;

        return this;
    }

    public LocationBuilder SetSocial(Action<SocialContextBuilder> socialBuilder)
    {
        SocialContextBuilder builder = new SocialContextBuilder();
        socialBuilder?.Invoke(builder);
        SocialContext socialContext = builder.Build();

        this.locationProperties.Legality = socialContext.Legality;
        this.locationProperties.Tension = socialContext.Tension;

        return this;
    }

    public LocationBuilder SetActivity(Action<ActivityPropertiesBuilder> activityBuilder)
    {
        ActivityPropertiesBuilder builder = new ActivityPropertiesBuilder();
        activityBuilder?.Invoke(builder);
        ActivityProperties activityProperties = builder.Build();

        this.locationProperties.Complexity = activityProperties.Complexity;

        return this;
    }

    public Location Build()
    {
        return new Location()
        {
            Archetype = locationArchetype,
            LocationSpots = locationSpots,
            LocationType = locationType,
            LocationName = locationName,
            Properties = locationProperties,
            TravelConnections = travelConnections
        };
    }
}