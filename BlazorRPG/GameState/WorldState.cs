
public class WorldState
{
    // Core data collections
    public List<Location> locations { get; private set; } = new();
    public List<LocationSpot> locationSpots { get; private set; } = new();
    public List<ActionDefinition> actions { get; private set; } = new();
    public List<CommissionDefinition> commissions { get; private set; } = new();
    private List<Character> characters { get; set; } = new();
    private List<Opportunity> opportunities { get; set; } = new();

    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();
    public List<string> CompletedEncounters { get; } = new List<string>();

    // Game time
    public int CurrentDay { get; set; } = 1;
    public int CurrentTimeHours { get; set; }
    public TimeWindowTypes CurrentTimeWindow { get; set; }

    // Current location tracking
    public Location CurrentLocation { get; private set; }
    public LocationSpot CurrentLocationSpot { get; private set; }
    public List<CardDefinition> AllCards { get; set; } = new List<CardDefinition>();
    public List<CommissionDefinition> CompletedCommissions { get; internal set; }
    public List<CommissionDefinition> ActiveCommissions { get; internal set; }
    public List<CommissionDefinition> FailedCommissions { get; internal set; }

    public string GetLocationIdForSpot(string locationSpotId)
    {
        string? locationId = locationSpots.Where(x => x.Id == locationSpotId).Select(x => x.LocationId).FirstOrDefault();
        return locationId;
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

    public void SetCurrentLocation(Location location, LocationSpot currentLocationSpot)
    {
        CurrentLocation = location;
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

    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }

    public void AddOpportunity(Opportunity opp)
    {
        opportunities.Add(opp);
    }

    public List<Character> GetCharacters()
    {
        return characters;
    }

    public List<Opportunity> GetOpportunities()
    {
        return opportunities;
    }

}