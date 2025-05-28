public interface IAIProvider
{
    Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        IResponseStreamWatcher watcher);

    string Name { get; }
}
