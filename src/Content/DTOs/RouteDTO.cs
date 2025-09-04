using System.Collections.Generic;
/// <summary>
/// Data Transfer Object for deserializing route data from JSON.
/// Maps to the structure in routes.json.
/// </summary>
public class RouteDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string OriginLocationSpot { get; set; }
    public string DestinationLocationSpot { get; set; }
    public string Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TravelTimeMinutes { get; set; }
    public string DepartureTime { get; set; }
    public bool IsDiscovered { get; set; }
    public List<string> TerrainCategories { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; }
    public string Description { get; set; }
    public AccessRequirementDTO AccessRequirement { get; set; }
}