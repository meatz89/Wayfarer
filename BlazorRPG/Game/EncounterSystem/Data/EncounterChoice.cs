public class EncounterChoice
{
    // Core properties
    public int Index { get; }
    public string Description { get; }
    public ChoiceArchetypes Archetype { get; }
    public ChoiceApproaches Approach { get; }
    public EnergyTypes EnergyType { get; }

    // Value changes
    public List<BaseValueChange> BaseEncounterValueChanges { get; set; } = new();
    public List<ValueModification> ValueModifications { get; set; } = new();

    // Requirements and outcomes
    public List<Requirement> ModifiedRequirements { get; set; } = new();
    public List<Outcome> ModifiedCosts { get; set; } = new();
    public List<Outcome> ModifiedRewards { get; set; } = new();
    public int EnergyCost { get; set; }

    // For execution - just raw numbers for applying changes
    public Dictionary<ValueTypes, int> GetCombinedValues()
    {
        Dictionary<ValueTypes, int> combined = new Dictionary<ValueTypes, int>();

        // Add base values first 
        foreach (BaseValueChange baseChange in BaseEncounterValueChanges)
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

    public Dictionary<ValueTypes, (int TotalAmount, List<string> Sources)> GetDetailedChanges()
    {
        Dictionary<ValueTypes, (int Amount, List<string> Sources)> combined = new Dictionary<ValueTypes, (int Amount, List<string> Sources)>();

        // Add base changes
        foreach (BaseValueChange change in BaseEncounterValueChanges)
        {
            if (!combined.ContainsKey(change.ValueType))
                combined[change.ValueType] = (0, new List<string>());

            (int amount, List<string> sources) = combined[change.ValueType];
            combined[change.ValueType] = (amount + change.Amount, sources);
            sources.Add($"Base: {(change.Amount >= 0 ? "+" : "")}{change.Amount}");
        }

        // Add modifications
        foreach (ValueModification change in ValueModifications)
        {
            if (!combined.ContainsKey(change.ValueType))
                combined[change.ValueType] = (0, new List<string>());

            (int amount, List<string> sources) = combined[change.ValueType];
            combined[change.ValueType] = (amount + change.Amount, sources);
            sources.Add($"{change.Source}: {(change.Amount >= 0 ? "+" : "")}{change.Amount}");
        }

        return combined;
    }

    // Constructor remains the same
    public EncounterChoice(
        int index,
        string description,
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        bool requireTool,
        bool requireKnowledge,
        bool requireReputation)
    {
        Index = index;
        Description = description;
        Archetype = archetype;
        Approach = approach;
        EnergyType = archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }
}