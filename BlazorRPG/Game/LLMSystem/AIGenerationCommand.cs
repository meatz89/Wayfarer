public class AIGenerationCommand
{
    public List<ConversationEntry> Messages { get; }
    public string Model { get; }
    public string FallbackModel { get; }
    public IResponseStreamWatcher Watcher { get; }
    public int Priority { get; }
    public DateTime Timestamp { get; }
    public string SourceSystem { get; }
    public TaskCompletionSource<string> CompletionSource { get; }

    public AIGenerationCommand(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher,
        int priority,
        string sourceSystem)
    {
        Messages = messages;
        Model = model;
        FallbackModel = fallbackModel;
        Watcher = watcher;
        Priority = priority;
        Timestamp = DateTime.UtcNow;
        CompletionSource = new TaskCompletionSource<string>();
        SourceSystem = sourceSystem;
    }
}