public class LocationSpotBuilder
{
    private string name;
    private string description;
    private string locationName;
    private string? character;
    private bool playerKnowledge = true;

    private List<string> actionIds = new();
    private List<string> connectedTo = new();

    public Illumination? illumination { get; set; }
    public Population? population { get; set; }
    public Physical? physical { get; set; }
    public Atmosphere? atmosphere { get; set; }

    public LocationSpotBuilder(string locationName)
    {
        this.locationName = locationName;
    }

    public LocationSpotBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }
    public LocationSpotBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public LocationSpotBuilder WithPlayerKnowledge(bool playerKnowledge)
    {
        this.playerKnowledge = playerKnowledge;
        return this;
    }

    public LocationSpotBuilder WithCharacter(string? character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder AddActionId(string actionName)
    {
        this.actionIds.Add(actionName);
        return this;
    }

    public LocationSpotBuilder AddConnectionTo(string locationName)
    {
        this.connectedTo.Add(locationName);
        return this;
    }

    public LocationSpotBuilder WithIllumination(Illumination Illumination)
    {
        this.illumination = Illumination;
        return this;
    }

    public LocationSpotBuilder WithPopulation(Population accessability)
    {
        this.population = accessability;
        return this;
    }

    public LocationSpotBuilder WithPhysical(Physical Physical)
    {
        this.physical = Physical;
        return this;
    }

    public LocationSpotBuilder WithAtmosphere(Atmosphere socialDynamics)
    {
        this.atmosphere = socialDynamics;
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
            new LocationSpot(name, locationName)
            {
                Description = description,
                Population = population ?? Population.Quiet,
                Atmosphere = atmosphere ?? Atmosphere.Calm,
                Physical = physical ?? Physical.Confined,
                Illumination = illumination ?? Illumination.Bright,
                PlayerKnowledge = playerKnowledge,
            };

        if (character != null)
        {
            locationSpot.CharacterName = character;
        }
        return locationSpot;
    }

}

