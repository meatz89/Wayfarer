public class NarrativeStateValues
{
    public static NarrativeStateValues InitialState = new NarrativeStateValues(5, 5, 5, 5, 5);
    public static NarrativeStateValues NoChange => new NarrativeStateValues(0, 0, 0, 0, 0);
    public int Momentum { get; set; }
    public int Advantage { get; set; }
    public int Understanding { get; set; }
    public int Connection { get; set; }
    public int Tension { get; set; }

    public NarrativeStateValues(int momentum, int advantage, int understanding, int connection, int tension)
    {
        this.Momentum = momentum;
        this.Advantage = advantage;
        this.Understanding = understanding;
        this.Connection = connection;
        this.Tension = tension;
    }

    public void ApplyChanges(NarrativeStateValues changes)
    {
        this.Momentum += changes.Momentum;
        this.Advantage += changes.Advantage;
        this.Understanding += changes.Understanding;
        this.Connection += changes.Connection;
        this.Tension += changes.Tension;
    }
}