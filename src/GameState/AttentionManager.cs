using System;

/// <summary>
/// Manages the attention point system for conversations and deep interactions.
/// Players have 3 attention points per scene to spend on meaningful choices.
/// </summary>
public class AttentionManager
{
    public const int MAX_ATTENTION = 3;

    private int _currentAttention;
    private int _totalSpentThisScene;

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
            _currentAttention = Math.Max(0, Math.Min(value, MAX_ATTENTION));
        }
    }

    /// <summary>
    /// Maximum attention points available
    /// </summary>
    public int Max => MAX_ATTENTION;

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
    /// Reset attention for a new scene/conversation
    /// </summary>
    public void ResetForNewScene()
    {
        _currentAttention = MAX_ATTENTION;
        _totalSpentThisScene = 0;
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