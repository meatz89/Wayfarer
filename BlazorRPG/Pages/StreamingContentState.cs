public class StreamingContentState
{
    public string CurrentText { get; private set; } = string.Empty;
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }

    private string _targetText = string.Empty;
    private int _currentCharIndex;
    private DateTime _lastUpdateTime;
    private const int CHARS_PER_UPDATE = 3;

    public StreamingContentState()
    {
        IsStreaming = false;
        StreamProgress = 0;
    }

    public void BeginStreaming(string fullText)
    {
        if (string.IsNullOrEmpty(fullText)) return;

        CurrentText = string.Empty;
        _targetText = fullText;
        IsStreaming = true;
        StreamProgress = 0;
        _currentCharIndex = 0;
        _lastUpdateTime = DateTime.Now;
    }

    public void Update()
    {
        if (!IsStreaming) return;

        // Only update if enough time has passed (simulates token streaming)
        DateTime now = DateTime.Now;
        if ((now - _lastUpdateTime).TotalMilliseconds < 50) return;

        // Update a few characters at a time
        _currentCharIndex += CHARS_PER_UPDATE;
        if (_currentCharIndex >= _targetText.Length)
        {
            // Streaming complete
            _currentCharIndex = _targetText.Length;
            IsStreaming = false;
            StreamProgress = 1.0f;
        }
        else
        {
            StreamProgress = (float)_currentCharIndex / _targetText.Length;
        }

        // Update current text
        CurrentText = _targetText.Substring(0, _currentCharIndex);
        _lastUpdateTime = now;
    }

    public void SetFullText(string text)
    {
        CurrentText = text;
        IsStreaming = false;
        StreamProgress = 1.0f;
    }
}