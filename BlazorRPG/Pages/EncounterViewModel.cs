using BlazorRPG.Game.EncounterManager;

public class EncounterViewModel
{
    public EncounterManager CurrentEncounter;
    public List<UserEncounterChoiceOption> CurrentChoices;
    public string ChoiceSetName;
    public EncounterState State;
    public EncounterResult EncounterResult;
}
