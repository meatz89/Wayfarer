public class LocationSpot
{
    public string Name { get; set; }
    public LocationNames LocationName { get; set; }
    public List<ActionImplementation> Actions { get; set; } = new();
    public CharacterNames? Character { get; set; }

    public Accessibility Accessibility { get; set; }
    public Engagement Engagement { get; set; }
    public Atmosphere Atmosphere { get; set; }
    public RoomLayout RoomLayout { get; set; }
    public Temperature Temperature { get; set; }

    public void AddAction(ActionImplementation action)
    {
        Actions.Add(action);
    }

    public bool HasProperty<T>(T locationSpotProperty) where T : struct, Enum // Added constraint for enum
    {
        if (locationSpotProperty is Accessibility accessibility)
        {
            return Accessibility == accessibility;
        }
        else if (locationSpotProperty is Engagement engagement)
        {
            return Engagement == engagement;
        }
        else if (locationSpotProperty is Atmosphere atmosphere)
        {
            return Atmosphere == atmosphere;
        }
        else if (locationSpotProperty is RoomLayout roomLayout)
        {
            return RoomLayout == roomLayout;
        }
        else if (locationSpotProperty is Temperature temperature)
        {
            return Temperature == temperature;
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
        Accessibility accessibility,
        Engagement engagement,
        Atmosphere atmosphere,
        RoomLayout roomLayout,
        Temperature temperature
        )
    {
        Name = name;
        LocationName = locationName;
        Accessibility = accessibility;
        Engagement = engagement;
        Atmosphere = atmosphere;
        RoomLayout = roomLayout;
        Temperature = temperature;
    }
}