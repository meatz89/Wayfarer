public class WorldState
{
    // Current location tracking
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }

    // Navigation options
    public List<UserLocationTravelOption> CurrentTravelOptions { get; set; } = new();
    public List<UserLocationSpotOption> CurrentLocationSpotOptions { get; set; } = new();

    // Time tracking - moved here since it affects world state
    public int CurrentTimeInHours { get; set; }
    public TimeWindows WorldTime { get; private set; }
    public WeatherTypes WorldWeather { get; private set; } 

    public void SetCurrentTime(int hours)
    {
        CurrentTimeInHours = (CurrentTimeInHours + hours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeSlot = (CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        DetermineCurrentTimeSlot(timeSlot);
    }

    public void SetNewLocation(Location location)
    {
        CurrentLocation = location;
        CurrentLocationSpot = location.LocationSpots.FirstOrDefault();
    }

    public void SetNewLocationSpot(LocationSpot locationSpot)
    {
        CurrentLocationSpot = locationSpot;
    }

    public void SetCurrentTravelOptions(List<UserLocationTravelOption> options)
    {
        CurrentTravelOptions = options;
    }

    public void SetCurrentLocationSpotOptions(List<UserLocationSpotOption> options)
    {
        CurrentLocationSpotOptions = options;
    }

    public void DetermineCurrentTimeSlot(int timeSlot)
    {
        WorldTime = timeSlot switch
        {
            0 => TimeWindows.Night,
            1 => TimeWindows.Morning,
            2 => TimeWindows.Afternoon,
            _ => TimeWindows.Evening
        };
    }

    public void ChangeWeather(WeatherTypes weatherType)
    {
        WorldWeather = weatherType;
    }
}