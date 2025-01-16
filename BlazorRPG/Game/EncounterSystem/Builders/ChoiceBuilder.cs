public class ChoiceBuilder
{
    private int index;
    private string description;
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private List<BaseValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> baseCosts = new();
    private List<Outcome> baseRewards = new();

    // Most methods unchanged, but SetBaseValueChanges updated:
    private void SetBaseValueChanges()
    {
        baseValueChanges.Clear();

        // Add Outcome change based on approach
        int outcomeChange = approach switch
        {
            ChoiceApproaches.Direct => 2,
            ChoiceApproaches.Pragmatic => 1,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        baseValueChanges.Add(new BaseValueChange(ValueTypes.Outcome, outcomeChange));

        // Add Pressure change based on approach
        int pressureChange = approach switch
        {
            ChoiceApproaches.Direct => 2,
            ChoiceApproaches.Pragmatic => 0,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        baseValueChanges.Add(new BaseValueChange(ValueTypes.Pressure, pressureChange));

        // Add archetype-specific value changes
        switch (archetype)
        {
            case ChoiceArchetypes.Focus:
                int insightChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                baseValueChanges.Add(new BaseValueChange(ValueTypes.Insight, insightChange));
                break;
            case ChoiceArchetypes.Social:
                int resonanceChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                baseValueChanges.Add(new BaseValueChange(ValueTypes.Resonance, resonanceChange));
                break;
        }
    }

    // Build method updated to use new types
    public EncounterChoice Build()
    {
        string description = $"{archetype} - {approach}";
        bool requireTool = IsRequireTool();
        bool requireKnowledge = IsRequireKnowledge();
        bool requireReputation = IsRequireReputation();

        EncounterChoice choice = new(
            index,
            description,
            archetype,
            approach,
            requireTool,
            requireKnowledge,
            requireReputation);

        choice.BaseEncounterValueChanges = baseValueChanges;
        choice.ModifiedRequirements = requirements;

        return choice;
    }

    public ChoiceBuilder WithIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ChoiceBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ChoiceBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.archetype = archetype;
        // Set base energy cost based on archetype
        SetBaseEnergyCost();
        return this;
    }

    public ChoiceBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        // Set base value changes based on approach
        SetBaseValueChanges();
        // Set requirements based on approach
        SetBaseRequirements();
        return this;
    }


    public ChoiceBuilder WithRequirements(List<Requirement> requirements)
    {
        this.requirements = requirements;
        return this;
    }

    private bool IsRequireReputation()
    {
        return archetype == ChoiceArchetypes.Social && approach == ChoiceApproaches.Tactical;
    }

    private bool IsRequireKnowledge()
    {
        return archetype == ChoiceArchetypes.Focus && approach == ChoiceApproaches.Tactical;
    }

    private bool IsRequireTool()
    {
        return archetype == ChoiceArchetypes.Physical && approach == ChoiceApproaches.Tactical;
    }

    private void SetBaseEnergyCost()
    {
        // Base energy costs are determined by archetype
        int baseCost = archetype switch
        {
            ChoiceArchetypes.Physical => 2,
            ChoiceArchetypes.Focus => 2,
            ChoiceArchetypes.Social => 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        EnergyTypes energyType = archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentOutOfRangeException()
        };

        baseCosts.Add(new EnergyOutcome(energyType, baseCost));
    }

    private EnergyTypes GetEnergyType()
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }

    private SkillTypes GetRelevantSkill()
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => SkillTypes.Strength,
            ChoiceArchetypes.Focus => SkillTypes.Perception,
            ChoiceArchetypes.Social => SkillTypes.Charisma,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }

    private void SetBaseRequirements()
    {
        requirements.Clear();

        // Set requirements based on approach
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Requires sufficient energy
                requirements.Add(new EnergyRequirement(GetEnergyType(), 2));
                break;
            case ChoiceApproaches.Pragmatic:
                // Requires relevant skill and low pressure
                requirements.Add(new SkillRequirement(GetRelevantSkill(), 1));
                requirements.Add(new MaxPressureRequirement(5));
                break;
            case ChoiceApproaches.Tactical:
                // No requirements
                break;
            case ChoiceApproaches.Improvised:
                // No requirements
                break;
        }
    }

}