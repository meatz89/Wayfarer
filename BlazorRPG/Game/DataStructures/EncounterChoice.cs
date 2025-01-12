public class EncounterChoice
{
    // Identity & Description
    public int Index { get; set; }
    public string Description { get; set; }

    // Core properties that define the choice's behavior
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }

    public SkillTypes ChoiceRelevantSkill { get; set; }
    public EnergyTypes EnergyType { get; private set; }
    public int EnergyCost { get; set; }

    // Base values determined by archetype-approach combination
    public List<ValueChange> BaseValueChanges { get; set; } = new();
    public List<Requirement> BaseRequirements { get; set; } = new();
    public List<Outcome> BaseCosts { get; set; } = new();
    public List<Outcome> BaseRewards { get; set; } = new();

    // Calculated Values and Modifications
    public List<ChoiceModification> Modifications { get; set; } = new();
    public List<ValueChange> ModifiedValueChanges { get; set; } = new();
    public List<Requirement> ModifiedRequirements { get; set; } = new();
    public List<Outcome> ModifiedCosts { get; set; } = new();
    public List<Outcome> ModifiedRewards { get; set; } = new();
    public int ModifiedEnergyCost { get; set; }
    public bool RequireTool { get; set; }
    public bool RequireKnowledge { get; set; }
    public bool RequireReputation { get; set; }

    public EncounterChoice(
        int index, 
        string description, 
        ChoiceArchetypes archetype, 
        ChoiceApproaches approach,
        bool requireTool,
        bool requireKnowledge,
        bool requireReputation
        )
    {
        this.Index = index;
        this.Description = description;
        this.RequireTool = requireTool;
        this.RequireKnowledge = requireKnowledge;
        this.RequireReputation = requireReputation;

        Archetype = archetype;
        Approach = approach;

        // Initialize base values based on archetype and approach
        BaseValueChanges = GenerateBaseValueChanges();
        BaseRequirements = GenerateBaseRequirements();

        // Initialize modifications as copies
        ResetModifications();
    }

    private List<ValueChange> GenerateBaseValueChanges()
    {
        List<ValueChange> changes = new();

        // First, apply Approach-based changes
        switch (Approach)
        {
            case ChoiceApproaches.Direct:
                changes.Add(new ValueChange(ValueTypes.Outcome, 2));
                changes.Add(new ValueChange(ValueTypes.Pressure, 1));
                break;
            case ChoiceApproaches.Pragmatic:
                changes.Add(new ValueChange(ValueTypes.Outcome, 1));
                // No pressure change
                break;
            case ChoiceApproaches.Tactical:
                // No immediate outcome gain
                changes.Add(new ValueChange(ValueTypes.Pressure, -1));
                break;
            case ChoiceApproaches.Improvised:
                changes.Add(new ValueChange(ValueTypes.Outcome, 1));
                changes.Add(new ValueChange(ValueTypes.Pressure, 2));
                break;
        }

        // Then, apply Archetype-specific changes
        switch (Archetype)
        {
            case ChoiceArchetypes.Focus:
                if (Approach == ChoiceApproaches.Tactical)
                    changes.Add(new ValueChange(ValueTypes.Insight, 2));
                else
                    changes.Add(new ValueChange(ValueTypes.Insight, 1));
                break;
            case ChoiceArchetypes.Social:
                if (Approach == ChoiceApproaches.Tactical)
                    changes.Add(new ValueChange(ValueTypes.Resonance, 2));
                else
                    changes.Add(new ValueChange(ValueTypes.Resonance, 1));
                break;
                // Physical focuses on pure Outcome gains, already handled by Approach
        }

        return changes;
    }

    private List<Requirement> GenerateBaseRequirements()
    {
        List<Requirement> requirements = new();

        this.EnergyType = GetArchetypeEnergy(Archetype);

        switch (Approach)
        {
            case ChoiceApproaches.Direct:
                // Only requires sufficient energy
                requirements.Add(new EnergyRequirement(EnergyType, 2));
                break;

            case ChoiceApproaches.Pragmatic:
                // Requires skill and low pressure
                requirements.Add(new SkillRequirement(GetArchetypeSkill(Archetype), 1));
                requirements.Add(new MaxPressureRequirement(5));
                break;

            case ChoiceApproaches.Tactical:
                // Add archetype-specific requirement
                switch (Archetype)
                {
                    case ChoiceArchetypes.Physical:
                        requirements.Add(new ItemRequirement(ItemTypes.Tool));
                        break;
                    case ChoiceArchetypes.Focus:
                        requirements.Add(new KnowledgeRequirement(KnowledgeTypes.LocalHistory));
                        break;
                    case ChoiceArchetypes.Social:
                        requirements.Add(new ReputationRequirement(ReputationTypes.Reliable, 5));
                        break;
                }
                break;
        }

        return requirements;
    }

    private SkillTypes GetArchetypeSkill(ChoiceArchetypes archetype)
    {
        throw new NotImplementedException();
    }

    private void ResetModifications()
    {
        ModifiedValueChanges = new List<ValueChange>(BaseValueChanges);
        ModifiedRequirements = new List<Requirement>(BaseRequirements);
        ModifiedEnergyCost = GetBaseEnergyCost();
    }

    private int GetBaseEnergyCost()
    {
        return Approach switch
        {
            ChoiceApproaches.Direct => 3,
            ChoiceApproaches.Pragmatic => 2,
            ChoiceApproaches.Tactical => 1,
            ChoiceApproaches.Improvised => 0,
            _ => throw new ArgumentException("Invalid approach")
        };
    }

    private static EnergyTypes GetArchetypeEnergy(ChoiceArchetypes archetype)
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }
}