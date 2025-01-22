public class EncounterChoice
{
    // Core properties
    public int Index { get; }
    public string ChoiceType { get; set; }
    public string Designation { get; set; }
    public string Narrative { get; set; }
    public ChoiceArchetypes Archetype { get; }
    public ChoiceApproaches Approach { get; }
    public EnergyTypes EnergyType { get; }
    public int EnergyCost { get; set; }
    public ChoiceCalculationResult CalculationResult { get; set; }

    // Constructor remains the same
    public EncounterChoice(
        int index,
        string choiceType,
        string description,
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        bool requireTool,
        bool requireKnowledge,
        bool requireReputation)
    {
        Index = index;
        ChoiceType = choiceType;
        Designation = description;
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

    public List<DetailedRequirement> GetDetailedRequirements(GameState gameState)
    {
        if (CalculationResult == null)
        {
            return new List<DetailedRequirement>();
        }

        return CalculationResult.Requirements.Select(req => new DetailedRequirement
        {
            RequirementType = GetRequirementType(req),
            Description = req.GetDescription(),
            IsSatisfied = req.IsSatisfied(gameState)
        }).ToList();
    }


    private RequirementTypes GetRequirementType(Requirement req)
    {
        return req switch
        {
            PressureRequirement => RequirementTypes.MaxPressure,
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
                PlayerStatusTypes.Wet => RequirementTypes.Wet,
                PlayerStatusTypes.Cold => RequirementTypes.Cold,
                PlayerStatusTypes.Hungry => RequirementTypes.Hungry,
                PlayerStatusTypes.Tired => RequirementTypes.Tired,
                PlayerStatusTypes.Injured => RequirementTypes.Injured,
                _ => RequirementTypes.Other
            },
            _ => RequirementTypes.Other
        };
    }
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
    Wet,
    Cold,
    Hungry,
    Tired,
    Injured,
}