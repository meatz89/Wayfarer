

public class TimeManager
{
    private readonly WorldState worldState;

    public TimeManager(WorldState worldState)
    {
        this.worldState = worldState;
    }

    // Advance time by hours
    public void AdvanceTime(int hours)
    {
        int currentHour = worldState.CurrentTimeHours;
        int newHour = (currentHour + hours) % 24;

        // Track day change
        bool dayChanged = (newHour < currentHour);

        // Update hour
        worldState.CurrentTimeHours = newHour;

        // Update time window (morning, afternoon, evening, night)
        UpdateTimeWindow();

        // Handle day change
        if (dayChanged)
        {
            worldState.CurrentDay++;
            OnDayChanged();
        }
    }

    // Update the current time window based on hour
    private void UpdateTimeWindow()
    {
        int hour = worldState.CurrentTimeHours;

        if (hour >= 5 && hour < 12)
        {
            worldState.TimeWindow = TimeWindow.Morning;
        }
        else if (hour >= 12 && hour < 17)
        {
            worldState.TimeWindow = TimeWindow.Afternoon;
        }
        else if (hour >= 17 && hour < 21)
        {
            worldState.TimeWindow = TimeWindow.Evening;
        }
        else
        {
            worldState.TimeWindow = TimeWindow.Night;
        }
    }

    // Handle day change effects
    private void OnDayChanged()
    {
        // Future implementation: Handle day change effects
    }

    public TimeWindow GetCurrentTimeWindow()
    {
        return worldState.TimeWindow;
    }

    public string PreviewTimeAdvancement(string timeWindow)
    {
        switch (worldState.TimeWindow)
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
