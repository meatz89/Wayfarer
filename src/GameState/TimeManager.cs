public class TimeManager
{
    public const int TimeDayStart = 6;
    public const int MaxDailyTimeBlocks = 5;
    
    private Player player;
    private WorldState worldState;
    private int usedTimeBlocks = 0;

    public int CurrentTimeHours { get; private set; }
    public TimeBlocks CurrentTimeBlock { get; private set; }
    
    // Time block constraint properties
    public int UsedTimeBlocks => usedTimeBlocks;
    public int RemainingTimeBlocks => MaxDailyTimeBlocks - usedTimeBlocks;
    public bool CanPerformTimeBlockAction => usedTimeBlocks < MaxDailyTimeBlocks;

    public TimeManager(Player player, WorldState worldState)
    {
        this.player = player;
        this.worldState = worldState;
    }

    public void SetNewTime(int hours)
    {
        CurrentTimeHours = hours;
        UpdateTimeWindow();
    }

    public void AdvanceTime(int duration)
    {
        ConsumeTimeBlock(duration);
        SetNewTime(CurrentTimeHours + duration);
    }

    public int GetCurrentHour()
    {
        return CurrentTimeHours;
    }

    public void UpdateTimeWindow()
    {
        int maxAP = player.MaxActionPoints;
        int currentAP = player.CurrentActionPoints();
        int actionsUsed = maxAP - currentAP;

        int activeDayStartHour = TimeDayStart;  // 6 AM
        int activeDayEndHour = 24;   // Midnight
        int activeDayHours = activeDayEndHour - activeDayStartHour;  // 18 hours

        double hoursPerAP = activeDayHours / (double)maxAP;
        double totalHoursAdvanced = actionsUsed * hoursPerAP;

        int newHour = activeDayStartHour + (int)Math.Floor(totalHoursAdvanced);
        if (newHour >= 24)
            newHour = 23;  // Cap to prevent overflow, last action sits near midnight

        CurrentTimeHours = newHour;

        // Now update TimeWindow based on newHour
        TimeBlocks newWindow;
        if (newHour >= TimeDayStart && newHour < 12)
            newWindow = TimeBlocks.Morning;
        else if (newHour >= 12 && newHour < 18)
            newWindow = TimeBlocks.Afternoon;
        else if (newHour >= 18 && newHour < 24)
            newWindow = TimeBlocks.Evening;
        else
            newWindow = TimeBlocks.Night;  // should never hit this because of cap

        CurrentTimeBlock = newWindow;

        if (currentAP == 0)
        {
            CurrentTimeHours = 0;
            CurrentTimeBlock = TimeBlocks.Night;
        }
    }

    public void StartNewDay()
    {
        usedTimeBlocks = 0; // Reset time blocks for new day
        worldState.CurrentDay++;
        SetNewTime(TimeDayStart);
    }

    public TimeBlocks GetCurrentTimeWindow()
    {
        return CurrentTimeBlock;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (CurrentTimeBlock)
        {
            case TimeBlocks.Morning:
                return timeWindow == "Half" ? "Morning" : "Afternoon";
            case TimeBlocks.Afternoon:
                return timeWindow == "Half" ? "Afternoon" : "Evening";
            case TimeBlocks.Evening:
                return timeWindow == "Half" ? "Evening" : "Night";
            case TimeBlocks.Night:
                return timeWindow == "Half" ? "Night" : "Morning";
        }

        return timeWindow;
    }
    
    /// <summary>
    /// Consumes the specified number of time blocks for actions.
    /// Enforces the daily limit of 5 time blocks.
    /// </summary>
    /// <param name="blocks">Number of time blocks to consume</param>
    /// <throws>InvalidOperationException if exceeding daily limit</throws>
    public void ConsumeTimeBlock(int blocks)
    {
        if (usedTimeBlocks + blocks > MaxDailyTimeBlocks)
        {
            throw new InvalidOperationException($"Cannot exceed daily time block limit of {MaxDailyTimeBlocks}. Attempting to consume {blocks} blocks but only {RemainingTimeBlocks} remaining.");
        }
        
        usedTimeBlocks += blocks;
    }
    
    /// <summary>
    /// Validates whether the specified number of time blocks can be consumed
    /// without exceeding the daily limit.
    /// </summary>
    /// <param name="blocks">Number of time blocks to validate</param>
    /// <returns>True if the action can be performed, false otherwise</returns>
    public bool ValidateTimeBlockAction(int blocks)
    {
        return usedTimeBlocks + blocks <= MaxDailyTimeBlocks;
    }
}
