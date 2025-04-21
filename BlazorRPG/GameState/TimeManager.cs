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

        // Update environmental properties
        UpdateEnvironmentalProperties();

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
            worldState.WorldTime = TimeWindows.Morning;
        else if (hour >= 12 && hour < 17)
            worldState.WorldTime = TimeWindows.Afternoon;
        else if (hour >= 17 && hour < 21)
            worldState.WorldTime = TimeWindows.Evening;
        else
            worldState.WorldTime = TimeWindows.Night;
    }

    // Update TimeManager.UpdateEnvironmentalProperties() method
    public void UpdateEnvironmentalProperties()
    {
        Location currentLocation = worldState.CurrentLocation;
        if (currentLocation == null) return;

        List<LocationSpot> locationSpots = currentLocation.LocationSpots;

        foreach (LocationSpot locationSpot in locationSpots)
        {
            switch (worldState.WorldTime)
            {
                case TimeWindows.Morning:
                case TimeWindows.Afternoon:
                    ModifyLocationIllumination(locationSpot, Illumination.Bright);
                    break;

                case TimeWindows.Evening:
                    ModifyLocationIllumination(locationSpot, Illumination.Shadowy);
                    break;

                case TimeWindows.Night:
                    ModifyLocationIllumination(locationSpot, Illumination.Dark);
                    break;
            }
        }
    }

    // Change SetLocationIllumination to ModifyLocationIllumination to clarify its limited role
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