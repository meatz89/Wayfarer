public class WorldState
{
    // Core data collections
    public List<Location> Locations { get; private set; } = new();
    private List<Character> characters { get; set; } = new();
    private List<Opportunity> opportunities { get; set; } = new();

    // Forward progression tracking
    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();

    // Track last hub visited and depth
    private Dictionary<string, int> LocationDepths { get; } = new Dictionary<string, int>();
    public string LastHubLocationId { get; set; }
    public int LastHubDepth { get; set; } = 0;

    public List<string> CompletedEncounters { get; } = new List<string>();
    private Dictionary<string, int> ActionCounts { get; } = new Dictionary<string, int>();

    public void IncrementActionCount(string actionId)
    {
        if (!ActionCounts.ContainsKey(actionId))
            ActionCounts[actionId] = 0;

        ActionCounts[actionId]++;
    }

    public void SetLocationDepth(string locationId, int depth)
    {
        if (!LocationDepths.ContainsKey(locationId))
        {
            LocationDepths.Add(locationId, depth);
        }
        else
        {
            LocationDepths[locationId] = depth;
        }
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
        return Locations.FirstOrDefault(x =>
        {
            return x.Name == name;
        }); ;
    }

    public Character GetCharacter(string name)
    {
        return characters.FirstOrDefault(x =>
        {
            return x.Name == name;
        });
    }

    public Opportunity GetOpportunity(string name)
    {
        return opportunities.FirstOrDefault(x =>
        {
            return x.Name == name;
        });
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
    public int CurrentTimeMinutes { get; set; }
    public TimeWindow TimeWindow { get; set; }


    // Current location tracking
    public Location CurrentLocation { get; private set; }
    public LocationSpot CurrentLocationSpot { get; private set; }

    // Navigation options
    public List<UserLocationTravelOption> CurrentTravelOptions { get; set; } = new();

    public int CurrentDay { get; set; } = 1;

    public void SetCurrentLocation(Location location, LocationSpot currentLocationSpot)
    {
        CurrentLocation = location;
        SetCurrentLocationSpot(currentLocationSpot);

        if (location == null) return;
    }

    public void SetCurrentLocationSpot(LocationSpot locationSpot)
    {
        CurrentLocationSpot = locationSpot;
    }

    public void SetCurrentTravelOptions(List<UserLocationTravelOption> options)
    {
        CurrentTravelOptions = options;
    }

    public void AdvanceTime(int hours)
    {
        this.CurrentTimeInHours += hours;
    }

    public bool IsEncounterCompleted(string actionId)
    {
        return CompletedEncounters.Contains(actionId);
    }

    public void MarkEncounterCompleted(string actionId)
    {
        CompletedEncounters.Add(actionId);
    }
}