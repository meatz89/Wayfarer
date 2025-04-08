public class WorldState
{
    // Core data collections
    public List<Location> Locations { get; private set; } = new();
    private List<Character> characters { get; set; } = new();
    private List<Opportunity> opportunities { get; set; } = new();

    // Forward progression tracking
    private HashSet<string> CompletedEncounterIds { get; } = new HashSet<string>();
    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();

    // Track last hub visited and depth

    private Dictionary<string, int> LocationDepths { get; } = new Dictionary<string, int>();
    public string LastHubLocationId { get; set; }
    public int LastHubDepth { get; set; } = 0;

    public void SetLocationDepth(string locationId, int depth)
    {
        LocationDepths[locationId] = depth;
    }

    public int GetLocationDepth(string locationId)
    {
        return LocationDepths.TryGetValue(locationId, out int depth) ? depth : 0;
    }

    public void UpdateHubTracking(Location location)
    {
        if (location.LocationType == LocationTypes.Hub && location.Depth > LastHubDepth)
        {
            LastHubLocationId = location.Name;
            LastHubDepth = location.Depth;
        }
    }

    public void MarkEncounterCompleted(string encounterId)
    {
        CompletedEncounterIds.Add(encounterId);
    }

    public bool IsEncounterCompleted(string encounterId)
    {
        return CompletedEncounterIds.Contains(encounterId);
    }


    public void RecordLocationVisit(string locationId)
    {
        if (!LocationVisitCounts.ContainsKey(locationId))
        {
            LocationVisitCounts[locationId] = 0;
        }

        LocationVisitCounts[locationId]++;
    }

    public int GetLocationVisitCount(string locationId)
    {
        return LocationVisitCounts.TryGetValue(locationId, out int count) ? count : 0;
    }

    public bool IsFirstVisit(string locationId)
    {
        return GetLocationVisitCount(locationId) == 0;
    }

    public List<Location> GetLocations()
    {
        return Locations.ToList();
    }

    public List<Opportunity> GetOpportunities()
    {
        return opportunities.ToList();
    }

    public List<Character> GetCharacters()
    {
        return characters.ToList();
    }

    public Location GetLocation(string name)
    {
        return Locations.FirstOrDefault(x => x.Name == name); ;
    }

    public Character GetCharacter(string name)
    {
        return characters.FirstOrDefault(x => x.Name == name);
    }

    public Opportunity GetOpportunity(string name)
    {
        return opportunities.FirstOrDefault(x => x.Name == name);
    }


    public void AddLocations(List<Location> newLocations)
    {
        Locations.AddRange(newLocations);
    }

    public void AddCharacters(List<Character> newCharacters)
    {
        characters.AddRange(newCharacters);
    }

    public void AddOpportunities(List<Opportunity> newOpportunities)
    {
        opportunities.AddRange(newOpportunities);
    }

    public void AddLocation(Location location)
    {
        Locations.Add(location);
    }

    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }

    public void AddOpportunity(Opportunity opportunity)
    {
        opportunities.Add(opportunity);
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

        if (location.LocationSpots == null || !location.LocationSpots.Any())
        {
            Console.WriteLine($"WARNING: Location {location.Name} has no spots!");
            return;
        }

        // Set current spot to first spot and log
        LocationSpot firstSpot = location.LocationSpots.FirstOrDefault();
        SetCurrentLocationSpot(firstSpot);
        Console.WriteLine($"Set current location spot to: {firstSpot?.Name ?? "NULL"}");
    }

    public void SetCurrentLocationSpot(LocationSpot locationSpot)
    {
        CurrentLocationSpot = locationSpot;
    }

    public void SetCurrentTravelOptions(List<UserLocationTravelOption> options)
    {
        CurrentTravelOptions = options;
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