public class LocationBuilder
{
    private LocationNames locationName;

    private List<LocationNames> travelConnections = new();
    private List<LocationSpot> locationSpots = new();
    private int difficultyLevel;

    // Location properties
    private bool playerKnowledge;

    public LocationBuilder ForLocation(LocationNames name)
    {
        this.locationName = name;
        return this;
    }

    public LocationBuilder AddTravelResonance(LocationNames resonance)
    {
        this.travelConnections.Add(resonance);
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

    public LocationBuilder WithPlayerKnowledge(bool playerKnowledge)
    {
        this.playerKnowledge = playerKnowledge;
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
            locationName,
            travelConnections,
            locationSpots,
            difficultyLevel,
            playerKnowledge
        );
    }
}