/// <summary>
/// Calculates travel times and costs between locations.
/// </summary>
public class TravelTimeCalculator
{
    private readonly GameWorld _gameWorld;

    public TravelTimeCalculator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Get base travel time between two locations in segments.
    /// Calculates distance using hex coordinates (Location.HexPosition).
    /// PHASE 6D: Accept Location objects instead of IDs
    /// </summary>
    public int GetBaseTravelTime(Location fromLocation, Location toLocation)
    {
        if (fromLocation == null)
            throw new ArgumentNullException(nameof(fromLocation));

        if (toLocation == null)
            throw new ArgumentNullException(nameof(toLocation));

        // Same Location = no travel time
        // HIGHLANDER: Object equality, not Id comparison
        if (fromLocation == toLocation)
        {
            return 0;
        }

        // Verify locations have hex positions
        if (fromLocation.HexPosition == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location '{fromLocation.Name}' missing HexPosition");

        if (toLocation.HexPosition == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location '{toLocation.Name}' missing HexPosition");

        // Calculate hex distance (number of hexes between locations)
        int hexDistance = fromLocation.HexPosition.Value.DistanceTo(toLocation.HexPosition.Value);

        // Return hex distance as travel time in segments
        return hexDistance;
    }

    /// <summary>
    /// Calculate actual travel time with transport method modifier and route improvements.
    /// Uses Route.Id directly for improvement lookup (no ID construction/parsing).
    /// DDR-007: All adjustments are flat segment additions/subtractions.
    /// </summary>
    public int CalculateTravelTime(RouteOption route, TravelMethods transportMethod)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        // PHASE 7B: Use location object references directly
        Location fromLocation = route.OriginLocation;
        Location toLocation = route.DestinationLocation;

        int baseTime = GetBaseTravelTime(fromLocation, toLocation);

        // DDR-007: Apply transport method as flat segment adjustment
        int transportAdjustment = GetTransportTimeAdjustment(transportMethod);
        int actualTime = baseTime + transportAdjustment;

        // DDR-007: Apply weather effects as flat segment adjustment
        int weatherAdjustment = GetWeatherTimeAdjustment();
        actualTime = actualTime + weatherAdjustment;

        // Apply route improvements (V2 Obligation System)
        // HIGHLANDER: Query by Route object reference, not deleted RouteId property
        List<RouteImprovement> improvements = _gameWorld.RouteImprovements
            .Where(ri => ri.Route == route)
            .ToList();

        if (improvements != null && improvements.Count > 0)
        {
            foreach (RouteImprovement improvement in improvements)
            {
                actualTime -= improvement.TimeReduction;
            }
        }

        return Math.Max(1, actualTime); // Minimum 1 segment travel time
    }

    /// <summary>
    /// Get transport method time adjustment in segments (DDR-007: flat adjustments).
    /// Negative = faster, Positive = slower.
    /// </summary>
    private int GetTransportTimeAdjustment(TravelMethods transportMethod)
    {
        return transportMethod switch
        {
            TravelMethods.Walking => 0,       // Base speed (no adjustment)
            TravelMethods.Horseback => -2,    // 2 segments faster
            TravelMethods.Carriage => -1,     // 1 segment faster
            TravelMethods.Cart => 2,          // 2 segments slower (cargo)
            TravelMethods.Boat => -1,         // 1 segment faster (water routes)
            _ => 0
        };
    }

    /// <summary>
    /// Get weather time adjustment in segments (DDR-007: flat adjustments).
    /// </summary>
    private int GetWeatherTimeAdjustment()
    {
        WeatherCondition weather = _gameWorld.CurrentWeather;

        return weather switch
        {
            WeatherCondition.Rain => 1,       // +1 segment in rain
            WeatherCondition.Snow => 2,       // +2 segments in snow
            WeatherCondition.Storm => 3,      // +3 segments in storm
            _ => 0                            // No effect
        };
    }

    /// <summary>
    /// Calculate coin cost for travel (DDR-007: flat costs per transport type).
    /// </summary>
    public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
    {
        // DDR-007: Each transport type has a flat coin cost
        return transportMethod switch
        {
            TravelMethods.Walking => 0,        // Free
            TravelMethods.Horseback => 8,      // 8 coins (premium service)
            TravelMethods.Carriage => 12,      // 12 coins (luxury service)
            TravelMethods.Cart => 5,           // 5 coins (standard cargo)
            TravelMethods.Boat => 6,           // 6 coins (ferry fee)
            _ => 0
        };
    }

    /// <summary>
    /// Check if travel is possible given current time constraints.
    /// </summary>
    public bool CanTravelInTime(int travelTimeSegments, int availableSegments)
    {
        return travelTimeSegments <= availableSegments;
    }
}
