using System;

/// <summary>
/// Simple wrapper that manages a single persistent AttentionManager
/// Attention only resets on rest, not on time blocks
/// </summary>
public class TimeBlockAttentionManager
{
    private readonly AttentionManager _attention;

    public TimeBlockAttentionManager()
    {
        // Create a single attention manager that persists
        _attention = new AttentionManager();
        _attention.SetMaxAttention(7);
    }

    /// <summary>
    /// Get the current attention manager
    /// </summary>
    public AttentionManager GetCurrentAttention(TimeBlocks currentTime)
    {
        // Just return the same attention manager regardless of time
        // Attention persists until explicitly restored
        return _attention;
    }

    /// <summary>
    /// Force a refresh (e.g., after sleeping or special events)
    /// </summary>
    public void ForceRefresh()
    {
        Console.WriteLine("[Attention] Forcing attention refresh to full");
        _attention.ResetToFull();
    }

    /// <summary>
    /// Calculate the morning refresh amount based on hunger
    /// Formula: 10 - (hunger ÷ 25), minimum 2
    /// </summary>
    public int GetMorningRefreshAmount(int hunger)
    {
        // Implement the formula from documentation: 10 - (hunger ÷ 25), minimum 2
        int baseAttention = 10;
        int hungerPenalty = hunger / 25;
        int refreshAmount = Math.Max(2, baseAttention - hungerPenalty);
        
        Console.WriteLine($"[Attention] Morning refresh calculation: hunger={hunger}, penalty={hungerPenalty}, result={refreshAmount}");
        return refreshAmount;
    }

    /// <summary>
    /// Check if player has any attention left
    /// </summary>
    public bool HasAttentionRemaining()
    {
        return _attention?.GetAvailableAttention() > 0;
    }

    /// <summary>
    /// Get a summary of attention state for UI display
    /// </summary>
    public (int current, int max) GetAttentionState()
    {
        if (_attention == null)
            return (0, 7);

        return (_attention.GetAvailableAttention(), _attention.GetMaxAttention());
    }

    /// <summary>
    /// Clear all stored attention states (for new day/reset)
    /// </summary>
    public void ClearAllStates()
    {
        _attention.ResetToFull();
    }
}