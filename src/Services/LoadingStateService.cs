/// <summary>
/// Simple loading state service for UI
/// </summary>
public class LoadingStateService
{
public bool IsLoading { get; private set; }
public string LoadingMessage { get; private set; }
public int Progress { get; private set; }
private bool stateChangedFlag;

public LoadingStateService()
{
    LoadingMessage = "";
    Progress = 0;
}

public void StartLoading(string message)
{
    IsLoading = true;
    LoadingMessage = message;
    stateChangedFlag = true;
}

public void StopLoading()
{
    IsLoading = false;
    LoadingMessage = "";
    stateChangedFlag = true;
}

public bool HasStateChanged()
{
    bool changed = stateChangedFlag;
    stateChangedFlag = false;
    return changed;
}
}