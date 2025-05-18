public class ChoiceProjection
{
    public EncounterOption Choice { get; }
    public string Description { get; set; }
    public int ProgressGained { get; set; }
    public bool EncounterWillEnd { get; set; }
    public EncounterOutcomes ProjectedOutcome { get; set; } = EncounterOutcomes.Partial;
    public int HealthChange { get; set; }
    public int ConcentrationChange { get; set; }

    public ChoiceProjection(EncounterOption choice)
    {
        Choice = choice;
    }
}
