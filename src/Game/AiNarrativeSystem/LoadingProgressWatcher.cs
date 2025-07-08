public class LoadingProgressWatcher : IResponseStreamWatcher
{
    private LoadingStateService _loadingStateService;
    private string _fullText = string.Empty;
    private int _estimatedFullLength = 1000;

    public LoadingProgressWatcher(LoadingStateService loadingStateService)
    {
        _loadingStateService = loadingStateService;
    }

    public void OnStreamUpdate(string text)
    {
        _fullText += text;

        // Calculate progress (capped at 99%)
        int progress = Math.Min(99, (int)(_fullText.Length / (float)_estimatedFullLength * 100));

        // Update loading state progress
        _loadingStateService.Progress = progress;
    }

    public void OnError(Exception ex)
    {
    }

    public void OnStreamComplete(string completeResponse)
    {
        // Set progress to 100% when complete
        _loadingStateService.Progress = 100;
    }
}