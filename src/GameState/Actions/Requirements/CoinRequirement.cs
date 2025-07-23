namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement that checks if player has enough coins for an action
/// </summary>
public class CoinRequirement : IActionRequirement
{
    private readonly int _coinsRequired;
    
    public CoinRequirement(int coinsRequired)
    {
        _coinsRequired = coinsRequired;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        return player.Coins >= _coinsRequired;
    }
    
    public string GetDescription()
    {
        return $"{_coinsRequired} coin{(_coinsRequired > 1 ? "s" : "")}";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        return $"Not enough coins! Need {_coinsRequired}, have {player.Coins}";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        return "Work for NPCs or deliver letters to earn coins";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        if (player.Coins >= _coinsRequired) return 1.0;
        return (double)player.Coins / _coinsRequired;
    }
}