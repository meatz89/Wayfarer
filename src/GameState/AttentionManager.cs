using System;

/// <summary>
/// Manages the attention point system for conversations and interactions.
/// Attention is consumed by actions and restored by rest/food/drink.
/// </summary>
public class AttentionManager
{
    public const int BASE_ATTENTION = 7;

    private int _currentAttention;
    private int _maxAttention = BASE_ATTENTION;

    /// <summary>
    /// Current available attention points
    /// </summary>
    public int Current
    {
        get => _currentAttention;
        private set => _currentAttention = Math.Max(0, value);
    }

    /// <summary>
    /// Maximum attention points available
    /// </summary>
    public int Max => _maxAttention;

    public AttentionManager()
    {
        // Start at full attention
        _currentAttention = _maxAttention;
    }

    /// <summary>
    /// Check if player has enough attention for a choice
    /// </summary>
    public bool CanAfford(int cost)
    {
        return cost <= _currentAttention;
    }

    /// <summary>
    /// Spend attention points on a choice
    /// </summary>
    public bool TrySpend(int cost)
    {
        if (!CanAfford(cost))
            return false;

        _currentAttention -= cost;
        return true;
    }

    /// <summary>
    /// Reset attention to full (after rest/sleep)
    /// </summary>
    public void ResetToFull()
    {
        _currentAttention = _maxAttention;
    }

    /// <summary>
    /// Get current available attention
    /// </summary>
    public int GetAvailableAttention()
    {
        return _currentAttention;
    }

    /// <summary>
    /// Get maximum attention
    /// </summary>
    public int GetMaxAttention()
    {
        return _maxAttention;
    }

    /// <summary>
    /// Set maximum attention
    /// </summary>
    public void SetMaxAttention(int max)
    {
        _maxAttention = max;
        _currentAttention = max;  // Always start at full when setting max
    }

    /// <summary>
    /// Restore attention (from eating, drinking, etc)
    /// </summary>
    public void RestoreAttention(int amount)
    {
        _currentAttention = Math.Min(_currentAttention + amount, _maxAttention);
    }

    /// <summary>
    /// Add attention points (for coin-based refresh or special items)
    /// Can exceed normal maximum
    /// </summary>
    public void AddAttention(int amount)
    {
        // Allow exceeding normal maximum for special cases
        int newAttention = _currentAttention + amount;
        _currentAttention = Math.Min(newAttention, GameRules.ATTENTION_REFRESH_MAX_TOTAL);
    }
    
    /// <summary>
    /// Set attention to a specific value (e.g., from lodging/rest).
    /// </summary>
    public void SetAttention(int value)
    {
        // Set to specific value, capped at maximum
        _currentAttention = Math.Min(value, GameRules.ATTENTION_REFRESH_MAX_TOTAL);
    }

    /// <summary>
    /// Get the narrative description of current attention state
    /// </summary>
    public string GetNarrativeDescription()
    {
        return _currentAttention switch
        {
            >= 6 => "Your mind is clear and focused, ready to absorb every detail.",
            >= 4 => "You remain attentive, though some of your focus has been spent.",
            >= 2 => "Your concentration wavers. You must choose your focus carefully.",
            1 => "Mental fatigue clouds your thoughts. You can only respond simply.",
            0 => "You are exhausted and cannot focus on anything complex.",
            _ => "Your attention drifts."
        };
    }

    /// <summary>
    /// Get description for a specific attention cost
    /// </summary>
    public static string GetCostDescription(int cost)
    {
        return cost switch
        {
            0 => "A simple response",
            1 => "Requires focus",
            2 => "Demands concentration",
            3 => "Requires deep thought",
            _ => "Unknown mental effort"
        };
    }
}