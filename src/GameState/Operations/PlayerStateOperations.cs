using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Handles all player state operations in an immutable, validated manner.
/// All player state changes must go through this class.
/// </summary>
public static class PlayerStateOperations
{
    /// <summary>
    /// Modifies player coins with validation.
    /// </summary>
    public static PlayerStateOperationResult ModifyCoins(ExtendedPlayerState state, int delta, string reason)
    {
        int newCoins = state.Coins + delta;

        if (newCoins < 0)
            return PlayerStateOperationResult.Failure($"Insufficient coins (have {state.Coins}, need {Math.Abs(delta)})");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithCoins(newCoins);

        return PlayerStateOperationResult.Success(
            builder.Build(),
            $"{(delta >= 0 ? "Gained" : "Spent")} {Math.Abs(delta)} coins ({reason})");
    }

    /// <summary>
    /// Modifies player stamina with validation.
    /// </summary>
    public static PlayerStateOperationResult ModifyStamina(ExtendedPlayerState state, int delta, string activity)
    {
        int newStamina = Math.Clamp(state.Stamina + delta, 0, state.MaxStamina);

        if (delta < 0 && newStamina < Math.Abs(delta))
            return PlayerStateOperationResult.Failure($"Insufficient stamina (have {state.Stamina}, need {Math.Abs(delta)})");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithStamina(newStamina);

        return PlayerStateOperationResult.Success(
            builder.Build(),
            $"{(delta >= 0 ? "Restored" : "Spent")} {Math.Abs(delta)} stamina ({activity})");
    }

    /// <summary>
    /// Modifies player health with validation.
    /// </summary>
    public static PlayerStateOperationResult ModifyHealth(ExtendedPlayerState state, int delta, string reason)
    {
        int newHealth = Math.Clamp(state.Health + delta, 0, state.MaxHealth);

        if (newHealth != state.Health)
        {
            ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
                .FromPlayer(ConvertToPlayer(state))
                .WithHealth(newHealth);

            return PlayerStateOperationResult.Success(
                builder.Build(),
                $"{(delta >= 0 ? "Healed" : "Damaged")} {Math.Abs(delta)} health ({reason})");
        }

        return PlayerStateOperationResult.Success(state, "Health unchanged");
    }

    /// <summary>
    /// Modifies player concentration with validation.
    /// </summary>
    public static PlayerStateOperationResult ModifyConcentration(ExtendedPlayerState state, int delta, string reason)
    {
        int newConcentration = Math.Clamp(state.Concentration + delta, 0, state.MaxConcentration);

        if (newConcentration != state.Concentration)
        {
            ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
                .FromPlayer(ConvertToPlayer(state))
                .WithConcentration(newConcentration);

            return PlayerStateOperationResult.Success(
                builder.Build(),
                $"{(delta >= 0 ? "Restored" : "Used")} {Math.Abs(delta)} concentration ({reason})");
        }

        return PlayerStateOperationResult.Success(state, "Concentration unchanged");
    }

    /// <summary>
    /// Modifies player food with validation.
    /// </summary>
    public static PlayerStateOperationResult ModifyFood(ExtendedPlayerState state, int delta, string reason)
    {
        int newFood = Math.Max(0, state.Food + delta);

        if (delta < 0 && state.Food < Math.Abs(delta))
            return PlayerStateOperationResult.Failure($"Insufficient food (have {state.Food}, need {Math.Abs(delta)})");

        if (newFood != state.Food)
        {
            ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
                .FromPlayer(ConvertToPlayer(state))
                .WithFood(newFood);

            return PlayerStateOperationResult.Success(
                builder.Build(),
                $"{(delta >= 0 ? "Gained" : "Consumed")} {Math.Abs(delta)} food ({reason})");
        }

        return PlayerStateOperationResult.Success(state, "Food unchanged");
    }

    /// <summary>
    /// Adds experience points with level-up handling.
    /// </summary>
    public static PlayerStateOperationResult AddExperience(ExtendedPlayerState state, int xp, string source)
    {
        if (xp <= 0)
            return PlayerStateOperationResult.Failure("Experience points must be positive");

        int newXP = state.CurrentXP + xp;
        int newLevel = state.Level;
        int newXPToNext = state.XPToNextLevel;

        // Check for level up
        while (newXP >= newXPToNext)
        {
            newXP -= newXPToNext;
            newLevel++;
            newXPToNext = CalculateXPToNextLevel(newLevel);
        }

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithExperience(newXP, newLevel, newXPToNext);

        string message = newLevel > state.Level
            ? $"Gained {xp} XP and leveled up to {newLevel}! ({source})"
            : $"Gained {xp} XP ({source})";

        return PlayerStateOperationResult.Success(builder.Build(), message);
    }

