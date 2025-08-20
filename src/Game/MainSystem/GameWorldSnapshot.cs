using System.Collections.Generic;
using System.Linq;

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
    // ConversationChoice system removed - using new conversation system
    public string LastChoiceLabel { get; private set; }
    public bool LastChoiceSuccess { get; private set; }
    public bool IsConversationComplete { get; private set; }
    public bool ConversationPending { get; internal set; }

    public GameWorldSnapshot(GameWorld gameWorld)
    {
        // Encounter system removed - using letter queue and conversations
        HasActiveEncounter = false;

        StreamingContentState streamingState = gameWorld.StreamingContentState;
        StreamingText = streamingState.CurrentText;
        IsStreaming = streamingState.IsStreaming;
        StreamProgress = streamingState.StreamProgress;

        // Conversation state system removed - using new conversation architecture
        ConversationPending = false;
        IsAwaitingAIResponse = false;
        CanSelectChoice = false;
        IsConversationComplete = false;
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
        // Conversation choice comparison removed - using new conversation system

        return true;
    }
}