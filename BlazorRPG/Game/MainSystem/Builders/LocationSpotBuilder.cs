public class LocationSpotBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private LocationNames locationName;
    private CharacterNames? character;
    private LocationProperties properties;

    public LocationSpotBuilder(LocationNames locationName)
    {
        this.locationName = locationName;
    }

    public LocationSpotBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public LocationSpotBuilder ForActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public LocationSpotBuilder WithCharacter(CharacterNames? character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder WithLocationProperties(Action<LocationPropertiesBuilder> buildProperties)
    {
        LocationPropertiesBuilder builder = new LocationPropertiesBuilder();
        buildProperties(builder);

        properties = builder.Build();
        return this;
    }

    public LocationSpot Build()
    {
        // Validation: Ensure name and actionType are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("LocationSpot must have a name.");
        }

        LocationSpot locationSpot = new LocationSpot(name, locationName, actionType, properties);
        if (character != null)
        {
            locationSpot.Character = character;
        }
        return locationSpot;
    }
}

