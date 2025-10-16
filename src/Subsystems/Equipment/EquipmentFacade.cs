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
        {
            Console.WriteLine($"[EquipmentFacade] Purchased '{equipment.Name}' for {cost} coins");
        }

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
        {
            Console.WriteLine($"[EquipmentFacade] Sold '{equipment.Name}' for {sellPrice} coins");
        }

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
}
