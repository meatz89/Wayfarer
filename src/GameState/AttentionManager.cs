using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the attention point system for conversations and deep interactions.
/// Players have 3 attention points per scene to spend on meaningful choices.
/// Location tags dynamically modify available attention.
/// </summary>
public class AttentionManager
{
    public const int BASE_ATTENTION = 3;

    private int _currentAttention;
    private int _totalSpentThisScene;
    private int _maxAttentionThisScene;
    private int _atmosphereModifier = 0;

    /// <summary>
    /// Current available attention points
    /// </summary>
    public int Current
    {
        get
        {
            return _currentAttention;
        }

        private set
        {
            _currentAttention = Math.Max(0, Math.Min(value, _maxAttentionThisScene));
        }
    }

    /// <summary>
    /// Maximum attention points available (dynamically calculated from location tags)
    /// </summary>
    public int Max => _maxAttentionThisScene;

    /// <summary>
    /// Total attention spent in the current scene
    /// </summary>
    public int TotalSpentThisScene => _totalSpentThisScene;

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
        _totalSpentThisScene += cost;
        return true;
    }

    /// <summary>
    /// Reset attention for a new TIME BLOCK (not per conversation anymore!)
    /// This should only be called when time advances to a new period
    /// </summary>
    public void ResetForNewScene()
    {
        _atmosphereModifier = 0;
        _maxAttentionThisScene = CalculateMaxAttention();
        _currentAttention = _maxAttentionThisScene;
        _totalSpentThisScene = 0;
    }

    /// <summary>
    /// Reset attention with atmosphere modifier from NPC presence
    /// Note: This should only happen on time block changes now
    /// </summary>
    public void ResetForNewScene(int atmosphereModifier)
    {
        _atmosphereModifier = atmosphereModifier;
        _maxAttentionThisScene = CalculateMaxAttention();
        _currentAttention = _maxAttentionThisScene;
        _totalSpentThisScene = 0;
    }

    /// <summary>
    /// Get current available attention without resetting
    /// </summary>
    public int GetAvailableAttention()
    {
        return _currentAttention;
    }

    /// <summary>
    /// Get maximum attention for this time block
    /// </summary>
    public int GetMaxAttention()
    {
        return _maxAttentionThisScene;
    }

    /// <summary>
    /// Set maximum attention (used by TimeBlockAttentionManager)
    /// </summary>
    public void SetMaxAttention(int max)
    {
        _maxAttentionThisScene = Math.Clamp(max, 1, 7);
        if (_currentAttention > _maxAttentionThisScene)
            _currentAttention = _maxAttentionThisScene;
    }

    /// <summary>
    /// Calculate maximum attention based on atmosphere
    /// </summary>
    private int CalculateMaxAttention()
    {
        // Simple calculation: base attention + atmosphere modifier
        // Clamped between 1 and 5 for game balance
        return Math.Max(1, Math.Min(5, BASE_ATTENTION + _atmosphereModifier));
    }

    /// <summary>
    /// Restore some attention (rare, for special circumstances)
    /// </summary>
    public void RestoreAttention(int amount)
    {
        Current = _currentAttention + amount;
    }

    /// <summary>
    /// Get the narrative description of current attention state
    /// </summary>
    public string GetNarrativeDescription()
    {
        return _currentAttention switch
        {
            3 => "Your mind is clear and focused, ready to absorb every detail.",
            2 => "You remain attentive, though some of your focus has been spent.",
            1 => "Your concentration wavers. You must choose your focus carefully.",
            0 => "Mental fatigue clouds your thoughts. You can only respond simply.",
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
            2 => "Demands deep concentration",
            3 => "Exhausts your full attention",
            _ => "Unknown mental effort"
        };
    }
}