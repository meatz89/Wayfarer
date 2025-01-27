public class LocationSpotBuilder
{
    private string name;
    private LocationNames locationName;
    private CharacterNames? character;

    public Accessibility? accessibility { get; private set; }
    public Engagement? engagement { get; private set; }
    public Atmosphere? atmosphere { get; private set; }
    public RoomLayout? roomLayout { get; private set; }
    public Temperature? temperature { get; private set; }

    public LocationSpotBuilder(LocationNames locationName)
    {
        this.locationName = locationName;
    }

    public LocationSpotBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public LocationSpotBuilder WithCharacter(CharacterNames? character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder WithAccessibility(Accessibility accessability)
    {
        accessibility = accessability;
        return this;
    }

    public LocationSpotBuilder WithEngagement(Engagement engagement)
    {
        this.engagement = engagement;
        return this;
    }

    public LocationSpotBuilder WithAtmosphere(Atmosphere socialDynamics)
    {
        atmosphere = socialDynamics;
        return this;
    }

    public LocationSpotBuilder WithRoomLayout(RoomLayout roomLayout)
    {
        this.roomLayout = roomLayout;
        return this;
    }

    public LocationSpotBuilder WithTemperature(Temperature temperature)
    {
        this.temperature = temperature;
        return this;
    }

    public LocationSpot Build()
    {
        // Validation: Ensure name and actionType are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("LocationSpot must have a name.");
        }

        LocationSpot locationSpot =
            new LocationSpot(
                name,
                locationName,
                accessibility,
                engagement,
                atmosphere,
                roomLayout,
                temperature
            );

        if (character != null)
        {
            locationSpot.Character = character;
        }
        return locationSpot;
    }
}

