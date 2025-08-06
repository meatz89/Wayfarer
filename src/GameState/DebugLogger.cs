using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Centralized debug logging service for tracking game state and flow
/// </summary>
public class DebugLogger
{
    private readonly List<DebugLogEntry> _logs = new List<DebugLogEntry>();
    private readonly int _maxLogs = 1000;
    private bool _enabled = true;
    private readonly ITimeManager _timeManager;
    private readonly ConversationStateManager _conversationStateManager;

    public DebugLogger(ITimeManager timeManager, ConversationStateManager conversationStateManager)
    {
        _timeManager = timeManager;
        _conversationStateManager = conversationStateManager;
    }

    public bool IsEnabled
    {
        get
        {
            return _enabled;
        }

        set
        {
            _enabled = value;
        }
    }

    public void LogStateTransition(string fromState, string toState, string context = null)
    {
        if (!_enabled) return;

        string message = $"STATE: {fromState} → {toState}";
        if (!string.IsNullOrEmpty(context))
            message += $" | Context: {context}";

        AddLog(DebugLogCategory.StateTransition, message);
    }

    public void LogNavigation(string fromScreen, string toScreen, string reason = null)
    {
        if (!_enabled) return;

        string message = $"NAV: {fromScreen} → {toScreen}";
        if (!string.IsNullOrEmpty(reason))
            message += $" | Reason: {reason}";

        AddLog(DebugLogCategory.Navigation, message);
    }

    public void LogNPCActivity(string activity, string npcId = null, string details = null)
    {
        if (!_enabled) return;

        string message = $"NPC: {activity}";
        if (!string.IsNullOrEmpty(npcId))
            message += $" | ID: {npcId}";
        if (!string.IsNullOrEmpty(details))
            message += $" | {details}";

        AddLog(DebugLogCategory.NPC, message);
    }

    public void LogConversation(string stage, string details = null)
    {
        if (!_enabled) return;

        string message = $"CONV: {stage}";
        if (!string.IsNullOrEmpty(details))
            message += $" | {details}";

        AddLog(DebugLogCategory.Conversation, message);
    }

    public void LogTimeChange(string fromTime, string toTime, int hoursAdvanced)
    {
        if (!_enabled) return;

        string message = $"TIME: {fromTime} → {toTime} | Advanced {hoursAdvanced} hours";
        AddLog(DebugLogCategory.Time, message);
    }

    public void LogAction(string action, string target = null, string result = null)
    {
        if (!_enabled) return;

        string message = $"ACTION: {action}";
        if (!string.IsNullOrEmpty(target))
            message += $" | Target: {target}";
        if (!string.IsNullOrEmpty(result))
            message += $" | Result: {result}";

        AddLog(DebugLogCategory.Action, message);
    }

    public void LogPolling(string component, string state)
    {
        if (!_enabled) return;

        string message = $"POLL: {component} | {state}";
        AddLog(DebugLogCategory.Polling, message, verbose: true);
    }

    public void LogError(string component, string error, Exception ex = null)
    {
        if (!_enabled) return;

        string message = $"ERROR: {component} | {error}";
        if (ex != null)
            message += $" | Exception: {ex.GetType().Name}: {ex.Message}";

        AddLog(DebugLogCategory.Error, message);
    }

    public void LogWarning(string component, string warning)
    {
        if (!_enabled) return;

        string message = $"WARN: {component} | {warning}";
        AddLog(DebugLogCategory.Warning, message);
    }

    public void LogDebug(string message)
    {
        if (!_enabled) return;

        AddLog(DebugLogCategory.Debug, message);
    }

    /// <summary>
    /// Get recent logs, optionally filtered by category
    /// </summary>
    public List<DebugLogEntry> GetRecentLogs(int count = 50, DebugLogCategory? category = null)
    {
        IEnumerable<DebugLogEntry> query = _logs.AsEnumerable();

        if (category.HasValue)
            query = query.Where(l => l.Category == category.Value);

        return query.OrderByDescending(l => l.Timestamp)
                   .Take(count)
                   .ToList();
    }

    /// <summary>
    /// Get logs since a specific time
    /// </summary>
    public List<DebugLogEntry> GetLogsSince(DateTime since, DebugLogCategory? category = null)
    {
        IEnumerable<DebugLogEntry> query = _logs.Where(l => l.Timestamp >= since);

        if (category.HasValue)
            query = query.Where(l => l.Category == category.Value);

        return query.OrderByDescending(l => l.Timestamp).ToList();
    }

