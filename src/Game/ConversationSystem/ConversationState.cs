/// <summary>
/// Tracks the state of an ongoing conversation
/// </summary>
public class ConversationState
{
    public Player Player { get; set; }
    public NPC CurrentNPC { get; set; }
    
    // Conversation progress
    public int DurationCounter { get; set; }
    public int MaxDuration { get; set; }
    public bool IsConversationComplete { get; set; }
    public string CurrentNarrative { get; set; }
    public string LastChoiceNarrative { get; set; }
    
    // Focus for complex conversations
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; set; }
    
    // Conversation outcome tracking
    public ConversationOutcome Outcome { get; set; }
    public int ConversationSeed { get; }
    
    public ConversationState(Player player, NPC npc, int maxFocusPoints = 10, int maxDuration = 8)
    {
        Player = player;
        CurrentNPC = npc;
        MaxFocusPoints = maxFocusPoints;
        MaxDuration = maxDuration;
        FocusPoints = MaxFocusPoints;
        DurationCounter = 0;
        IsConversationComplete = false;
        ConversationSeed = Environment.TickCount;
    }
    
    public void AdvanceDuration(int amount = 1)
    {
        DurationCounter += amount;
        
        if (DurationCounter >= MaxDuration)
        {
            IsConversationComplete = true;
            Outcome = ConversationOutcome.TimedOut;
        }
    }
}

public enum ConversationOutcome
{
    Ongoing,
    Success,
    Failure,
    TimedOut,
    Abandoned
}