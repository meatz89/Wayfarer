public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly List<Encounter> encounters;

    public EncounterSystem(
        GameState gameState,
        GameContentProvider contentProvider,
        ChoiceSystem choiceSystem
        )
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
        this.encounters = contentProvider.GetEncounters();
    }

    public Encounter GetAvailableEncounter(BasicActionTypes action, LocationNames location)
    {
        return encounters.FirstOrDefault(x =>
            x.ActionType == action &&
            x.LocationName == location);
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice encounterChoice)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        EncounterStateValues changes = encounterChoice.EncounterStateChanges;
        encounter.InitialState.ApplyChanges(changes);

    }

    public List<EncounterChoice> GetCurrentStageChoices(Encounter encounter)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        EncounterActionContext context = new EncounterActionContext
        {
            ActionType = encounter.ActionType,
            LocationType = encounter.LocationType,
            TimeSlot = encounter.TimeSlot,
            CurrentValues = encounter.InitialState,
        };

        choices = choiceSystem.GenerateChoices(context);
        return choices;
    }

    public bool GetNextStage(Encounter encounter)
    {
        if (IsComplete(encounter))
        {
            return false;
        }

        encounter.currentStage++;
        return true;
    }

    public bool IsComplete(Encounter encounter)
    {
        if (encounter.currentStage >= encounter.numberOfStages) return true;
        return false;
    }

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.Stages[encounter.currentStage - 1];
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
    }
}