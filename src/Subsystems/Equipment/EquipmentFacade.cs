using System;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Facade for equipment management operations
/// Handles purchase, exhaustion, repair, and selling of equipment
/// </summary>
public class EquipmentFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;

    public EquipmentFacade(GameWorld gameWorld, ItemRepository itemRepository)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
    }

    /// <summary>
    /// Purchase equipment from vendor
    /// </summary>
    public bool PurchaseEquipment(string equipmentId)
    {
        Item item = _itemRepository.GetItemById(equipmentId);
        if (item == null)
        {
            throw new InvalidOperationException($"Equipment '{equipmentId}' not found");
        }

        if (!(item is Equipment equipment))
        {
            throw new InvalidOperationException($"Item '{equipmentId}' is not equipment");
        }

        int cost = equipment.BuyPrice;
        bool success = _gameWorld.PurchaseEquipment(equipmentId, cost);

        if (success)
        {}

        return success;
    }

    /// <summary>
    /// Sell equipment to vendor
    /// </summary>
    public bool SellEquipment(string equipmentId)
    {
        Item item = _itemRepository.GetItemById(equipmentId);
        if (item == null || !(item is Equipment equipment))
        {
            throw new InvalidOperationException($"Equipment '{equipmentId}' not found");
        }

        int sellPrice = equipment.SellPrice;
        bool success = _gameWorld.SellEquipment(equipmentId, sellPrice);

        if (success)
        {}

        return success;
    }

    /// <summary>
    /// Get all equipment in player inventory
    /// </summary>
    public System.Collections.Generic.List<Equipment> GetPlayerEquipment()
    {
        Player player = _gameWorld.GetPlayer();
        System.Collections.Generic.List<Equipment> equipment = new System.Collections.Generic.List<Equipment>();

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item is Equipment eq)
                {
                    equipment.Add(eq);
                }
            }
        }

        return equipment;
    }

    /// <summary>
    /// Check if equipment matches a context (for obstacle resolution)
    /// </summary>
    public bool EquipmentMatchesContext(string equipmentId, ObstacleContext context)
    {
        Item item = _itemRepository.GetItemById(equipmentId);
        if (item == null || !(item is Equipment equipment))
        {
            return false;
        }

        return equipment.MatchesContext(context);
    }

    /// <summary>
    /// Get total intensity reduction for a given context from all equipment
    /// </summary>
    public int GetContextReduction(ObstacleContext context)
    {
        Player player = _gameWorld.GetPlayer();
        int totalReduction = 0;

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item is Equipment equipment && equipment.MatchesContext(context))
                {
                    totalReduction += equipment.IntensityReduction;
                }
            }
        }

        return totalReduction;
    }

    // ============================================
    // CORE LOOP: Equipment Economics
    // ============================================

    /// <summary>
    /// Check if player can afford equipment
    /// </summary>
    public bool CanAffordEquipment(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
            return false;

        Player player = _gameWorld.GetPlayer();
        return player.Coins >= item.BuyPrice;
    }

    /// <summary>
    /// Get all player equipment that applies to this obstacle's contexts
    /// Returns equipment with matching ApplicableContexts
    /// </summary>
    public System.Collections.Generic.List<Equipment> GetApplicableEquipment(Obstacle obstacle, Player player)
    {
        if (obstacle == null || player == null)
            return new System.Collections.Generic.List<Equipment>();

        System.Collections.Generic.List<Equipment> applicableEquipment = new System.Collections.Generic.List<Equipment>();

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item is Equipment equipment)
                {
                    // Check if equipment has any context matching obstacle contexts
                    foreach (ObstacleContext obstacleContext in obstacle.Contexts)
                    {
                        if (equipment.ApplicableContexts.Contains(obstacleContext))
                        {
                            applicableEquipment.Add(equipment);
                            break; // Count equipment once
                        }
                    }
                }
            }
        }

        return applicableEquipment;
    }

    /// <summary>
    /// Calculate effective obstacle intensity after equipment reductions
    /// Returns final intensity (minimum 0)
    /// </summary>
    public int CalculateEffectiveIntensity(Obstacle obstacle, System.Collections.Generic.List<Equipment> equipment)
    {
        if (obstacle == null)
            return 0;

        int baseIntensity = obstacle.Intensity;
        int totalReduction = 0;

        if (equipment != null)
        {
            foreach (Equipment eq in equipment)
            {
                // Check if this equipment applies to ANY of the obstacle's contexts
                foreach (ObstacleContext obstacleContext in obstacle.Contexts)
                {
                    if (eq.ApplicableContexts.Contains(obstacleContext))
                    {
                        totalReduction += eq.IntensityReduction;
                        break; // Count each equipment once
                    }
                }
            }
        }

        return System.Math.Max(0, baseIntensity - totalReduction);
    }

    /// <summary>
    /// Apply equipment to obstacle (mark as used if consumable)
    /// Consumable equipment is removed from inventory after use
    /// Permanent equipment stays in inventory
    /// </summary>
    public void ApplyEquipmentToObstacle(string equipmentId, string obstacleId)
    {
        Item item = _itemRepository.GetItemById(equipmentId);
        if (item == null || !(item is Equipment equipment))
        {
            throw new InvalidOperationException($"Equipment '{equipmentId}' not found");
        }

        if (equipment.UsageType == EquipmentUsageType.Consumable)
        {
            Player player = _gameWorld.GetPlayer();
            player.Inventory.RemoveItem(equipmentId);}
    }

    /// <summary>
    /// Repair exhausted equipment (future: costs coins)
    /// Currently placeholder for future durability system
    /// </summary>
    public void RepairEquipment(string equipmentId, int coinCost)
    {
        Item item = _itemRepository.GetItemById(equipmentId);
        if (item == null || !(item is Equipment equipment))
        {
            throw new InvalidOperationException($"Equipment '{equipmentId}' not found");
        }

        Player player = _gameWorld.GetPlayer();
        if (player.Coins < coinCost)
        {
            throw new InvalidOperationException($"Not enough coins to repair '{equipment.Name}'. Need {coinCost}, have {player.Coins}");
        }

        player.ModifyCoins(-coinCost);// Future: restore equipment durability
        // equipment.Durability = equipment.MaxDurability;
    }
}
