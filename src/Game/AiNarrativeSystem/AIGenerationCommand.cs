public class AIGenerationCommand
{
    public string Id { get; }
    public List<ConversationEntry> Messages { get; }
    public List<IResponseStreamWatcher> Watchers { get; }
    public int Priority { get; }
    public string SourceSystem { get; }
    public DateTime Timestamp { get; }
    public TaskCompletionSource<string> CompletionSource { get; }

    public AIGenerationCommand(
        string id,
        List<ConversationEntry> messages,
        List<IResponseStreamWatcher> watchers,
        int priority,
        string sourceSystem)
    {
        Id = id;
        Messages = messages;
        Watchers = watchers;
        Priority = priority;
        SourceSystem = sourceSystem;
        Timestamp = DateTime.UtcNow;
        CompletionSource = new TaskCompletionSource<string>();
    }
}
