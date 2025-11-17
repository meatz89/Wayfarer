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
        if (fromLocation.Id == toLocation.Id)
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
    /// </summary>
    public int CalculateTravelTime(RouteOption route, TravelMethods transportMethod)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        // PHASE 7B: Use location object references directly
        Location fromLocation = route.OriginLocation;
        Location toLocation = route.DestinationLocation;

        int baseTime = GetBaseTravelTime(fromLocation, toLocation);

        // Apply transport method modifier
        double modifier = GetTransportModifier(transportMethod);
        int actualTime = (int)(baseTime * modifier);

        // Apply weather effects if any
        actualTime = ApplyWeatherEffects(actualTime);

        // Apply route improvements (V2 Obligation System)
        // Use route.Name directly - no string construction/parsing
        List<RouteImprovement> improvements = _gameWorld.RouteImprovements
            .Where(ri => ri.RouteId == route.Name)
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
    /// Get transport method speed modifier.
    /// </summary>
    private double GetTransportModifier(TravelMethods transportMethod)
    {
        return transportMethod switch
        {
            TravelMethods.Walking => 1.0,      // Base speed
            TravelMethods.Horseback => 0.5,    // Twice as fast
            TravelMethods.Carriage => 0.7,     // Moderate speed boost
            TravelMethods.Cart => 1.3,          // Slower due to cargo
            TravelMethods.Boat => 0.8,          // Good for water routes
            _ => 1.0
        };
    }

    /// <summary>
    /// Apply weather effects to travel time.
    /// </summary>
    private int ApplyWeatherEffects(int baseTime)
    {
        WeatherCondition weather = _gameWorld.CurrentWeather;

        return weather switch
        {
            WeatherCondition.Rain => (int)(baseTime * 1.2),      // 20% slower in rain
            WeatherCondition.Snow => (int)(baseTime * 1.5),      // 50% slower in snow
            WeatherCondition.Storm => (int)(baseTime * 2.0),     // Double time in storm
            _ => baseTime                                         // No effect
        };
    }

    /// <summary>
    /// Calculate coin cost for travel.
    /// </summary>
    public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
    {
        int baseCost = route.BaseCoinCost;

        // Apply transport method cost modifier
        baseCost = transportMethod switch
        {
            TravelMethods.Walking => 0,                  // Free
            TravelMethods.Horseback => baseCost * 2,     // More expensive
            TravelMethods.Carriage => baseCost * 3,      // Most expensive
            TravelMethods.Cart => baseCost,              // Standard cost
            TravelMethods.Boat => baseCost * 2,          // Moderate cost
            _ => baseCost
        };

        return baseCost;
    }

    /// <summary>
    /// Check if travel is possible given current time constraints.
    /// </summary>
    public bool CanTravelInTime(int travelTimeSegments, int availableSegments)
    {
        return travelTimeSegments <= availableSegments;
    }
}
