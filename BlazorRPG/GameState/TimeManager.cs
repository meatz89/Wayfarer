public class TimeManager
{
    public const int TimeDayStart = 6;
    private readonly PlayerState playerState;
    private readonly WorldState worldState;

    public TimeManager(PlayerState playerState, WorldState worldState)
    {
        this.playerState = playerState;
        this.worldState = worldState;
    }

    public void UpdateTimeWindow()
    {
        int maxAP = playerState.MaxActionPoints;
        int currentAP = playerState.CurrentActionPoints();
        int actionsUsed = maxAP - currentAP;

        int activeDayStartHour = TimeDayStart;  // 6 AM
        int activeDayEndHour = 24;   // Midnight
        int activeDayHours = activeDayEndHour - activeDayStartHour;  // 18 hours

        double hoursPerAP = activeDayHours / (double)maxAP;
        double totalHoursAdvanced = actionsUsed * hoursPerAP;

        int newHour = activeDayStartHour + (int)Math.Floor(totalHoursAdvanced);
        if (newHour >= 24)
            newHour = 23;  // Cap to prevent overflow, last action sits near midnight

        worldState.CurrentTimeHours = newHour;

        // Now update TimeWindow based on newHour
        TimeWindows newWindow;
        if (newHour >= TimeDayStart && newHour < 12)
            newWindow = TimeWindows.Morning;
        else if (newHour >= 12 && newHour < 18)
            newWindow = TimeWindows.Afternoon;
        else if (newHour >= 18 && newHour < 24)
            newWindow = TimeWindows.Evening;
        else
            newWindow = TimeWindows.Night;  // should never hit this because of cap

        worldState.CurrentTimeWindow = newWindow;

        if (currentAP == 0)
        {
            worldState.CurrentTimeHours = 0;
            worldState.CurrentTimeWindow = TimeWindows.Night;
        }
    }

    public void StartNewDay()
    {
        worldState.CurrentDay++;
        SetNewTime(TimeDayStart);
    }

    public void SetNewTime(int hours)
    {
        worldState.CurrentTimeHours = hours;
        UpdateTimeWindow();
    }

    public TimeWindows GetCurrentTimeWindow()
    {
        return worldState.CurrentTimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (worldState.CurrentTimeWindow)
        {
            case TimeWindows.Morning:
                return timeWindow == "Half" ? "Morning" : "Afternoon";
            case TimeWindows.Afternoon:
                return timeWindow == "Half" ? "Afternoon" : "Evening";
            case TimeWindows.Evening:
                return timeWindow == "Half" ? "Evening" : "Night";
            case TimeWindows.Night:
                return timeWindow == "Half" ? "Night" : "Morning";
        }

        return timeWindow;
    }
}
