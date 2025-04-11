
public class LocationSpotBuilder
{
    private string name;
    private string description;
    private string locationName;
    private string? character;

    private List<string> actionIds = new();

    public Illumination? illumination { get; set; }
    public Population? population { get; set; }
    public Economic? economic { get; set; }
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


    public LocationSpotBuilder WithCharacter(string? character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder AddAction(ActionNames actionName)
    {
        this.actionIds.Add(actionName.ToString());
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

    public LocationSpotBuilder WithEconomic(Economic Economic)
    {
        this.economic = Economic;
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
            new LocationSpot(
                name,
                description,
                locationName,
                population,
                economic,
                atmosphere,
                physical,
                illumination,
                actionIds
            );

        if (character != null)
        {
            locationSpot.Character = character;
        }
        return locationSpot;
    }

}

