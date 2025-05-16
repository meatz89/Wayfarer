public class AIGenerationCommand
{
    public string Id { get; }
    public List<ConversationEntry> Messages { get; }
    public string Model { get; }
    public string FallbackModel { get; }
    public IResponseStreamWatcher Watcher { get; }
    public int Priority { get; }
    public string SourceSystem { get; }
    public DateTime Timestamp { get; }
    public TaskCompletionSource<string> CompletionSource { get; }

    public AIGenerationCommand(
        string id,
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        Id = id;
        Messages = messages;
        Model = model;
        FallbackModel = fallbackModel;
        Watcher = watcher;
        Priority = priority;
        SourceSystem = sourceSystem;
        Timestamp = DateTime.UtcNow;
        CompletionSource = new TaskCompletionSource<string>();
    }
}
