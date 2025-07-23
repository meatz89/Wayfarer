using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState.Validation;

/// <summary>
/// Validates game state invariants to ensure consistency.
/// </summary>
public class GameStateValidator
{
    private readonly List<IStateInvariant> _invariants;
    
    public GameStateValidator()
    {
        _invariants = new List<IStateInvariant>
        {
            new TimeInvariant(),
            new ResourceInvariant(),
            new InventoryInvariant(),
            new LetterQueueInvariant(),
            new LocationInvariant()
        };
    }
    
    /// <summary>
    /// Validates all game state invariants.
    /// </summary>
    public ValidationResult ValidateGameState(GameWorld gameWorld)
    {
        var errors = new List<string>();
        
        foreach (var invariant in _invariants)
        {
            var result = invariant.Validate(gameWorld);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
    
    /// <summary>
    /// Validates a specific aspect of game state.
    /// </summary>
    public ValidationResult ValidateSpecific<TInvariant>(GameWorld gameWorld) 
        where TInvariant : IStateInvariant
    {
        var invariant = _invariants.OfType<TInvariant>().FirstOrDefault();
        if (invariant == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new[] { $"Invariant {typeof(TInvariant).Name} not found" }
            };
        }
        
        return invariant.Validate(gameWorld);
    }
}

/// <summary>
/// Interface for state invariant validators.
/// </summary>
public interface IStateInvariant
{
    string Name { get; }
    ValidationResult Validate(GameWorld gameWorld);
}

/// <summary>
/// Result of state validation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Validates time-related invariants.
/// </summary>
public class TimeInvariant : IStateInvariant
{
    public string Name => "Time State";
    
    public ValidationResult Validate(GameWorld gameWorld)
    {
        var errors = new List<string>();
        
        var timeManager = gameWorld.TimeManager;
        if (timeManager == null)
        {
            errors.Add("TimeManager is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        
        var currentHour = timeManager.CurrentTimeHours;
        if (currentHour < 0 || currentHour >= 24)
        {
            errors.Add($"Invalid hour: {currentHour}");
        }
        
        var currentDay = gameWorld.CurrentDay;
        if (currentDay < 1)
        {
            errors.Add($"Invalid day: {currentDay}");
        }
        
        var calculatedTimeBlock = timeManager.GetCurrentTimeBlock();
        var storedTimeBlock = gameWorld.CurrentTimeBlock;
        if (calculatedTimeBlock != storedTimeBlock)
        {
            errors.Add($"Time block mismatch: calculated {calculatedTimeBlock}, stored {storedTimeBlock}");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

/// <summary>
/// Validates resource-related invariants.
/// </summary>
public class ResourceInvariant : IStateInvariant
{
    public string Name => "Resource State";
    
    public ValidationResult Validate(GameWorld gameWorld)
    {
        var errors = new List<string>();
        var player = gameWorld.GetPlayer();
        
        if (player == null)
        {
            errors.Add("Player is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        
        // Validate coins
        if (player.Coins < 0)
        {
            errors.Add($"Negative coins: {player.Coins}");
        }
        
        // Validate stamina
        if (player.Stamina < 0 || player.Stamina > player.MaxStamina)
        {
            errors.Add($"Invalid stamina: {player.Stamina}/{player.MaxStamina}");
        }
        
        // Validate health
        if (player.Health < 0 || player.Health > player.MaxHealth)
        {
            errors.Add($"Invalid health: {player.Health}/{player.MaxHealth}");
        }
        
        // Validate concentration
        if (player.Concentration < 0 || player.Concentration > player.MaxConcentration)
        {
            errors.Add($"Invalid concentration: {player.Concentration}/{player.MaxConcentration}");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

/// <summary>
/// Validates inventory-related invariants.
/// </summary>
public class InventoryInvariant : IStateInvariant
{
    public string Name => "Inventory State";
    
    public ValidationResult Validate(GameWorld gameWorld)
    {
        var errors = new List<string>();
        var player = gameWorld.GetPlayer();
        
        if (player?.Inventory == null)
        {
            errors.Add("Player inventory is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        
        var inventory = player.Inventory;
        
        if (inventory.CurrentItemCount > inventory.Size)
        {
            errors.Add($"Inventory overfull: {inventory.CurrentItemCount}/{inventory.Size}");
        }
        
        if (inventory.CurrentItemCount < 0)
        {
            errors.Add($"Negative inventory count: {inventory.CurrentItemCount}");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

/// <summary>
/// Validates letter queue invariants.
/// </summary>
public class LetterQueueInvariant : IStateInvariant
{
    public string Name => "Letter Queue State";
    
    public ValidationResult Validate(GameWorld gameWorld)
    {
        var errors = new List<string>();
        var player = gameWorld.GetPlayer();
        
        if (player == null)
        {
            errors.Add("Player is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        
        var queue = player.LetterQueue;
        if (queue == null)
        {
            errors.Add("Letter queue is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
        
        if (queue.Length != 8)
        {
            errors.Add($"Invalid queue size: {queue.Length} (expected 8)");
        }
        
        // Check for duplicate letters
        var letterIds = queue.Where(l => l != null).Select(l => l.Id).ToList();
        var duplicates = letterIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        
        foreach (var duplicate in duplicates)
        {
            errors.Add($"Duplicate letter in queue: {duplicate}");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}

/// <summary>
/// Validates location-related invariants.
/// </summary>
public class LocationInvariant : IStateInvariant
{
    public string Name => "Location State";
    
    public ValidationResult Validate(GameWorld gameWorld)
    {
        var errors = new List<string>();
        
        var currentLocation = gameWorld.CurrentLocation;
        var currentSpot = gameWorld.CurrentLocationSpot;
        
        if (currentLocation == null)
        {
            errors.Add("Current location is null");
        }
        else if (currentSpot == null)
        {
            errors.Add("Current location spot is null");
        }
        else if (!currentLocation.Spots.Contains(currentSpot))
        {
            errors.Add($"Current spot '{currentSpot.Id}' not found in location '{currentLocation.Id}'");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}