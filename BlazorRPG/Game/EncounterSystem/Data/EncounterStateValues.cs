using System.Security.Cryptography.X509Certificates;

public class EncounterStateValues
{
    public int Outcome { get; set; }
    public int Pressure { get; set; }
    public int Momentum { get; set; }
    public int Insight { get; set; }
    public int Resonance { get; set; }

    // Track last choice for insight decay
    public ChoiceArchetypes? LastChoiceType { get; set; }
    public ChoiceApproaches? LastChoiceApproach { get; set; }

    // Constructor for initial state
    public EncounterStateValues(int difficulty, int playerLevel)
    {
        Outcome = 2 + (playerLevel - difficulty); // Base calculation
        Pressure = 2;
        Momentum = 2;
        Insight = 2;
        Resonance = 2;
        LastChoiceType = null;
        LastChoiceApproach = null;
    }

    public void ClampValues()
    {
        Outcome = Math.Max(0, Outcome);
        Momentum = Math.Max(0, Momentum);
        Insight = Math.Max(0, Insight);
        Resonance = Math.Max(0, Resonance);
        Pressure = Math.Max(0, Pressure);
    }

    public static EncounterStateValues WithValues(int outcome, int momentum, int insight, int resonance, int pressure)
    {
        EncounterStateValues encounterStateValues = new EncounterStateValues(1, 1);
        encounterStateValues.Outcome = Math.Max(0, outcome);
        encounterStateValues.Momentum = Math.Max(0, momentum);
        encounterStateValues.Insight = Math.Max(0, insight);
        encounterStateValues.Resonance = Math.Max(0, resonance);
        encounterStateValues.Pressure = Math.Max(0, pressure);
        return encounterStateValues;
    }
}