public class Location
{
    public LocationTypes LocationType { get; set; } // Industrial/Commercial/etc
    public LocationNames LocationName { get; set; }
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings
    public int Difficulty { get; set; }
    public LocationArchetypes LocationArchetype { get; }
    public CrowdDensity CrowdDensity { get; }
    public OpportunityTypes Opportunity { get; }
    public ResourceTypes ResourceType { get; }
    public bool PlayerKnowledge { get; }

    public Location(
        LocationTypes locationType,
        LocationNames locationName,
        List<LocationNames> travelConnections,
        List<LocationSpot> locationSpots,
        int difficultyLevel,
        LocationArchetypes locationArchetype,
        CrowdDensity crowdDensity,
        OpportunityTypes opportunity,
        ResourceTypes resourceTypes,
        bool playerKnowledge
        )
    {
        LocationType = locationType;
        LocationName = locationName;
        TravelConnections = travelConnections;
        LocationSpots = locationSpots;
        Difficulty = difficultyLevel;
        LocationArchetype = locationArchetype;
        CrowdDensity = crowdDensity;
        Opportunity = opportunity;
        ResourceType = resourceTypes;
        PlayerKnowledge = playerKnowledge;
    }

    public bool HasProperty<T>(T locationProperty)
    {
        if (locationProperty is LocationArchetypes archetype)
        {
            return LocationArchetype == archetype;
        }
        else if (locationProperty is ResourceTypes resource)
        {
            return ResourceType == resource;
        }
        else if (locationProperty is CrowdDensity density)
        {
            return CrowdDensity == density;
        }
        else if (locationProperty is OpportunityTypes scale)
        {
            return Opportunity == scale;
        }
        else
        {
            // Handle cases where the provided property type is not supported.
            // You might throw an exception, return false, or log an error.
            throw new ArgumentException($"Unsupported property type: {typeof(T)}");
        }
    }
}
