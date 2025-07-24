using System;


/// <summary>
/// Immutable state container for player resources with validation.
/// Ensures resource values stay within valid bounds.
/// </summary>
public sealed class PlayerResourceState
{
    // Resource limits
    public const int MIN_COINS = 0;
    public const int MAX_COINS = 999999;
    public const int MIN_STAMINA = 0;
    public const int MAX_STAMINA = 10;
    public const int MIN_HEALTH = 0;
    public const int MAX_HEALTH = 10;
    public const int MIN_CONCENTRATION = 0;
    public const int MAX_CONCENTRATION = 10;

    // Private fields
    private readonly int _coins;
    private readonly int _stamina;
    private readonly int _health;
    private readonly int _concentration;
    private readonly int _maxStamina;
    private readonly int _maxHealth;
    private readonly int _maxConcentration;

    // Public properties
    public int Coins => _coins;

    public int Stamina => _stamina;

    public int Health => _health;

    public int Concentration => _concentration;

    public int MaxStamina => _maxStamina;

    public int MaxHealth => _maxHealth;

    public int MaxConcentration => _maxConcentration;

    // Derived properties
    public bool IsExhausted => _stamina == 0;

    public bool IsHealthy => _health == _maxHealth;

    public bool IsFocused => _concentration >= 5;

    public bool CanPerformDangerousAction => _stamina >= 4;

    public bool CanPerformSocialAction => _stamina >= 3;

    public PlayerResourceState(
        int coins = 5,
        int stamina = 6,
        int health = 10,
        int concentration = 10,
        int maxStamina = MAX_STAMINA,
        int maxHealth = MAX_HEALTH,
        int maxConcentration = MAX_CONCENTRATION)
    {
        _coins = ValidateAndClamp(coins, MIN_COINS, MAX_COINS, nameof(coins));
        _stamina = ValidateAndClamp(stamina, MIN_STAMINA, maxStamina, nameof(stamina));
        _health = ValidateAndClamp(health, MIN_HEALTH, maxHealth, nameof(health));
        _concentration = ValidateAndClamp(concentration, MIN_CONCENTRATION, maxConcentration, nameof(concentration));

        _maxStamina = ValidateAndClamp(maxStamina, 1, MAX_STAMINA, nameof(maxStamina));
        _maxHealth = ValidateAndClamp(maxHealth, 1, MAX_HEALTH, nameof(maxHealth));
        _maxConcentration = ValidateAndClamp(maxConcentration, 1, MAX_CONCENTRATION, nameof(maxConcentration));
    }

    /// <summary>
    /// Creates a new state with modified coins.
    /// </summary>
    public ResourceModificationResult ModifyCoins(int delta)
    {
        int newValue = _coins + delta;
        int clampedValue = Math.Clamp(newValue, MIN_COINS, MAX_COINS);

        if (clampedValue == _coins)
        {
            return ResourceModificationResult.NoChange("Coins");
        }

        PlayerResourceState newState = new PlayerResourceState(
            clampedValue, _stamina, _health, _concentration,
            _maxStamina, _maxHealth, _maxConcentration
        );

        return ResourceModificationResult.Success(
            "Coins", _coins, clampedValue, delta, newState
        );
    }

    /// <summary>
    /// Creates a new state with modified stamina.
    /// </summary>
    public ResourceModificationResult ModifyStamina(int delta)
    {
        int newValue = _stamina + delta;
        int clampedValue = Math.Clamp(newValue, MIN_STAMINA, _maxStamina);

        if (clampedValue == _stamina)
        {
            return ResourceModificationResult.NoChange("Stamina");
        }

        PlayerResourceState newState = new PlayerResourceState(
            _coins, clampedValue, _health, _concentration,
            _maxStamina, _maxHealth, _maxConcentration
        );

        return ResourceModificationResult.Success(
            "Stamina", _stamina, clampedValue, delta, newState
        );
    }

    /// <summary>
    /// Creates a new state with modified health.
    /// </summary>
    public ResourceModificationResult ModifyHealth(int delta)
    {
        int newValue = _health + delta;
        int clampedValue = Math.Clamp(newValue, MIN_HEALTH, _maxHealth);

        if (clampedValue == _health)
        {
            return ResourceModificationResult.NoChange("Health");
        }

        PlayerResourceState newState = new PlayerResourceState(
            _coins, _stamina, clampedValue, _concentration,
            _maxStamina, _maxHealth, _maxConcentration
        );

        return ResourceModificationResult.Success(
            "Health", _health, clampedValue, delta, newState
        );
    }

    /// <summary>
    /// Creates a new state with modified concentration.
    /// </summary>
    public ResourceModificationResult ModifyConcentration(int delta)
    {
        int newValue = _concentration + delta;
        int clampedValue = Math.Clamp(newValue, MIN_CONCENTRATION, _maxConcentration);

        if (clampedValue == _concentration)
        {
            return ResourceModificationResult.NoChange("Concentration");
        }

        PlayerResourceState newState = new PlayerResourceState(
            _coins, _stamina, _health, clampedValue,
            _maxStamina, _maxHealth, _maxConcentration
        );

        return ResourceModificationResult.Success(
            "Concentration", _concentration, clampedValue, delta, newState
        );
    }

    /// <summary>
    /// Creates a new state with full restoration of all resources.
    /// </summary>
    public PlayerResourceState RestoreFully()
    {
        if (_stamina == _maxStamina && _health == _maxHealth && _concentration == _maxConcentration)
        {
            return this; // Already fully restored
        }

        return new PlayerResourceState(
            _coins, _maxStamina, _maxHealth, _maxConcentration,
            _maxStamina, _maxHealth, _maxConcentration
        );
    }

    /// <summary>
    /// Applies categorical stamina recovery based on lodging type.
    /// </summary>
    public ResourceModificationResult ApplyLodgingRecovery(string lodgingType)
    {
        int recoveryAmount = lodgingType?.ToLower() switch
        {
            "rough" => 2,
            "common" => 4,
            "private" => 6,
            "noble" or "noble_invitation" => 8,
            _ => 2
        };

        return ModifyStamina(recoveryAmount);
    }

    private static int ValidateAndClamp(int value, int min, int max, string paramName)
    {
        if (min > max)
            throw new ArgumentException($"Min ({min}) cannot be greater than max ({max})");

        return Math.Clamp(value, min, max);
    }

    public override string ToString()
    {
        return $"Coins: {_coins}, Stamina: {_stamina}/{_maxStamina}, Health: {_health}/{_maxHealth}, Concentration: {_concentration}/{_maxConcentration}";
    }
}

/// <summary>
/// Result of a resource modification operation.
/// </summary>
public class ResourceModificationResult
{
    public bool Changed { get; init; }
    public string ResourceName { get; init; }
    public int OldValue { get; init; }
    public int NewValue { get; init; }
    public int Delta { get; init; }
    public PlayerResourceState NewState { get; init; }

    public static ResourceModificationResult NoChange(string resourceName)
    {
        return new ResourceModificationResult
        {
            Changed = false,
            ResourceName = resourceName
        };
    }

    public static ResourceModificationResult Success(
        string resourceName,
        int oldValue,
        int newValue,
        int requestedDelta,
        PlayerResourceState newState)
    {
        return new ResourceModificationResult
        {
            Changed = true,
            ResourceName = resourceName,
            OldValue = oldValue,
            NewValue = newValue,
            Delta = newValue - oldValue,
            NewState = newState
        };
    }
}