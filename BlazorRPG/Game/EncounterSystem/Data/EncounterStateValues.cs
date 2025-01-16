
public class EncounterStateValues
{
    public int Outcome { get; set; }
    public int Momentum { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }
    public int Pressure { get; set; }
    public ChoiceArchetypes? LastChoiceType { get; set; }

    public EncounterStateValues(int outcome, int momentum, int insight, int resonance, int pressure)
    {
        Outcome = outcome;
        Momentum = momentum;
        Insight = insight;
        Resonance = resonance;
        Pressure = pressure;
    }

    public void Clamp()
    {
        Outcome = Math.Max(0, Outcome);
        Momentum = Math.Max(0, Momentum);
        Insight = Math.Max(0, Insight);
        Resonance = Math.Max(0, Resonance);
        Pressure = Math.Max(0, Pressure);
    }
}