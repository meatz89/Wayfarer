
public class EncounterState
{
    public static EncounterState PreviousEncounterState { get; set; }
    public EncounterOption PreviousChoice { get; set; }
    public int CurrentProgress { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public int CurrentTurn { get; private set; }
    public Encounter EncounterInfo { get; }
    public LocationSpot LocationSpot { get; set; }

    public EncounterState(Encounter encounterInfo, PlayerState playerState)
    {
        CurrentProgress = 0;
        CurrentStageIndex = 0;
        CurrentTurn = 0;
        EncounterInfo = encounterInfo;
        PreviousEncounterState = this;
    }

    public static EncounterState CreateDeepCopy(EncounterState originalState, PlayerState playerState)
    {
        EncounterState copy = new EncounterState(originalState.EncounterInfo, playerState.Serialize());
        copy.CurrentProgress = originalState.CurrentProgress;
        copy.CurrentStageIndex = originalState.CurrentStageIndex;
        copy.CurrentTurn = originalState.CurrentTurn;
        copy.LocationSpot = originalState.LocationSpot;
        return copy;
    }

    public ChoiceProjection ApplyChoice(PlayerState playerState, Encounter encounterInfo, EncounterOption choice)
    {
        UpdateStateHistory(choice);
        ChoiceProjection projection = CreateChoiceProjection(choice, playerState);
        ApplyChoiceProjection(playerState, encounterInfo, projection);
        return projection;
    }

    private void ApplyChoiceProjection(PlayerState playerState, Encounter encounterInfo, ChoiceProjection projection)
    {
        CurrentProgress += projection.ProgressGained;
        CurrentTurn++;

        if (CurrentStageIndex < EncounterInfo.Stages.Count)
        {
            EncounterInfo.Stages[CurrentStageIndex].IsCompleted = true;

            if (CurrentStageIndex < EncounterInfo.Stages.Count - 1)
            {
                CurrentStageIndex++;
            }
        }
    }

    public void UpdateStateHistory(EncounterOption selectedChoice)
    {
        PreviousChoice = selectedChoice;
    }

    public ChoiceProjection CreateChoiceProjection(EncounterOption choice, PlayerState playerState)
    {
        Location location = playerState.CurrentLocation;
        return SkillCheckService.CreateChoiceProjection(
            choice,
            CurrentProgress,
            EncounterInfo.TotalProgress,
            CurrentTurn,
            playerState,
            location);
    }

}