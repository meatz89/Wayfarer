public interface IAIProvider
{
    Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        List<IResponseStreamWatcher> watchers);

    string Name { get; }
}
