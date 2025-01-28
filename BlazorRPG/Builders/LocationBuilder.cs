public class LocationBuilder
{
    private LocationTypes locationType;
    private LocationNames locationName;

    private List<LocationNames> travelConnections = new();
    private List<LocationSpot> locationSpots = new();
    private int difficultyLevel;

    // Location properties
    private LocationArchetypes locationArchetype;
    private CrowdDensity crowdDensity;
    private OpportunityTypes opportunity;

    public LocationBuilder WithArchetype(LocationArchetypes archetype)
    {
        this.locationArchetype = archetype;
        return this;
    }

    public LocationBuilder WithCrowdLevel(CrowdDensity crowdDensity)
    {
        this.crowdDensity = crowdDensity;
        return this;
    }

    public LocationBuilder WithOpportunity(OpportunityTypes opportunity)
    {
        this.opportunity = opportunity;
        return this;
    }

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
            travelConnections,
            locationSpots,
            difficultyLevel,
            locationArchetype,
            crowdDensity,
            opportunity,
            ResourceTypes.None
        );
    }
}