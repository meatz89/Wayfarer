public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int CurrentFocusPoints { get; private set; }
    public int MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }

    // Added fields for integration
    public AIResponse CurrentAIResponse { get; private set; }
    public bool IsAwaitingAIResponse { get; private set; }
    public bool CanSelectChoice { get; private set; }

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        HasActiveEncounter = gameWorld.CurrentEncounter != null;

        if (HasActiveEncounter)
        {
            CurrentFocusPoints = gameWorld.ActionStateTracker.EncounterManager.state.FocusPoints;
            MaxFocusPoints = gameWorld.ActionStateTracker.EncounterManager.state.MaxFocusPoints;
            ActiveFlags = gameWorld.ActionStateTracker.EncounterManager.state.FlagManager.GetAllActiveFlags();
        }

        StreamingText = gameWorld.StreamingContentState.CurrentText;
        IsStreaming = gameWorld.StreamingContentState.IsStreaming;
        StreamProgress = gameWorld.StreamingContentState.StreamProgress;

        CurrentAIResponse = gameWorld.CurrentAIResponse;
        IsAwaitingAIResponse = gameWorld.IsAwaitingAIResponse;

        // Player can select a choice when:
        // 1. There is an AI response with choices
        // 2. We're not waiting for a new AI response
        // 3. No text is currently streaming
        CanSelectChoice = gameWorld.CurrentAIResponse != null &&
                           !gameWorld.IsAwaitingAIResponse &&
                           !gameWorld.StreamingContentState.IsStreaming;
    }
}