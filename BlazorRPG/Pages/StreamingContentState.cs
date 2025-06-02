public class StreamingContentState
{
    public string CurrentText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    private string targetText;
    private int currentCharIndex;
    private DateTime lastUpdateTime;
    private const int CHARS_PER_UPDATE = 3;

    public StreamingContentState()
    {
        CurrentText = string.Empty;
        IsStreaming = false;
        StreamProgress = 0;
        targetText = string.Empty;
        currentCharIndex = 0;
    }

    public void BeginStreaming(string fullText)
    {
        CurrentText = string.Empty;
        targetText = fullText;
        IsStreaming = true;
        StreamProgress = 0;
        currentCharIndex = 0;
        lastUpdateTime = DateTime.Now;
    }

    public void Update()
    {
        if (!IsStreaming)
        {
            return;
        }

        // Only update if enough time has passed (simulates token streaming)
        DateTime now = DateTime.Now;
        if ((now - lastUpdateTime).TotalMilliseconds < 50)
        {
            return;
        }

        // Update a few characters at a time
        currentCharIndex += CHARS_PER_UPDATE;
        if (currentCharIndex >= targetText.Length)
        {
            // Streaming complete
            currentCharIndex = targetText.Length;
            IsStreaming = false;
            StreamProgress = 1.0f;
        }
        else
        {
            StreamProgress = (float)currentCharIndex / targetText.Length;
        }

        // Update current text
        CurrentText = targetText.Substring(0, currentCharIndex);
        lastUpdateTime = now;
    }
}