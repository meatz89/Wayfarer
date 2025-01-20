public class ChoiceCalculationResult
{
    // The new state after applying all changes
    public EncounterValues NewStateValues { get; }

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
        EncounterValues newStateValues,
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
    public Dictionary<ChangeTypes, int> GetCombinedValues()
    {
        Dictionary<ChangeTypes, int> combined = new Dictionary<ChangeTypes, int>();

        // Add base values first
        foreach (BaseValueChange baseChange in BaseValueChanges)
        {
            ChangeTypes changeType = GameRules.ConvertValueTypeToChangeType(baseChange.ValueType);
            if (!combined.ContainsKey(changeType))
                combined[changeType] = 0;
            combined[changeType] += baseChange.Amount;
        }

        // Add modifications
        foreach (ValueModification modification in ValueModifications)
        {
            ChangeTypes changeType;
            if (modification is EncounterValueModification evm)
            {
                changeType = GameRules.ConvertValueTypeToChangeType(evm.ValueType);
            }
            else if (modification is EnergyCostReduction em)
            {
                changeType = GameRules.ConvertEnergyTypeToChangeType(em.EnergyType);
            }
            else
            {
                throw new InvalidOperationException("Unknown ValueModification type.");
            }

            if (!combined.ContainsKey(changeType))
                combined[changeType] = 0;
            combined[changeType] += modification.Amount;
        }

        // Subtract energy cost
        ChangeTypes energyChangeType = GameRules.ConvertEnergyTypeToChangeType(EnergyType);
        if (!combined.ContainsKey(energyChangeType))
            combined[energyChangeType] = 0;
        combined[energyChangeType] -= EnergyCost;

        return combined;
    }

}