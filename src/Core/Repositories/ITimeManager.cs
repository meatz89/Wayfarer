/// <summary>
/// Interface for time management operations
/// </summary>
public interface ITimeManager
{
    /// <summary>
    /// Get the current time in hours
    /// </summary>
    int GetCurrentTimeHours();

    /// <summary>
    /// Get the current day
    /// </summary>
    int GetCurrentDay();

    /// <summary>
    /// Get the current time block
    /// </summary>
    TimeBlocks GetCurrentTimeBlock();

    /// <summary>
    /// Check if an action requiring specified hours can be performed
    /// </summary>
    bool CanPerformAction(int hoursRequired = 1);
    void AdvanceTime(int hours);
    bool SpendHours(int hours);

    /// <summary>
    /// Get remaining hours in the current day
    /// </summary>
    int HoursRemaining { get; }
}