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

    public bool CanPerformTimeBlockAction
    {
        get
        {
            // Time blocks are only for scheduling, not action limits
            return true;
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
        // Time blocks are managed directly, not based on action points

        // Time progression is now handled by time blocks, not action points
        // This method is kept for compatibility but doesn't need to do anything specific
    }

    public void StartNewDay()
    {
        worldState.CurrentDay++;

        // Reset time blocks for the new day

        // Reset time to dawn - no action point regeneration per Period-Based Activity Planning user story
        SetNewTime(TimeDayStart);
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
    
    /// <summary>
    /// Advance time by the specified number of hours
    /// </summary>
    public void AdvanceTime(int hours)
    {
        if (hours <= 0) return;
        
        CurrentTimeHours += hours;
        
        // Handle day rollover
        while (CurrentTimeHours >= 24)
        {
            CurrentTimeHours -= 24;
            StartNewDay();
        }
        
        // Update WorldState to stay synchronized
        worldState.CurrentTimeBlock = GetCurrentTimeBlock();
    }
}
