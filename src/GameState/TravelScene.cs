
/// <summary>
/// TravelScene - Physical challenge system for route obstacles.
/// Represents an encounter on a route that requires the player to choose an approach.
/// Part of the Physical tactical layer (distinct from old equipment-based Scene system).
/// </summary>
public class TravelScene
{
    // HIGHLANDER: NO Id property - TravelScene identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }
    public SceneType Type { get; set; }

    // HIGHLANDER: Object references ONLY, no VenueId or RouteId
    // Placement context (NOT ownership)
    public Venue Venue { get; set; }
    public RouteOption Route { get; set; }

    // Approach options
    public List<SceneApproach> Approaches { get; set; } = new List<SceneApproach>();
}
