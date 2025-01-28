public class LocationSpotAvailabilityConditions
{
    public List<LocationPropertyChoiceEffect> ChoiceEffects { get; set; } = new();

    // Action Availability
    public Accessibility? Accessibility { get; set; }
    public bool IsAccessabilitySet { get; private set; } = false;
    public Engagement? Engagement { get; set; }
    public bool IsEngagementSet { get; private set; } = false;
    public Atmosphere? Atmosphere { get; set; }
    public bool IsAtmosphereSet { get; private set; } = false;
    public RoomLayout? RoomLayout { get; set; }
    public bool IsRoomLayoutSet { get; private set; } = false;
    public Temperature? Temperature { get; set; }
    public bool IsTemperatureSet { get; private set; } = false;

    public object GetProperty(LocationSpotPropertyTypes propertyType)
    {
        switch (propertyType)
        {
            case LocationSpotPropertyTypes.Accessibility:
                return Accessibility;
            case LocationSpotPropertyTypes.Engagement:
                return Engagement;
            case LocationSpotPropertyTypes.Atmosphere:
                return Atmosphere;
            case LocationSpotPropertyTypes.RoomLayout:
                return RoomLayout;
            case LocationSpotPropertyTypes.Temperature:
                return Temperature;

            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }

    // SetProperty now also sets the corresponding IsSet flag
    public void SetProperty(LocationSpotPropertyTypes propertyType, object value)
    {
        switch (propertyType)
        {
            case LocationSpotPropertyTypes.Accessibility:
                Accessibility = (Accessibility)value;
                IsAccessabilitySet = true;
                break;
            case LocationSpotPropertyTypes.Engagement:
                Engagement = (Engagement)value;
                IsEngagementSet = true;
                break;
            case LocationSpotPropertyTypes.Atmosphere:
                Atmosphere = (Atmosphere)value;
                IsAtmosphereSet = true;
                break;
            case LocationSpotPropertyTypes.RoomLayout:
                RoomLayout = (RoomLayout)value;
                IsRoomLayoutSet = true;
                break;
            case LocationSpotPropertyTypes.Temperature:
                Temperature = (Temperature)value;
                IsTemperatureSet = true;
                break;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}