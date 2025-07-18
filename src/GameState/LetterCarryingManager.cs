using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the physical carrying of letters in the player's inventory.
/// Letters in the queue must be picked up and carried to be delivered.
/// Letters compete with items for inventory space based on their size.
/// </summary>
public class LetterCarryingManager
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly MessageSystem _messageSystem;
    
    public LetterCarryingManager(GameWorld gameWorld, ItemRepository itemRepository, MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _messageSystem = messageSystem;
    }
    
    /// <summary>
    /// Check if a letter can be picked up based on inventory capacity
    /// </summary>
    public bool CanPickUpLetter(Letter letter, TravelMethods? currentTransport = null)
    {
        if (letter == null) return false;
        
        var player = _gameWorld.GetPlayer();
        int requiredSlots = letter.GetRequiredSlots();
        int availableSlots = GetAvailableCarryingCapacity(currentTransport);
        
        return requiredSlots <= availableSlots;
    }
    
    /// <summary>
    /// Pick up a letter from the queue into physical inventory
    /// </summary>
    public bool PickUpLetter(Letter letter, TravelMethods? currentTransport = null)
    {
        if (!CanPickUpLetter(letter, currentTransport))
        {
            _messageSystem.AddSystemMessage(
                $"Cannot pick up letter: Not enough carrying capacity. Need {letter.GetRequiredSlots()} slots.",
                SystemMessageTypes.Warning
            );
            return false;
        }
        
        var player = _gameWorld.GetPlayer();
        
        // Check physical constraints
        if (letter.HasPhysicalProperty(LetterPhysicalProperties.Fragile) && currentTransport == TravelMethods.Horseback)
        {
            _messageSystem.AddSystemMessage(
                "Warning: Fragile letter may be damaged during horseback travel!",
                SystemMessageTypes.Warning
            );
        }
        
        if (letter.RequiredEquipment.HasValue && !HasRequiredEquipment(letter.RequiredEquipment.Value))
        {
            _messageSystem.AddSystemMessage(
                $"Cannot pick up letter: Requires {letter.RequiredEquipment.Value.ToString().Replace("_", " ")}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        player.CarriedLetters.Add(letter);
        _messageSystem.AddSystemMessage(
            $"Picked up {letter.Size} letter from {letter.SenderName} ({letter.GetRequiredSlots()} slots)",
            SystemMessageTypes.Success
        );
        
        return true;
    }
    
    /// <summary>
    /// Drop a carried letter (returns to queue or is lost)
    /// </summary>
    public void DropLetter(Letter letter)
    {
        var player = _gameWorld.GetPlayer();
        if (!player.CarriedLetters.Contains(letter))
        {
            return;
        }
        
        player.CarriedLetters.Remove(letter);
        _messageSystem.AddSystemMessage(
            $"Dropped letter from {letter.SenderName}. It returns to your queue.",
            SystemMessageTypes.Info
        );
    }
    
    /// <summary>
    /// Get total carrying capacity for letters
    /// </summary>
    public int GetTotalCarryingCapacity(TravelMethods? currentTransport = null)
    {
        var player = _gameWorld.GetPlayer();
        return player.Inventory.GetMaxSlots(_itemRepository, currentTransport);
    }
    
    /// <summary>
    /// Get available carrying capacity (total - used by items and letters)
    /// </summary>
    public int GetAvailableCarryingCapacity(TravelMethods? currentTransport = null)
    {
        var player = _gameWorld.GetPlayer();
        
        // Get total capacity
        int totalCapacity = GetTotalCarryingCapacity(currentTransport);
        
        // Subtract slots used by items
        int itemSlots = player.Inventory.GetUsedSlots(_itemRepository);
        
        // Subtract slots used by carried letters
        int letterSlots = player.CarriedLetters.Sum(l => l.GetRequiredSlots());
        
        return totalCapacity - itemSlots - letterSlots;
    }
    
    /// <summary>
    /// Get movement penalties from carried letters
    /// </summary>
    public List<string> GetMovementPenalties()
    {
        var penalties = new List<string>();
        var player = _gameWorld.GetPlayer();
        
        // Check for heavy letters
        bool hasHeavyLetters = player.CarriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy));
        if (hasHeavyLetters)
        {
            penalties.Add("Heavy letters: +1 stamina cost");
        }
        
        // Check for bulky letters
        bool hasBulkyLetters = player.CarriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Bulky));
        if (hasBulkyLetters)
        {
            penalties.Add("Bulky letters: movement restricted");
        }
        
        // Check for perishable letters
        bool hasPerishableLetters = player.CarriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Perishable));
        if (hasPerishableLetters)
        {
            penalties.Add("Perishable letters: time-sensitive");
        }
        
        return penalties;
    }
    
    /// <summary>
    /// Get slots used by carried letters
    /// </summary>
    public int GetUsedLetterSlots()
    {
        var player = _gameWorld.GetPlayer();
        return player.CarriedLetters.Sum(l => l.GetRequiredSlots());
    }
    
    /// <summary>
    /// Check if player has required equipment for a letter
    /// </summary>
    private bool HasRequiredEquipment(ItemCategory requiredEquipment)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check each item in inventory for the required equipment category
        foreach (var itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            var item = _itemRepository.GetItemById(itemId);
            if (item?.Categories?.Contains(requiredEquipment) == true)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Validate carried letters for physical constraints
    /// </summary>
    public List<string> ValidateCarriedLetters(TravelMethods transport, WeatherCondition weather)
    {
        var warnings = new List<string>();
        var player = _gameWorld.GetPlayer();
        
        foreach (var letter in player.CarriedLetters)
        {
            // Check fragile letters
            if (letter.HasPhysicalProperty(LetterPhysicalProperties.Fragile))
            {
                if (transport == TravelMethods.Horseback || transport == TravelMethods.Cart)
                {
                    warnings.Add($"{letter.SenderName}'s letter is fragile - risk of damage!");
                }
            }
            
            // Check weather protection
            if (letter.HasPhysicalProperty(LetterPhysicalProperties.RequiresProtection))
            {
                if (weather == WeatherCondition.Rain && !HasWaterproofContainer())
                {
                    warnings.Add($"{letter.SenderName}'s letter needs waterproof protection!");
                }
            }
            
            // Check heavy letters
            if (letter.HasPhysicalProperty(LetterPhysicalProperties.Heavy))
            {
                if (transport == TravelMethods.Walking)
                {
                    warnings.Add($"{letter.SenderName}'s letter is heavy - will slow movement!");
                }
            }
            
            // Check perishable letters
            if (letter.HasPhysicalProperty(LetterPhysicalProperties.Perishable))
            {
                warnings.Add($"{letter.SenderName}'s letter is perishable - deadline decreasing faster!");
            }
        }
        
        return warnings;
    }
    
    /// <summary>
    /// Check if player has waterproof container
    /// </summary>
    private bool HasWaterproofContainer()
    {
        return HasRequiredEquipment(ItemCategory.Weather_Protection);
    }
    
    /// <summary>
    /// Calculate movement penalty from carried letters
    /// </summary>
    public int GetMovementPenalty()
    {
        var player = _gameWorld.GetPlayer();
        int penalty = 0;
        
        foreach (var letter in player.CarriedLetters)
        {
            if (letter.HasPhysicalProperty(LetterPhysicalProperties.Heavy))
            {
                penalty += 1; // Each heavy letter adds 1 to travel time
            }
            
            if (letter.Size == LetterSize.Large)
            {
                penalty += 1; // Large letters are cumbersome
            }
        }
        
        return penalty;
    }
    
    /// <summary>
    /// Get a summary of carried letters for UI display
    /// </summary>
    public string GetCarriedLettersSummary()
    {
        var player = _gameWorld.GetPlayer();
        if (!player.CarriedLetters.Any())
        {
            return "No letters carried";
        }
        
        int count = player.CarriedLetters.Count;
        int slots = GetUsedLetterSlots();
        return $"Carrying {count} letter{(count > 1 ? "s" : "")} ({slots} slot{(slots > 1 ? "s" : "")})";
    }
}