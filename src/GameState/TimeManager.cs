/// <summary>
/// Manages all time-related operations in the game.
/// Single source of truth for time state and progression.
/// </summary>
public class TimeManager : ITimeManager
{
    private readonly TimeModel _timeModel;
    private readonly ILogger<TimeManager> _logger;
    private readonly MessageSystem _messageSystem;

    public TimeModel TimeModel => _timeModel;

    public int CurrentHour => _timeModel.CurrentHour;

    public int CurrentDay => _timeModel.CurrentDay;

    public TimeBlocks CurrentTimeBlock => _timeModel.CurrentTimeBlock;

    public int ActiveHoursRemaining => _timeModel.ActiveHoursRemaining;

    // ITimeManager compatibility properties
    public int HoursRemaining => ActiveHoursRemaining;

    public int CurrentTimeHours => CurrentHour;

    public TimeManager(
        TimeModel timeModel,
        MessageSystem messageSystem,
        ILogger<TimeManager> logger)
    {
        _timeModel = timeModel ?? throw new ArgumentNullException(nameof(timeModel));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Events removed per architecture guidelines - handle results directly
    }

    /// <summary>
    /// Checks if an action requiring specified hours can be performed.
    /// </summary>
    public bool CanPerformAction(int hoursRequired = 1)
    {
        return _timeModel.CanPerformAction(hoursRequired);
    }

    // ITimeManager implementation methods
    public int GetCurrentTimeHours()
    {
        return CurrentTimeHours;
    }
    
    public int GetCurrentMinutes()
    {
        return _timeModel.CurrentState.CurrentMinute;
    }

    public int GetCurrentDay()
    {
        return CurrentDay;
    }

    public TimeBlocks GetCurrentTimeBlock()
    {
        return CurrentTimeBlock;
    }

    // Compatibility method from old TimeManager
    public bool SpendHours(int hours)
    {
        if (hours <= 0) return false;
        if (!CanPerformAction(hours)) return false;

        Task<bool> task = SpendTime(hours, "Action");
        task.Wait(); // Synchronous for compatibility
        return task.Result;
    }

    public void AdvanceTime(int hours)
    {
        if (hours <= 0) return;

        TimeTransaction transaction = CreateTransaction()
            .WithHours(hours, "Time advancement")
            .RequireActiveHours(false);

        transaction.Execute();
    }

    public void AdvanceTimeMinutes(int minutes)
    {
        if (minutes <= 0) return;

        // Always show time passing message
        string timeDescription = GetTimePassingDescription(minutes);
        _messageSystem.AddSystemMessage(
            $"⏱️ {timeDescription}",
            SystemMessageTypes.Info);

        // Use the new minute-based advancement in TimeModel
        var result = _timeModel.AdvanceTimeMinutes(minutes);
        
        // Log the time advancement
        _logger.LogDebug($"Advanced time by {minutes} minutes. New time: Day {result.NewState.CurrentDay}, {result.NewState.CurrentHour:D2}:{result.NewState.CurrentMinute:D2}");
    }
    
    private string GetTimePassingDescription(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} minutes pass...";
        else if (minutes == 60)
            return "An hour passes...";
        else if (minutes < 120)
            return $"An hour and {minutes - 60} minutes pass...";
        else
            return $"{minutes / 60} hours pass...";
    }

    public void SetNewTime(int hours)
    {
        // This is a bit tricky - we need to calculate the difference
        int currentHour = CurrentHour;
        int difference = hours - currentHour;

        if (difference > 0)
        {
            AdvanceTime(difference);
        }
        else if (difference < 0)
        {
            // Can't go backwards in time, advance to next day at target hour
            int hoursToAdvance = (24 - currentHour) + hours;
            AdvanceTime(hoursToAdvance);
        }
    }

    /// <summary>
    /// Creates a new time transaction for complex time-based operations.
    /// </summary>
    public TimeTransaction CreateTransaction()
    {
        return new TimeTransaction(_timeModel);
    }

    /// <summary>
    /// Simple time advancement for basic actions.
    /// </summary>
    public async Task<bool> SpendTime(int hours, string description = null)
    {
        if (!CanPerformAction(hours))
        {
            _messageSystem.AddSystemMessage(
                $"Not enough time remaining. Need {hours} hours, have {ActiveHoursRemaining}.",
                SystemMessageTypes.Warning);
            return false;
        }

        TimeTransaction transaction = CreateTransaction()
            .WithHours(hours, description);

        TimeTransactionResult result = transaction.Execute();

        if (result.IsSuccess)
        {
            string timeDesc = hours == 1 ? "1 hour" : $"{hours} hours";
            _messageSystem.AddSystemMessage(
                $"⏱️ {description ?? "Time passed"}: {timeDesc}",
                SystemMessageTypes.Info);
        }

        return result.IsSuccess;
    }

    /// <summary>
    /// Gets a description of the current time state.
    /// </summary>
    public string GetTimeDescription()
    {
        return _timeModel.GetTimeDescription();
    }

    /// <summary>
    /// Gets the current time as a formatted string.
    /// </summary>
    public string GetTimeString()
    {
        return _timeModel.GetTimeString();
    }

    // Handle time advancement result directly
    private void HandleTimeAdvancement(TimeAdvancementResult result)
    {
        _logger.LogDebug("Time advanced by {Hours} hours to Day {Day}, Hour {Hour}",
            result.HoursAdvanced,
            result.NewState.CurrentDay,
            result.NewState.CurrentHour);

        // Log time block transitions
        if (result.CrossedTimeBlock)
        {
            _logger.LogInformation("Time block changed from {OldBlock} to {NewBlock}",
                result.OldTimeBlock,
                result.NewTimeBlock);
        }
    }
}