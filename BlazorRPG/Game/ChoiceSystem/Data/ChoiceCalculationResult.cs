public class ChoiceCalculationResult
{
    // Base values and modifications stored separately for UI/preview
    public List<ValueModification> ValueModifications { get; } = new();

    // Energy costs and requirements
    public List<Requirement> Requirements { get; } = new();

    // Costs and rewards
    public List<Outcome> Costs { get; } = new();
    public List<Outcome> Rewards { get; } = new();

    public ChoiceCalculationResult(
        List<ValueModification> valueModifications,
        List<Requirement> requirements,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        ValueModifications = valueModifications;
        Requirements = requirements;
        Costs = costs;
        Rewards = rewards;
    }

    // Helper for execution to get combined changes
    public Dictionary<ValueTypes, int> GetCombinedValues()
    {
        Dictionary<ValueTypes, int> combined = new Dictionary<ValueTypes, int>();

        // Add base values first
        //foreach (BaseValueChange baseChange in BaseValueChanges)
        //{
        //    ChangeTypes changeType = GameRules.ConvertValueTypeToChangeType(baseChange.ValueType);
        //    if (!combined.ContainsKey(changeType))
        //        combined[changeType] = 0;
        //    combined[changeType] += baseChange.Amount;
        //}

        // Add modifications
        foreach (ValueModification modification in ValueModifications)
        {
            ValueTypes changeType;
            if (modification is MomentumModification evm)
            {
                changeType = ValueTypes.Momentum;
            }
            if (modification is PressureModification evp)
            {
                changeType = ValueTypes.Pressure;
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
        return combined;
    }

}