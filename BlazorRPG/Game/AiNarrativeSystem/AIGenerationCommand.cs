public class AIGenerationCommand
{
    public string Id { get; }
    public List<ConversationEntry> Messages { get; }
    public IResponseStreamWatcher Watcher { get; }
    public int Priority { get; }
    public string SourceSystem { get; }
    public DateTime Timestamp { get; }
    public TaskCompletionSource<string> CompletionSource { get; }

    public AIGenerationCommand(
        string id,
        List<ConversationEntry> messages,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        Id = id;
        Messages = messages;
        Watcher = watcher;
        Priority = priority;
        SourceSystem = sourceSystem;
        Timestamp = DateTime.UtcNow;
        CompletionSource = new TaskCompletionSource<string>();
    }
}
