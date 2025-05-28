public class PreGenerationManager
{
    private Dictionary<string, Task<AIGameMasterResponse>> _pendingGenerations;
    private Dictionary<string, AIGameMasterResponse> _cachedResults;
    private CancellationTokenSource _cancellationTokenSource;
    private ILogger<EncounterSystem> _logger;

    public PreGenerationManager(ILogger<EncounterSystem> logger = null)
    {
        _pendingGenerations = new Dictionary<string, Task<AIGameMasterResponse>>();
        _cachedResults = new Dictionary<string, AIGameMasterResponse>();
        _cancellationTokenSource = new CancellationTokenSource();
        _logger = logger;
    }

    public void StartPreGeneration(string choiceId, Task<AIGameMasterResponse> generationTask)
    {
        _logger?.LogInformation($"Starting pre-generation for choice: {choiceId}");
        _pendingGenerations[choiceId] = generationTask;
    }

    public bool TryGetCachedResult(string choiceId, out AIGameMasterResponse result)
    {
        bool hasResult = _cachedResults.TryGetValue(choiceId, out result);
        if (hasResult)
        {
            _logger?.LogInformation($"Using pre-generated result for choice: {choiceId}");
        }
        return hasResult;
    }

    public void StoreCompletedResult(string choiceId, AIGameMasterResponse result)
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