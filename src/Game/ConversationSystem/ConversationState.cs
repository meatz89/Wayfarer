/// <summary>
/// Tracks the state of an ongoing conversation
/// </summary>
public class ConversationState
{
    public Player Player { get; set; }
    public NPC CurrentNPC { get; set; }
    public GameWorld GameWorld { get; set; }

    // Conversation progress
    public int DurationCounter { get; set; }
    public int MaxDuration { get; set; }
    public bool IsConversationComplete { get; set; }
    public string CurrentNarrative { get; set; }
    public string LastChoiceNarrative { get; set; }

    // Focus for complex conversations
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; set; }

    // UI state flags
    public bool IsQueueInterfaceOpen { get; set; }

    // Conversation seed for deterministic generation
    public int ConversationSeed { get; }

    public ConversationState(Player player, NPC npc, GameWorld gameWorld, int maxFocusPoints, int maxDuration)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        CurrentNPC = npc ?? throw new ArgumentNullException(nameof(npc));
        GameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        MaxFocusPoints = maxFocusPoints;
        MaxDuration = maxDuration;
        FocusPoints = MaxFocusPoints;
        DurationCounter = 0;
        IsConversationComplete = false;
        ConversationSeed = Environment.TickCount;
    }

    public void AdvanceDuration(int amount)
    {
        DurationCounter += amount;

        if (DurationCounter >= MaxDuration)
        {
            IsConversationComplete = true;
        }
    }
}