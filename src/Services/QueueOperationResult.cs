public class QueueOperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<ConnectionType, int> TokensSpent { get; set; }
    public DeliveryObligation[] UpdatedQueue { get; set; }

    public QueueOperationResult(bool success, string errorMessage, Dictionary<ConnectionType, int> tokensSpent, DeliveryObligation[] updatedQueue)
    {
        Success = success;
        ErrorMessage = errorMessage;
        TokensSpent = tokensSpent ?? new Dictionary<ConnectionType, int>();
        UpdatedQueue = updatedQueue;
    }
}
