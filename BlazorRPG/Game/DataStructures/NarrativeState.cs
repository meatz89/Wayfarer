public class NarrativeState
{
    public static NarrativeState InitialState = new NarrativeState(5, 5, 5, 5, 5);
    public static NarrativeState NoChange => new NarrativeState(0, 0, 0, 0, 0);
    public int Momentum { get; set; }
    public int Advantage { get; set; }
    public int Understanding { get; set; }
    public int Connection { get; set; }
    public int Tension { get; set; }

    public NarrativeState(int m, int a, int u, int c, int t)
    {
        this.Momentum = m;
        this.Advantage = a;
        this.Understanding = u;
        this.Connection = c;
        this.Tension = t;
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