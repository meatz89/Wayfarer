public interface IAIProvider
{
    Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages);
    string Name { get; }
}
