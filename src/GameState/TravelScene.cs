using Wayfarer.GameState.Enums;

/// <summary>
/// TravelScene - Physical challenge system for route obstacles.
/// Represents an encounter on a route that requires the player to choose an approach.
/// Part of the Physical tactical layer (distinct from old equipment-based Scene system).
/// </summary>
public class TravelScene
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SceneType Type { get; set; }

    // Placement context (NOT ownership)
    public string VenueId { get; set; }
    public string RouteId { get; set; }

    // Approach options
    public List<SceneApproach> Approaches { get; set; } = new List<SceneApproach>();
}
