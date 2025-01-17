public class ChoiceEffectsGenerator
{
    public List<BaseValueChange> GenerateBaseValueChanges(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<BaseValueChange> changes = new();

        // First apply archetype-specific changes
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                AddPhysicalArchetypeChanges(changes, approach);
                break;
            case ChoiceArchetypes.Focus:
                AddFocusArchetypeChanges(changes, approach);
                break;
            case ChoiceArchetypes.Social:
                AddSocialArchetypeChanges(changes, approach);
                break;
        }

        return changes;
    }

    private void AddPhysicalArchetypeChanges(List<BaseValueChange> changes, ChoiceApproaches approach)
    {
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // High risk, immediate conversion
                changes.Add(new BaseValueChange(ValueTypes.Momentum, 2));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 3));
                break;

            case ChoiceApproaches.Pragmatic:
                // Safe momentum building
                changes.Add(new BaseValueChange(ValueTypes.Momentum, 2));
                break;

            case ChoiceApproaches.Tactical:
                // Pressure management through momentum spending
                // Actual spending happens in execution phase
                changes.Add(new BaseValueChange(ValueTypes.Momentum, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, -1));
                break;

            case ChoiceApproaches.Improvised:
                // Small gain with conditional conversion
                changes.Add(new BaseValueChange(ValueTypes.Momentum, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                break;
        }
    }

    private void AddFocusArchetypeChanges(List<BaseValueChange> changes, ChoiceApproaches approach)
    {
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Understanding breakthrough
                changes.Add(new BaseValueChange(ValueTypes.Insight, 2));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                // Outcome conversion if Insight > Pressure happens in execution
                break;

            case ChoiceApproaches.Pragmatic:
                // Careful study
                changes.Add(new BaseValueChange(ValueTypes.Insight, 2));
                break;

            case ChoiceApproaches.Tactical:
                // Knowledge application
                // Insight conversion happens in execution
                changes.Add(new BaseValueChange(ValueTypes.Insight, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, -1));
                break;

            case ChoiceApproaches.Improvised:
                // Basic observation
                changes.Add(new BaseValueChange(ValueTypes.Insight, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                break;
        }
    }

    private void AddSocialArchetypeChanges(List<BaseValueChange> changes, ChoiceApproaches approach)
    {
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Social leverage
                changes.Add(new BaseValueChange(ValueTypes.Resonance, 2));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                break;

            case ChoiceApproaches.Pragmatic:
                // Relationship building
                changes.Add(new BaseValueChange(ValueTypes.Resonance, 2));
                break;

            case ChoiceApproaches.Tactical:
                changes.Add(new BaseValueChange(ValueTypes.Resonance, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, -1));
                break;

            case ChoiceApproaches.Improvised:
                // Basic interaction
                changes.Add(new BaseValueChange(ValueTypes.Resonance, 1));
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2));
                break;
        }
    }

    public List<Requirement> GenerateSpecialRequirements(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Requirement> requirements = new();

        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Only requires sufficient energy
                break;

            case ChoiceApproaches.Pragmatic:
                // Requires skill and low pressure
                requirements.Add(new SkillRequirement(GetArchetypeSkill(archetype), 1));
                requirements.Add(new MaxPressureRequirement(5));
                break;

            case ChoiceApproaches.Tactical:
                // Add archetype-specific requirement
                switch (archetype)
                {
                    case ChoiceArchetypes.Physical:
                        requirements.Add(new ItemRequirement(ItemTypes.Tool));
                        break;
                    case ChoiceArchetypes.Focus:
                        requirements.Add(new KnowledgeRequirement(KnowledgeTypes.LocalHistory));
                        break;
                    case ChoiceArchetypes.Social:
                        requirements.Add(new ReputationRequirement(5));
                        break;
                }
                break;
        }

        return requirements;
    }

    public int GenerateBaseEnergyCost(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        return approach switch
        {
            ChoiceApproaches.Direct => 3,
            ChoiceApproaches.Pragmatic => 2,
            ChoiceApproaches.Tactical => 1,
            ChoiceApproaches.Improvised => 0,
            _ => throw new ArgumentException("Invalid approach")
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

        // Direct approaches have additional costs
        GetDirectChoiceOutcome(archetype, approach, costs);

        return costs;
    }

    private static void GetDirectChoiceOutcome(ChoiceArchetypes archetype, ChoiceApproaches approach, List<Outcome> costs)
    {
        if (approach == ChoiceApproaches.Direct)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical:
                    costs.Add(new HealthOutcome(-1));
                    break;
                case ChoiceArchetypes.Focus:
                    // Add concentration cost for intense focus
                    costs.Add(new ConcentrationOutcome(-1));
                    break;
                case ChoiceArchetypes.Social:
                    // Reputation Loss for direct social approaches
                    costs.Add(new ReputationOutcome(-1));
                    break;
            }
        }
    }

    public List<Outcome> GenerateBaseRewards(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> rewards = new();

        // Base rewards by archetype
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                rewards.Add(new CoinsOutcome(2));
                break;

            case ChoiceArchetypes.Focus:
                if (approach == ChoiceApproaches.Tactical)
                    rewards.Add(new KnowledgeOutcome(KnowledgeTypes.LocalHistory, 1));
                break;

            case ChoiceArchetypes.Social:
                if (approach == ChoiceApproaches.Tactical)
                    rewards.Add(new ReputationOutcome(2));
                else
                    rewards.Add(new ReputationOutcome(1));
                break;
        }

        return rewards;
    }
}