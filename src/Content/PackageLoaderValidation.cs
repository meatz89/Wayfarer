/// <summary>
/// Validation helpers for PackageLoader.
/// Extracted from PackageLoader to reduce file size and improve organization.
/// </summary>
internal static class PackageLoaderValidation
{
    /// <summary>
    /// Helper class for grouping locations by venue during validation.
    /// HIGHLANDER: Uses Venue object reference, not string ID.
    /// </summary>
    private class VenueLocationGrouping
    {
        public Venue Venue { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
    }

    /// <summary>
    /// Validates crossroads configuration:
    /// 1. Each Venue has exactly one location with Connective/Hub role
    /// 2. All route endpoints have Connective/Hub role
    /// HIGHLANDER: Uses object references for venue comparison.
    /// </summary>
    public static void ValidateCrossroadsConfiguration(GameWorld gameWorld)
    {
        // Group Locations by Venue using object references (HIGHLANDER compliant)
        List<VenueLocationGrouping> spotsByLocation = new List<VenueLocationGrouping>();
        foreach (Location location in gameWorld.Locations)
        {
            // HIGHLANDER: Compare Venue objects directly, not string IDs
            int groupIndex = spotsByLocation.FindIndex(g => g.Venue == location.Venue);
            if (groupIndex == -1)
            {
                VenueLocationGrouping group = new VenueLocationGrouping();
                group.Venue = location.Venue;
                group.Locations.Add(location);
                spotsByLocation.Add(group);
            }
            else
            {
                spotsByLocation[groupIndex].Locations.Add(location);
            }
        }

        // Validate each venue has exactly one travel hub location (Connective or Hub role)
        foreach (Venue venue in gameWorld.Venues)
        {
            VenueLocationGrouping locationGroup = spotsByLocation.FirstOrDefault(g => g.Venue == venue);
            if (locationGroup == null || locationGroup.Venue == null)
            {
                throw new InvalidOperationException($"Venue '{venue.Name}' has no Locations defined");
            }

            List<Location> locations = locationGroup.Locations;
            List<Location> travelHubSpots = locations
                .Where(s => s.Role == LocationRole.Connective || s.Role == LocationRole.Hub)
                .ToList();

            if (travelHubSpots.Count == 0)
            {
                throw new InvalidOperationException($"Venue '{venue.Name}' has no Locations with Connective or Hub role. Every Venue must have exactly one travel hub location for travel.");
            }
            else if (travelHubSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", travelHubSpots.Select(s => $"'{s.Name}'"));
                throw new InvalidOperationException($"Venue '{venue.Name}' has {travelHubSpots.Count} Locations with Connective/Hub role: {spotsInfo}. Only one travel hub location is allowed per venue.");
            }
        }

        // Validate all route Locations have travel hub role
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in gameWorld.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocation.Name))
                routeSpotIds.Add(route.OriginLocation.Name);
            if (!routeSpotIds.Contains(route.DestinationLocation.Name))
                routeSpotIds.Add(route.DestinationLocation.Name);
        }

        foreach (string locationName in routeSpotIds)
        {
            // HIGHLANDER: LINQ query on already-parsed locations
            Location location = gameWorld.Locations.FirstOrDefault(l => l.Name == locationName);
            if (location == null)
            {
                // Create skeleton location with Connective role (required for routes)
                location = SkeletonGenerator.GenerateSkeletonSpot(
                    locationName,
                    "unknown_location",
                    $"travel_hub_validation_{locationName}"
                );

                // Ensure skeleton has Connective role for route connectivity
                location.Role = LocationRole.Connective;

                gameWorld.AddOrUpdateLocation(locationName, location);
                gameWorld.AddSkeleton(locationName, "Location");
            }

            // Ensure route endpoints have travel hub role (Connective or Hub)
            if (location.Role != LocationRole.Connective && location.Role != LocationRole.Hub)
            {
                location.Role = LocationRole.Connective;
            }
        }
    }

    /// <summary>
    /// Validates spatial venue assignments after location placement.
    /// FAIL-FAST: Throws if any location violates spatial constraints.
    /// </summary>
    public static void ValidateVenueAssignmentsSpatially(GameWorld gameWorld)
    {
        Console.WriteLine("[SpatialValidation] Starting spatial venue assignment validation");

        int validated = 0;

        foreach (Location location in gameWorld.Locations)
        {
            // Validation 1: Location must have HexPosition after placement
            if (!location.HexPosition.HasValue)
            {
                throw new InvalidOperationException(
                    $"Location '{location.Name}' has no HexPosition after PlaceLocations(). " +
                    $"All locations must have hex positions assigned by placement algorithm.");
            }

            // Validation 2: Location must have Venue after placement
            if (location.Venue == null)
            {
                throw new InvalidOperationException(
                    $"Location '{location.Name}' has no Venue after PlaceLocations(). " +
                    $"PlaceLocation() should have assigned venue atomically with hex position.");
            }

            // Validation 3: Location's hex must be within assigned venue's territory
            if (!location.Venue.ContainsHex(location.HexPosition.Value))
            {
                throw new InvalidOperationException(
                    $"SPATIAL CONSTRAINT VIOLATION: Location '{location.Name}' at hex ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R}) " +
                    $"is assigned to venue '{location.Venue.Name}' but hex is OUTSIDE venue's allocated territory. " +
                    $"Placement algorithm violated spatial constraints - venue.ContainsHex() returned false.");
            }

            Console.WriteLine($"[SpatialValidation] Location '{location.Name}' correctly assigned to venue '{location.Venue.Name}' at hex ({location.HexPosition.Value.Q}, {location.HexPosition.Value.R})");
            validated++;
        }

        Console.WriteLine($"[SpatialValidation] Spatial validation complete: {validated} locations verified");
    }
}
