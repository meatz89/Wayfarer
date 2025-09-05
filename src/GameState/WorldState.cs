using System.Collections.Generic;
using System.Linq;

public class WorldState
{
    // Hierarchical world organization
    public List<Region> Regions { get; set; } = new();
    public List<District> Districts { get; set; } = new();

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

    // Hierarchy lookup methods
    public District GetDistrictForLocation(string locationId)
    {
        Location? location = locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null || string.IsNullOrEmpty(location.District))
            return null;

        return Districts.FirstOrDefault(d => d.Id == location.District);
    }

    public Region GetRegionForDistrict(string districtId)
    {
        District? district = Districts.FirstOrDefault(d => d.Id == districtId);
        if (district == null || string.IsNullOrEmpty(district.RegionId))
            return null;

        return Regions.FirstOrDefault(r => r.Id == district.RegionId);
    }

    public string GetFullLocationPath(string locationId)
    {
        Location? location = locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null) return "";

        District district = GetDistrictForLocation(locationId);
        if (district == null) return location.Name;

        Region region = GetRegionForDistrict(district.Id);
        if (region == null) return $"{location.Name}, {district.Name}";

        return $"{location.Name}, {district.Name}, {region.Name}";
    }

}