public class LocationNarrative
{
    public LocationNames LocationName { get; }
    public string Description { get; }

    public LocationNarrative(LocationNames locationName, string description)
    {
        LocationName = locationName;
        Description = description;
    }
}