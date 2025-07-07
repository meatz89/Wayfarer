public class TimeManager
{
    public const int TimeDayStart = 6;
    private Player player;
    private WorldState worldState;

    public int CurrentTimeHours { get; private set; }
    public TimeBlocks CurrentTimeWindow { get; private set; }

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

        CurrentTimeWindow = newWindow;

        if (currentAP == 0)
        {
            CurrentTimeHours = 0;
            CurrentTimeWindow = TimeBlocks.Night;
        }
    }

    public void StartNewDay()
    {
        worldState.CurrentDay++;
        SetNewTime(TimeDayStart);
    }

    public TimeBlocks GetCurrentTimeWindow()
    {
        return CurrentTimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (CurrentTimeWindow)
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
}