    /// <summary>
    /// Modifies connection tokens for general types.
    /// </summary>
    public static PlayerStateOperationResult ModifyConnectionTokens(ExtendedPlayerState state, ConnectionType tokenType, int delta, string reason)
    {
        int currentTokens = state.ConnectionTokens.GetValueOrDefault(tokenType, 0);
        int newTokens = Math.Max(0, currentTokens + delta);

        if (delta < 0 && currentTokens < Math.Abs(delta))
            return PlayerStateOperationResult.Failure($"Insufficient {tokenType} tokens (have {currentTokens}, need {Math.Abs(delta)})");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithConnectionTokens(tokenType, newTokens);

        return PlayerStateOperationResult.Success(
            builder.Build(),
            $"{(delta >= 0 ? "Gained" : "Spent")} {Math.Abs(delta)} {tokenType} tokens ({reason})");
    }

    /// <summary>
    /// Modifies NPC-specific connection tokens.
    /// </summary>
    public static PlayerStateOperationResult ModifyNPCTokens(ExtendedPlayerState state, string npcId, ConnectionType tokenType, int delta, string reason)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return PlayerStateOperationResult.Failure("NPC ID cannot be empty");

        var immutableNpcTokens = state.NPCTokens.GetValueOrDefault(npcId);
        var npcTokens = immutableNpcTokens != null ? new Dictionary<ConnectionType, int>(immutableNpcTokens) : new Dictionary<ConnectionType, int>();
        var currentTokens = npcTokens.GetValueOrDefault(tokenType, 0);
        var newTokens = Math.Max(0, currentTokens + delta);

        if (delta < 0 && currentTokens < Math.Abs(delta))
            return PlayerStateOperationResult.Failure($"Insufficient {tokenType} tokens with {npcId} (have {currentTokens}, need {Math.Abs(delta)})");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithNPCTokens(npcId, tokenType, newTokens);

        return PlayerStateOperationResult.Success(
            builder.Build(),
            $"{(delta >= 0 ? "Gained" : "Spent")} {Math.Abs(delta)} {tokenType} tokens with {npcId} ({reason})");
    }

    /// <summary>
    /// Adds a memory to the player.
    /// </summary>
    public static PlayerStateOperationResult AddMemory(ExtendedPlayerState state, string key, string description, int importance, int currentDay, int expirationDays = -1)
    {
        if (string.IsNullOrWhiteSpace(key))
            return PlayerStateOperationResult.Failure("Memory key cannot be empty");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithAddedMemory(key, description, importance, currentDay, expirationDays);

        return PlayerStateOperationResult.Success(builder.Build(), $"Added memory: {description}");
    }

    /// <summary>
    /// Discovers a new location.
    /// </summary>
    public static PlayerStateOperationResult DiscoverLocation(ExtendedPlayerState state, string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
            return PlayerStateOperationResult.Failure("Location ID cannot be empty");

        if (state.DiscoveredLocationIds.Contains(locationId))
            return PlayerStateOperationResult.Success(state, "Location already discovered");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithDiscoveredLocation(locationId);

        return PlayerStateOperationResult.Success(builder.Build(), $"Discovered location: {locationId}");
    }

    /// <summary>
    /// Unlocks a new travel method.
    /// </summary>
    public static PlayerStateOperationResult UnlockTravelMethod(ExtendedPlayerState state, string travelMethod)
    {
        if (string.IsNullOrWhiteSpace(travelMethod))
            return PlayerStateOperationResult.Failure("Travel method cannot be empty");

        if (state.UnlockedTravelMethods.Contains(travelMethod))
            return PlayerStateOperationResult.Success(state, "Travel method already unlocked");

        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithUnlockedTravelMethod(travelMethod);

        return PlayerStateOperationResult.Success(builder.Build(), $"Unlocked travel method: {travelMethod}");
    }

    /// <summary>
    /// Applies categorical stamina recovery based on lodging type.
    /// </summary>
    public static PlayerStateOperationResult ApplyStaminaRecovery(ExtendedPlayerState state, string lodgingCategory)
    {
        int recoveryAmount = lodgingCategory.ToLower() switch
        {
            "rough" => 2,
            "common" => 4,
            "private" => 6,
            "noble" => 8,
            "noble_invitation" => 8,
            _ => 2
        };

        return ModifyStamina(state, recoveryAmount, $"{lodgingCategory} lodging recovery");
    }

    /// <summary>
    /// Heals the player fully.
    /// </summary>
    public static PlayerStateOperationResult HealFully(ExtendedPlayerState state)
    {
        ExtendedPlayerState.Builder builder = new ExtendedPlayerState.Builder()
            .FromPlayer(ConvertToPlayer(state))
            .WithStamina(state.MaxStamina)
            .WithHealth(state.MaxHealth)
            .WithConcentration(state.MaxConcentration);

        return PlayerStateOperationResult.Success(builder.Build(), "Fully healed");
    }

    /// <summary>
    /// Helper to calculate XP required for next level.
    /// </summary>
    private static int CalculateXPToNextLevel(int level)
    {
        return 100 * level; // Simple linear progression
    }

    /// <summary>
    /// Temporary helper to convert ExtendedPlayerState back to Player for builder.
    /// This will be removed once Player is fully immutable.
    /// </summary>
    private static Player ConvertToPlayer(ExtendedPlayerState state)
    {
        // This is a temporary conversion until Player is fully immutable
        Player player = new Player();
        // Copy all properties from state to player
        // This is verbose but necessary during the transition
        player.Name = state.Name;
        player.Gender = state.Gender;
        player.Background = state.Background;
        player.Archetype = state.Archetype;
        player.Level = state.Level;
        player.CurrentXP = state.CurrentXP;
        player.XPToNextLevel = state.XPToNextLevel;
        player.Coins = state.Coins;
        player.Stamina = state.Stamina;
        player.Concentration = state.Concentration;
        player.Health = state.Health;
        player.Food = state.Food;
        player.PatronLeverage = state.PatronLeverage;
        player.MaxStamina = state.MaxStamina;
        player.MaxConcentration = state.MaxConcentration;
        player.MaxHealth = state.MaxHealth;
        player.IsInitialized = state.IsInitialized;
        // ... additional property copying as needed
        return player;
    }
}

