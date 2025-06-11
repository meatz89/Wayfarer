public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int? CurrentFocusPoints { get; private set; }
    public int? MaxFocusPoints { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    public bool IsAwaitingAIResponse { get; private set; }
    public bool CanSelectChoice { get; private set; }
    public bool IsEncounterComplete { get; private set; }
    public bool SuccessfulOutcome { get; private set; }
    public int Energy { get; internal set; }
    public int Concentration { get; internal set; }
    public TimeOfDay CurrentTimeOfDay { get; internal set; } = new TimeOfDay();
    public List<FlagStates> ActiveFlags { get; private set; } = new List<FlagStates>();
    public List<EncounterChoice> AvailableChoices { get; private set; } = new List<EncounterChoice>();
    public string LastChoiceLabel { get; private set; }
    public bool LastChoiceSuccess { get; private set; }

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        EncounterManager encounterManager = gameWorld.ActionStateTracker.CurrentEncounterManager;
        HasActiveEncounter = encounterManager != null;

        StreamingContentState streamingState = gameWorld.StreamingContentState;
        StreamingText = streamingState.CurrentText;
        IsStreaming = streamingState.IsStreaming;
        StreamProgress = streamingState.StreamProgress;

        if (HasActiveEncounter)
        {
            EncounterState state = encounterManager.GetEncounterState();

            LastChoiceLabel = state.LastChoiceNarrative;
            LastChoiceSuccess = state.LastBeatOutcome == BeatOutcomes.Success;

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

    public bool IsEqualTo(GameWorldSnapshot snapshot)
    {
        if (snapshot == null) return false;

        if (HasActiveEncounter != snapshot.HasActiveEncounter) return false;
        if (CurrentFocusPoints != snapshot.CurrentFocusPoints) return false;
        if (MaxFocusPoints != snapshot.MaxFocusPoints) return false;
        if (StreamingText != snapshot.StreamingText) return false;
        if (IsStreaming != snapshot.IsStreaming) return false;
        if (StreamProgress != snapshot.StreamProgress) return false;
        if (IsAwaitingAIResponse != snapshot.IsAwaitingAIResponse) return false;
        if (CanSelectChoice != snapshot.CanSelectChoice) return false;
        if (IsEncounterComplete != snapshot.IsEncounterComplete) return false;
        if (SuccessfulOutcome != snapshot.SuccessfulOutcome) return false;
        if (Energy != snapshot.Energy) return false;
        if (Concentration != snapshot.Concentration) return false;

        if (ActiveFlags.Count != snapshot.ActiveFlags.Count) return false;
        for (int i = 0; i < ActiveFlags.Count; i++)
        {
            if (ActiveFlags[i] != snapshot.ActiveFlags[i]) return false;
        }

        if ((AvailableChoices == null) != (snapshot.AvailableChoices == null)) return false;
        if (AvailableChoices != null)
        {
            if (AvailableChoices.Count != snapshot.AvailableChoices.Count) return false;
            for (int i = 0; i < AvailableChoices.Count; i++)
            {
                if (AvailableChoices[i] != snapshot.AvailableChoices[i]) return false;
            }
        }

        return true;
    }
}