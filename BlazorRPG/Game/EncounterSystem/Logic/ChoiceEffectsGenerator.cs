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

        if (archetype != ChoiceArchetypes.Physical)
        {
            changes.Add(new BaseValueChange(ValueTypes.Momentum, -1)); // Base progress
        }

        // Combine changes of the same ValueType
        return BaseValueChange.CombineBaseValueChanges(changes);
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

    public List<Requirement> GenerateStrategicRequirements(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Requirement> requirements = new();

        // Special choices (4th option) require high mastery values
        if (approach == ChoiceApproaches.Strategic)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical:
                    requirements.Add(new MomentumRequirement(GameRules.StrategicMomentumRequirement));
                    break;
                case ChoiceArchetypes.Focus:
                    requirements.Add(new InsightRequirement(GameRules.StrategicInsightRequirement));
                    break;
                case ChoiceArchetypes.Social:
                    requirements.Add(new ResonanceRequirement(GameRules.StrategicResonanceRequirement));
                    break;
            }
        }

        return requirements;
    }
}