public class LocationBuilder
{
    private string name;

    private List<string> travelConnections = new();
    private List<LocationSpot> locationSpots = new();
    private int difficulty;

    // Location properties
    private bool playerKnowledge = true;
    private string description;
    private string detailedDescription;
    private int depth;

    public LocationTypes locationType { get; private set; }

    public LocationBuilder WithName(string name)
    {
        this.name = name.ToString();
        return this;
    }

    public LocationBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public LocationBuilder WithDetailedDescription(string description)
    {
        this.detailedDescription = description;
        return this;
    }

    public LocationBuilder AddTravelConnection(string connectedLocation)
    {
        this.travelConnections.Add(connectedLocation.ToString());
        return this;
    }

    public LocationBuilder AddLocationSpot(Action<LocationSpotBuilder> buildLocationSpot)
    {
        LocationSpotBuilder locationSpotBuilder = new LocationSpotBuilder(name);
        buildLocationSpot(locationSpotBuilder);
        LocationSpot newSpot = locationSpotBuilder.Build();

        // Check for duplicate LocationSpot names within the same Location
        if (locationSpots.Any(spot => spot.Name == newSpot.Name))
        {
            throw new InvalidOperationException($"Duplicate LocationSpot name '{newSpot.Name}' found in location '{name}'.");
        }

        this.locationSpots.Add(newSpot);
        return this;
    }

    public LocationBuilder WithPlayerKnowledge(bool playerKnowledge)
    {
        this.playerKnowledge = playerKnowledge;
        return this;
    }

    public LocationBuilder WithDifficultyLevel(int difficultyLevel)
    {
        this.difficulty = difficultyLevel;
        return this;
    }

    public LocationBuilder WithDepth(int depth)
    {
        this.depth = depth;
        return this;
    }

    public LocationBuilder WithLocationType(LocationTypes locationType)
    {
        this.locationType = locationType;
        return this;
    }

    public Location Build()
    {
        Location location = new Location
        {
            Name = name,
            Description = description,
            DetailedDescription = detailedDescription,
            Difficulty = difficulty,
            Depth = depth,
            LocationType = locationType,
            LocationSpots = locationSpots
        };

        location.PlayerKnowledge = playerKnowledge;

        EnvironmentalPropertyManager.UpdateLocationForTime(location, TimeWindows.Morning);

        return location;
    }
}