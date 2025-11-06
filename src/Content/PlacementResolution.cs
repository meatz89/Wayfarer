namespace Wayfarer.Content;

/// <summary>
/// Result of resolving concrete placement from PlacementRelation enum
/// Replaces value tuple (PlacementType, string)
/// </summary>
public class PlacementResolution
{
    public PlacementType PlacementType { get; init; }
    public string PlacementId { get; init; }

    public PlacementResolution(PlacementType placementType, string placementId)
    {
        PlacementType = placementType;
        PlacementId = placementId;
    }
}
