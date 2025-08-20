using System;

/// <summary>
/// Simple loading state service for UI
/// </summary>
public class LoadingStateService
{
    public bool IsLoading { get; private set; }
    public string LoadingMessage { get; private set; } = "";
    public string Message => LoadingMessage; // Alias for UI compatibility
    public int Progress { get; private set; } = 0;
    
    public event Action OnLoadingStateChanged;
    
    public void StartLoading(string message = "Loading...")
    {
        IsLoading = true;
        LoadingMessage = message;
        OnLoadingStateChanged?.Invoke();
    }
    
    public void StopLoading()
    {
        IsLoading = false;
        LoadingMessage = "";
        OnLoadingStateChanged?.Invoke();
    }
}