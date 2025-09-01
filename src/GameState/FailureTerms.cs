public class FailureTerms
{
    public int DeadlineHours { get; set; }
    public int QueuePosition { get; set; } // 1 = forced first
    public int Payment { get; set; }
    // Additional failure consequences
    public int ReputationLoss { get; set; }
    public string CreatesBurden { get; set; } // New burden card ID to add
    public TokenType? LoseToken { get; set; } // Token type to lose on failure
}