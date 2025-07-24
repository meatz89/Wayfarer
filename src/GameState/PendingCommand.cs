/// <summary>
/// Represents a command that requires additional steps to complete
/// </summary>
public class PendingCommand
{
    public string CommandId { get; set; }
    public string CommandType { get; set; }
    public IGameCommand Command { get; set; }
    public string Description { get; set; }
    public DateTime StartedAt { get; set; }
    
    // Context data for resuming the action
    public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
}