public class ContentValidationResult
{
public List<MissingLocationReference> MissingLocations { get; } = new List<MissingLocationReference>();
public List<MissingConnectedLocationReference> MissingConnectedLocations { get; } = new List<MissingConnectedLocationReference>();

public void AddMissingLocation(string locationId, Venue referencingLocation)
{
    MissingLocations.Add(new MissingLocationReference(locationId, referencingLocation));
}

public void AddMissingConnectedLocation(string venueId, Venue referencingLocation)
{
    MissingConnectedLocations.Add(new MissingConnectedLocationReference(venueId, referencingLocation));
}

public bool HasMissingReferences => MissingLocations.Count > 0 ||
            MissingConnectedLocations.Count > 0;
}

public class MissingLocationReference
{
public string LocationId { get; }
public Venue ReferencingLocation { get; }

public MissingLocationReference(string locationId, Venue referencingLocation)
{
    LocationId = locationId;
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