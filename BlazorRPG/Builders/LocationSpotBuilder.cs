
public class LocationSpotBuilder
{
    private string name;
    private string locationName;
    private string? character;

    private List<ActionNames> actionNames = new();

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

    public LocationSpotBuilder WithCharacter(string? character)
    {
        this.character = character;
        return this;
    }

    public LocationSpotBuilder AddAction(ActionNames actionNames)
    {
        this.actionNames.Add(actionNames);
        return this;
    }

    public LocationSpotBuilder WithIllumination(Illumination temperature)
    {
        this.illumination = temperature;
        return this;
    }

    public LocationSpotBuilder WithPopulation(Population accessability)
    {
        this.population = accessability;
        return this;
    }

    public LocationSpotBuilder WithEconomic(Economic engagement)
    {
        this.economic = engagement;
        return this;
    }

    public LocationSpotBuilder WithPhysical(Physical roomLayout)
    {
        this.physical = roomLayout;
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
                locationName,
                population,
                economic,
                atmosphere,
                physical,
                illumination,
                actionNames
            );

        if (character != null)
        {
            locationSpot.Character = character;
        }
        return locationSpot;
    }

}

