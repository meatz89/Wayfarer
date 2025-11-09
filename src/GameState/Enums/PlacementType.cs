/// <summary>
/// Where a situation appears in the UI
/// Determines which screen/context shows the situation button
/// </summary>
public enum PlacementType
{
/// <summary>
/// Situation appears at a specific location
/// PlacementLocationId must be set
/// </summary>
Location,

/// <summary>
/// Situation appears when interacting with a specific NPC
/// PlacementNpcId must be set
/// </summary>
NPC,

/// <summary>
/// Situation appears on a travel route (scouting, pathfinding, etc.)
/// PlacementRouteId must be set
/// </summary>
Route
}
