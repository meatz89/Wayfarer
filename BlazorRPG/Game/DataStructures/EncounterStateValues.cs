public record EncounterStateValues
{
    public static EncounterStateValues NoChange => new EncounterStateValues(0, 0, 0, 0);
    public static EncounterStateValues InitialState = new EncounterStateValues(0, 5, 5, 0);
    public int Outcome { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }
    public int Pressure { get; set; }

    public EncounterStateValues()
    {
    }

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
}