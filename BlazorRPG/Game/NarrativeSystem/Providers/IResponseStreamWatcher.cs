public interface IResponseStreamWatcher
{
    void OnResponseChunk(string chunk);
    void OnResponseComplete(string completeResponse);
    void OnError(Exception ex);
}
