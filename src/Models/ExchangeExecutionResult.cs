namespace Wayfarer.Models
{
    /// <summary>
    /// Strongly-typed result for exchange execution operations.
    /// Replaces anonymous objects in ConversationFacade exchange handling.
    /// </summary>
    public record ExchangeExecutionResult
    {
        /// <summary>
        /// Whether the exchange operation was successful
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        /// Create a successful exchange result
        /// </summary>
        public static ExchangeExecutionResult Successful() => new(true, null);

        /// <summary>
        /// Create a failed exchange result with error message
        /// </summary>
        public static ExchangeExecutionResult Failed(string errorMessage) => new(false, errorMessage);

        /// <summary>
        /// Private constructor to enforce factory methods
        /// </summary>
        private ExchangeExecutionResult(bool success, string? error)
        {
            Success = success;
            Error = error;
        }
    }
}