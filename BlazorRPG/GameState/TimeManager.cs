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
        int currentHour = worldState.CurrentTimeInHours;
        int newHour = (currentHour + hours) % 24;

        // Track day change
        bool dayChanged = (newHour < currentHour);

        // Update hour
        worldState.CurrentTimeInHours = newHour;

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
        int hour = worldState.CurrentTimeInHours;

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

    private void ModifyLocationIllumination(LocationSpot locationSpot, Illumination illumination)
    {
        locationSpot.Illumination = illumination;
    }

    // Handle day change effects
    private void OnDayChanged()
    {
        // Future implementation: Handle day change effects
    }
}
