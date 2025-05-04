public class TimeManager
{
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
        int currentAP = playerState.ActionPoints;
        int actionsUsed = maxAP - currentAP;

        // Total hours in day = 24
        double hoursPerAP = 24.0 / maxAP;
        double totalHoursAdvanced = actionsUsed * hoursPerAP;

        int baseHour = 5; // Let's say day starts at 6 AM (Morning starts)
        int newHour = (baseHour + (int)Math.Floor(totalHoursAdvanced)) % 24;

        worldState.CurrentTimeHours = newHour;

        // Now update TimeWindow based on newHour
        TimeWindow newWindow;
        if (newHour >= 5 && newHour < 11)
            newWindow = TimeWindow.Morning;
        else if (newHour >= 11 && newHour < 17)
            newWindow = TimeWindow.Afternoon;
        else if (newHour >= 17 && newHour < 23)
            newWindow = TimeWindow.Evening;
        else
            newWindow = TimeWindow.Night;

        worldState.CurrentTimeWindow = newWindow;
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
