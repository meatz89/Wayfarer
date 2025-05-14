public class ProgressTrackingWatcher : IResponseStreamWatcher
{
    private readonly IResponseStreamWatcher _innerWatcher;
    private readonly LoadingStateService _loadingStateService;
    private string _fullText = string.Empty;
    private readonly int _estimatedFullLength = 2000;

    public ProgressTrackingWatcher(IResponseStreamWatcher innerWatcher, LoadingStateService loadingStateService)
    {
        _innerWatcher = innerWatcher;
        _loadingStateService = loadingStateService;
    }

    public void OnStreamUpdate(string text)
    {
        _fullText += text;

        // Calculate progress (capped at 99%)
        int progress = Math.Min(99, (int)(_fullText.Length / (float)_estimatedFullLength * 100));

        // Update loading state progress
        _loadingStateService.Progress = progress;

        // Forward to inner watcher
        _innerWatcher?.OnStreamUpdate(text);
    }

    public void OnError(Exception ex)
    {
        _innerWatcher?.OnError(ex);
    }

    public void OnStreamComplete(string completeResponse)
    {
        // Set progress to 100% when complete
        _loadingStateService.Progress = 100;

        _innerWatcher?.OnStreamComplete(completeResponse);
    }
}