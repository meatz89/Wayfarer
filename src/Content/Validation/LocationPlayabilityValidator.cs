/// <summary>
/// Validates that locations are functionally playable.
/// PRINCIPLE: Unplayable content worse than crash (playability over compilation).
/// Fail-fast on validation errors - forces fixing root cause in content authoring.
/// Applies to ALL locations (authored + generated) - Catalogue Pattern compliance.
/// </summary>
public class LocationPlayabilityValidator
{
    /// <summary>
    /// Validate that location meets playability requirements.
    /// Throws InvalidOperationException on validation failure (fail-fast).
    /// </summary>
    public void ValidateLocation(Location location, GameWorld gameWorld)
    {
        List<string> errors = new List<string>();

        // 1. Hex position must exist
        if (!location.HexPosition.HasValue)
        {
            errors.Add($"Location '{location.Id}' has no hex position - cannot be placed on world map");
        }
        else
        {
            // Verify hex exists at those coordinates
            Hex hex = gameWorld.WorldHexGrid.GetHex(location.HexPosition.Value.Q, location.HexPosition.Value.R);
            if (hex == null)
            {
                errors.Add($"Location '{location.Id}' has invalid hex position ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) - hex does not exist");
            }
        }

        // 2. Location must be reachable from player (or within same venue)
        if (!IsReachableFromPlayer(location, gameWorld))
        {
            errors.Add($"Location '{location.Id}' is not reachable - no route exists and not in same venue as player");
        }

        // 3. Venue must exist and be valid
        if (string.IsNullOrEmpty(location.VenueId))
        {
            errors.Add($"Location '{location.Id}' has no venue ID - every location must belong to a venue");
        }
        else if (location.Venue == null)
        {
            errors.Add($"Location '{location.Id}' has venue ID '{location.VenueId}' but venue object not resolved");
        }

        // 4. Required properties must be present
        if (location.LocationProperties == null || !location.LocationProperties.Any())
        {
            errors.Add($"Location '{location.Id}' has no properties - locations must have at least one property for action generation");
        }

        // 5. IsLocked validation DELETED - new architecture uses query-based accessibility via GrantsLocationAccess
        // Locations accessible when active situation grants access, not via flag modification

        // If any errors, throw with complete error list
        if (errors.Any())
        {
            string errorMessage = $"Location playability validation failed:\n" + string.Join("\n", errors);
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// Check if location is reachable from player current position.
    /// Location is reachable if: same venue as player OR route exists.
    /// </summary>
    private bool IsReachableFromPlayer(Location location, GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        if (player == null) return true; // During initialization, player may not exist yet

        Location playerLocation = gameWorld.GetPlayerCurrentLocation();
        if (playerLocation == null) return true; // Player not yet placed

        // Check if same venue (instant/free travel)
        if (location.VenueId == playerLocation.VenueId)
        {
            return true;
        }

        // Check if any route exists (either direction)
        bool routeExists = gameWorld.Routes.Any(r =>
            (r.OriginLocationId == playerLocation.Id && r.DestinationLocationId == location.Id) ||
            (r.OriginLocationId == location.Id && r.DestinationLocationId == playerLocation.Id)
        );

        return routeExists;
    }

    // CheckUnlockMechanism DELETED - new architecture uses query-based accessibility
    // Locations accessible when active situation grants access via GrantsLocationAccess property
    // No flag-based lock/unlock mechanism exists anymore
}
