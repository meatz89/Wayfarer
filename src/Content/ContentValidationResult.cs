public class ContentValidationResult
{
    public List<MissingLocationSpotReference> MissingLocationSpots { get; } = new List<MissingLocationSpotReference>();
    public List<MissingConnectedLocationReference> MissingConnectedLocations { get; } = new List<MissingConnectedLocationReference>();

    public void AddMissingLocationSpot(string locationSpotId, Location referencingLocation)
    {
        MissingLocationSpots.Add(new MissingLocationSpotReference(locationSpotId, referencingLocation));
    }

    public void AddMissingConnectedLocation(string locationId, Location referencingLocation)
    {
        MissingConnectedLocations.Add(new MissingConnectedLocationReference(locationId, referencingLocation));
    }

    public bool HasMissingReferences
    {
        get
        {
            return MissingLocationSpots.Count > 0 ||
                MissingConnectedLocations.Count > 0;
        }
    }
}

public class MissingLocationSpotReference
{
    public string LocationSpotId { get; }
    public Location ReferencingLocation { get; }

    public MissingLocationSpotReference(string locationSpotId, Location referencingLocation)
    {
        LocationSpotId = locationSpotId;
        ReferencingLocation = referencingLocation;
    }
}

public class MissingConnectedLocationReference
{
    public string LocationId { get; }
    public Location ReferencingLocation { get; }

    public MissingConnectedLocationReference(string locationId, Location referencingLocation)
    {
        LocationId = locationId;
        ReferencingLocation = referencingLocation;
    }
}