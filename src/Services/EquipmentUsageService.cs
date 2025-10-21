using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Domain Service for equipment usage, exhaustion, and repair
/// Handles lifecycle of Exhaustible equipment (multi-use with repair mechanics)
/// </summary>
public class EquipmentUsageService
{
    private readonly GameWorld _gameWorld;

    public EquipmentUsageService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Use equipment and track usage/exhaustion
    /// Permanent: Always works, no tracking
    /// Consumable: Single use, destroyed after use
    /// Exhaustible: Increment usage, check if exhausted
    /// </summary>
    public EquipmentUsageResult UseEquipment(string equipmentId)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify player owns this equipment
        if (!player.Inventory.HasItem(equipmentId))
        {
            return new EquipmentUsageResult
            {
                Success = false,
                Message = "Player does not own this equipment"
            };
        }

        // Get equipment from GameWorld
        Equipment equipment = _gameWorld.Items.OfType<Equipment>().FirstOrDefault(e => e.Id == equipmentId);
        if (equipment == null)
        {
            return new EquipmentUsageResult
            {
                Success = false,
                Message = $"Equipment {equipmentId} not found in GameWorld.Items"
            };
        }

        // Check if equipment is functional
        if (equipment.CurrentState == EquipmentState.Exhausted)
        {
            return new EquipmentUsageResult
            {
                Success = false,
                Message = $"{equipment.Name} is exhausted and needs repair ({equipment.RepairCost} coins)"
            };
        }

