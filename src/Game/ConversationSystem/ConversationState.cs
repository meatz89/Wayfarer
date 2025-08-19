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

    // Focus for complex conversations (maps to Patience in UI)
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; set; }

    // Comfort tracking for letter generation thresholds
    public int TotalComfort { get; set; }
    public int StartingPatience { get; set; }

    // UI state flags
    public bool IsQueueInterfaceOpen { get; set; }

    // Card game mechanics - minimal state tracking for this conversation only
    public HashSet<string> PlayedCardIds { get; private set; } = new HashSet<string>(); // Cards played this conversation
    
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
        StartingPatience = maxFocusPoints;
        TotalComfort = 0;
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

    /// <summary>
    /// Add comfort from successful card plays
    /// </summary>
    public void AddComfort(int amount)
    {
        TotalComfort += amount;
    }

    /// <summary>
    /// Check if comfort threshold for letter generation is met
    /// </summary>
    public bool HasReachedLetterThreshold()
    {
        return TotalComfort >= StartingPatience;
    }

    /// <summary>
    /// Check if comfort threshold for perfect conversation is met
    /// </summary>
    public bool HasReachedPerfectThreshold()
    {
        return TotalComfort >= (StartingPatience * 1.5);
    }

    /// <summary>
    /// Check if minimum comfort to maintain relationship is met
    /// </summary>
    public bool HasReachedMaintainThreshold()
    {
        return TotalComfort >= (StartingPatience / 2);
    }

    // Card game mechanics - single responsibility: track played cards
    
    /// <summary>
    /// Mark a card as played this conversation
    /// </summary>
    public void PlayCard(string cardId)
    {
        PlayedCardIds.Add(cardId);
    }

    /// <summary>
    /// Check if a card has been played this conversation
    /// </summary>
    public bool IsCardPlayed(string cardId)
    {
        return PlayedCardIds.Contains(cardId);
    }

    /// <summary>
    /// Reset for new conversation (shuffle discarded cards back into deck)
    /// </summary>
    public void ResetForNewConversation()
    {
        PlayedCardIds.Clear();
    }
}