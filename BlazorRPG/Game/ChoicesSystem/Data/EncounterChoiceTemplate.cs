public class EncounterChoiceTemplate
{
    public string Name { get; set; }
    public ChoiceSlotPersistence ChoiceSlotType { get; set; }
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }
    public List<ValueModification> ValueModifications { get; } = new();
    public List<Requirement> Requirements { get; } = new();
    public List<Outcome> Costs { get; } = new();
    public List<Outcome> Rewards { get; } = new();
    public List<EncounterChoiceSlot> ChoiceSlotModifications { get; } = new();
    public EncounterResults? EncounterResults { get; set; }

    public EncounterChoiceTemplate(
        string name,
        ChoiceSlotPersistence choiceSlotType,
        ChoiceArchetypes choiceArchetype,
        ChoiceApproaches choiceApproach,
        List<ValueModification> valueModifications,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards,
        EncounterResults? encounterResult,
        List<EncounterChoiceSlot> choiceSlotModifications
        )
    {
        Name = name;
        ChoiceSlotType = choiceSlotType;
        Archetype = choiceArchetype;
        Approach = choiceApproach;
        ValueModifications = valueModifications;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
        EncounterResults = encounterResult;
        ChoiceSlotModifications = choiceSlotModifications;
    }
}

