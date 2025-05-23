public class ChoiceOutcome
{
    public int ProgressGained { get; }
    public bool IsEncounterOver { get; }
    public EncounterOutcomes Outcome { get; }
    public string NarrativeDescription { get; }
    public string MechanicalDescription { get; }
    public int HealthChange { get; }
    public int ConcentrationChange { get; }

    public ChoiceOutcome(
        int progressGained,
        string narrativeDescription,
        string mechanicalDescription,
        bool encounterWillEnd,
        EncounterOutcomes projectedOutcome)
    {
        ProgressGained = progressGained;
        NarrativeDescription = narrativeDescription;
        MechanicalDescription = mechanicalDescription;
        IsEncounterOver = encounterWillEnd;
        Outcome = projectedOutcome;
    }
}