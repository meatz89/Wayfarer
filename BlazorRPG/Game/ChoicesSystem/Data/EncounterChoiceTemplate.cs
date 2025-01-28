public class EncounterChoiceTemplate
{
    public string Name { get; set; }
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }
    public List<ValueModification> ValueModifications { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }
    public EncounterResults? EncounterResults { get; set; }

    public EncounterChoiceTemplate(
        string name,
        ChoiceArchetypes choiceArchetype,
        ChoiceApproaches choiceApproach,
        List<ValueModification> valueModifications,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards,
        EncounterResults? encounterResult
        )
    {
        Name = name;
        Archetype = choiceArchetype;
        Approach = choiceApproach;
        ValueModifications = valueModifications;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
        EncounterResults = encounterResult;
    }
}

