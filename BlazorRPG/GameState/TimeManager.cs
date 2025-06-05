public class TimeManager
{
    public const int TimeDayStart = 6;
    private Player player;
    private WorldState worldState;

    public int CurrentTimeHours { get; private set; }
    public TimeWindowTypes CurrentTimeWindow { get; private set; }

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
        TimeWindowTypes newWindow;
        if (newHour >= TimeDayStart && newHour < 12)
            newWindow = TimeWindowTypes.Morning;
        else if (newHour >= 12 && newHour < 18)
            newWindow = TimeWindowTypes.Afternoon;
        else if (newHour >= 18 && newHour < 24)
            newWindow = TimeWindowTypes.Evening;
        else
            newWindow = TimeWindowTypes.Night;  // should never hit this because of cap

        CurrentTimeWindow = newWindow;

        if (currentAP == 0)
        {
            CurrentTimeHours = 0;
            CurrentTimeWindow = TimeWindowTypes.Night;
        }
    }

    public void StartNewDay()
    {
        worldState.CurrentDay++;
        SetNewTime(TimeDayStart);
    }

    public TimeWindowTypes GetCurrentTimeWindow()
    {
        return CurrentTimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (CurrentTimeWindow)
        {
            case TimeWindowTypes.Morning:
                return timeWindow == "Half" ? "Morning" : "Afternoon";
            case TimeWindowTypes.Afternoon:
                return timeWindow == "Half" ? "Afternoon" : "Evening";
            case TimeWindowTypes.Evening:
                return timeWindow == "Half" ? "Evening" : "Night";
            case TimeWindowTypes.Night:
                return timeWindow == "Half" ? "Night" : "Morning";
        }

        return timeWindow;
    }
}
