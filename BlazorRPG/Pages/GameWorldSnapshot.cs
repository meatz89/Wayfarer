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

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        EncounterManager encounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager;
        HasActiveEncounter = encounterManager != null;

        // Capture streaming state regardless of encounter state
        StreamingText = gameWorld.StreamingContentState.CurrentText;
        IsStreaming = gameWorld.StreamingContentState.IsStreaming;
        StreamProgress = gameWorld.StreamingContentState.StreamProgress;

        if (HasActiveEncounter)
        {
            // Get encounter state
            EncounterState state = encounterManager.GetEncounterState();
            CurrentFocusPoints = state.FocusPoints;
            MaxFocusPoints = state.MaxFocusPoints;
            ActiveFlags = state.FlagManager?.GetAllActiveFlags() ?? new List<FlagStates>();
            IsEncounterComplete = state.IsEncounterComplete;

            // Handle choices
            AvailableChoices = encounterManager.GetCurrentChoices();
            IsAwaitingAIResponse = encounterManager._isAwaitingAIResponse;

            // Determine if player can make a choice
            CanSelectChoice = AvailableChoices != null &&
                              AvailableChoices.Count > 0 &&
                              !IsAwaitingAIResponse &&
                              !IsStreaming;

            // Determine outcome (simple implementation - can be expanded)
            SuccessfulOutcome = state.Progress >= state.ProgressThreshold;
        }
    }
}