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
        int maxAP = playerState.TurnActionPoints;
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
        TimeWindow newWindow;
        if (newHour >= TimeDayStart && newHour < 12)
            newWindow = TimeWindow.Morning;
        else if (newHour >= 12 && newHour < 18)
            newWindow = TimeWindow.Afternoon;
        else if (newHour >= 18 && newHour < 24)
            newWindow = TimeWindow.Evening;
        else
            newWindow = TimeWindow.Night;  // should never hit this because of cap

        worldState.CurrentTimeWindow = newWindow;

        if (currentAP == 0)
        {
            worldState.CurrentTimeHours = 0;
            worldState.CurrentTimeWindow = TimeWindow.Night;
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

    public TimeWindow GetCurrentTimeWindow()
    {
        return worldState.CurrentTimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (worldState.CurrentTimeWindow)
        {
            case TimeWindow.Morning:
                return timeWindow == "Half" ? "Morning" : "Afternoon";
            case TimeWindow.Afternoon:
                return timeWindow == "Half" ? "Afternoon" : "Evening";
            case TimeWindow.Evening:
                return timeWindow == "Half" ? "Evening" : "Night";
            case TimeWindow.Night:
                return timeWindow == "Half" ? "Night" : "Morning";
        }

        return timeWindow;
    }
}
