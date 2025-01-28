public class EncounterChoiceSlotBuilder
{
    private string name;
    private BasicActionTypes actionType;

    private LocationPropertyCondition locationProperty;
    private LocationSpotPropertyCondition locationSpotProperty;
    private PlayerStatusPropertyCondition playerStatusProperty;
    private WorldStatePropertyCondition worldStateProperty;
    private EncounterStateCondition encounterStateProperty;

    public HashSet<(LocationNames, KnowledgeTypes)> LocationKnowledge = new();
    private List<EncounterChoiceTemplate> encounterChoiceTemplates = new();
    private ChoiceSlotPersistence choiceSlotType;

    public EncounterChoiceSlotBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public EncounterChoiceSlotBuilder WithChoiceSlotType(ChoiceSlotPersistence choiceSlotType)
    {
        this.choiceSlotType = choiceSlotType;
        return this;
    }

    public EncounterChoiceSlotBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationProperty(LocationPropertyCondition condition)
    {
        this.locationProperty = condition;
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationArchetype(LocationArchetypes archetype)
    {
        this.locationProperty = new LocationPropertyCondition(LocationPropertyTypes.LocationArchetype, archetype);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationCrowdDensity(CrowdDensity crowdDensity)
    {
        this.locationProperty = new LocationPropertyCondition(LocationPropertyTypes.CrowdDensity, crowdDensity);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationOpportunity(OpportunityTypes opportunityType)
    {
        this.locationProperty = new LocationPropertyCondition(LocationPropertyTypes.Opportunity, opportunityType);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotProperty(LocationSpotPropertyCondition condition)
    {
        this.locationSpotProperty = condition;
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotAccessability(Accessibility accessibility)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Accessibility, accessibility);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotEngagement(Engagement engagement)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Engagement, engagement);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotAtmosphere(Atmosphere atmosphere)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Atmosphere, atmosphere);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotRoomLayout(RoomLayout roomLayout)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.RoomLayout, roomLayout);
        return this;
    }

    public EncounterChoiceSlotBuilder WithLocationSpotTemperature(Temperature temperature)
    {
        this.locationSpotProperty = new LocationSpotPropertyCondition(
            LocationSpotPropertyTypes.Temperature, temperature);
        return this;
    }

    public EncounterChoiceSlotBuilder WithWorldStateProperty(WorldStatePropertyCondition condition)
    {
        this.worldStateProperty = condition;
        return this;
    }

    public EncounterChoiceSlotBuilder WithWorldStateTime(TimeWindows time)
    {
        this.worldStateProperty = new WorldStatePropertyCondition(
            WorldStatusTypes.Time, time);
        return this;
    }

    public EncounterChoiceSlotBuilder WithWorldStateTime(WeatherTypes weather)
    {
        this.worldStateProperty = new WorldStatePropertyCondition(
            WorldStatusTypes.Weather, weather);
        return this;
    }

    public EncounterChoiceSlotBuilder WithPlayerStatusProperty(PlayerStatusPropertyCondition condition)
    {
        this.playerStatusProperty = condition;
        return this;
    }

    public EncounterChoiceSlotBuilder WithPlayerNegativeEffect(PlayerStatusTypes negativeEffect)
    {
        this.playerStatusProperty = new PlayerStatusPropertyCondition(
            PlayerStatusTypes.NegativeEffect, negativeEffect);
        return this;
    }

    public EncounterChoiceSlotBuilder WithPlayerNegativeEffect(PlayerReputationTypes reputationTypes)
    {
        this.playerStatusProperty = new PlayerStatusPropertyCondition(
            PlayerStatusTypes.Reputation, reputationTypes);
        return this;
    }

    public EncounterChoiceSlotBuilder WithEncounterStateCondition(Action<EncounterStateConditionBuilder> buildCondition)
    {
        EncounterStateConditionBuilder builder = new();
        buildCondition(builder);
        encounterStateProperty = builder.Build();
        return this;
    }

    public EncounterChoiceSlotBuilder AddEncounterChoice(Action<EncounterChoiceTemplateBuilder> buildChoiceTemplate)
    {
        EncounterChoiceTemplateBuilder builder = new EncounterChoiceTemplateBuilder();
        buildChoiceTemplate(builder);
        encounterChoiceTemplates.Add(builder.Build());
        return this;
    }

    public EncounterChoiceSlot Build()
    {
        return new EncounterChoiceSlot(
            name,
            actionType,
            locationProperty,
            locationSpotProperty,
            worldStateProperty,
            playerStatusProperty,
            encounterStateProperty,
            encounterChoiceTemplates,
            choiceSlotType
            );
    }
}