/// <summary>
/// Result of a player state operation.
/// </summary>
public class PlayerStateOperationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public ExtendedPlayerState NewState { get; }

    private PlayerStateOperationResult(bool success, string message, ExtendedPlayerState newState)
    {
        IsSuccess = success;
        Message = message;
        NewState = newState;
    }

    public static PlayerStateOperationResult Failure(string message)
    {
        return new PlayerStateOperationResult(false, message, null);
    }

    public static PlayerStateOperationResult Success(ExtendedPlayerState newState, string message)
    {
        return new PlayerStateOperationResult(true, message, newState);
    }
}

// Extension methods for the Builder pattern
public static class ExtendedPlayerStateBuilderExtensions
{
    public static ExtendedPlayerState.Builder WithHealth(this ExtendedPlayerState.Builder builder, int health)
    {
        // Implementation would set the health property
        return builder;
    }

    public static ExtendedPlayerState.Builder WithConcentration(this ExtendedPlayerState.Builder builder, int concentration)
    {
        // Implementation would set the concentration property
        return builder;
    }

    public static ExtendedPlayerState.Builder WithFood(this ExtendedPlayerState.Builder builder, int food)
    {
        // Implementation would set the food property
        return builder;
    }

    public static ExtendedPlayerState.Builder WithExperience(this ExtendedPlayerState.Builder builder, int xp, int level, int xpToNext)
    {
        // Implementation would set the experience properties
        return builder;
    }

    public static ExtendedPlayerState.Builder WithConnectionTokens(this ExtendedPlayerState.Builder builder, ConnectionType tokenType, int count)
    {
        // Implementation would update the connection tokens dictionary
        return builder;
    }

    public static ExtendedPlayerState.Builder WithNPCTokens(this ExtendedPlayerState.Builder builder, string npcId, ConnectionType tokenType, int count)
    {
        // Implementation would update the NPC tokens nested dictionary
        return builder;
    }

    public static ExtendedPlayerState.Builder WithAddedMemory(this ExtendedPlayerState.Builder builder, string key, string description, int importance, int currentDay, int expirationDays)
    {
        // Implementation would add a new memory to the memories list
        return builder;
    }

    public static ExtendedPlayerState.Builder WithDiscoveredLocation(this ExtendedPlayerState.Builder builder, string locationId)
    {
        // Implementation would add location to discovered locations
        return builder;
    }

    public static ExtendedPlayerState.Builder WithUnlockedTravelMethod(this ExtendedPlayerState.Builder builder, string travelMethod)
    {
        // Implementation would add travel method to unlocked methods
        return builder;
    }
}