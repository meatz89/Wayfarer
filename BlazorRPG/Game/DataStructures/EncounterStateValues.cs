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

    public void ApplyChanges(EncounterStateValues changes)
    {
        this.Advantage += changes.Advantage;
        this.Understanding += changes.Understanding;
        this.Connection += changes.Connection;
        this.Tension += changes.Tension;
    }
}