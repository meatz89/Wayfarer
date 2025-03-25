public class LocationSpot
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }

    // Connections
    public string LocationName { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    // Interaction
    public string InteractionType { get; set; }  // "Character", "Quest", "Shop", "Feature", etc.
    public string InteractionDescription { get; set; }

    // Visual/positioning data (for map display)
    public string Position { get; set; }  // "North", "Center", "Southeast", etc.

    public Population? Accessibility { get; set; }
    public Economic? Engagement { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public Physical? RoomLayout { get; set; }
    public Illumination? Temperature { get; set; }
    public List<ActionNames> ActionNames { get; } = new();
    public List<ActionImplementation> Actions { get; } = new();
    public string Character { get; internal set; }

    public void AddAction(ActionImplementation baseAction)
    {
        Actions.Add(baseAction);
    }

    public bool HasProperty<T>(T locationSpotProperty) where T : IEnvironmentalProperty
    {
        if (locationSpotProperty is Population accessibility)
        {
            return Accessibility != null && Accessibility == accessibility;
        }
        else if (locationSpotProperty is Economic engagement)
        {
            return Engagement != null && Engagement == engagement;
        }
        else if (locationSpotProperty is Physical roomLayout)
        {
            return RoomLayout != null && RoomLayout == roomLayout;
        }
        else if (locationSpotProperty is Illumination temperature)
        {
            return Temperature != null && Temperature == temperature;
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
        string locationName,
        Population? accessibility,
        Economic? engagement,
        Atmosphere? atmosphere,
        Physical? roomLayout,
        Illumination? temperature,
        List<ActionNames> actionNames)
    {
        Name = name;
        LocationName = locationName;
        Accessibility = accessibility;
        Engagement = engagement;
        Atmosphere = atmosphere;
        RoomLayout = roomLayout;
        Temperature = temperature;
        ActionNames = actionNames;
    }
}