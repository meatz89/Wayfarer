
/// <summary>
/// The final outcome of a conversation session
/// </summary>
public class ConversationOutcome
{
    /// <summary>
    /// Total comfort achieved
    /// </summary>
    public int TotalComfort { get; set; }


    /// <summary>
    /// Final emotional state
    /// </summary>
    public EmotionalState FinalState { get; set; }

    /// <summary>
    /// Number of turns used
    /// </summary>
    public int TurnsUsed { get; set; }

    /// <summary>
    /// Tokens earned (negative means lost)
    /// </summary>
    public int TokensEarned { get; set; }

    /// <summary>
    /// Whether a letter was unlocked
    /// </summary>
    public bool LetterUnlocked { get; set; }

    /// <summary>
    /// Whether this was a perfect conversation (20+ comfort)
    /// </summary>
    public bool PerfectConversation { get; set; }

    /// <summary>
    /// Any letter that was delivered during conversation
    /// </summary>
    public string DeliveredLetterId { get; set; }

    /// <summary>
    /// Whether obligations were manipulated
    /// </summary>
    public bool ObligationsManipulated { get; set; }

    /// <summary>
    /// The tier of letter unlocked (Simple/Important/Urgent/Critical)
    /// </summary>
    public string LetterTier { get; set; }
    
    /// <summary>
    /// Whether a goal card was successfully played
    /// </summary>
    public bool GoalAchieved { get; set; }

    /// <summary>
    /// Get summary text for the outcome
    /// </summary>
    public string GetSummary()
    {
        if (PerfectConversation)
            return "A profound connection was made.";
        
        if (LetterUnlocked)
            return "Trust was built and help offered.";
        
        if (TokensEarned > 0)
            return "The relationship was maintained.";
        
        if (TokensEarned < 0)
            return "The conversation strained the relationship.";
        
        return "The conversation ended without resolution.";
    }
}