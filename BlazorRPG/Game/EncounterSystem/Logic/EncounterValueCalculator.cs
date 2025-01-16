public class EncounterValueCalculator
{
    public void ApplyEncounterStateChanges(Encounter encounter)
    {
        // Apply decay
        ApplyValueDecay(encounter);

        // Check for cascade failures
        ApplyCascadeEffects(encounter);

        // Clamp values within allowed ranges
        ClampValues(encounter.Context.CurrentValues);
    }

    private void ApplyValueDecay(Encounter encounter)
    {
        var values = encounter.Context.CurrentValues;
        var currentChoice = encounter.GetCurrentStage().Choices.FirstOrDefault()?.Archetype;
        var lastChoice = encounter.Context.CurrentValues.LastChoiceType;
        // Base decay
        values.Momentum = Math.Max(0, values.Momentum - 2); // Momentum decays faster

        // Decay when not used
        if (currentChoice != ChoiceArchetypes.Physical)
            values.Momentum = Math.Max(0, values.Momentum - 1);
        if (currentChoice != ChoiceArchetypes.Focus)
            values.Insight = Math.Max(0, values.Insight - 1);
        if (currentChoice != ChoiceArchetypes.Social)
            values.Resonance = Math.Max(0, values.Resonance - 1);

        // Extra decay for repeating choices
        if (lastChoice == currentChoice)
        {
            switch (currentChoice)
            {
                case ChoiceArchetypes.Physical:
                    values.Momentum = Math.Max(0, values.Momentum - 2);
                    break;
                case ChoiceArchetypes.Focus:
                    values.Insight = Math.Max(0, values.Insight - 1);
                    break;
                case ChoiceArchetypes.Social:
                    values.Resonance = Math.Max(0, values.Resonance - 1);
                    break;
            }
        }
    }

    private void ApplyCascadeEffects(Encounter encounter)
    {
        var values = encounter.Context.CurrentValues;
        // Pressure from zero values
        if (values.Momentum == 0) values.Pressure += 2;
        if (values.Insight == 0) values.Pressure += 2;
        if (values.Resonance == 0) values.Pressure += 2;
    }

    private void ClampValues(EncounterStateValues values)
    {
        // Clamp values within allowed ranges
        values.Outcome = Math.Max(0, values.Outcome);
        values.Momentum = Math.Max(0, values.Momentum);
        values.Insight = Math.Max(0, values.Insight);
        values.Resonance = Math.Max(0, values.Resonance);
        values.Pressure = Math.Clamp(values.Pressure, 0, 8); // Assuming 8 is the max pressure
    }
}