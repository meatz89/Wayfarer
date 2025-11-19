/// <summary>
/// Maintains synchronization between Location.HexPosition and Hex.Location.
/// HIGHLANDER PRINCIPLE: Location.HexPosition is source of truth, Hex.Location is derived lookup (object reference).
/// This service ensures the derived lookup stays synchronized.
/// </summary>
public class HexSynchronizationService
{
    /// <summary>
    /// Sync Hex.Location when Location.HexPosition is set.
    /// Called when location is added or hex position changes.
    /// HIGHLANDER: Assign Location object to hex, not string ID
    /// </summary>
    public void SyncLocationToHex(Location location, GameWorld gameWorld)
    {
        if (!location.HexPosition.HasValue)
        {
            // Location has no hex position - nothing to sync
            return;
        }

        // Find hex at location's position
        Hex hex = gameWorld.WorldHexGrid.GetHex(location.HexPosition.Value.Q, location.HexPosition.Value.R);
        if (hex == null)
        {
            throw new InvalidOperationException(
                $"Location {location.Name} has hex position ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) " +
                $"but no hex exists at those coordinates. HexMap may be incorrectly initialized."
            );
        }

        // Update derived lookup with object reference
        hex.Location = location;
    }

    /// <summary>
    /// Clear Hex.Location when location is removed.
    /// Called during location cleanup to maintain referential integrity.
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    public void ClearHexLocationReference(Location location, GameWorld gameWorld)
    {
        if (location == null || !location.HexPosition.HasValue)
            return;

        // Find hex at location's position
        Hex hex = gameWorld.WorldHexGrid.GetHex(location.HexPosition.Value.Q, location.HexPosition.Value.R);
        if (hex != null && hex.Location == location)
        {
            // Clear derived lookup
            hex.Location = null;
        }
    }
}
