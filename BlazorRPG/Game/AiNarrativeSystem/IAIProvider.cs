public interface IAIProvider
{
    Task<bool> IsAvailableAsync();

    Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        List<IResponseStreamWatcher> watchers);

    string Name { get; }
}
