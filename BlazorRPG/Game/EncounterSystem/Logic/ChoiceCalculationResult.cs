public class ChoiceCalculationResult
{
    // The new state values after applying the choice
    public EncounterStateValues NewStateValues { get; }

    // All changes that will be applied
    public List<ValueChange> ValueChanges { get; }
    public EnergyTypes EnergyTypes { get; }
    public int EnergyCost { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }

    public ChoiceCalculationResult(
        EncounterStateValues newStateValues,
        List<ValueChange> valueChanges,
        EnergyTypes energyTypes,
        int energyCost,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        NewStateValues = newStateValues;
        ValueChanges = valueChanges;
        EnergyTypes = energyTypes;
        EnergyCost = energyCost;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
    }

    // Helper to check if the result is valid
    public bool IsValid => Requirements.Count > 0 && ValueChanges.Count > 0;
}
