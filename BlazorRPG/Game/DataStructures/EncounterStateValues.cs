public record EncounterStateValues
{
    public static EncounterStateValues NoChange => new EncounterStateValues(0, 0, 0, 0);
    public static EncounterStateValues InitialState = new EncounterStateValues(0, 5, 5, 0);
    public int Advantage { get; set; }
    public int Understanding { get; set; }
    public int Connection { get; set; }
    public int Tension { get; set; }

    public EncounterStateValues(int advantage, int understanding, int connection, int tension)
    {
        this.Advantage = advantage;
        this.Understanding = understanding;
        this.Connection = connection;
        this.Tension = tension;
    }

    public void ApplyChanges(List<EncounterValueChange> valueChanges)
    {
        foreach (EncounterValueChange change in valueChanges)
        {
            switch (change.ValueType)
            {
                case EncounterValues.Advantage:
                    Advantage += change.Change;
                    break;
                case EncounterValues.Understanding:
                    Understanding += change.Change;
                    break;
                case EncounterValues.Connection:
                    Connection += change.Change;
                    break;
                case EncounterValues.Tension:
                    Tension += change.Change;
                    break;
            }
        }
    }
}