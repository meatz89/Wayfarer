public class EncounterChoice
{
    // Core properties
    public int Index { get; }
    public string Description { get; }
    public ChoiceArchetypes Archetype { get; }
    public ChoiceApproaches Approach { get; }
    public EnergyTypes EnergyType { get; }
    public int EnergyCost { get; set; }

    // Value changes
    public List<BaseValueChange> BaseEncounterValueChanges { get; set; } = new();
    public List<ValueModification> ValueModifications { get; set; } = new();

    // Requirements and outcomes
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; } = new();
    public List<Outcome> Rewards { get; set; } = new();

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

    // For execution - just raw numbers for applying changes
    public List<CombinedValue> GetCombinedValues()
    {
        List<CombinedValue> combined = new List<CombinedValue>();

        // Add base values first
        foreach (BaseValueChange baseChange in BaseEncounterValueChanges)
        {
            AddToCombinedValues(combined, ConvertValueTypeToChangeType(baseChange.ValueType), baseChange.Amount);
        }

        // Add modifications
        foreach (ValueModification modification in ValueModifications)
        {
            if (modification is EncounterValueModification evm)
            {
                AddToCombinedValues(combined, ConvertValueTypeToChangeType(evm.ValueType), evm.Amount);
            }
            else if (modification is EnergyCostReduction em)
            {
                AddToCombinedValues(combined, ConvertEnergyTypeToChangeType(em.EnergyType), em.Amount);
            }
        }

        // Add Energy Cost as a negative modification
        AddToCombinedValues(combined, ConvertEnergyTypeToChangeType(EnergyType), -EnergyCost);


        return combined;
    }

    public List<DetailedChange> GetDetailedChanges()
    {
        List<DetailedChange> combined = new List<DetailedChange>();

        // Add base changes
        foreach (BaseValueChange change in BaseEncounterValueChanges)
        {
            AddDetailedChange(combined, ConvertValueTypeToChangeType(change.ValueType), "Base", change.Amount);
        }

        // Add modifications
        foreach (ValueModification change in ValueModifications)
        {
            if (change is EncounterValueModification evm)
            {
                AddDetailedChange(combined, ConvertValueTypeToChangeType(evm.ValueType), change.Source, change.Amount);
            }
            else if (change is EnergyCostReduction em)
            {
                AddDetailedChange(combined, ConvertEnergyTypeToChangeType(em.EnergyType), change.Source, change.Amount);
            }
        }

        // Add Energy Cost as a negative modification
        if(EnergyCost > 0)
            AddDetailedChange(combined, ConvertEnergyTypeToChangeType(EnergyType), "Base", -EnergyCost);

        return combined;
    }


    // Helper methods for adding to combined values and detailed changes
    private void AddToCombinedValues(List<CombinedValue> combined, ChangeTypes changeType, int amount)
    {
        bool found = false;
        foreach (CombinedValue cv in combined)
        {
            if (cv.ChangeType == changeType)
            {
                cv.Amount += amount;
                found = true;
                break;
            }
        }

        if (!found)
        {
            combined.Add(new CombinedValue { ChangeType = changeType, Amount = amount });
        }
    }

    private void AddDetailedChange(List<DetailedChange> combined, ChangeTypes changeType, string source, int amount)
    {
        bool found = false;
        foreach (DetailedChange dc in combined)
        {
            if (dc.ChangeType == changeType)
            {
                dc.ChangeValues.TotalAmount += amount;
                dc.ChangeValues.Sources.Add($"{source}: {(amount >= 0 ? "+" : "")}{amount}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            combined.Add(new DetailedChange
            {
                ChangeType = changeType,
                ChangeValues = new ChangeValues
                {
                    TotalAmount = amount,
                    Sources = new List<string> { $"{source}: {(amount >= 0 ? "+" : "")}{amount}" }
                }
            });
        }
    }

    // Conversion methods
    private ChangeTypes ConvertValueTypeToChangeType(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Outcome => ChangeTypes.Outcome,
            ValueTypes.Momentum => ChangeTypes.Momentum,
            ValueTypes.Insight => ChangeTypes.Insight,
            ValueTypes.Resonance => ChangeTypes.Resonance,
            ValueTypes.Pressure => ChangeTypes.Pressure,
            _ => throw new ArgumentException("Invalid ValueType")
        };
    }

    private ChangeTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ChangeTypes.PhysicalEnergy,
            EnergyTypes.Focus => ChangeTypes.FocusEnergy,
            EnergyTypes.Social => ChangeTypes.SocialEnergy,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }
}
