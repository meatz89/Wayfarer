public class EncounterChoiceTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }
    public List<ValueModification> ValueModifications { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }

    public EncounterChoiceTemplate(
        string name,
        BasicActionTypes actionType,
        ChoiceArchetypes choiceArchetype,
        ChoiceApproaches choiceApproach,
        List<ValueModification> valueModifications,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards
        )
    {
        Name = name;
        ActionType = actionType;
        Archetype = choiceArchetype;
        Approach = choiceApproach;
        ValueModifications = valueModifications;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
    }
}