        // Handle based on usage type
        switch (equipment.UsageType)
        {
            case EquipmentUsageType.Permanent:
                // Permanent equipment never exhausts, always functional
                return new EquipmentUsageResult
                {
                    Success = true,
                    Message = $"Used {equipment.Name} (Permanent)",
                    UsesRemaining = -1 // Infinite uses
                };

            case EquipmentUsageType.Consumable:
                // Remove from inventory after single use
                player.Inventory.RemoveItem(equipmentId);
                return new EquipmentUsageResult
                {
                    Success = true,
                    Message = $"Used {equipment.Name} (Consumed)",
                    WasConsumed = true,
                    UsesRemaining = 0
                };

            case EquipmentUsageType.Exhaustible:
                // Increment usage and check exhaustion
                equipment.CurrentUses++;
                int usesRemaining = equipment.ExhaustAfterUses - equipment.CurrentUses;

                if (equipment.CurrentUses >= equipment.ExhaustAfterUses)
                {
                    // Equipment exhausted
                    equipment.CurrentState = EquipmentState.Exhausted;
                    return new EquipmentUsageResult
                    {
                        Success = true,
                        Message = $"Used {equipment.Name} - NOW EXHAUSTED (needs repair: {equipment.RepairCost} coins)",
                        WasExhausted = true,
                        UsesRemaining = 0
                    };
                }
                else
                {
                    return new EquipmentUsageResult
                    {
                        Success = true,
                        Message = $"Used {equipment.Name} ({usesRemaining} uses remaining)",
                        UsesRemaining = usesRemaining
                    };
                }

            default:
                return new EquipmentUsageResult
                {
                    Success = false,
                    Message = $"Unknown equipment usage type: {equipment.UsageType}"
                };
        }
    }

    /// <summary>
    /// Repair exhausted equipment (costs coins, resets usage)
    /// </summary>
    public EquipmentRepairResult RepairEquipment(string equipmentId)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify player owns this equipment
        if (!player.Inventory.HasItem(equipmentId))
        {
            return new EquipmentRepairResult
            {
                Success = false,
                Message = "Player does not own this equipment"
            };
        }

        // Get equipment from GameWorld
        Equipment equipment = _gameWorld.Items.OfType<Equipment>().FirstOrDefault(e => e.Id == equipmentId);
        if (equipment == null)
        {
            return new EquipmentRepairResult
            {
                Success = false,
                Message = $"Equipment {equipmentId} not found in GameWorld.Items"
            };
        }

        // Only Exhaustible equipment can be repaired
        if (equipment.UsageType != EquipmentUsageType.Exhaustible)
        {
            return new EquipmentRepairResult
            {
                Success = false,
                Message = $"{equipment.Name} cannot be repaired (not Exhaustible type)"
            };
        }

        // Check if equipment needs repair
        if (equipment.CurrentState == EquipmentState.Functional)
        {
            return new EquipmentRepairResult
            {
                Success = false,
                Message = $"{equipment.Name} is already functional and does not need repair"
            };
        }

        // Check if player has enough coins
        if (player.Coins < equipment.RepairCost)
        {
            return new EquipmentRepairResult
            {
                Success = false,
                Message = $"Not enough coins to repair {equipment.Name} (need {equipment.RepairCost}, have {player.Coins})"
            };
        }

        // Perform repair
        player.ModifyCoins(-equipment.RepairCost);
        equipment.CurrentUses = 0;
        equipment.CurrentState = EquipmentState.Functional;

        return new EquipmentRepairResult
        {
            Success = true,
            Message = $"Repaired {equipment.Name} for {equipment.RepairCost} coins (restored to {equipment.ExhaustAfterUses} uses)",
            CoinsPaid = equipment.RepairCost
        };
    }

    /// <summary>
    /// Get all functional equipment that matches the given obstacle context
    /// Returns only equipment player owns and is currently functional
    /// </summary>
    public List<Equipment> GetApplicableEquipment(ObstacleContext context)
    {
        Player player = _gameWorld.GetPlayer();

        // Get all equipment player owns
        List<string> ownedEquipmentIds = player.Inventory.GetAllItems();

        // Filter to functional equipment matching context
        List<Equipment> applicableEquipment = _gameWorld.Items
            .OfType<Equipment>()
            .Where(e => ownedEquipmentIds.Contains(e.Id))
            .Where(e => e.CurrentState == EquipmentState.Functional)
            .Where(e => e.MatchesContext(context))
            .ToList();

        return applicableEquipment;
    }

    /// <summary>
    /// Get equipment status (for UI display)
    /// </summary>
    public EquipmentStatus GetEquipmentStatus(string equipmentId)
    {
        Equipment equipment = _gameWorld.Items.OfType<Equipment>().FirstOrDefault(e => e.Id == equipmentId);
        if (equipment == null)
        {
            return null;
        }

        return new EquipmentStatus
        {
            EquipmentId = equipment.Id,
            Name = equipment.Name,
            UsageType = equipment.UsageType,
            CurrentState = equipment.CurrentState,
            CurrentUses = equipment.CurrentUses,
            MaxUses = equipment.ExhaustAfterUses,
            UsesRemaining = equipment.ExhaustAfterUses - equipment.CurrentUses,
            RepairCost = equipment.RepairCost,
            CanUse = equipment.CurrentState == EquipmentState.Functional,
            NeedsRepair = equipment.CurrentState == EquipmentState.Exhausted
        };
    }
}

/// <summary>
/// Result of equipment usage attempt
/// </summary>
public class EquipmentUsageResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public bool WasConsumed { get; set; } = false;
    public bool WasExhausted { get; set; } = false;
    public int UsesRemaining { get; set; } = 0;
}

/// <summary>
/// Result of equipment repair attempt
/// </summary>
public class EquipmentRepairResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int CoinsPaid { get; set; } = 0;
}

/// <summary>
/// Equipment status for UI display
/// </summary>
public class EquipmentStatus
{
    public string EquipmentId { get; set; }
    public string Name { get; set; }
    public EquipmentUsageType UsageType { get; set; }
    public EquipmentState CurrentState { get; set; }
    public int CurrentUses { get; set; }
    public int MaxUses { get; set; }
    public int UsesRemaining { get; set; }
    public int RepairCost { get; set; }
    public bool CanUse { get; set; }
    public bool NeedsRepair { get; set; }
}
