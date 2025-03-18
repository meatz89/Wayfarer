public class EncounterViewModel
{
    public EncounterManager CurrentEncounter { get; set; }
    public List<UserEncounterChoiceOption> CurrentChoices { get; set; }
    public string ChoiceSetName { get; set; }
    public EncounterState State { get; set; }
    public EncounterResult EncounterResult { get; set; }
}
