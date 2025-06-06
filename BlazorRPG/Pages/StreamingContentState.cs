public class StreamingContentState
{
    public string CurrentText { get; private set; } = string.Empty;
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    public bool HasError { get; private set; }
    public string ErrorMessage { get; private set; }

    private const int ESTIMATED_TOTAL_TOKENS = 1000;

    public StreamingContentState()
    {
        IsStreaming = false;
        StreamProgress = 0;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void BeginStreaming(string initialText = "")
    {
        CurrentText = initialText;
        IsStreaming = true;
        StreamProgress = 0;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void UpdateStreamingText(string partialText)
    {
        if (!IsStreaming) return;

        CurrentText = partialText;

        // Estimate progress based on token count (approximate)
        int estimatedTokens = partialText.Length / 4; // Rough estimate: 4 chars per token
        StreamProgress = Math.Min(0.95f, (float)estimatedTokens / ESTIMATED_TOTAL_TOKENS);
    }

    public void CompleteStreaming(string completeText)
    {
        if (string.IsNullOrEmpty(completeText)) return;

        CurrentText = completeText;
        IsStreaming = false;
        StreamProgress = 1.0f;
    }

    public void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        IsStreaming = false;
        StreamProgress = 1.0f;
    }

    public void SetFullText(string text)
    {
        CurrentText = text;
        IsStreaming = false;
        StreamProgress = 1.0f;
    }
}