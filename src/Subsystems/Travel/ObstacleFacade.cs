using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Public facade for travel obstacle operations.
/// Handles obstacle encounters, approach selection, and route improvements.
/// </summary>
public class ObstacleFacade
{
    private readonly GameWorld _gameWorld;
    private readonly TravelObstacleService _obstacleService;
    private readonly ItemRepository _itemRepository;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    public ObstacleFacade(
        GameWorld gameWorld,
        TravelObstacleService obstacleService,
        ItemRepository itemRepository,
        TimeManager timeManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _obstacleService = obstacleService ?? throw new ArgumentNullException(nameof(obstacleService));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Creates an obstacle context for UI rendering.
    /// This is called when the player encounters an obstacle during travel.
    /// </summary>
    public async Task<ObstacleContext> CreateObstacleContext(string obstacleId, RouteOption route = null)
    {
        await Task.CompletedTask; // For async consistency

        TravelObstacle obstacle = _gameWorld.TravelObstacles.FirstOrDefault(o => o.Id == obstacleId);
        if (obstacle == null)
        {
            throw new ArgumentException($"Obstacle with ID {obstacleId} not found");
        }

        Player player = _gameWorld.GetPlayer();

        // Get current location info
        LocationSpot currentSpot = _gameWorld.WorldState.locationSpots
            .FirstOrDefault(s => s.SpotID == player.CurrentLocationSpot?.SpotID);

        Location currentLocation = _gameWorld.WorldState.locations
            .FirstOrDefault(l => l.Id == currentSpot?.LocationId);

        ObstacleContext context = new ObstacleContext
        {
            Obstacle = obstacle,
            Player = player,
            ItemRepository = _itemRepository,
            Route = route,
            LocationInfo = currentLocation != null ? new LocationInfo
            {
                LocationId = currentLocation.Id,
                Name = currentLocation.Name,
                Description = currentLocation.Description
            } : null,
            CurrentTimeBlock = _timeManager.GetCurrentTimeBlock()
        };

        return context;
    }

    /// <summary>
    /// Processes a player's approach to an obstacle.
    /// Returns the result including success/failure, time consumed, and rewards.
    /// </summary>
    public async Task<ObstacleAttemptResult> AttemptObstacle(string obstacleId, string approachId, RouteOption route = null)
    {
        await Task.CompletedTask; // For async consistency

        TravelObstacle obstacle = _gameWorld.TravelObstacles.FirstOrDefault(o => o.Id == obstacleId);
        if (obstacle == null)
        {
            throw new ArgumentException($"Obstacle with ID {obstacleId} not found");
        }

        Player player = _gameWorld.GetPlayer();

        ObstacleApproach approach = obstacle.Approaches.FirstOrDefault(a => a.Id == approachId);
        if (approach == null)
        {
            throw new ArgumentException($"Approach with ID {approachId} not found in obstacle");
        }

        if (!approach.CanUseApproach(player, _itemRepository))
        {
            throw new InvalidOperationException($"Player cannot use approach {approachId} - requirements not met");
        }

        // Process the approach through the service
        string routeId = route?.Id ?? "";
        ObstacleAttemptResult result = _obstacleService.AttemptObstacle(player, obstacle, approach, routeId);

        // Route improvements are handled internally by TravelObstacleService
        if (result.RouteImproved && !string.IsNullOrEmpty(result.ImprovementDescription))
        {
            _messageSystem.AddSystemMessage($"Route improved: {result.ImprovementDescription}", SystemMessageTypes.Success);
        }

        return result;
    }

    /// <summary>
    /// Checks for obstacles on a specific route.
    /// Returns the first unresolved obstacle, or null if route is clear.
    /// </summary>
    public TravelObstacle CheckForObstacle(RouteOption route)
    {
        if (route == null)
            return null;

        Player player = _gameWorld.GetPlayer();

        // Check for obstacles on this route by matching RouteId
        TravelObstacle obstacle = _gameWorld.TravelObstacles
            .FirstOrDefault(o => o.RouteId == route.Id);

        return obstacle;
    }

    /// <summary>
    /// Gets available approaches for an obstacle that the player can use.
    /// </summary>
    public List<ObstacleApproach> GetAvailableApproaches(string obstacleId)
    {
        TravelObstacle obstacle = _gameWorld.TravelObstacles.FirstOrDefault(o => o.Id == obstacleId);
        if (obstacle == null)
            return new List<ObstacleApproach>();

        Player player = _gameWorld.GetPlayer();

        return obstacle.Approaches
            .Where(a => a.CanUseApproach(player, _itemRepository))
            .ToList();
    }

    /// <summary>
    /// Checks if player can bypass an obstacle.
    /// </summary>
    public bool CanBypassObstacle(string obstacleId)
    {
        // Some obstacles may be bypassable with specific knowledge or items
        // For now, always require approach selection
        return false;
    }
}
