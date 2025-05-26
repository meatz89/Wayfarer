public class ChoiceOutcome
{
    public int ProgressGained { get; }
    public string NarrativeDescription { get; }
    public string MechanicalDescription { get; }
    public bool IsEncounterOver { get; }
    public EncounterOutcomes Outcome { get; }
    public bool SkillCheckSuccess { get; set; }

    public ChoiceOutcome(
        int progressGained,
        string narrativeDescription,
        string mechanicalDescription,
        bool isEncounterOver,
        EncounterOutcomes outcome)
    {
        ProgressGained = progressGained;
        NarrativeDescription = narrativeDescription;
        MechanicalDescription = mechanicalDescription;
        IsEncounterOver = isEncounterOver;
        Outcome = outcome;
    }
}