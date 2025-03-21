public class LocationSpot
{
    public string Name { get; set; }
    public LocationNames LocationName { get; set; }
    public CharacterNames? Character { get; set; }
    public Accessibility? Accessibility { get; set; }
    public LocationType? Engagement { get; set; }
    public Atmosphere? Atmosphere { get; set; }
    public RoomLayout? RoomLayout { get; set; }
    public Temperature? Temperature { get; set; }
    public List<ActionNames> ActionNames { get; } = new();
    public List<ActionImplementation> Actions { get; } = new();

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
    public LocationSpot(
        string name,
        LocationNames locationName,
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