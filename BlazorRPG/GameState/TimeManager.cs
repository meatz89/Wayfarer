public class TimeManager
{
    public const int TimeDayStart = 6;
    private readonly Player playerState;
    private readonly WorldState worldState;

    public TimeManager(Player playerState, WorldState worldState)
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
        TimeWindowTypes newWindow;
        if (newHour >= TimeDayStart && newHour < 12)
            newWindow = TimeWindowTypes.Morning;
        else if (newHour >= 12 && newHour < 18)
            newWindow = TimeWindowTypes.Afternoon;
        else if (newHour >= 18 && newHour < 24)
            newWindow = TimeWindowTypes.Evening;
        else
            newWindow = TimeWindowTypes.Night;  // should never hit this because of cap

        worldState.CurrentTimeWindow = newWindow;

        if (currentAP == 0)
        {
            worldState.CurrentTimeHours = 0;
            worldState.CurrentTimeWindow = TimeWindowTypes.Night;
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

    public TimeWindowTypes GetCurrentTimeWindow()
    {
        return worldState.CurrentTimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (worldState.CurrentTimeWindow)
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
