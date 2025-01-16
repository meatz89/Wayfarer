public class EncounterChoice
{
    // Core properties - no change
    public int Index { get; }
    public string Description { get; }
    public ChoiceArchetypes Archetype { get; }
    public ChoiceApproaches Approach { get; }

    // Base values - will be set by ChoiceBaseValueGenerator
    public List<ValueChange> BaseEncounterValueChanges { get; set; } = new();
    public List<Requirement> BaseRequirements { get; set; } = new();
    public List<Outcome> BaseCosts { get; set; } = new();
    public List<Outcome> BaseRewards { get; set; } = new();

    // Modified values - will be set by ChoiceCalculator
    public List<ValueChange> ModifiedEncounterValueChanges { get; set; } = new();
    public List<Requirement> ModifiedRequirements { get; set; } = new();
    public List<Outcome> ModifiedCosts { get; set; } = new();
    public List<Outcome> ModifiedRewards { get; set; } = new();

    // Requirement flags - used for UI/display
    public bool RequireTool { get; }
    public bool RequireKnowledge { get; }
    public bool RequireReputation { get; }

    public EnergyTypes EnergyType { get; }
    public int EnergyCost { get; set; }

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
        RequireTool = requireTool;
        RequireKnowledge = requireKnowledge;
        RequireReputation = requireReputation;

        // Set energy type based on archetype
        EnergyType = archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }
}