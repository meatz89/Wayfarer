

public class WorldState
{
    // Core data collections
    public List<Location> locations { get; private set; } = new();
    public List<LocationSpot> locationSpots { get; private set; } = new();
    public List<ActionDefinition> actions { get; private set; } = new();
    private List<Character> characters { get; set; } = new();
    private List<Opportunity> opportunities { get; set; } = new();

    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();
    public List<string> CompletedEncounters { get; } = new List<string>();

    // Game time
    public int CurrentDay { get; set; } = 1;
    public int CurrentTimeHours { get; set; }
    public TimeWindow TimeWindow { get; set; }

    // Current location tracking
    public Location CurrentLocation { get; private set; }
    public LocationSpot CurrentLocationSpot { get; private set; }

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

    public bool IsEncounterCompleted(string actionId)
    {
        return CompletedEncounters.Contains(actionId);
    }

    public void MarkEncounterCompleted(string actionId)
    {
        CompletedEncounters.Add(actionId);
    }

    internal void AddCharacter(Character character)
    {
        characters.Add(character);
    }

    internal void AddOpportunity(Opportunity opp)
    {
        opportunities.Add(opp);
    }

    internal List<Character> GetCharacters()
    {
        return characters;
    }

    internal List<Opportunity> GetOpportunities()
    {
        return opportunities;
    }
}