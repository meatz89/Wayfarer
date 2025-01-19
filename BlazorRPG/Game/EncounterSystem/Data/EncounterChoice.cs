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

    public ChoiceCalculationResult CalculationResult { get; set; }

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

    // Get Combined Values from Calculation Result
    public List<CombinedValue> GetCombinedValues()
    {
        if (CalculationResult == null)
        {
            return new List<CombinedValue>(); // Or handle it appropriately, e.g., throw an exception
        }

        return CalculationResult.GetCombinedValues()
            .Select(cv => new CombinedValue { ChangeType = cv.Key, Amount = cv.Value })
            .ToList();
    }

    public List<DetailedRequirement> GetDetailedRequirements(PlayerState playerState)
    {
        if (CalculationResult == null)
        {
            return new List<DetailedRequirement>();
        }

        return CalculationResult.Requirements.Select(req => new DetailedRequirement
        {
            RequirementType = GetRequirementType(req),
            Description = req.GetDescription(),
            IsSatisfied = req.IsSatisfied(playerState)
        }).ToList();
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

    private RequirementTypes GetRequirementType(Requirement req)
    {
        return req switch
        {
            MaxPressureRequirement => RequirementTypes.MaxPressure,
            InsightRequirement => RequirementTypes.MinInsight,
            EnergyRequirement energyReq => energyReq.EnergyType switch
            {
                EnergyTypes.Physical => RequirementTypes.PhysicalEnergy,
                EnergyTypes.Focus => RequirementTypes.FocusEnergy,
                EnergyTypes.Social => RequirementTypes.SocialEnergy,
                _ => RequirementTypes.Other
            },
            HealthRequirement => RequirementTypes.Health,
            ConcentrationRequirement => RequirementTypes.Concentration,
            ReputationRequirement => RequirementTypes.Reputation,
            CoinsRequirement => RequirementTypes.Coins,
            SkillRequirement skillReq => skillReq.SkillType switch
            {
                SkillTypes.Strength => RequirementTypes.Strength,
                SkillTypes.Perception => RequirementTypes.Perception,
                SkillTypes.Charisma => RequirementTypes.Charisma,
                _ => RequirementTypes.Other
            },
            ItemRequirement itemReq => itemReq.ResourceType switch
            {
                ItemTypes.Tool => RequirementTypes.Tool,
                _ => RequirementTypes.Other
            },
            ResourceRequirement resourceReq => resourceReq.ResourceType switch
            {
                ResourceTypes.Wood => RequirementTypes.Wood,
                ResourceTypes.Metal => RequirementTypes.Metal,
                _ => RequirementTypes.Other
            },
            InventorySlotsRequirement => RequirementTypes.InventorySlots,
            KnowledgeRequirement knowledgeReq => knowledgeReq.KnowledgeType switch
            {
                KnowledgeTypes.LocalHistory => RequirementTypes.LocalHistory,
                _ => RequirementTypes.Other
            },
            SkillLevelRequirement skillLevelReq => skillLevelReq.SkillType switch
            {
                SkillTypes.Strength => RequirementTypes.Strength,
                SkillTypes.Perception => RequirementTypes.Perception,
                SkillTypes.Charisma => RequirementTypes.Charisma,
                _ => RequirementTypes.Other
            },
            StatusRequirement statusReq => statusReq.Status switch
            {
                PlayerStatusTypes.Inspired => RequirementTypes.Inspired,
                _ => RequirementTypes.Other
            },
            _ => RequirementTypes.Other
        };
    }
}

public class DetailedRequirement
{
    public string Description { get; internal set; }
    public bool IsSatisfied { get; internal set; }
    internal RequirementTypes RequirementType { get; set; }
}

public enum RequirementTypes
{
    Other,

    //Skills
    Strength,
    Charisma,
    Perception,

    //Inventory
    InventorySlots,
    Coins,

    //Knowledge
    LocalHistory,

    //Items
    Tool,

    //Player State
    Health,
    Concentration,
    Reputation,

    //Energy
    PhysicalEnergy,
    FocusEnergy,
    SocialEnergy,

    //Encounter States
    MaxPressure,
    MinInsight,

    //Resources
    Wood,
    Metal,

    //Player Status
    Inspired
}