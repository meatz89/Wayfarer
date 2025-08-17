namespace Wayfarer.GameState;

/// <summary>
/// Manages attention persistence across time blocks instead of per-conversation
/// This is the KEY CHANGE that fixes the infinite conversation exploit
/// </summary>
public class TimeBlockAttentionManager
{
    private readonly Dictionary<string, AttentionManager> _timeBlockAttention = new();
    private string _currentTimeBlock;
    private AttentionManager _currentAttention;

    public TimeBlockAttentionManager()
    {
        // Initialize with morning attention
        _currentTimeBlock = GetTimeBlockKey(TimeBlocks.Morning);
        _currentAttention = CreateFreshAttention();
        _timeBlockAttention[_currentTimeBlock] = _currentAttention;
    }

    /// <summary>
    /// Get the attention manager for the current time block
    /// Creates new one if time block has changed
    /// </summary>
    public AttentionManager GetCurrentAttention(TimeBlocks currentTime)
    {
        string timeBlockKey = GetTimeBlockKey(currentTime);

        // Check if we've moved to a new time block
        if (timeBlockKey != _currentTimeBlock)
        {
            Console.WriteLine($"[TimeBlockAttention] Transitioning from {_currentTimeBlock} to {timeBlockKey}");

            // Archive the old attention state
            _timeBlockAttention[_currentTimeBlock] = _currentAttention;

            // Create or retrieve attention for new time block
            if (!_timeBlockAttention.ContainsKey(timeBlockKey))
            {
                _timeBlockAttention[timeBlockKey] = CreateFreshAttention();
                Console.WriteLine($"[TimeBlockAttention] Created fresh attention for {timeBlockKey}");
            }

            _currentAttention = _timeBlockAttention[timeBlockKey];
            _currentTimeBlock = timeBlockKey;
        }

        return _currentAttention;
    }

    /// <summary>
    /// Force a refresh (e.g., after sleeping or special events)
    /// </summary>
    public void ForceRefresh()
    {
        Console.WriteLine("[TimeBlockAttention] Forcing attention refresh");
        _currentAttention = CreateFreshAttention();
        _timeBlockAttention[_currentTimeBlock] = _currentAttention;
    }

    /// <summary>
    /// Check if player has any attention left in current time block
    /// </summary>
    public bool HasAttentionRemaining()
    {
        return _currentAttention?.GetAvailableAttention() > 0;
    }

    /// <summary>
    /// Get a summary of attention state for UI display
    /// </summary>
    public (int current, int max) GetAttentionState()
    {
        if (_currentAttention == null)
            return (0, 5);

        return (_currentAttention.GetAvailableAttention(), _currentAttention.GetMaxAttention());
    }

    private AttentionManager CreateFreshAttention()
    {
        AttentionManager attention = new AttentionManager();
        // Set to 5 base attention (can be modified by location/state)
        attention.SetMaxAttention(5);
        attention.ResetForNewScene(); // This now means "reset for new time block"
        return attention;
    }

    private string GetTimeBlockKey(TimeBlocks timeBlock)
    {
        // Each time block has its own attention pool - no grouping!
        return timeBlock switch
        {
            TimeBlocks.Dawn => "dawn",
            TimeBlocks.Morning => "morning",
            TimeBlocks.Afternoon => "afternoon",
            TimeBlocks.Evening => "evening",
            TimeBlocks.Night => "night",
            TimeBlocks.LateNight => "latenight",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Clear all stored attention states (for new day)
    /// </summary>
    public void StartNewDay()
    {
        Console.WriteLine("[TimeBlockAttention] Starting new day - clearing all attention states");
        _timeBlockAttention.Clear();
        _currentAttention = CreateFreshAttention();
        _timeBlockAttention[_currentTimeBlock] = _currentAttention;
    }

    /// <summary>
    /// Modify current attention based on location or events
    /// </summary>
    public void ModifyCurrentAttention(int modifier, string reason)
    {
        if (_currentAttention != null)
        {
            int newMax = Math.Clamp(_currentAttention.GetMaxAttention() + modifier, 3, 7);
            _currentAttention.SetMaxAttention(newMax);
            Console.WriteLine($"[TimeBlockAttention] Modified attention by {modifier} due to {reason}. New max: {newMax}");
        }
    }
}