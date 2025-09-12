using System;
using System.Linq;

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Validates movement between locations and spots.
    /// Enforces movement rules and access requirements.
    /// </summary>
    public class MovementValidator
    {
        private readonly GameWorld _gameWorld;
        private readonly AccessRequirementChecker _accessChecker;

        public MovementValidator(GameWorld gameWorld, AccessRequirementChecker accessChecker)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _accessChecker = accessChecker ?? throw new ArgumentNullException(nameof(accessChecker));
        }

        /// <summary>
        /// Validate that a spot name is valid.
        /// </summary>
        public bool ValidateSpotName(string spotName)
        {
            return !string.IsNullOrEmpty(spotName);
        }

        /// <summary>
        /// Validate the current state before movement.
        /// </summary>
        public bool ValidateCurrentState(Player player, Location currentLocation, LocationSpot currentSpot)
        {
            if (player == null) return false;
            if (currentSpot == null) return false;
            if (currentLocation == null) return false;

            // Verify consistency between location and spot
            if (!currentSpot.LocationId.Equals(currentLocation.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the player is already at the target spot.
        /// </summary>
        public bool IsAlreadyAtSpot(LocationSpot currentSpot, string targetSpotIdentifier)
        {
            if (currentSpot == null || string.IsNullOrEmpty(targetSpotIdentifier)) return false;

            return currentSpot.Name.Equals(targetSpotIdentifier, StringComparison.OrdinalIgnoreCase) ||
                   currentSpot.SpotID.Equals(targetSpotIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validate movement from one spot to another.
        /// </summary>
        public MovementValidationResult ValidateMovement(Location currentLocation, LocationSpot currentSpot, LocationSpot targetSpot)
        {
            MovementValidationResult result = new MovementValidationResult { IsValid = true };

            // Check target spot exists
            if (targetSpot == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "Target spot does not exist";
                return result;
            }

            // Verify spot belongs to current location
            if (!targetSpot.LocationId.Equals(currentLocation.Id, StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot move to a spot in a different location";
                return result;
            }

            // Check access requirements
            if (targetSpot.AccessRequirement != null)
            {
                AccessCheckResult accessCheck = _accessChecker.CheckSpotAccess(targetSpot);
                if (!accessCheck.IsAllowed)
                {
                    result.IsValid = false;
                    result.ErrorMessage = accessCheck.BlockedMessage ?? "Cannot access this spot";
                    return result;
                }
            }

            // Check if movement is possible based on spot properties
            if (!CanMoveFromSpot(currentSpot))
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot move from current spot at this time";
                return result;
            }

            // Check if target spot is accessible at current time
            if (!IsSpotAccessible(targetSpot))
            {
                result.IsValid = false;
                result.ErrorMessage = "Target spot is not accessible at this time";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Check if movement is possible from a spot.
        /// </summary>
        public bool CanMoveFromSpot(LocationSpot spot)
        {
            if (spot == null) return false;

            // Check if spot has any restrictions on leaving
            // For now, all spots allow movement unless explicitly restricted
            // if (spot.SpotProperties?.Contains(SpotPropertyType.NoExit) == true)
            // {
            //     return false;
            // }

            return true;
        }

        /// <summary>
        /// Check if a spot is accessible at the current time.
        /// </summary>
        public bool IsSpotAccessible(LocationSpot spot)
        {
            if (spot == null) return false;

            // Check if spot has time-based restrictions
            // These restrictions are now handled through time-specific properties
            // if (spot.SpotProperties?.Contains(SpotPropertyType.NightOnly) == true)
            // {
            //     TimeBlocks currentTime = _gameWorld.CurrentTimeBlock;
            //     if (currentTime != TimeBlocks.Night && currentTime != TimeBlocks.LateNight)
            //     {
            //         return false;
            //     }
            // }

            return true;
        }

        /// <summary>
        /// Validate travel between locations.
        /// </summary>
        public TravelValidationResult ValidateTravelToLocation(string targetLocationId, RouteOption route)
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

            // Route access is determined by actual requirements (tokens, permissions, etc.)
            // defined in the route's AccessRequirement property, not by arbitrary tiers

            // Check if route is discovered
            if (!route.IsDiscovered)
            {
                result.IsValid = false;
                result.ErrorMessage = "Route not yet discovered";
                result.RequiresDiscovery = true;
                return result;
            }

            // Check access requirements
            AccessCheckResult accessCheck = _accessChecker.CheckRouteAccess(route);
            if (!accessCheck.IsAllowed)
            {
                result.IsValid = false;
                result.ErrorMessage = accessCheck.BlockedMessage ?? "Cannot access this route";
                result.BlockedByAccess = true;
                return result;
            }

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

        /// <summary>
        /// Check if player can enter a location.
        /// </summary>
        public bool CanEnterLocation(string locationId)
        {
            Player player = _gameWorld.GetPlayer();

            // Check if location is unlocked
            if (player.UnlockedLocationIds?.Contains(locationId) == true)
            {
                return true;
            }

            // Check if location has been discovered
            if (player.DiscoveredLocationIds?.Contains(locationId) == true)
            {
                return true;
            }

            // Check if it's a starting location (always accessible)
            if (locationId == "greystone_market" || locationId == "sleeping_fox_inn")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the reason why a location cannot be entered.
        /// </summary>
        public string GetLocationBlockedReason(string locationId)
        {
            if (CanEnterLocation(locationId)) return null;

            Player player = _gameWorld.GetPlayer();

            if (!player.DiscoveredLocationIds?.Contains(locationId) == true)
            {
                return "Location not yet discovered";
            }

            return "Location is locked";
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
}