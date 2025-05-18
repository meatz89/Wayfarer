public class ChoiceOutcome
{
    public int ProgressGained { get; }
    public bool IsEncounterOver { get; }
    public EncounterOutcomes Outcome { get; }
    public string Description { get; }
    public int HealthChange { get; }
    public int ConcentrationChange { get; }

    public ChoiceOutcome(
        int progressGained,
        string description,
        bool encounterWillEnd,
        EncounterOutcomes projectedOutcome,
        int healthChange,
        int concentrationChange)
    {
        ProgressGained = progressGained;
        Description = description;
        IsEncounterOver = encounterWillEnd;
        Outcome = projectedOutcome;
        HealthChange = healthChange;
        ConcentrationChange = concentrationChange;
    }
}