

/// <summary>
/// Strongly-typed parameters for letter tier configuration.
/// Replaces tuple usage in ConversationContent.GetTierParameters().
/// </summary>
public record LetterTierParameters
{
    /// <summary>
    /// Deadline for the letter in game segments
    /// </summary>
    public int DeadlineInSegments { get; init; }

    /// <summary>
    /// Payment amount in coins for completing the letter
    /// </summary>
    public int PaymentInCoins { get; init; }

    /// <summary>
    /// The stakes involved if the letter delivery fails
    /// </summary>
    public StakeType Stakes { get; init; }

    /// <summary>
    /// The emotional intensity level of the letter
    /// </summary>
    public EmotionalFocus EmotionalFocus { get; init; }

    /// <summary>
    /// Create letter tier parameters
    /// </summary>
    public LetterTierParameters(int deadlineInSegments, int paymentInCoins, StakeType stakes, EmotionalFocus emotionalFocus)
    {
        DeadlineInSegments = deadlineInSegments;
        PaymentInCoins = paymentInCoins;
        Stakes = stakes;
        EmotionalFocus = emotionalFocus;
    }
}
