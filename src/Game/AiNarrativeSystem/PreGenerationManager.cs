public class PreGenerationManager
{
    private Dictionary<string, Task<AIResponse>> _pendingGenerations;
    private Dictionary<string, AIResponse> _cachedResults;
    private CancellationTokenSource _cancellationTokenSource;
    private ILogger<ConversationFactory> _logger;

    public PreGenerationManager(ILogger<ConversationFactory> logger = null)
    {
        _pendingGenerations = new Dictionary<string, Task<AIResponse>>();
        _cachedResults = new Dictionary<string, AIResponse>();
        _cancellationTokenSource = new CancellationTokenSource();
        _logger = logger;
    }

    public void StartPreGeneration(string choiceId, Task<AIResponse> generationTask)
    {
        _logger?.LogInformation($"Starting pre-generation for choice: {choiceId}");
        _pendingGenerations[choiceId] = generationTask;
    }

    public bool TryGetCachedResult(string choiceId, out AIResponse result)
    {
        bool hasResult = _cachedResults.TryGetValue(choiceId, out result);
        if (hasResult)
        {
            _logger?.LogInformation($"Using pre-generated result for choice: {choiceId}");
        }
        return hasResult;
    }

    public void StoreCompletedResult(string choiceId, AIResponse result)
    {
        _logger?.LogInformation($"Completed pre-generation for choice: {choiceId}");
        _cachedResults[choiceId] = result;
        _pendingGenerations.Remove(choiceId);
    }

    public void CancelAllPendingGenerations()
    {
        int pendingCount = _pendingGenerations.Count;
        if (pendingCount > 0)
        {
            _logger?.LogInformation($"Cancelling {pendingCount} pending pre-generations");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _pendingGenerations.Clear();
        }
    }

    public CancellationToken GetCancellationToken()
    {
        return _cancellationTokenSource.Token;
    }

    public void Clear()
    {
        CancelAllPendingGenerations();
        _cachedResults.Clear();
    }
}