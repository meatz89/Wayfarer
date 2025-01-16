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
    public List<CombinedValue> GetCombinedValues()
    {
        List<CombinedValue> combined = new List<CombinedValue>();

        // Add base values first 
        foreach (BaseValueChange baseChange in BaseEncounterValueChanges)
        {
            bool found = false;
            foreach (CombinedValue cv in combined)
            {
                if (cv.ValueType == baseChange.ValueType)
                {
                    cv.Amount += baseChange.Amount;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                combined.Add(new CombinedValue { ValueType = baseChange.ValueType, Amount = baseChange.Amount });
            }
        }

        // Add modifications
        foreach (ValueModification modification in ValueModifications)
        {
            bool found = false;
            foreach (CombinedValue cv in combined)
            {
                if (cv.ValueType == modification.ValueType)
                {
                    cv.Amount += modification.Amount;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                combined.Add(new CombinedValue { ValueType = modification.ValueType, Amount = modification.Amount });
            }
        }

        return combined;
    }

    public List<DetailedChange> GetDetailedChanges()
    {
        List<DetailedChange> combined = new List<DetailedChange>();

        // Add base changes
        foreach (BaseValueChange change in BaseEncounterValueChanges)
        {
            bool found = false;
            foreach (DetailedChange dc in combined)
            {
                if (dc.ValueType == change.ValueType)
                {
                    dc.ChangeValues.TotalAmount += change.Amount;
                    dc.ChangeValues.Sources.Add($"Base: {(change.Amount >= 0 ? "+" : "")}{change.Amount}");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                combined.Add(new DetailedChange
                {
                    ValueType = change.ValueType,
                    ChangeValues = new ChangeValues
                    {
                        TotalAmount = change.Amount,
                        Sources = new List<string> { $"Base: {(change.Amount >= 0 ? "+" : "")}{change.Amount}" }
                    }
                });
            }
        }

        // Add modifications
        foreach (ValueModification change in ValueModifications)
        {
            bool found = false;
            foreach (DetailedChange dc in combined)
            {
                if (dc.ValueType == change.ValueType)
                {
                    dc.ChangeValues.TotalAmount += change.Amount;
                    dc.ChangeValues.Sources.Add($"{change.Source}: {(change.Amount >= 0 ? "+" : "")}{change.Amount}");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                combined.Add(new DetailedChange
                {
                    ValueType = change.ValueType,
                    ChangeValues = new ChangeValues
                    {
                        TotalAmount = change.Amount,
                        Sources = new List<string> { $"{change.Source}: {(change.Amount >= 0 ? "+" : "")}{change.Amount}" }
                    }
                });
            }
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


public class DetailedChange
{
    public ValueTypes ValueType { get; set; }
    public ChangeValues ChangeValues { get; set; }
}

public class ChangeValues
{
    public int TotalAmount { get; set; }
    public List<string> Sources { get; set; } = new List<string>();
}

public class CombinedValue
{
    public ValueTypes ValueType { get; set; }
    public int Amount { get; set; }
}