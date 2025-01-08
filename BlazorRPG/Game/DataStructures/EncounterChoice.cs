public class EncounterChoice
{
    public int Index { get; set; }
    public string Encounter { get; set; }
    public ChoiceTypes ChoiceType { get; set; }
    public string Description { get; set; }
    public EncounterStateValues EncounterStateChanges { get; set; } = EncounterStateValues.NoChange;
    public List<EncounterValueChange> EncounterValueChanges { get; set; } = new();
    public List<Requirement> ChoiceRequirements { get; set; } = new();
    public List<Outcome> PermanentCosts { get; set; } = new();
    public List<Outcome> PermanentRewards { get; set; } = new();

}