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
public const int MIN_HUNGER = 0;
public const int MAX_HUNGER = 100;

// Private fields
private readonly int _coins;
private readonly int _stamina;
private readonly int _health;
private readonly int _hunger;
private readonly int _maxStamina;
private readonly int _maxHealth;
private readonly int _maxHunger;

// Public properties
public int Coins => _coins;

public int Stamina => _stamina;

public int Health => _health;

public int Hunger => _hunger;

public int MaxStamina => _maxStamina;

public int MaxHealth => _maxHealth;

public int MaxHunger => _maxHunger;

// Derived properties
public bool IsExhausted => _stamina == 0;

public bool IsHealthy => _health == _maxHealth;

public bool CanPerformDangerousAction => _stamina >= 4;

public bool CanPerformSocialAction => _stamina >= 3;

public PlayerResourceState(
    int coins = 5,
    int stamina = 6,
    int health = 10,
    int hunger = 0,
    int maxStamina = MAX_STAMINA,
    int maxHealth = MAX_HEALTH,
    int maxHunger = MAX_HUNGER)
{
    _coins = ValidateAndClamp(coins, MIN_COINS, MAX_COINS, nameof(coins));
    _stamina = ValidateAndClamp(stamina, MIN_STAMINA, maxStamina, nameof(stamina));
    _health = ValidateAndClamp(health, MIN_HEALTH, maxHealth, nameof(health));
    _hunger = ValidateAndClamp(hunger, MIN_HUNGER, maxHunger, nameof(hunger));

    _maxStamina = ValidateAndClamp(maxStamina, 1, MAX_STAMINA, nameof(maxStamina));
    _maxHealth = ValidateAndClamp(maxHealth, 1, MAX_HEALTH, nameof(maxHealth));
    _maxHunger = ValidateAndClamp(maxHunger, 1, MAX_HUNGER, nameof(maxHunger));
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
        clampedValue, _stamina, _health, _hunger,
        _maxStamina, _maxHealth, _maxHunger
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
        _coins, clampedValue, _health, _hunger,
        _maxStamina, _maxHealth, _maxHunger
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
        _coins, _stamina, clampedValue, _hunger,
        _maxStamina, _maxHealth, _maxHunger
    );

    return ResourceModificationResult.Success(
        "Health", _health, clampedValue, delta, newState
    );
}

/// <summary>
/// Creates a new state with modified hunger.
/// </summary>
public ResourceModificationResult ModifyHunger(int delta)
{
    int newValue = _hunger + delta;
    int clampedValue = Math.Clamp(newValue, MIN_HUNGER, _maxHunger);

    if (clampedValue == _hunger)
    {
        return ResourceModificationResult.NoChange("Hunger");
    }

    PlayerResourceState newState = new PlayerResourceState(
        _coins, _stamina, _health, clampedValue,
        _maxStamina, _maxHealth, _maxHunger
    );

    return ResourceModificationResult.Success(
        "Hunger", _hunger, clampedValue, delta, newState
    );
}

/// <summary>
/// Creates a new state with full restoration of all resources.
/// </summary>
public PlayerResourceState RestoreFully()
{
    if (_stamina == _maxStamina && _health == _maxHealth)
    {
        return this; // Already fully restored
    }

    return new PlayerResourceState(
        _coins, _maxStamina, _maxHealth, 0,
        _maxStamina, _maxHealth, _maxHunger
    );
}

/// <summary>
/// Applies categorical stamina recovery based on lodging type.
/// </summary>
public ResourceModificationResult ApplyLodgingRecovery(string lodgingType)
{
    if (string.IsNullOrEmpty(lodgingType))
        lodgingType = "rough";

    int recoveryAmount = lodgingType.ToLower() switch
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
    return $"Coins: {_coins}, Stamina: {_stamina}/{_maxStamina}, Health: {_health}/{_maxHealth}, Hunger: {_hunger}/{_maxHunger}";
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