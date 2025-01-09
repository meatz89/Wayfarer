public record EncounterStateValues
{
    // Initial state is now determined by Location and Player State
    public static EncounterStateValues InitialState => new EncounterStateValues(0, 0, 0, 0);

    public int Outcome { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }
    public int Pressure { get; set; }


    public EncounterStateValues(int outcome, int insight, int resonance, int pressure)
    {
        this.Outcome = outcome;
        this.Insight = insight;
        this.Resonance = resonance;
        this.Pressure = pressure;
    }

    public void ApplyChanges(List<ValueChange> valueChanges)
    {
        foreach (ValueChange change in valueChanges)
        {
            switch (change.ValueType)
            {
                case ValueTypes.Outcome:
                    Outcome += change.Change;
                    break;
                case ValueTypes.Insight:
                    Insight += change.Change;
                    break;
                case ValueTypes.Resonance:
                    Resonance += change.Change;
                    break;
                case ValueTypes.Pressure:
                    Pressure += change.Change;
                    break;
            }
        }
    }

    internal void ClampValues()
    {

        Outcome = Math.Clamp(Outcome, 0, 20);
        Insight = Math.Clamp(Insight, 0, 20);
        Resonance = Math.Clamp(Resonance, 0, 20);
        Pressure = Math.Clamp(Pressure, 0, 20);
    }
}