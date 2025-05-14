public interface IAIProvider
{
    Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher);

    string Name { get; }
}
