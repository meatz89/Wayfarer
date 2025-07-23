namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement that checks if player has enough stamina for an action
/// </summary>
public class StaminaRequirement : IActionRequirement
{
    private readonly int _staminaRequired;
    
    public StaminaRequirement(int staminaRequired)
    {
        _staminaRequired = staminaRequired;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        return player.Stamina >= _staminaRequired;
    }
    
    public string GetDescription()
    {
        return $"{_staminaRequired} stamina";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        return $"Not enough stamina! Need {_staminaRequired}, have {player.Stamina}";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        return "Rest to recover stamina or eat food";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        if (player.Stamina >= _staminaRequired) return 1.0;
        return (double)player.Stamina / _staminaRequired;
    }
}