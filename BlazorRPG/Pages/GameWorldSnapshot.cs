public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int CurrentFocusPoints { get; private set; }
    public int MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    public List<EncounterChoice> AvailableChoices { get; }

    // Added fields for integration
    public AIResponse CurrentAIResponse { get; private set; }
    public bool IsAwaitingAIResponse { get; private set; }
    public bool CanSelectChoice { get; private set; }

    public GameWorldSnapshot(GameWorld gameWorld, List<EncounterChoice> availableChoices)
    {
        HasActiveEncounter = gameWorld.CurrentEncounterManager != null;

        if (HasActiveEncounter)
        {
            EncounterState state = gameWorld.CurrentEncounterManager.GetEncounterState();
            CurrentFocusPoints = state.FocusPoints;
            MaxFocusPoints = state.MaxFocusPoints;
            ActiveFlags = state.FlagManager.GetAllActiveFlags();

            StreamingContentState streamingState = gameWorld.CurrentEncounterManager.GetStreamingState();
            StreamingText = streamingState.CurrentText;
            IsStreaming = streamingState.IsStreaming;
            StreamProgress = streamingState.StreamProgress;

            AvailableChoices = gameWorld.CurrentEncounterManager.GetCurrentChoices();
            CanSelectChoice = !IsStreaming && AvailableChoices != null && AvailableChoices.Count > 0;
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
        AvailableChoices = availableChoices;
    }
}