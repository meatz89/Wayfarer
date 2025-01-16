public class ChoiceCalculationResult
{
    // The new state after applying all changes
    public EncounterStateValues NewStateValues { get; }

    // Base values and modifications stored separately for UI/preview
    public List<BaseValueChange> BaseValueChanges { get; }
    public List<ValueModification> ValueModifications { get; }

    // Energy costs and requirements
    public EnergyTypes EnergyType { get; }
    public int EnergyCost { get; }
    public List<Requirement> Requirements { get; }

    // Costs and rewards
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }

    public ChoiceCalculationResult(
        EncounterStateValues newStateValues,
        List<BaseValueChange> baseValueChanges,
        List<ValueModification> valueModifications,
        EnergyTypes energyType,
        int energyCost,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        NewStateValues = newStateValues;
        BaseValueChanges = baseValueChanges;
        ValueModifications = valueModifications;
        EnergyType = energyType;
        EnergyCost = energyCost;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
    }

    // Helper to check if the result is valid (has either base changes or modifications)
    public bool IsValid => (BaseValueChanges.Count > 0 || ValueModifications.Count > 0) && Requirements.Count > 0;

    // Helper for execution to get combined changes
    public Dictionary<ValueTypes, int> GetCombinedValues()
    {
        Dictionary<ValueTypes, int> combined = new Dictionary<ValueTypes, int>();

        // Add base values first
        foreach (BaseValueChange baseChange in BaseValueChanges)
        {
            if (!combined.ContainsKey(baseChange.ValueType))
                combined[baseChange.ValueType] = 0;
            combined[baseChange.ValueType] += baseChange.Amount;
        }

        // Add modifications
        foreach (ValueModification modification in ValueModifications)
        {
            if (!combined.ContainsKey(modification.ValueType))
                combined[modification.ValueType] = 0;
            combined[modification.ValueType] += modification.Amount;
        }

        return combined;
    }
}