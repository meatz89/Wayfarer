using System.Runtime.CompilerServices;

public class ChoiceBuilder
{
    // Basic properties
    private int index;
    private string description;

    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private List<ValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> baseCosts = new();
    private List<Outcome> baseRewards = new();
    private bool requireTool;
    private bool requireKnowledge;
    private bool requireReputation;

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

    private void SetBaseValueChanges()
    {
        // Clear existing value changes
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
        baseValueChanges.Add(new ValueChange(ValueTypes.Outcome, outcomeChange));

        // Add Pressure change based on approach
        int pressureChange = approach switch
        {
            ChoiceApproaches.Direct => 2,
            ChoiceApproaches.Pragmatic => 0,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        baseValueChanges.Add(new ValueChange(ValueTypes.Pressure, pressureChange));

        // Add archetype-specific value changes
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                // Physical choices can't modify Insight
                break;
            case ChoiceArchetypes.Focus:
                int insightChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                baseValueChanges.Add(new ValueChange(ValueTypes.Insight, insightChange));
                break;
            case ChoiceArchetypes.Social:
                int resonanceChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                baseValueChanges.Add(new ValueChange(ValueTypes.Resonance, resonanceChange));
                break;
        }
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
                GetTacticalRequirement();
                break;
            case ChoiceApproaches.Improvised:
                // No requirements
                break;
        }
    }

    private void GetTacticalRequirement()
    {
        switch(archetype)
        {
            case ChoiceArchetypes.Physical: this.requireTool = true; break;
            case ChoiceArchetypes.Focus: this.requireKnowledge = true; break;
            case ChoiceArchetypes.Social: this.requireReputation = true; break;
            default: throw new ArgumentOutOfRangeException();
        };
    }


    public EncounterChoice Build()
    {
        EncounterChoice encounterChoice = new EncounterChoice
                (
                    index,
                    description,
                    archetype,
                    approach,
                    requireTool,
                    requireKnowledge,
                    requireReputation
                );
        return encounterChoice;
    }
}