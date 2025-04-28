public class ContentValidationResult
{
    public List<MissingLocationReference> MissingLocations { get; } = new List<MissingLocationReference>();
    public List<MissingActionReference> MissingActions { get; } = new List<MissingActionReference>();
    public List<MissingConnectedLocationReference> MissingConnectedLocations { get; } = new List<MissingConnectedLocationReference>();

    public void AddMissingLocation(string locationId, LocationSpot referencingSpot)
    {
        MissingLocations.Add(new MissingLocationReference(locationId, referencingSpot));
    }

    public void AddMissingAction(string actionId, LocationSpot referencingSpot)
    {
        MissingActions.Add(new MissingActionReference(actionId, referencingSpot));
    }

    public void AddMissingConnectedLocation(string locationId, Location referencingLocation)
    {
        MissingConnectedLocations.Add(new MissingConnectedLocationReference(locationId, referencingLocation));
    }

    public bool HasMissingReferences
    {
        get
        {
            return MissingLocations.Count > 0 ||
                MissingActions.Count > 0 ||
                MissingConnectedLocations.Count > 0;
        }
    }
}

public class MissingLocationReference
{
    public string LocationId { get; }
    public LocationSpot ReferencingSpot { get; }

    public MissingLocationReference(string locationId, LocationSpot referencingSpot)
    {
        LocationId = locationId;
        ReferencingSpot = referencingSpot;
    }
}

public class MissingActionReference
{
    public string ActionId { get; }
    public LocationSpot ReferencingSpot { get; }

    public MissingActionReference(string actionId, LocationSpot referencingSpot)
    {
        ActionId = actionId;
        ReferencingSpot = referencingSpot;
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