public class WorldState
{
    // Core data collections
    public List<Location> locations { get; set; } = new();
    public List<LocationSpot> locationSpots { get; set; } = new();
    public List<NPC> NPCs { get; set; } = new();
    public List<LetterTemplate> LetterTemplates { get; set; } = new();
    public List<StandingObligation> StandingObligationTemplates { get; set; } = new();

    private Dictionary<string, int> LocationVisitCounts { get; } = new Dictionary<string, int>();
    public List<string> CompletedConversations { get; } = new List<string>();

    // Weather conditions (no seasons - game timeframe is only days/weeks)
    public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;

    // Route blocking system
    private Dictionary<string, int> TemporaryRouteBlocks { get; } = new Dictionary<string, int>();

    // New properties
    public List<Item> Items { get; set; } = new List<Item>();
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();

    // Card system removed - using conversation and location action systems

    // Progression tracking
    public List<RouteDiscovery> RouteDiscoveries { get; set; } = new List<RouteDiscovery>();
    public List<NetworkUnlock> NetworkUnlocks { get; set; } = new List<NetworkUnlock>();

    // Token Favor System
    public List<TokenFavor> TokenFavors { get; set; } = new List<TokenFavor>();


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


    public bool IsConversationCompleted(string actionId)
    {
        return CompletedConversations.Contains(actionId);
    }

    public void MarkConversationCompleted(string actionId)
    {
        CompletedConversations.Add(actionId);
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
    public void AddTemporaryRouteBlock(string routeId, int daysBlocked, int currentDay)
    {
        TemporaryRouteBlocks[routeId] = currentDay + daysBlocked;
    }

    /// <summary>
    /// Check if a route is temporarily blocked
    /// </summary>
    public bool IsRouteBlocked(string routeId, int currentDay)
    {
        if (TemporaryRouteBlocks.TryGetValue(routeId, out int unblockDay))
        {
            if (currentDay >= unblockDay)
            {
                TemporaryRouteBlocks.Remove(routeId);
                return false;
            }
            return true;
        }
        return false;
    }

}