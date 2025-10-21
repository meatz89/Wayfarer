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
        return _gameWorld.Obstacles;
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
        Location location = _gameWorld.Locations.FindById(locationId);
        if (location == null)
            return new List<Obstacle>();

        return location.ObstacleIds
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
    public List<Equipment> GetPlayerEquipment()
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

    // ============================================
    // CORE LOOP: Equipment-Obstacle Matching
    // ============================================

    /// <summary>
    /// Get all equipment in player inventory that matches obstacle contexts
    /// Returns equipment where ApplicableContexts intersects obstacle.Contexts
    /// </summary>
    public List<Equipment> GetApplicableEquipment(string obstacleId)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return new List<Equipment>();

        List<Equipment> playerEquipment = GetPlayerEquipment();
        List<Equipment> applicableEquipment = new List<Equipment>();

        foreach (Equipment equipment in playerEquipment)
        {
            bool hasMatchingContext = false;
            foreach (Wayfarer.GameState.Enums.ObstacleContext equipContext in equipment.ApplicableContexts)
            {
                if (obstacle.Contexts.Contains(equipContext))
                {
                    hasMatchingContext = true;
                    break;
                }
            }

            if (hasMatchingContext)
            {
                applicableEquipment.Add(equipment);
            }
        }

        return applicableEquipment;
    }

    /// <summary>
    /// Calculate effective intensity after equipment reductions
    /// Formula: EffectiveIntensity = Max(0, BaseIntensity - Sum(MatchingEquipmentReductions))
    /// </summary>
    public int CalculateEffectiveIntensity(string obstacleId, List<string> equippedItemIds)
    {
        Obstacle obstacle = GetObstacleById(obstacleId);
        if (obstacle == null)
            return 0;

        int effectiveIntensity = obstacle.Intensity;

        foreach (string itemId in equippedItemIds)
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item is Equipment equipment)
            {
                bool hasMatchingContext = false;
                foreach (Wayfarer.GameState.Enums.ObstacleContext equipContext in equipment.ApplicableContexts)
                {
                    if (obstacle.Contexts.Contains(equipContext))
                    {
                        hasMatchingContext = true;
                        break;
                    }
                }

                if (hasMatchingContext)
                {
                    effectiveIntensity -= equipment.IntensityReduction;
                }
            }
        }

        return Math.Max(0, effectiveIntensity);
    }

    /// <summary>
    /// Check if player has equipment to fully resolve obstacle (reduce intensity to 0)
    /// </summary>
    public bool CanResolveWithEquipment(string obstacleId)
    {
        List<Equipment> applicableEquipment = GetApplicableEquipment(obstacleId);
        List<string> equippedItemIds = new List<string>();
        foreach (Equipment equipment in applicableEquipment)
        {
            equippedItemIds.Add(equipment.Id);
        }

        int effectiveIntensity = CalculateEffectiveIntensity(obstacleId, equippedItemIds);
        return effectiveIntensity == 0;
    }
}
