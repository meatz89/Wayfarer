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
    public bool IsEncounterWinningChoice { get; set; }
    public bool IsEncounterFailingChoice { get; set; }
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
            ChoiceArchetypes.Focus => EnergyTypes.Concentration,
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
            RequirementType = RequirementMapper.GetRequirementType(req),
            Description = req.GetDescription(),
            IsSatisfied = req.IsSatisfied(gameState)
        }).ToList();
    }
}
