public class LocationSpot
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; }  // "Character", "Quest", "Shop", "Feature", etc.
    public List<string> ActionIds { get; set; } = new();

    // Connections
    public string LocationName { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    // Interaction
    public string InteractionDescription { get; set; }

    // Visual/positioning data (for map display)
    public string Position { get; set; }  // "North", "Center", "Southeast", etc.

    public Population? Population { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Physical? Physical { get; set; }
    public Illumination? Illumination { get; set; }
    public string Character { get; set; }

    public bool HasProperty<T>(T locationSpotProperty) where T : IEnvironmentalProperty
    {
        if (locationSpotProperty is Population Population)
        {
            return Population != null && Population == Population;
        }
        else if (locationSpotProperty is Physical Physical)
        {
            return Physical != null && Physical == Physical;
        }
        else if (locationSpotProperty is Illumination Illumination)
        {
            return Illumination != null && Illumination == Illumination;
        }
        else
        {
            // You can handle other types or throw an exception if needed
            throw new ArgumentException($"Unsupported property type: {typeof(T)}");
        }
    }

    public LocationSpot()
    {
    }

    public LocationSpot(
        string name,
        string description,
        string locationName,
        Population? population,
        Atmosphere? atmosphere,
        Physical? physical,
        Illumination? illumination,
        List<string> actionIds)
    {
        Name = name;
        Description = description;
        LocationName = locationName;
        Population = population;
        Atmosphere = atmosphere;
        Physical = physical;
        Illumination = illumination;
        ActionIds = actionIds;
    }
}