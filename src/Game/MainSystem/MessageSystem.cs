public class MessageSystem
{
    private readonly GameWorld _gameWorld;

    public MessageSystem(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        // Different durations based on message importance
        int duration = type switch
        {
            SystemMessageTypes.Danger => 8000,   // 8 seconds for critical messages
            SystemMessageTypes.Warning => 6000,  // 6 seconds for warnings
            SystemMessageTypes.Success => 5000,  // 5 seconds for success
            SystemMessageTypes.Tutorial => 6000, // 6 seconds for tutorial
            SystemMessageTypes.Info => 4000,     // 4 seconds for info
            _ => 5000
        };

        SystemMessage systemMessage = new SystemMessage(message, type, duration);
        _gameWorld.SystemMessages.Add(systemMessage);
        // Also add to permanent event log
        _gameWorld.EventLog.Add(systemMessage);
    }

    /// <summary>
    /// Add categorical letter positioning message for UI translation
    /// Backend only provides data categories, frontend translates to text
    /// </summary>
    public void AddLetterPositioningMessage(string senderName, LetterPositioningReason reason, int position, int strength, int debt)
    {
        LetterPositioningMessage positioningMessage = new LetterPositioningMessage
        {
            SenderName = senderName,
            Reason = reason,
            Position = position,
            RelationshipStrength = strength,
            RelationshipDebt = debt
        };

        // Store categorical data for UI translation
        _gameWorld.LetterPositioningMessages.Add(positioningMessage);
    }

    /// <summary>
    /// Add categorical special letter event for UI translation
    /// Backend only provides event categories, frontend translates to narrative text
    /// </summary>
    public void AddSpecialLetterEvent(SpecialLetterEvent letterEvent)
    {
        // Store categorical data for UI translation
        _gameWorld.SpecialLetterEvents.Add(letterEvent);

        // Also create a simple system message for immediate display
        SystemMessageTypes severity = letterEvent.Severity switch
        {
            NarrativeSeverity.Success => SystemMessageTypes.Success,
            NarrativeSeverity.Warning => SystemMessageTypes.Warning,
            NarrativeSeverity.Danger => SystemMessageTypes.Danger,
            NarrativeSeverity.Celebration => SystemMessageTypes.Success,
            _ => SystemMessageTypes.Info
        };

        // Add basic categorical reference for UI to translate
        AddSystemMessage($"SPECIAL_LETTER_EVENT:{letterEvent.EventType}:{letterEvent.TargetNPCId}", severity);
    }

    // Special letter requests now handled through new conversation card system
}