/// <summary>
/// Maintains synchronization between Location.HexPosition and Hex.LocationId.
/// HIGHLANDER PRINCIPLE: Location.HexPosition is source of truth, Hex.LocationId is derived lookup.
/// This service ensures the derived lookup stays synchronized.
/// </summary>
public class HexSynchronizationService
{
    /// <summary>
    /// Sync Hex.LocationId when Location.HexPosition is set.
    /// Called when location is added or hex position changes.
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
                $"Location {location.Id} has hex position ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) " +
                $"but no hex exists at those coordinates. HexMap may be incorrectly initialized."
            );
        }

        // Update derived lookup
        hex.LocationId = location.Id;
    }

    /// <summary>
    /// Clear Hex.LocationId when location is removed.
    /// Called during location cleanup to maintain referential integrity.
    /// </summary>
    public void ClearHexLocationReference(string locationId, GameWorld gameWorld)
    {
        // Find hex via reverse lookup (before we clear the reference)
        Hex hex = gameWorld.WorldHexGrid.GetHexForLocation(locationId);
        if (hex != null)
        {
            // Clear derived lookup
            hex.LocationId = null;
        }
    }
}
