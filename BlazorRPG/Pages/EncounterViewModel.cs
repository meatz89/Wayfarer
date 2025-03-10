using BlazorRPG.Game.EncounterManager;

public class EncounterViewModel
{
    public Encounter CurrentEncounter;
    public List<UserEncounterChoiceOption> CurrentChoices;
    public string ChoiceSetName;
    public EncounterState State;
}
