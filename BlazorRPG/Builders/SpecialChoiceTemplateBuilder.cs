
public class SpecialChoiceTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;

    private LocationPropertyCondition locationProperty;
    private LocationSpotPropertyCondition locationSpotProperty;
    private PlayerStatusPropertyCondition playerStatusProperty;
    private WorldStatePropertyCondition worldStateProperty;
    private EncounterStateCondition encounterStateProperty;

    public HashSet<(LocationNames, KnowledgeTypes)> LocationKnowledge = new();

    public SpecialChoiceTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationProperty(LocationPropertyCondition condition)
    {
        this.locationProperty = condition;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationArchetype(LocationArchetypes archetype)
    {
        this.locationProperty = new LocationPropertyCondition(LocationPropertyTypes.LocationArchetype, archetype);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationCrowdDensity(CrowdDensity crowdDensity)
    {
        this.locationProperty = new LocationPropertyCondition(LocationPropertyTypes.CrowdDensity, crowdDensity);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotProperty(LocationSpotPropertyCondition condition)
    {
        this.locationSpotProperty = condition;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotAccessability(Accessibility accessibility)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Accessibility, accessibility);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotEngagement(Engagement engagement)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Engagement, engagement);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotAtmosphere(Atmosphere atmosphere)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Atmosphere, atmosphere);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotRoomLayout(RoomLayout roomLayout)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.RoomLayout, roomLayout);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithLocationSpotTemperature(Temperature temperature)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Temperature, temperature);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithWorldStateProperty(WorldStatePropertyCondition condition)
    {
        this.worldStateProperty = condition;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithWorldStateTime(TimeWindows time)
    {
        this.worldStateProperty = new WorldStatePropertyCondition(
            WorldStatusTypes.Time, time);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithWorldStateTime(WeatherTypes weather)
    {
        this.worldStateProperty = new WorldStatePropertyCondition(
            WorldStatusTypes.Weather, weather);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithPlayerStatusProperty(PlayerStatusPropertyCondition condition)
    {
        this.playerStatusProperty = condition;
        return this;
    }

    public SpecialChoiceTemplateBuilder WithPlayerNegativeEffect(PlayerStatusTypes negativeEffect)
    {
        this.playerStatusProperty = new PlayerStatusPropertyCondition(
            PlayerStatusTypes.NegativeEffect, negativeEffect);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithPlayerNegativeEffect(PlayerReputationTypes reputationTypes)
    {
        this.playerStatusProperty = new PlayerStatusPropertyCondition(
            PlayerStatusTypes.Reputation, reputationTypes);
        return this;
    }

    public SpecialChoiceTemplateBuilder WithEncounterStateCondition(Action<EncounterStateConditionBuilder> buildCondition)
    {
        EncounterStateConditionBuilder builder = new();
        buildCondition(builder);
        encounterStateProperty = builder.Build();
        return this;
    }

    public SpecialChoiceTemplateBuilder RewardsKnowledge(KnowledgeTypes workOpportunity, LocationNames market)
    {
        this.LocationKnowledge.Add((market, workOpportunity));
        return this;
    }

    public SpecialChoiceTemplate Build()
    {
        return new SpecialChoiceTemplate(
            name,
            actionType,
            locationProperty,
            locationSpotProperty,
            worldStateProperty,
            playerStatusProperty,
            encounterStateProperty);
    }

}
