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
    /// </summary>
    public int GetBaseTravelTime(string fromLocationId, string toLocationId)
    {
        // Same Location = no travel time
        if (fromLocationId == toLocationId)
        {
            return 0;
        }

        // Get locations from GameWorld
        Location fromLocation = _gameWorld.GetLocation(fromLocationId);
        Location toLocation = _gameWorld.GetLocation(toLocationId);

        if (fromLocation == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location not found: {fromLocationId}");

        if (toLocation == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location not found: {toLocationId}");

        // Verify locations have hex positions
        if (fromLocation.HexPosition == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location '{fromLocationId}' missing HexPosition");

        if (toLocation.HexPosition == null)
            throw new InvalidOperationException($"[TravelTimeCalculator] Location '{toLocationId}' missing HexPosition");

        // Calculate hex distance (number of hexes between locations)
        int hexDistance = fromLocation.HexPosition.Value.DistanceTo(toLocation.HexPosition.Value);

        // Return hex distance as travel time in segments
        return hexDistance;
    }

    /// <summary>
    /// Calculate actual travel time with transport method modifier and route improvements.
    /// </summary>
    public int CalculateTravelTime(string fromLocationId, string toLocationId, TravelMethods transportMethod)
    {
        int baseTime = GetBaseTravelTime(fromLocationId, toLocationId);

        // Apply transport method modifier
        double modifier = GetTransportModifier(transportMethod);
        int actualTime = (int)(baseTime * modifier);

        // Apply weather effects if any
        actualTime = ApplyWeatherEffects(actualTime);

        // Apply route improvements (V2 Obligation System)
        // Find route by matching RouteOption.Id or constructing route key
        string routeKey = $"{fromLocationId}_to_{toLocationId}";
        List<RouteImprovement> improvements = _gameWorld.RouteImprovements.Where(ri => ri.RouteId == routeKey).ToList();
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
