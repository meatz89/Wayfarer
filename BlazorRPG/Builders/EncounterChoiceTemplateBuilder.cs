public class EncounterChoiceTemplateBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private LocationPropertyCondition locationProperty;
    private LocationSpotPropertyCondition locationSpotProperty;
    private PlayerStatusPropertyCondition playerStatusProperty;
    private WorldStatePropertyCondition worldStateProperty;
    private EncounterStateCondition encounterStateProperty;

    public HashSet<(LocationNames, KnowledgeTypes)> LocationKnowledge = new();

    public EncounterChoiceTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public EncounterChoiceTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsKnowledge(KnowledgeTypes workOpportunity, LocationNames market)
    {
        this.LocationKnowledge.Add((market, workOpportunity));
        return this;
    }

    public EncounterChoiceTemplate Build()
    {
        return new EncounterChoiceTemplate(
            name,
            actionType,
            locationProperty,
            locationSpotProperty,
            worldStateProperty,
            playerStatusProperty,
            encounterStateProperty);
    }
}
