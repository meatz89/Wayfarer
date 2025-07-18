using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing route data from JSON.
/// Maps to the structure in routes.json.
/// </summary>
public class RouteDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public string Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int? TimeBlockCost { get; set; } // Legacy field - will be converted to TravelTimeHours
    public int? TravelTimeHours { get; set; } // New field - takes precedence over TimeBlockCost
    public string DepartureTime { get; set; }
    public bool IsDiscovered { get; set; }
    public List<string> TerrainCategories { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; }
    public string Description { get; set; }
    public AccessRequirementDTO AccessRequirement { get; set; }
    
    /// <summary>
    /// Get the actual travel time in hours, with fallback to legacy TimeBlockCost * 3
    /// </summary>
    public int GetTravelTimeHours()
    {
        if (TravelTimeHours.HasValue)
            return TravelTimeHours.Value;
        
        // Legacy conversion: 1 time block = ~3 hours
        if (TimeBlockCost.HasValue)
            return TimeBlockCost.Value * 3;
        
        // Default to 3 hours if neither is specified
        return 3;
    }
}