public class EncounterValues
{
    public int Momentum { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }
    public int Outcome { get; set; }
    public int Pressure { get; set; }

    // Track last choice for insight decay
    public ChoiceArchetypes? LastChoiceType { get; set; }
    public ChoiceApproaches? LastChoiceApproach { get; set; }

    // Constructor for initial state
    public EncounterValues()
    {
        Momentum = 0;
        Insight = 0;
        Resonance = 0;
        Outcome = 0;
        Pressure = 0;
        LastChoiceType = null;
        LastChoiceApproach = null;
    }

    public void ClampValues()
    {
        Momentum = Math.Max(0, Momentum);
        Insight = Math.Max(0, Insight);
        Resonance = Math.Max(0, Resonance);
        Outcome = Math.Max(0, Outcome);
        Pressure = Math.Max(0, Pressure);
    }

    public static EncounterValues WithValues(int momentum, int insight, int resonance, int outcome, int pressure)
    {
        EncounterValues encounterStateValues = new EncounterValues();
        encounterStateValues.Momentum = Math.Max(0, momentum);
        encounterStateValues.Insight = Math.Max(0, insight);
        encounterStateValues.Resonance = Math.Max(0, resonance);
        encounterStateValues.Outcome = Math.Max(0, outcome);
        encounterStateValues.Pressure = Math.Max(0, pressure);
        return encounterStateValues;
    }
}