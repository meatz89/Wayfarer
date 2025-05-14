public class LoadingStateService
{
    private bool isLoading;
    private string message = string.Empty;
    private int progress;

    public static LoadingStateService Current { get; private set; }

    public LoadingStateService()
    {
        Current = this;
    }

    // Simple getters and setters
    public bool IsLoading
    {
        get => isLoading;
        set => isLoading = value;
    }

    public string Message
    {
        get => message;
        set => message = value;
    }

    public int Progress
    {
        get => progress;
        set => progress = value;
    }

    public void StartLoading(string newMessage = null)
    {
        Message = newMessage ?? "Generating narrative...";
        Progress = 0;
        IsLoading = true;
    }

    public void UpdateProgress(int newProgress)
    {
        Progress = newProgress;
    }

    public void StopLoading()
    {
        IsLoading = false;
        Message = string.Empty;
        Progress = 0;
    }
}