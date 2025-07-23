namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement that checks if player has enough time (hours) for an action
/// </summary>
public class TimeRequirement : IActionRequirement
{
    private readonly int _hoursRequired;
    
    public TimeRequirement(int hoursRequired)
    {
        _hoursRequired = hoursRequired;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        return world.TimeManager.HoursRemaining >= _hoursRequired;
    }
    
    public string GetDescription()
    {
        return $"{_hoursRequired} hour{(_hoursRequired > 1 ? "s" : "")}";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        var remaining = world.TimeManager.HoursRemaining;
        return $"Not enough time! Need {_hoursRequired} hour{(_hoursRequired > 1 ? "s" : "")}, have {remaining}";
    }
    
    public bool CanBeRemedied => false; // Can't add more hours to the day
    
    public string GetRemediationHint()
    {
        return "Rest until tomorrow to get more hours";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        var remaining = world.TimeManager.HoursRemaining;
        if (remaining >= _hoursRequired) return 1.0;
        return (double)remaining / _hoursRequired;
    }
}