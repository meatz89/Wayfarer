public class EncounterChoiceBuilder
{
    private string name;
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private List<ValueModification> valueModifications = new();
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private EncounterResults? encounterResult = EncounterResults.Ongoing;

    private KnowledgeTags? requiredKnowledge;
    private int requiredKnowledgeLevel;
    private KnowledgeTags? rewardKnowledge;

    public EncounterChoiceBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public EncounterChoiceBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.archetype = archetype;
        return this;
    }

    public EncounterChoiceBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        return this;
    }

    public EncounterChoiceBuilder RequiresInventorySlots(int slots)
    {
        this.requirements.Add(new InventorySlotsRequirement(slots));
        return this;
    }

    public EncounterChoiceBuilder RequiresKnowledge(KnowledgeTags tag, int level = 1)
    {
        requiredKnowledge = tag;
        requiredKnowledgeLevel = level;
        requirements.Add(new KnowledgeRequirement(requiredKnowledge.Value, requiredKnowledgeLevel));
        return this;
    }

    public EncounterChoiceBuilder RewardsKnowledge(KnowledgeTags knowledgeTag, KnowledgeCategories knowledgeCategory)
    {
        rewards.Add(new KnowledgeOutcome(knowledgeTag, knowledgeCategory));
        return this;
    }

    public EncounterChoiceBuilder ExpendsHealth(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new HealthRequirement(cost));
        costs.Add(new HealthOutcome(cost));
        return this;
    }

    public EncounterChoiceBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public EncounterChoiceBuilder ExpendsFood(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(ResourceTypes.Food, cost));
        costs.Add(new ResourceOutcome(ResourceTypes.Food, -cost));
        return this;
    }

    public EncounterChoiceBuilder ExpendsItem(ResourceTypes item, int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(item, cost));
        costs.Add(new ResourceOutcome(item, -cost));
        return this;
    }

    public EncounterChoiceBuilder RewardsResource(ResourceTypes resourceType, int count)
    {
        rewards.Add(new ResourceOutcome(resourceType, count));
        return this;
    }

    public EncounterChoiceBuilder RewardsCoins(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public EncounterChoiceBuilder RewardsFood(int count)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, count));
        return this;
    }

    public EncounterChoiceBuilder RewardsTrust(int count, CharacterNames characterNames)
    {
        return this;
    }

    public EncounterChoiceBuilder UnlockLocationSpotActions(LocationNames locationName, BasicActionTypes actionType)
    {
        rewards.Add(new InformationOutcome(InformationTypes.ActionOpportunity,
            new ActionOpportunityInformation(locationName, actionType)));

        return this;
    }

    public EncounterChoiceBuilder RewardsLocationInformation(LocationNames locationName)
    {
        rewards.Add(new InformationOutcome(InformationTypes.Location, new LocationInformation(locationName)));
        return this;
    }

    public EncounterChoiceBuilder RewardsHealth(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public EncounterChoiceBuilder RewardsEnergy(int count, EnergyTypes energyType)
    {
        rewards.Add(new EnergyOutcome(energyType, count));
        return this;
    }

    public EncounterChoiceBuilder RewardsReputation(int count)
    {
        rewards.Add(new ReputationOutcome(count));
        return this;
    }

    public EncounterChoiceBuilder UnlocksAchievement(AchievementTypes achievementType)
    {
        rewards.Add(new AchievementOutcome(achievementType));
        return this;
    }

    public EncounterChoiceBuilder EndsEndcounter(EncounterResults encounterResult)
    {
        this.encounterResult = encounterResult;
        return this;
    }

    public EncounterChoiceTemplate Build()
    {
        return new EncounterChoiceTemplate(
            name,
            archetype,
            approach,
            valueModifications,
            requirements,
            costs,
            rewards,
            encounterResult
        );
    }
}
