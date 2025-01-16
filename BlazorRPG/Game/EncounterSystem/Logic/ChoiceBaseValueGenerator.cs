public class ChoiceBaseValueGenerator
{
    public List<BaseValueChange> GenerateBaseValueChanges(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<BaseValueChange> changes = new();

        // First apply Approach-based changes - preserved from original
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                changes.Add(new BaseValueChange(ValueTypes.Outcome, 2));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 1));
                break;
            case ChoiceApproaches.Pragmatic:
                changes.Add(new BaseValueChange(ValueTypes.Outcome, 1));
                // Pragmatic has no inherent pressure change
                break;
            case ChoiceApproaches.Tactical:
                // Tactical sacrifices immediate outcome for pressure reduction
                changes.Add(new BaseValueChange(ValueTypes.Pressure, -1));
                break;
            case ChoiceApproaches.Improvised:
                changes.Add(new BaseValueChange(ValueTypes.Outcome, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                break;
        }

        // Then add archetype-specific base changes
        // These represent the core competency of each archetype
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                int momentumGain = approach switch
                {
                    ChoiceApproaches.Direct => 3,
                    ChoiceApproaches.Pragmatic => 2,
                    ChoiceApproaches.Tactical => 1,
                    _ => 0
                };
                if (momentumGain > 0)
                    changes.Add(new BaseValueChange(ValueTypes.Momentum, momentumGain));
                break;

            case ChoiceArchetypes.Focus:
                int insightGain = approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new BaseValueChange(ValueTypes.Insight, insightGain));
                break;

            case ChoiceArchetypes.Social:
                int resonanceGain = approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new BaseValueChange(ValueTypes.Resonance, resonanceGain));
                break;
        }

        return changes;
    }

    public List<Requirement> GenerateBaseRequirements(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Requirement> requirements = new();

        EnergyTypes energyType = GetArchetypeEnergy(archetype);

        // Preserved from original requirement generation
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Only requires sufficient energy
                requirements.Add(new EnergyRequirement(energyType, 2));
                break;

            case ChoiceApproaches.Pragmatic:
                // Requires skill and low pressure
                requirements.Add(new SkillRequirement(GetArchetypeSkill(archetype), 1));
                requirements.Add(new MaxPressureRequirement(5));
                break;

            case ChoiceApproaches.Tactical:
                // Add archetype-specific requirement - preserved from original
                switch (archetype)
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

    public int GenerateBaseEnergyCost(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // Preserved from original
        return approach switch
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

    private static SkillTypes GetArchetypeSkill(ChoiceArchetypes archetype)
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => SkillTypes.Strength,
            ChoiceArchetypes.Focus => SkillTypes.Perception,
            ChoiceArchetypes.Social => SkillTypes.Charisma,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }

    public List<Outcome> GenerateBaseCosts(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> costs = new();

        // Add base energy cost
        costs.Add(new EnergyOutcome(
            GetArchetypeEnergy(archetype),
            GenerateBaseEnergyCost(archetype, approach)));

        // Direct approaches have additional costs
        if (approach == ChoiceApproaches.Direct)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical:
                    costs.Add(new HealthOutcome(-1));
                    break;
                case ChoiceArchetypes.Focus:
                    // Add stress cost for intense focus
                    costs.Add(new StressOutcome(1));
                    break;
                case ChoiceArchetypes.Social:
                    // Risk to reputation for direct social approaches
                    costs.Add(new ReputationOutcome(ReputationTypes.Reliable, -1));
                    break;
            }
        }

        return costs;
    }

    public List<Outcome> GenerateBaseRewards(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> rewards = new();

        // Base rewards by archetype
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                rewards.Add(new CoinsOutcome(2));
                if (approach == ChoiceApproaches.Direct)
                    rewards.Add(new ReputationOutcome(ReputationTypes.Reliable, 1));
                break;

            case ChoiceArchetypes.Focus:
                if (approach == ChoiceApproaches.Tactical)
                    rewards.Add(new KnowledgeOutcome(KnowledgeTypes.LocalHistory, 1));
                break;

            case ChoiceArchetypes.Social:
                if (approach == ChoiceApproaches.Tactical)
                    rewards.Add(new ReputationOutcome(ReputationTypes.Reliable, 2));
                else
                    rewards.Add(new ReputationOutcome(ReputationTypes.Reliable, 1));
                break;
        }

        return rewards;
    }
}