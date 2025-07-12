public class RouteRepository
{
    private readonly GameWorld _gameWorld;

    public RouteRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    // Check if a route is blocked
    public bool IsRouteBlocked(string routeId)
    {
        return _gameWorld.WorldState.IsRouteBlocked(routeId);
    }

    // Get current weather (weather affects route availability)
    public WeatherCondition GetCurrentWeather()
    {
        return _gameWorld.WorldState.CurrentWeather;
    }
}