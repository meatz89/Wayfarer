public interface IResponseStreamWatcher
{
    void OnStreamUpdate(string chunk);
    void OnStreamComplete(string completeResponse);
    void OnError(Exception ex);
}
