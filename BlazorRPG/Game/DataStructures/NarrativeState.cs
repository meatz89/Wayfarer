public class NarrativeState
{
    public static NarrativeState InitialState = new NarrativeState(5, 5, 5, 5, 5);
    public static NarrativeState NoChange => new NarrativeState(0, 0, 0, 0, 0);
    public int Momentum { get; set; }
    public int Advantage { get; set; }
    public int Understanding { get; set; }
    public int Connection { get; set; }
    public int Tension { get; set; }

    public NarrativeState()
    {
        
    }

    public NarrativeState(int momentum, int advantage, int understanding, int connection, int tension)
    {
        this.Momentum = momentum;
        this.Advantage = advantage;
        this.Understanding = understanding;
        this.Connection = connection;
        this.Tension = tension;
    }

    public void ApplyChanges(NarrativeState changes)
    {
        this.Momentum += changes.Momentum;
        this.Advantage += changes.Advantage;
        this.Understanding += changes.Understanding;
        this.Connection += changes.Connection;
        this.Tension += changes.Tension;
    }
}