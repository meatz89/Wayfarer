/// <summary>
/// Strongly typed context for contextual messages
/// </summary>
public class MessageContext
{
    public string Venue { get; set; }
    public string Time { get; set; }
    public bool Urgency { get; set; }
    public string NpcName { get; set; }
    public string ActionType { get; set; }
    public int? Value { get; set; }
}

/// <summary>
/// Manages the MessageSystem for the Narrative subsystem.
/// Wraps the existing MessageSystem to provide narrative-focused operations.
/// </summary>
public class MessageSystemManager
{
    private readonly MessageSystem _messageSystem;
    private readonly GameWorld _gameWorld;

    public MessageSystemManager(MessageSystem messageSystem, GameWorld gameWorld)
    {
        _messageSystem = messageSystem;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Add a standard system message
    /// </summary>
    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        _messageSystem.AddSystemMessage(message, type);
    }

    /// <summary>
    /// Add a narrative-styled message
    /// </summary>
    public void AddNarrativeMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        // Add narrative flourish based on type
        string styledMessage = ApplyNarrativeStyling(message, type);
        _messageSystem.AddSystemMessage(styledMessage, type);
    }

    /// <summary>
    /// Add multiple messages as a narrative sequence
    /// </summary>
    public void AddNarrativeSequence(string[] messages, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        foreach (string message in messages)
        {
            if (!string.IsNullOrEmpty(message))
            {
                AddNarrativeMessage(message, type);
            }
        }
    }

    /// <summary>
    /// Add a timed narrative message that appears after a delay
    /// </summary>
    public void AddDelayedNarrativeMessage(string message, int delayMs, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        // For now, add immediately - could be enhanced to support actual delays
        AddNarrativeMessage(message, type);
    }

    /// <summary>
    /// Clear all current messages
    /// </summary>
    public void ClearMessages()
    {
        _gameWorld.SystemMessages.Clear();
    }

    /// <summary>
    /// Get all current system messages
    /// </summary>
    public List<SystemMessage> GetCurrentMessages()
    {
        return new List<SystemMessage>(_gameWorld.SystemMessages);
    }

    /// <summary>
    /// Get the event log
    /// </summary>
    public List<SystemMessage> GetEventLog()
    {
        return new List<SystemMessage>(_gameWorld.EventLog);
    }

    /// <summary>
    /// Check if there are any active messages of a specific type
    /// </summary>
    public bool HasMessagesOfType(SystemMessageTypes type)
    {
        return _gameWorld.SystemMessages.Exists(m => m.Type == type);
    }

    /// <summary>
    /// Apply narrative styling to messages based on type
    /// </summary>
    private string ApplyNarrativeStyling(string message, SystemMessageTypes type)
    {
        // Add contextual prefixes or styling based on message type
        return type switch
        {
            SystemMessageTypes.Danger => $"[WARNING] {message}",
            SystemMessageTypes.Warning => $"[NOTICE] {message}",
            SystemMessageTypes.Success => $"[SUCCESS] {message}",
            SystemMessageTypes.Tutorial => $"[HINT] {message}",
            _ => message
        };
    }

    /// <summary>
    /// Add a contextual message based on game state
    /// </summary>
    public void AddContextualMessage(string baseMessage, MessageContext context)
    {
        string contextualMessage = baseMessage;

        // Add contextual elements
        if (!string.IsNullOrEmpty(context?.Venue))
        {
            contextualMessage = $"At {context.Venue}: {contextualMessage}";
        }

        if (!string.IsNullOrEmpty(context?.Time))
        {
            contextualMessage = $"[{context.Time}] {contextualMessage}";
        }

        if (context?.Urgency == true)
        {
            AddNarrativeMessage(contextualMessage, SystemMessageTypes.Warning);
        }
        else
        {
            AddNarrativeMessage(contextualMessage, SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// Generate and add a progress message
    /// </summary>
    public void AddProgressMessage(string action, int current, int total)
    {
        string message = $"{action}: {current}/{total}";

        SystemMessageTypes type = SystemMessageTypes.Info;
        if (current == total)
        {
            message = $"{action}: Complete!";
            type = SystemMessageTypes.Success;
        }
        else if (current > total * 0.75)
        {
            message = $"{action}: Nearly done ({current}/{total})";
        }

        AddNarrativeMessage(message, type);
    }

    /// <summary>
    /// Add a relationship change message
    /// </summary>
    public void AddRelationshipMessage(string npcName, string change, bool isPositive)
    {
        string message = $"Your relationship with {npcName} {change}";
        SystemMessageTypes type = isPositive ? SystemMessageTypes.Success : SystemMessageTypes.Warning;
        AddNarrativeMessage(message, type);
    }

    /// <summary>
    /// Add a discovery message for observations or secrets
    /// </summary>
    public void AddDiscoveryMessage(string discovery)
    {
        string message = $"{{icon:magnifying-glass}} You discovered: {discovery}";
        AddNarrativeMessage(message, SystemMessageTypes.Info);
    }

    /// <summary>
    /// Add a consequence message for player actions
    /// </summary>
    public void AddConsequenceMessage(string action, string consequence, bool isPositive)
    {
        string message = $"{action} resulted in: {consequence}";
        SystemMessageTypes type = isPositive ? SystemMessageTypes.Success : SystemMessageTypes.Warning;
        AddNarrativeMessage(message, type);
    }

    /// <summary>
    /// Add a time pressure message
    /// </summary>
    public void AddTimePressureMessage(string event_, int timeRemaining, string timeUnit)
    {
        SystemMessageTypes type = SystemMessageTypes.Info;
        string urgency = "";

        if (timeRemaining <= 1)
        {
            type = SystemMessageTypes.Danger;
            urgency = "URGENT: ";
        }
        else if (timeRemaining <= 3)
        {
            type = SystemMessageTypes.Warning;
            urgency = "Warning: ";
        }

        string message = $"{urgency}{event_} in {timeRemaining} {timeUnit}";
        AddNarrativeMessage(message, type);
    }

    /// <summary>
    /// Add an achievement or milestone message
    /// </summary>
    public void AddMilestoneMessage(string achievement)
    {
        string message = $"{{icon:round-star}} Achievement: {achievement}";
        AddNarrativeMessage(message, SystemMessageTypes.Success);
    }

    /// <summary>
    /// Add a hint or guidance message
    /// </summary>
    public void AddHintMessage(string hint)
    {
        string message = $"Hint: {hint}";
        AddNarrativeMessage(message, SystemMessageTypes.Tutorial);
    }
}
