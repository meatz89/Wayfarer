public class EncounterStateValues
{
    public static EncounterStateValues InitialState => new EncounterStateValues(0, 0, 0, 0, 0);

    public int Outcome { get; set; }
    public int Momentum { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }
    public int Pressure { get; set; }
    public ChoiceArchetypes? LastChoiceType { get; private set; }

    public EncounterStateValues(int outcome, int momentum, int insight, int resonance, int pressure)
    {
        this.Outcome = outcome;
        this.Momentum = insight;
        this.Insight = insight;
        this.Resonance = resonance;
        this.Pressure = pressure;
    }

    public void ApplyChoiceEffects(ChoiceArchetypes currentChoice, List<ValueChange> changes)
    {
        // First apply natural decay
        DecayValues(currentChoice);

        // Then apply choice changes
        foreach (ValueChange change in changes)
        {
            ApplyValueChange(change);
        }

        // Check for cascade failures
        CheckCascadeFailures();

        LastChoiceType = currentChoice;
    }

    private void DecayValues(ChoiceArchetypes currentChoice)
    {
        // Momentum decays faster
        Momentum = Math.Max(0, Momentum - 2);

        // Values decay when not used
        if (currentChoice != ChoiceArchetypes.Physical)
            Momentum = Math.Max(0, Momentum - 1);
        if (currentChoice != ChoiceArchetypes.Focus)
            Insight = Math.Max(0, Insight - 1);
        if (currentChoice != ChoiceArchetypes.Social)
            Resonance = Math.Max(0, Resonance - 1);

        // Extra decay for repeating same choice type
        if (LastChoiceType == currentChoice)
        {
            switch (currentChoice)
            {
                case ChoiceArchetypes.Physical:
                    Momentum = Math.Max(0, Momentum - 2);
                    break;
                case ChoiceArchetypes.Focus:
                    Insight = Math.Max(0, Insight - 1);
                    break;
                case ChoiceArchetypes.Social:
                    Resonance = Math.Max(0, Resonance - 1);
                    break;
            }
        }
    }

    private void CheckCascadeFailures()
    {
        // Add pressure when values hit 0
        if (Momentum == 0) Pressure += 2;
        if (Insight == 0) Pressure += 2;
        if (Resonance == 0) Pressure += 2;

        // Clamp pressure - encounter fails at 8
        Pressure = Math.Min(8, Pressure);
    }

    private void ApplyValueChange(ValueChange change)
    {
        int modifiedValue = CalculateModifiedGain(change);

        switch (change.ValueType)
        {
            case ValueTypes.Outcome:
                Outcome += modifiedValue;
                break;
            case ValueTypes.Momentum:
                if (Pressure <= Momentum)  // Only gain if pressure isn't too high
                    Momentum += modifiedValue;
                else
                    Pressure += 2;  // Failed gain adds pressure
                break;
            case ValueTypes.Insight:
                Insight += modifiedValue;
                break;
            case ValueTypes.Resonance:
                Resonance += modifiedValue;
                break;
            case ValueTypes.Pressure:
                Pressure += change.Change; // Pressure isn't reduced by other pressure
                break;
        }
    }

    private int CalculateModifiedGain(ValueChange change)
    {
        if (change.ValueType == ValueTypes.Pressure)
            return change.Change;

        // Pressure reduces all gains
        return Math.Max(0, change.Change - Pressure);
    }
}