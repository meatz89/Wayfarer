public class ContentValidationResult
{
    public List<MissingLocationSpotReference> MissingLocationSpots { get; } = new List<MissingLocationSpotReference>();
    public List<MissingConnectedLocationReference> MissingConnectedLocations { get; } = new List<MissingConnectedLocationReference>();

    public void AddMissingLocationSpot(string locationSpotId, Venue referencingLocation)
    {
        MissingLocationSpots.Add(new MissingLocationSpotReference(locationSpotId, referencingLocation));
    }

    public void AddMissingConnectedLocation(string venueId, Venue referencingLocation)
    {
        MissingConnectedLocations.Add(new MissingConnectedLocationReference(venueId, referencingLocation));
    }

    public bool HasMissingReferences => MissingLocationSpots.Count > 0 ||
                MissingConnectedLocations.Count > 0;
}

public class MissingLocationSpotReference
{
    public string LocationSpotId { get; }
    public Venue ReferencingLocation { get; }

    public MissingLocationSpotReference(string locationSpotId, Venue referencingLocation)
    {
        LocationSpotId = locationSpotId;
        ReferencingLocation = referencingLocation;
    }
}

public class MissingConnectedLocationReference
{
    public string VenueId { get; }
    public Venue ReferencingLocation { get; }

    public MissingConnectedLocationReference(string venueId, Venue referencingLocation)
    {
        VenueId = venueId;
        ReferencingLocation = referencingLocation;
    }
}