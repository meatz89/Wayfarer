public class ActionTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;

    private LocationPropertyCondition locationPropertyCondition;
    private LocationSpotPropertyCondition locationSpotPropertyCondition;
    private WorldStatePropertyCondition worldStatePropertyCondition;
    private PlayerStatusPropertyCondition playerStatusPropertyCondition;

    public ActionTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionTemplateBuilder SetWeather(WeatherTypes weatherTypes)
    {
        this.worldStatePropertyCondition = new WorldStatePropertyCondition
            (WorldStatusTypes.Weather, weatherTypes);

        return this;
    }

    public ActionTemplateBuilder SetTimeSlot(TimeWindows timeSlot)
    {
        this.worldStatePropertyCondition = new WorldStatePropertyCondition
            (WorldStatusTypes.Time, timeSlot);

        return this;
    }

    public ActionTemplateBuilder SetPlayerNegativeEffect(PlayerNegativeStatus playerNegativeStatus)
    {
        this.playerStatusPropertyCondition = new PlayerStatusPropertyCondition
            (PlayerStatusTypes.NegativeEffect, playerNegativeStatus);

        return this;
    }

    public ActionTemplateBuilder SetPlayerReputationType(PlayerReputationTypes playerReputation)
    {
        this.playerStatusPropertyCondition = new PlayerStatusPropertyCondition
            (PlayerStatusTypes.Reputation, playerReputation);

        return this;
    }

    public ActionTemplateBuilder SetLocationArchetype(LocationArchetypes archetype)
    {
        this.locationPropertyCondition = new LocationPropertyCondition(
            LocationPropertyTypes.LocationArchetype, archetype);

        return this;
    }

    public ActionTemplateBuilder SetCrowdDensity(CrowdDensity crowdDensity)
    {
        this.locationPropertyCondition = new LocationPropertyCondition(
            LocationPropertyTypes.CrowdDensity, crowdDensity);

        return this;
    }

    public ActionTemplateBuilder SetLocationScale(LocationScale locationScale)
    {
        this.locationPropertyCondition = new LocationPropertyCondition(
            LocationPropertyTypes.LocationScale, locationScale);

        return this;
    }

    public ActionTemplateBuilder WithAccessibility(Accessibility accessibility)
    {
        this.locationSpotPropertyCondition = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Accessibility, accessibility);

        return this;
    }

    public ActionTemplateBuilder WithEngagement(Engagement engagement)
    {
        this.locationSpotPropertyCondition = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Engagement, engagement);

        return this;
    }

    public ActionTemplateBuilder WithAtmosphere(Atmosphere atmosphere)
    {
        this.locationSpotPropertyCondition = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Atmosphere, atmosphere);

        return this;
    }

    public ActionTemplateBuilder WithRoomLayout(RoomLayout roomLayout)
    {
        this.locationSpotPropertyCondition = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.RoomLayout, roomLayout);

        return this;
    }

    public ActionTemplateBuilder WithTemperature(Temperature temperature)
    {
        this.locationSpotPropertyCondition = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Temperature, temperature);

        return this;
    }

    public ActionTemplate Build()
    {
        // Add validation to ensure required properties are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("ActionTemplate must have a name.");
        }
        return new ActionTemplate(
            name,
            actionType,
            locationPropertyCondition,
            locationSpotPropertyCondition,
            worldStatePropertyCondition,
            playerStatusPropertyCondition
        );
    }
}