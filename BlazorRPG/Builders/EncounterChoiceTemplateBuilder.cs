public class EncounterChoiceTemplateBuilder
{
    private string name;
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private List<ValueModification> valueModifications = new();
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private EncounterResults? encounterResult = EncounterResults.Ongoing;
    private List<EncounterChoiceSlot> modifiedChoiceSlots = new();

    public EncounterChoiceTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public EncounterChoiceTemplateBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.archetype = archetype;
        return this;
    }

    public EncounterChoiceTemplateBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        return this;
    }

    public EncounterChoiceTemplateBuilder RequiresInventorySlots(int slots)
    {
        this.requirements.Add(new InventorySlotsRequirement(slots));
        return this;
    }

    public EncounterChoiceTemplateBuilder ExpendsHealth(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new HealthRequirement(cost));
        costs.Add(new HealthOutcome(cost));
        return this;
    }

    public EncounterChoiceTemplateBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public EncounterChoiceTemplateBuilder ExpendsFood(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(ResourceTypes.Food, cost));
        costs.Add(new ResourceOutcome(ResourceTypes.Food, -cost));
        return this;
    }

    public EncounterChoiceTemplateBuilder ExpendsItem(ResourceTypes item, int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(item, cost));
        costs.Add(new ResourceOutcome(item, -cost));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsResource(ResourceTypes resourceType, int count)
    {
        rewards.Add(new ResourceOutcome(resourceType, count));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsCoins(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsFood(int count)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, count));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsTrust(int count, CharacterNames characterNames)
    {
        return this;
    }

    public EncounterChoiceTemplateBuilder UnlockLocationSpotActions(LocationNames locationName, BasicActionTypes actionType)
    {
        rewards.Add(new InformationOutcome(InformationTypes.ActionOpportunity,
            new ActionOpportunityInformation(locationName, actionType)));

        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsLocationInformation(LocationNames locationName)
    {
        rewards.Add(new InformationOutcome(InformationTypes.Location, new LocationInformation(locationName)));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsHealth(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsEnergy(int count, EnergyTypes energyType)
    {
        rewards.Add(new EnergyOutcome(energyType, count));
        return this;
    }

    public EncounterChoiceTemplateBuilder RewardsReputation(int count)
    {
        rewards.Add(new ReputationOutcome(count));
        return this;
    }

    public EncounterChoiceTemplateBuilder UnlocksAchievement(AchievementTypes achievementType)
    {
        rewards.Add(new AchievementOutcome(achievementType));
        return this;
    }

    public EncounterChoiceTemplateBuilder UnlocksModifiedChoiceSlot(Action<EncounterChoiceSlotBuilder> buildModifiedChoiceSlot)
    {
        EncounterChoiceSlotBuilder builder = new EncounterChoiceSlotBuilder();
        buildModifiedChoiceSlot(builder);
        modifiedChoiceSlots.Add(builder.Build());
        return this;
    }

    public EncounterChoiceTemplateBuilder EndsEndcounter(EncounterResults encounterResult)
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
            encounterResult,
            modifiedChoiceSlots
        );
    }
}
