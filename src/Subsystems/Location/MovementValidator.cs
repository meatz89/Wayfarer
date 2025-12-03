/// <summary>
/// Validates movement between locations and Locations.
/// Enforces movement rules and access requirements.
/// NEW ARCHITECTURE: Uses LocationAccessibilityService for query-based accessibility
/// </summary>
public class MovementValidator
{
    private readonly GameWorld _gameWorld;
    private readonly LocationAccessibilityService _accessibilityService;

    public MovementValidator(
        GameWorld gameWorld,
        LocationAccessibilityService accessibilityService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _accessibilityService = accessibilityService ?? throw new ArgumentNullException(nameof(accessibilityService));
    }

    /// <summary>
    /// Validate that a location name is valid.
    /// </summary>
    public bool ValidateSpotName(string spotName)
    {
        return !string.IsNullOrEmpty(spotName);
    }

    /// <summary>
    /// Validate the current state before movement.
    /// </summary>
    public bool ValidateCurrentState(Venue currentLocation, Location currentSpot)
    {
        Player player = _gameWorld.GetPlayer();
        if (player == null) return false;
        if (currentSpot == null) return false;
        if (currentLocation == null) return false;

        // Verify consistency between Venue and location
        // ADR-007: Use Venue object reference instead of deleted VenueId/Id
        if (currentSpot.Venue != currentLocation)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if the player is already at the target location.
    /// </summary>
    public bool IsAlreadyAtSpot(Location currentSpot, string targetSpotIdentifier)
    {
        if (currentSpot == null || string.IsNullOrEmpty(targetSpotIdentifier)) return false;

        // ADR-007: Use Name only (natural key, no Id property)
        return currentSpot.Name.Equals(targetSpotIdentifier, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validate movement from one location to another.
    /// </summary>
    public MovementValidationResult ValidateMovement(Venue currentLocation, Location currentSpot, Location targetSpot)
    {
        MovementValidationResult result = new MovementValidationResult { IsValid = true };

        // Check target location exists
        if (targetSpot == null)
        {
            result.IsValid = false;
            result.ErrorMessage = "Target location does not exist";
            return result;
        }

        // Verify location belongs to current venue
        // ADR-007: Use Venue object reference instead of deleted VenueId/Id
        if (targetSpot.Venue != currentLocation)
        {
            result.IsValid = false;
            result.ErrorMessage = "Cannot move to a location in a different venue";
            return result;
        }

        // Check if movement is possible based on location properties
        if (!CanMoveFromSpot(currentSpot))
        {
            result.IsValid = false;
            result.ErrorMessage = "Cannot move from current location at this time";
            return result;
        }

        // DUAL-MODEL ACCESSIBILITY (per TIER 1 No Soft-Locks principle):
        // - Authored locations: Always accessible (cost varies, access doesn't)
        // - Dependent locations: Require scene progression to unlock
        if (!IsSpotAccessible(targetSpot))
        {
            result.IsValid = false;
            result.ErrorMessage = "Target location is not accessible at this time";
            return result;
        }

        return result;
    }

    /// <summary>
    /// Check if movement is possible from a location.
    /// </summary>
    public bool CanMoveFromSpot(Location location)
    {
        if (location == null) return false;

        // Check if location has any restrictions on leaving
        // For now, all Locations allow movement unless explicitly restricted

        return true;
    }

    /// <summary>
    /// Check if a location is accessible at the current time
    ///
    /// DUAL-MODEL ACCESSIBILITY (ADR-012) via LocationAccessibilityService:
    /// - Authored locations (Origin == Authored): Always accessible (No Soft-Locks)
    /// - Scene-created locations (Origin == SceneCreated): Accessible when scene grants access
    ///
    /// Pure query pattern - no state modification.
    /// </summary>
    public bool IsSpotAccessible(Location location)
    {
        if (location == null) return false;

        return _accessibilityService.IsLocationAccessible(location);
    }

    /// <summary>
    /// Validate travel between locations.
    /// </summary>
    public TravelValidationResult ValidateTravelToLocation(string targetVenueId, RouteOption route)
    {
        TravelValidationResult result = new TravelValidationResult { IsValid = true };
        Player player = _gameWorld.GetPlayer();

        // Check if route exists
        if (route == null)
        {
            result.IsValid = false;
            result.ErrorMessage = "No route available to destination";
            return result;
        }

        // Core Loop: All routes physically exist and are always visible
        // PRINCIPLE 4: Economic affordability determines practical access (coins, stamina)
        // No boolean gates - equipment reduces costs, never hides routes

        // Check coin cost
        if (route.BaseCoinCost > player.Coins)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Need {route.BaseCoinCost} coins";
            result.RequiredCoins = route.BaseCoinCost;
            return result;
        }

        // Check health requirement for certain routes
        if (route.Method == TravelMethods.Walking && player.Health < 30)
        {
            result.IsValid = false;
            result.ErrorMessage = "Too weak to walk this route";
            result.RequiredHealth = 30;
            return result;
        }

        return result;
    }

}

/// <summary>
/// Result of movement validation.
/// </summary>
public class MovementValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
}

/// <summary>
/// Result of travel validation.
/// </summary>
public class TravelValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    // Removed RequiredTier - route access is based on actual requirements in JSON
    public int? RequiredCoins { get; set; }
    public int? RequiredHealth { get; set; }
    public bool RequiresDiscovery { get; set; }
    public bool BlockedByAccess { get; set; }
}
