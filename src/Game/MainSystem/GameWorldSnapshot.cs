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
    public int Stamina { get; internal set; }
    public int Concentration { get; internal set; }
    public TimeBlocks CurrentTimeBlock { get; internal set; } = new TimeBlocks();
    // Flag system removed - using connection tokens instead
    public List<ConversationChoice> AvailableChoices { get; private set; } = new List<ConversationChoice>();
    public string LastChoiceLabel { get; private set; }
    public bool LastChoiceSuccess { get; private set; }

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        // Encounter system removed - using letter queue and conversations
        HasActiveEncounter = false;

        StreamingContentState streamingState = gameWorld.StreamingContentState;
        StreamingText = streamingState.CurrentText;
        IsStreaming = streamingState.IsStreaming;
        StreamProgress = streamingState.StreamProgress;

        // Conversation state will be handled separately
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
        if (Stamina != snapshot.Stamina) return false;
        if (Concentration != snapshot.Concentration) return false;

        // Flag comparison removed - using connection tokens instead

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