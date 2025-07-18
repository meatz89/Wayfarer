using Wayfarer.GameState;

public class WorldState
{
    // Core data collections
    public List<Location> locations { get; set; } = new();
    public List<LocationSpot> locationSpots { get; set; } = new();
    public List<ActionDefinition> actions { get; set; } = new();
    public List<NPC> NPCs { get; set; } = new();
    public List<LetterTemplate> LetterTemplates { get; set; } = new();
    public List<StandingObligation> StandingObligationTemplates { get; set; } = new();

    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();
    public List<string> CompletedEncounters { get; } = new List<string>();

    // Game time
    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Morning;
    public int CurrentTimeHours { get; set; }

    // Weather conditions (no seasons - game timeframe is only days/weeks)
    public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;

    // Route blocking system
    private Dictionary<string, int> TemporaryRouteBlocks { get; } = new Dictionary<string, int>();

    // New properties
    public List<Item> Items { get; set; } = new List<Item>();
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();

    // Current location tracking
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }

    // Card tracking
    public List<SkillCard> AllCards { get; set; } = new List<SkillCard>();
    
    // Progression tracking
    public List<RouteDiscovery> RouteDiscoveries { get; set; } = new List<RouteDiscovery>();
    public List<NetworkUnlock> NetworkUnlocks { get; set; } = new List<NetworkUnlock>();


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
        CurrentLocationSpot = currentLocationSpot;
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
        NPCs.Add(character);
    }


    public List<NPC> GetCharacters()
    {
        return NPCs;
    }


    /// <summary>
    /// Add a temporary route block that expires after specified days
    /// </summary>
    public void AddTemporaryRouteBlock(string routeId, int daysBlocked)
    {
        TemporaryRouteBlocks[routeId] = CurrentDay + daysBlocked;
    }

    /// <summary>
    /// Check if a route is temporarily blocked
    /// </summary>
    public bool IsRouteBlocked(string routeId)
    {
        if (TemporaryRouteBlocks.TryGetValue(routeId, out int unblockDay))
        {
            if (CurrentDay >= unblockDay)
            {
                TemporaryRouteBlocks.Remove(routeId);
                return false;
            }
            return true;
        }
        return false;
    }

}