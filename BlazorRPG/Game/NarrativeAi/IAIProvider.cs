public interface IAIProvider
{
    Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages, string model, string fallbackModel);
    string Name { get; }
}
