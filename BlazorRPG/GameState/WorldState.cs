public class WorldState
{
    // Core data collections
    public List<Location> locations { get; private set; } = new();
    public List<LocationSpot> locationSpots { get; private set; } = new();
    public List<ActionDefinition> actions { get; private set; } = new();
    public List<ContractDefinition> Opportunities { get; private set; } = new();
    private List<NPC> characters { get; set; } = new();
    private List<Contract> contracts { get; set; } = new();

    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();
    public List<string> CompletedEncounters { get; } = new List<string>();

    // Game time
    public int CurrentDay { get; set; } = 1;

    // Current location tracking
    public Location CurrentLocation { get; private set; }
    public LocationSpot CurrentLocationSpot { get; private set; }
    public List<SkillCard> AllCards { get; set; } = new List<SkillCard>();
    public List<ContractDefinition> CompletedOpportunities { get; set; }
    public List<ContractDefinition> ActiveOpportunities { get; set; }
    public List<ContractDefinition> FailedOpportunities { get; set; }
    public TimeBlocks CurrentTimeWindow { get; internal set; }
    public int CurrentTimeHours { get; internal set; }

    public string GetLocationIdForSpot(string locationSpotId)
    {
        string? locationId = locationSpots.Where(x => x.SpotID == locationSpotId).Select(x => x.LocationId).FirstOrDefault();
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

    public void AddCharacter(NPC character)
    {
        characters.Add(character);
    }

    public void AddOpportunity(Contract opp)
    {
        contracts.Add(opp);
    }

    public List<NPC> GetCharacters()
    {
        return characters;
    }

    public List<Contract> GetOpportunities()
    {
        return contracts;
    }

}