using System.Collections.Generic;
using System.Linq;

public class WorldState
{
    // Hierarchical world organization
    public List<Region> Regions { get; set; } = new();
    public List<District> Districts { get; set; } = new();

    // Core data collections
    public List<Venue> venues { get; set; } = new();
    public List<Location> locations { get; set; } = new();
    public List<NPC> NPCs { get; set; } = new();
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

    // Card system removed - using conversation and Venue action systems

    // Progression tracking
    public List<RouteDiscovery> RouteDiscoveries { get; set; } = new List<RouteDiscovery>();



    public string GetVenueIdForLocation(string LocationId)
    {
        string? venueId = locations.Where(x => x.Id == LocationId).Select(x => x.VenueId).FirstOrDefault();
        return venueId;
    }

    public void RecordLocationVisit(string venueId)
    {
        if (!LocationVisitCounts.ContainsKey(venueId))
        {
            LocationVisitCounts[venueId] = 0;
        }

        LocationVisitCounts[venueId]++;
    }

    public int GetLocationVisitCount(string venueId)
    {
        return LocationVisitCounts.TryGetValue(venueId, out int count) ? count : 0;
    }

    public bool IsFirstVisit(string venueId)
    {
        return GetLocationVisitCount(venueId) == 0;
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
    public District GetDistrictForLocation(string venueId)
    {
        Venue? venue = venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null || string.IsNullOrEmpty(venue.District))
            return null;

        return Districts.FirstOrDefault(d => d.Id == venue.District);
    }

    public Region GetRegionForDistrict(string districtId)
    {
        District? district = Districts.FirstOrDefault(d => d.Id == districtId);
        if (district == null || string.IsNullOrEmpty(district.RegionId))
            return null;

        return Regions.FirstOrDefault(r => r.Id == district.RegionId);
    }

    public string GetFullLocationPath(string venueId)
    {
        Venue? venue = venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null) return "";

        District district = GetDistrictForLocation(venueId);
        if (district == null) return venue.Name;

        Region region = GetRegionForDistrict(district.Id);
        if (region == null) return $"{venue.Name}, {district.Name}";

        return $"{venue.Name}, {district.Name}, {region.Name}";
    }

}