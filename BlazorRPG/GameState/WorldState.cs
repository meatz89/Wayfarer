
public class WorldState
{
    // Core data collections
    public Dictionary<string, Location> Locations { get; private set; } = new Dictionary<string, Location>();
    private Dictionary<string, Character> characters { get; set; } = new Dictionary<string, Character>();
    private Dictionary<string, Opportunity> opportunities { get; set; } = new Dictionary<string, Opportunity>();

    internal List<Location> GetLocations()
    {
        return Locations.Values.ToList();
    }

    internal List<Opportunity> GetOpportunities()
    {
        return opportunities.Values.ToList();
    }

    public List<Character> GetCharacters()
    {
        return characters.Values.ToList();
    }

    public Location GetLocation(string name)
    {
        return Locations[name];
    }

    public Character GetCharacter(string name)
    {
        return characters[name];
    }

    public Opportunity GetOpportunity(string name)
    {
        return opportunities[name];
    }

    public void AddLocation(string name, Location location)
    {
        Locations.Add(name, location);
    }

    public void AddCharacter(string name, Character character)
    {
        characters.Add(name, character);
    }

    public void AddOpportunity(string name, Opportunity opportunity)
    {
        opportunities.Add(name, opportunity);
    }

    // Game time
    public int CurrentTimeInHours { get; set; }
    public int CurrentTimeMinutes { get; set; }  // Minutes since game start
    public TimeWindows WorldTime { get; set; }

    // World history
    public List<string> WorldEvents { get; set; } = new List<string>();
    public WeatherTypes WorldWeather { get; set; }

    // Current location tracking
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }

    // Navigation options
    public List<UserLocationTravelOption> CurrentTravelOptions { get; set; } = new();
    public List<UserLocationSpotOption> CurrentLocationSpotOptions { get; set; } = new();


    public void SetCurrentTime(int hours)
    {
        CurrentTimeInHours = (CurrentTimeInHours + hours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeWindow = (CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        DetermineCurrentTimeWindow(timeWindow);
    }

    public void SetCurrentLocation(Location location)
    {
        CurrentLocation = location;

        if (location == null) return;
        CurrentLocationSpot = location.Spots.FirstOrDefault();
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

