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

    public Accessibility? Accessibility { get; set; }
    public LocationType? Engagement { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public RoomLayout? RoomLayout { get; set; }
    public Temperature? Temperature { get; set; }
    public List<ActionNames> ActionNames { get; } = new();
    public List<ActionImplementation> Actions { get; } = new();
    public string Character { get; internal set; }

    public void AddAction(ActionImplementation baseAction)
    {
        Actions.Add(baseAction);
    }

    public bool HasProperty<T>(T locationSpotProperty) where T : struct, Enum // Added constraint for enum
    {
        if (locationSpotProperty is Accessibility accessibility)
        {
            return Accessibility.HasValue && Accessibility == accessibility;
        }
        else if (locationSpotProperty is LocationType engagement)
        {
            return Engagement.HasValue && Engagement == engagement;
        }
        else if (locationSpotProperty is RoomLayout roomLayout)
        {
            return RoomLayout.HasValue && RoomLayout == roomLayout;
        }
        else if (locationSpotProperty is Temperature temperature)
        {
            return Temperature.HasValue && Temperature == temperature;
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
        Accessibility? accessibility,
        LocationType? engagement,
        Atmosphere? atmosphere,
        RoomLayout? roomLayout,
        Temperature? temperature,
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