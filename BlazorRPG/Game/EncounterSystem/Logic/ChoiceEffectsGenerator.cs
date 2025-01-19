public class ChoiceEffectsGenerator
{
    public List<BaseValueChange> GenerateBaseValueChanges(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // Every choice gets base progress, then modifications based on approach and archetype
        List<BaseValueChange> changes = new();

        // First add the base outcome that every non-strategic choice gets
        if (approach != ChoiceApproaches.Strategic)
        {
            changes.Add(new BaseValueChange(ValueTypes.Outcome, 1)); // Base progress
        }

        // Then apply approach-specific base changes
        AddApproachBaseChanges(changes, approach);

        // Finally, add archetype-specific changes that represent mastery value gains
        AddArchetypeMasteryChanges(changes, archetype, approach);

        return changes;
    }


    private void AddApproachBaseChanges(List<BaseValueChange> changes, ChoiceApproaches approach)
    {
        // Each approach has a distinct effect pattern that creates its strategic identity
        switch (approach)
        {
            case ChoiceApproaches.Aggressive:
                // High risk, high reward
                changes.Add(new BaseValueChange(ValueTypes.Outcome, 2)); // +2 additional outcome
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2)); // But increases pressure
                break;

            case ChoiceApproaches.Careful:
                // Safety focused - either reduces pressure OR makes small progress
                // Note: We're implementing our refined careful approach that doesn't do both
                changes.Add(new BaseValueChange(ValueTypes.Pressure, -1));
                break;

            case ChoiceApproaches.Strategic:
                // Focused on building mastery values - no direct outcome changes
                // Mastery value changes will be added by archetype
                break;

            case ChoiceApproaches.Desperate:
                // Risky progress when needed
                changes.Add(new BaseValueChange(ValueTypes.Outcome, 1)); // Additional outcome
                changes.Add(new BaseValueChange(ValueTypes.Pressure, 2)); // But at high pressure cost
                break;
        }
    }

    private void AddArchetypeMasteryChanges(List<BaseValueChange> changes, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // Each archetype builds its specific mastery value
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                // Physical actions build Momentum
                if (approach == ChoiceApproaches.Strategic)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Momentum, 2));
                }
                else if (approach == ChoiceApproaches.Aggressive)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Momentum, 1));
                }
                break;

            case ChoiceArchetypes.Focus:
                // Focus actions build Insight
                if (approach == ChoiceApproaches.Strategic)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Insight, 2));
                }
                else if (approach != ChoiceApproaches.Desperate)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Insight, 1));
                }
                break;

            case ChoiceArchetypes.Social:
                // Social actions build Resonance
                if (approach == ChoiceApproaches.Strategic)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Resonance, 2));
                }
                else if (approach == ChoiceApproaches.Careful)
                {
                    changes.Add(new BaseValueChange(ValueTypes.Resonance, 1));
                }
                break;
        }
    }

    public int GenerateBaseEnergyCost(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // Each approach has a base energy cost that reflects its intensity
        return approach switch
        {
            ChoiceApproaches.Aggressive => 3, // High energy cost for aggressive actions
            ChoiceApproaches.Careful => 2,    // Moderate cost for careful actions
            ChoiceApproaches.Strategic => 2,   // Moderate cost for strategic actions
            ChoiceApproaches.Desperate => 1,   // Low cost as a fallback option
            _ => throw new ArgumentException("Invalid approach")
        };
    }

    // We'll keep this method but simplify its implementation as special requirements 
    // are now primarily handled through the mastery value system
    public List<Requirement> GenerateSpecialRequirements(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Requirement> requirements = new();

        // Special choices (4th option) require high mastery values
        if (approach == ChoiceApproaches.Strategic)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical:
                    requirements.Add(new MomentumRequirement(6));
                    break;
                case ChoiceArchetypes.Focus:
                    requirements.Add(new InsightRequirement(6));
                    break;
                case ChoiceArchetypes.Social:
                    requirements.Add(new ResonanceRequirement(6));
                    break;
            }
        }

        return requirements;
    }

    public List<Outcome> CalculatePressureCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = new();

        // Add pressure-based complications at high pressure
        if (context.CurrentValues.Pressure >= 7)
        {
            switch (choice.Archetype)
            {
                case ChoiceArchetypes.Physical:
                    costs.Add(new HealthOutcome(-1));
                    break;
                case ChoiceArchetypes.Focus:
                    costs.Add(new ConcentrationOutcome(-1));
                    break;
                case ChoiceArchetypes.Social:
                    costs.Add(new ReputationOutcome(-1));
                    break;
            }
        }

        return costs;
    }

    public List<Outcome> GenerateBaseCosts(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> costs = new List<Outcome>();

        return costs;
    }

    public List<Outcome> GenerateBaseRewards(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> rewards = new List<Outcome>();

        return rewards;
    }
}