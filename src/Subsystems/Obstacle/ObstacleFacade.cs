using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Public facade for obstacle-related operations
/// Coordinates GameWorld, equipment inventory, and obstacle intensity calculations
/// Provides clean interface for UI to display obstacles with equipment context matching
/// </summary>
public class ObstacleFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;

    public ObstacleFacade(GameWorld gameWorld, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Get all obstacles in the game world
    /// </summary>
    public List<Obstacle> GetAllObstacles()
    {
        return _gameWorld.Obstacles ?? new List<Obstacle>();
    }

    /// <summary>
    /// Get obstacle by ID
    /// </summary>
    public Obstacle GetObstacleById(string obstacleId)
    {
        return _gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
    }

    /// <summary>
    /// Get all obstacles at a specific location
    /// </summary>
    public List<Obstacle> GetObstaclesAtLocation(string locationId)
    {
        LocationEntry locationEntry = _gameWorld.Locations.FindById(locationId);
        if (locationEntry?.location == null)
            return new List<Obstacle>();

        return locationEntry.location.ObstacleIds
            .Select(id => GetObstacleById(id))
            .Where(o => o != null)
            .ToList();
    }

    /// <summary>
    /// Calculate effective obstacle intensity with player's current equipment
    /// </summary>
    public ObstacleIntensity CalculateEffectiveIntensity(string obstacleId)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return null;

        List<Equipment> playerEquipment = GetPlayerEquipment();
        return ObstacleIntensityCalculator.CalculateEffectiveIntensity(obstacle, playerEquipment);
    }

    /// <summary>
    /// Get all effective intensities for obstacles at a location
    /// Used for location display to show how player equipment affects obstacles
    /// </summary>
    public List<ObstacleIntensity> GetLocationObstacleIntensities(string locationId)
    {
        List<Obstacle> obstacles = GetObstaclesAtLocation(locationId);
        List<Equipment> playerEquipment = GetPlayerEquipment();

        return obstacles
            .Select(o => ObstacleIntensityCalculator.CalculateEffectiveIntensity(o, playerEquipment))
            .ToList();
    }

    /// <summary>
    /// Find equipment that matches a specific obstacle's contexts
    /// Returns list of equipment with matching contexts for UI display
    /// </summary>
    public List<EquipmentContextMatch> FindMatchingEquipmentForObstacle(string obstacleId)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return new List<EquipmentContextMatch>();

        List<Equipment> playerEquipment = GetPlayerEquipment();
        return EquipmentContextService.FindMatchingEquipment(playerEquipment, obstacle);
    }

    /// <summary>
    /// Check if player has equipment that reduces obstacle intensity
    /// </summary>
    public bool HasMatchingEquipmentForObstacle(string obstacleId)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return false;

        List<Equipment> playerEquipment = GetPlayerEquipment();
        return EquipmentContextService.HasMatchingEquipment(playerEquipment, obstacle);
    }

    /// <summary>
    /// Get player's equipment from inventory
    /// Filters inventory for Equipment items only
    /// </summary>
    private List<Equipment> GetPlayerEquipment()
    {
        Player player = _gameWorld.GetPlayer();
        List<string> inventoryItemIds = player.Inventory.GetItemIds();

        List<Equipment> equipment = new List<Equipment>();
        foreach (string itemId in inventoryItemIds)
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item is Equipment eq)
            {
                equipment.Add(eq);
            }
        }

        return equipment;
    }

    /// <summary>
    /// Apply obstacle property reduction from goal card reward
    /// Uses existing ObstacleRewardService for backward compatibility
    /// </summary>
    public bool ApplyPropertyReduction(string obstacleId, ObstaclePropertyReduction reduction)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return false;

        return ObstacleRewardService.ApplyPropertyReduction(obstacle, reduction);
    }
}
