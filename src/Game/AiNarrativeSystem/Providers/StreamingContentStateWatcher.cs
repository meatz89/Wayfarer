using System.Text;

public class StreamingContentStateWatcher : IResponseStreamWatcher
{
    private readonly StreamingContentState _streamingContentState;
    private readonly StringBuilder _buffer = new StringBuilder();

    public StreamingContentStateWatcher(StreamingContentState streamingContentState)
    {
        _streamingContentState = streamingContentState;
    }

    public void BeginStreaming()
    {
        _streamingContentState.BeginStreaming();
    }

    public void OnStreamUpdate(string chunk)
    {
        if (_streamingContentState == null || string.IsNullOrEmpty(chunk))
            return;

        // Add the chunk to our buffer
        _buffer.Append(chunk);

        _streamingContentState.UpdateStreamingText(_buffer.ToString());
    }

    public void OnStreamComplete(string completeResponse)
    {
        if (_streamingContentState == null)
            return;

        _streamingContentState.CompleteStreaming(completeResponse);

        // Clear buffer for next use
        _buffer.Clear();
    }

    public void OnError(Exception ex)
    {
        if (_streamingContentState == null)
            return;

        // Handle errors
        _streamingContentState.SetError(ex.Message);

        // Clear buffer for next use
        _buffer.Clear();
    }
}