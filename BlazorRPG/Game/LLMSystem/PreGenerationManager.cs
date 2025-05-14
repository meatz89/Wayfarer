public class PreGenerationManager
{
    private Dictionary<string, Task<NarrativeResult>> _pendingGenerations;
    private Dictionary<string, NarrativeResult> _cachedResults;
    private CancellationTokenSource _cancellationTokenSource;

    public PreGenerationManager()
    {
        _pendingGenerations = new Dictionary<string, Task<NarrativeResult>>();
        _cachedResults = new Dictionary<string, NarrativeResult>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartPreGeneration(string choiceId, Task<NarrativeResult> generationTask)
    {
        _pendingGenerations[choiceId] = generationTask;
    }

    public bool TryGetCachedResult(string choiceId, out NarrativeResult result)
    {
        return _cachedResults.TryGetValue(choiceId, out result);
    }

    public void StoreCompletedResult(string choiceId, NarrativeResult result)
    {
        _cachedResults[choiceId] = result;
        _pendingGenerations.Remove(choiceId);
    }

    public void CancelAllPendingGenerations()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _pendingGenerations.Clear();
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