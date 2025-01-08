public class LocationBuilder
{
    private LocationTypes locationType;
    private LocationNames locationName;
    private LocationArchetypes locationArchetype;
    private List<LocationNames> travelConnections = new();
    private List<LocationSpot> locationSpots = new();
    private int difficultyLevel;

    // Location properties
    private LocationProperties locationProperties;

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

    public LocationBuilder SetLocationArchetype(LocationArchetypes archetype)
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
        LocationSpotBuilder locationSpotBuilder = new LocationSpotBuilder(locationName);
        buildLocationSpot(locationSpotBuilder);
        LocationSpot newSpot = locationSpotBuilder.Build();

        // Check for duplicate LocationSpot names within the same Location
        if (locationSpots.Any(spot => spot.Name == newSpot.Name))
        {
            throw new InvalidOperationException($"Duplicate LocationSpot name '{newSpot.Name}' found in location '{locationName}'.");
        }

        this.locationSpots.Add(newSpot);
        return this;
    }

    public LocationBuilder WithLocationProperties(Action<LocationPropertiesBuilder> buildProperties)
    {
        LocationPropertiesBuilder builder = new LocationPropertiesBuilder();
        buildProperties(builder);

        locationProperties = builder.Build();
        return this;
    }

    public LocationBuilder WithDifficultyLevel(int difficultyLevel)
    {
        this.difficultyLevel = difficultyLevel;
        return this;
    }

    public Location Build()
    {
        return new Location(
            locationType,
            locationName,
            locationArchetype,
            travelConnections,
            locationSpots,
            locationProperties,
            difficultyLevel
        );
    }
}