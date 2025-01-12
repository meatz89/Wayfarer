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

        // Base Outcome change from approach
        int outcomeChange = Approach switch
        {
            ChoiceApproaches.Direct => 2,
            ChoiceApproaches.Pragmatic => 1,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 1,
            _ => throw new ArgumentException("Invalid approach")
        };
        changes.Add(new ValueChange(ValueTypes.Outcome, outcomeChange));

        // Pressure change from approach
        int pressureChange = Approach switch
        {
            ChoiceApproaches.Direct => 1,
            ChoiceApproaches.Pragmatic => 0,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 2,
            _ => throw new ArgumentException("Invalid approach")
        };
        changes.Add(new ValueChange(ValueTypes.Pressure, pressureChange));

        // Archetype-specific secondary value
        switch (Archetype)
        {
            case ChoiceArchetypes.Focus:
                int insightChange = Approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new ValueChange(ValueTypes.Insight, insightChange));
                break;
            case ChoiceArchetypes.Social:
                int resonanceChange = Approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new ValueChange(ValueTypes.Resonance, resonanceChange));
                break;
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