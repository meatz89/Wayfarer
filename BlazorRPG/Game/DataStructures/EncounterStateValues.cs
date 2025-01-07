public class EncounterStateValues
{
    public static EncounterStateValues InitialState = new EncounterStateValues(5, 5, 5, 5, 5);
    public static EncounterStateValues NoChange => new EncounterStateValues(0, 0, 0, 0, 0);
    public int Momentum { get; set; }
    public int Advantage { get; set; }
    public int Understanding { get; set; }
    public int Connection { get; set; }
    public int Tension { get; set; }

    public EncounterStateValues(int momentum, int advantage, int understanding, int connection, int tension)
    {
        this.Momentum = momentum;
        this.Advantage = advantage;
        this.Understanding = understanding;
        this.Connection = connection;
        this.Tension = tension;
    }

    public void ApplyChanges(EncounterStateValues changes)
    {
        this.Momentum += changes.Momentum;
        this.Advantage += changes.Advantage;
        this.Understanding += changes.Understanding;
        this.Connection += changes.Connection;
        this.Tension += changes.Tension;
    }
}