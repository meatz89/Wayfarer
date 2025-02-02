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
        int timeWindow = (CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        DetermineCurrentTimeWindow(timeWindow);
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

    public void DetermineCurrentTimeWindow(int timeWindow)
    {
        WorldTime = timeWindow switch
        {
            0 => TimeWindows.Midnight,
            1 => TimeWindows.Dawn,
            2 => TimeWindows.Noon,
            _ => TimeWindows.Dusk
        };
    }

    public void ChangeWeather(WeatherTypes weatherType)
    {
        WorldWeather = weatherType;
    }

    public bool HasProperty<T>(T worldStatusProperty) where T : struct, Enum
    {
        if (worldStatusProperty is TimeWindows timeWindow)
        {
            return WorldTime == timeWindow;
        }
        else if (worldStatusProperty is WeatherTypes weather)
        {
            return WorldWeather == weather;
        }
        else
        {
            // You can handle other types or throw an exception if needed
            throw new ArgumentException($"Unsupported property type: {typeof(T)}");
        }
    }
}

