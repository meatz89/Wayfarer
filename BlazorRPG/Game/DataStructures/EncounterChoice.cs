public class EncounterChoice
{
    public int Index { get; set; }
    public ChoiceTypes ChoiceType { get; set; }
    public string Description { get; set; }
    public string Encounter { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; } = new();
    public List<Outcome> Rewards { get; set; } = new();
    public EncounterStateValues EncounterStateChanges { get; set; } = EncounterStateValues.NoChange;
}