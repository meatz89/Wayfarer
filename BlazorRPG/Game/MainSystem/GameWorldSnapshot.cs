public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int? CurrentFocusPoints { get; private set; }
    public int? MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    public List<EncounterChoice> AvailableChoices { get; private set; }
    public bool IsAwaitingAIResponse { get; private set; }
    public bool CanSelectChoice { get; private set; }
    public bool IsEncounterComplete { get; private set; }
    public bool SuccessfulOutcome { get; private set; }
    public int Energy { get; internal set; }
    public int Concentration { get; internal set; }
    public TimeOfDay CurrentTimeOfDay { get; internal set; }

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        EncounterManager encounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager;
        HasActiveEncounter = encounterManager != null;

        StreamingContentState streamingState  = gameWorld.StreamingContentState;
        StreamingText = streamingState.CurrentText;
        IsStreaming = streamingState.IsStreaming;
        StreamProgress = streamingState.StreamProgress;

        if (HasActiveEncounter)
        {
            EncounterState state = encounterManager.GetEncounterState();
            CurrentFocusPoints = state.FocusPoints;
            MaxFocusPoints = state.MaxFocusPoints;
            ActiveFlags = state.FlagManager?.GetAllActiveFlags() ?? new List<FlagStates>();
            IsEncounterComplete = state.IsEncounterComplete;

            AvailableChoices = encounterManager.GetCurrentChoices();
            IsAwaitingAIResponse = encounterManager._isAwaitingAIResponse;

            CanSelectChoice = AvailableChoices != null &&
                              AvailableChoices.Count > 0 &&
                              !IsAwaitingAIResponse &&
                              !IsStreaming;

            SuccessfulOutcome = state.Progress >= state.ProgressThreshold;
        }
    }
}