    /// <summary>
    /// Clear all logs
    /// </summary>
    public void ClearLogs()
    {
        _logs.Clear();
    }

    /// <summary>
    /// Generate a state report for debugging
    /// </summary>
    public string GenerateStateReport(GameWorld gameWorld)
    {
        List<string> report = new List<string>();
        report.Add("=== GAME STATE REPORT ===");
        report.Add($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.Add("");

        // Player state
        Player player = gameWorld.GetPlayer();
        report.Add("PLAYER STATE:");
        report.Add($"  Location: {player.CurrentLocationSpot?.LocationId ?? "NULL"}");
        report.Add($"  Spot: {player.CurrentLocationSpot?.SpotID ?? "NULL"}");
        report.Add($"  Stamina: {player.Stamina}");
        report.Add($"  Coins: {player.Coins}");
        report.Add("");

        // Time state
        report.Add("TIME STATE:");
        report.Add($"  Current Day: {gameWorld.CurrentDay}");
        report.Add($"  Time Block: {_timeManager.GetCurrentTimeBlock()}");
        report.Add($"  Hours: {_timeManager.GetCurrentTimeHours()}");
        report.Add($"  Hours Remaining: {_timeManager.HoursRemaining}");
        report.Add("");

        // Conversation state
        report.Add("CONVERSATION STATE:");
        report.Add($"  Pending: {_conversationStateManager.ConversationPending}");
        report.Add($"  Manager exists: {_conversationStateManager.PendingConversationManager != null}");
        if (_conversationStateManager.PendingConversationManager != null)
        {
            report.Add($"  Is Awaiting Response: {_conversationStateManager.PendingConversationManager.IsAwaitingResponse}");
            report.Add($"  Choices Count: {_conversationStateManager.PendingConversationManager.Choices?.Count ?? 0}");
        }
        report.Add("");

        // NPCs at location
        report.Add("NPCS AT CURRENT LOCATION:");
        if (player.CurrentLocationSpot != null)
        {
            List<NPC> allNpcs = gameWorld.WorldState.NPCs;
            List<NPC> locationNpcs = allNpcs.Where(n => n.Location == player.CurrentLocationSpot.LocationId).ToList();
            List<NPC> spotNpcs = locationNpcs.Where(n => n.SpotId == player.CurrentLocationSpot.SpotID).ToList();
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            List<NPC> availableNpcs = spotNpcs.Where(n => n.IsAvailable(currentTime)).ToList();

            report.Add($"  Total NPCs in game: {allNpcs.Count}");
            report.Add($"  NPCs at location '{player.CurrentLocationSpot.LocationId}': {locationNpcs.Count}");
            report.Add($"  NPCs at spot '{player.CurrentLocationSpot.SpotID}': {spotNpcs.Count}");
            report.Add($"  Available at time '{currentTime}': {availableNpcs.Count}");

            foreach (NPC? npc in availableNpcs)
            {
                report.Add($"    - {npc.ID}: {npc.Name} ({npc.Profession})");
            }
        }
        report.Add("");

        // Recent logs
        report.Add("RECENT ACTIVITY:");
        List<DebugLogEntry> recentLogs = GetRecentLogs(20);
        foreach (DebugLogEntry? log in recentLogs.OrderBy(l => l.Timestamp))
        {
            report.Add($"  [{log.Timestamp:HH:mm:ss}] {log.Message}");
        }

        return string.Join("\n", report);
    }

    private void AddLog(DebugLogCategory category, string message, bool verbose = false)
    {
        // Skip verbose logs in production
        if (verbose && !IsDebugMode())
            return;

        DebugLogEntry entry = new DebugLogEntry
        {
            Timestamp = DateTime.Now,
            Category = category,
            Message = message
        };

        _logs.Add(entry);

        // Trim old logs
        while (_logs.Count > _maxLogs)
        {
            _logs.RemoveAt(0);
        }

        // Also write to console for immediate visibility
        Console.WriteLine($"[{entry.Timestamp:HH:mm:ss}] {entry.Message}");
    }

    private bool IsDebugMode()
    {
        // Check if debugging is enabled via the IsEnabled property
        // This can be controlled externally by the application
        return _enabled;
    }
}

public class DebugLogEntry
{
    public DateTime Timestamp { get; set; }
    public DebugLogCategory Category { get; set; }
    public string Message { get; set; }
}

public enum DebugLogCategory
{
    StateTransition,
    Navigation,
    NPC,
    Conversation,
    Time,
    Action,
    Polling,
    Error,
    Warning,
    Debug
}