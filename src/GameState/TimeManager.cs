/// <summary>
/// TimeManager manages the game's hour-based time system.
/// 
/// ARCHITECTURAL PRINCIPLE: Hours as Primary Resource
/// - Players have 16 active hours per day (6 AM - 10 PM)
/// - Every meaningful action costs 1+ hours
/// - Time periods (Dawn/Morning/etc) are for NPC availability only
/// - No complex time mechanics - just hour consumption
/// 
/// TIME PERIODS (for NPC scheduling):
/// - Dawn: 6:00-7:59 (2 hours) - Early risers available
/// - Morning: 8:00-11:59 (4 hours) - Most NPCs active  
/// - Afternoon: 12:00-15:59 (4 hours) - Business hours
/// - Evening: 16:00-19:59 (4 hours) - Social hours
/// - Night: 20:00-21:59 (2 hours) - Limited availability
/// - Late Night: 22:00-5:59 (8 hours) - Sleep/rest only
/// </summary>
public class TimeManager
{
    public const int TimeDayStart = 6;
    public const int TimeNightStart = 22;
    public const int HoursPerDay = 16; // 6 AM to 10 PM

    private Player player;
    private WorldState worldState;

    public int CurrentTimeHours { get; private set; }

    public int HoursRemaining => Math.Max(0, TimeNightStart - CurrentTimeHours);
    
    public bool CanPerformAction(int hoursRequired = 1)
    {
        return CurrentTimeHours + hoursRequired <= TimeNightStart;
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
    public bool SpendHours(int hours)
    {
        if (hours <= 0) return false;
        if (!CanPerformAction(hours)) return false;
        
        CurrentTimeHours += hours;
        
        // Update WorldState to stay synchronized
        worldState.CurrentTimeBlock = GetCurrentTimeBlock();
        return true;
    }
    
    /// <summary>
    /// Force time advancement (for sleep, etc)
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
