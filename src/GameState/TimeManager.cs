/// <summary>
/// TimeManager manages the game's time system with exactly 5 time blocks per day.
/// 
/// ARCHITECTURAL PRINCIPLE: Single Time Source Authority
/// - CurrentTimeHours is the ONLY authoritative time value
/// - All other time representations (TimeBlocks enum) are calculated from hours
/// - Time blocks are internal action mechanics - players see actual time progression
/// 
/// FIVE TIME BLOCKS SYSTEM:
/// - Dawn: 6:00-8:59 (3 hours) - Early morning activities
/// - Morning: 9:00-11:59 (3 hours) - Prime morning activities  
/// - Afternoon: 12:00-15:59 (4 hours) - Midday activities
/// - Evening: 16:00-19:59 (4 hours) - Late day activities
/// - Night: 20:00-5:59 (10 hours) - Rest and recovery period
/// 
/// Each action typically consumes 1 time block = ~3.6 hours of game time.
/// Players see time progression like "Dawn 6:00" → "Morning 9:00" → "Afternoon 14:00"
/// </summary>
public class TimeManager
{
    public const int TimeDayStart = 6;
    public const int MaxDailyTimeBlocks = 5; // Must match TimeBlocks enum count

    private Player player;
    private WorldState worldState;

    public int CurrentTimeHours { get; private set; }

    // Track time blocks used during the day - resets each day
    private int _usedTimeBlocks = 0;

    public int UsedTimeBlocks
    {
        get
        {
            return _usedTimeBlocks;
        }
    }

    public int RemainingTimeBlocks
    {
        get
        {
            return Math.Max(0, MaxDailyTimeBlocks - UsedTimeBlocks);
        }
    }

    public bool CanPerformTimeBlockAction
    {
        get
        {
            return UsedTimeBlocks < MaxDailyTimeBlocks;
        }
    }

    public TimeManager(Player player, WorldState worldState)
    {
        this.player = player;
        this.worldState = worldState;

        // Initialize time to start of day if not set
        if (CurrentTimeHours == 0)
        {
            CurrentTimeHours = TimeDayStart;
        }
    }

    public void SetNewTime(int hours)
    {
        CurrentTimeHours = hours;
        // Update WorldState to stay synchronized
        worldState.CurrentTimeBlock = GetCurrentTimeBlock();
    }


    public void UpdateCurrentTimeBlock()
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

        SetNewTime(newHour);

        if (currentAP == 0)
        {
            SetNewTime(0); // Midnight when no action points left
        }
    }

    public void StartNewDay()
    {
        worldState.CurrentDay++;

        // Reset time blocks for the new day
        _usedTimeBlocks = 0;

        // Reset time to dawn - no action point regeneration per Period-Based Activity Planning user story
        SetNewTime(TimeDayStart);
    }


    /// <summary>
    /// Consumes the specified number of time blocks by advancing actual clock time.
    /// This is the core fix: time block consumption must advance the clock.
    /// </summary>
    /// <param name="blocks">Number of time blocks to consume</param>
    /// <throws>InvalidOperationException if exceeding daily limit</throws>
    public void ConsumeTimeBlock(int blocks)
    {
        if (_usedTimeBlocks + blocks > MaxDailyTimeBlocks)
        {
            throw new InvalidOperationException($"Cannot exceed daily time block limit of {MaxDailyTimeBlocks}. Attempting to consume {blocks} blocks but only {RemainingTimeBlocks} remaining.");
        }

        // Track the time block consumption
        _usedTimeBlocks += blocks;

        // Calculate hours to advance based on time blocks
        double hoursPerTimeBlock = 18.0 / MaxDailyTimeBlocks; // 18 hours (6 AM to midnight) / 5 blocks = 3.6 hours per block
        int hoursToAdvance = (int)Math.Ceiling(blocks * hoursPerTimeBlock);

        // Advance the actual clock time
        int newHour = CurrentTimeHours + hoursToAdvance;

        // Cap at end of day (midnight)
        if (newHour >= 24)
        {
            newHour = 23; // Stay at 11 PM to avoid day overflow
        }

        SetNewTime(newHour);
    }

    /// <summary>
    /// Validates whether the specified number of time blocks can be consumed
    /// without exceeding the daily limit.
    /// </summary>
    /// <param name="blocks">Number of time blocks to validate</param>
    /// <returns>True if the action can be performed, false otherwise</returns>
    public bool ValidateTimeBlockAction(int blocks)
    {
        return UsedTimeBlocks + blocks <= MaxDailyTimeBlocks;
    }

    /// <summary>
    /// Get current time hours for GameWorldManager compatibility
    /// </summary>
    /// <returns>Current time hours</returns>
    public int GetCurrentTimeHours()
    {
        return CurrentTimeHours;
    }

    /// <summary>
    /// Get current day from WorldState
    /// </summary>
    /// <returns>Current day</returns>
    public int GetCurrentDay()
    {
        return worldState.CurrentDay;
    }

    /// <summary>
    /// Get current time block calculated from current hour.
    /// This is the core architectural fix: TimeBlocks calculated, not stored.
    /// Maps 24 hours to 5 time blocks: Dawn, Morning, Afternoon, Evening, Night
    /// </summary>
    /// <returns>Current time block</returns>
    public TimeBlocks GetCurrentTimeBlock()
    {
        return CurrentTimeHours switch
        {
            >= 6 and < 9 => TimeBlocks.Dawn,      // 6:00-8:59 (3 hours)
            >= 9 and < 12 => TimeBlocks.Morning,   // 9:00-11:59 (3 hours)
            >= 12 and < 16 => TimeBlocks.Afternoon, // 12:00-15:59 (4 hours)
            >= 16 and < 20 => TimeBlocks.Evening,   // 16:00-19:59 (4 hours)
            _ => TimeBlocks.Night                   // 20:00-5:59 (10 hours) - covers >= 20 or < 6
        };
    }
}
