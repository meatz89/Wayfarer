using Wayfarer.Game.MainSystem;

namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Effect that reveals available contracts from an NPC based on player's current capabilities
/// </summary>
public class ContractDiscoveryEffect : IMechanicalEffect
{
    private readonly string _npcId;
    private readonly ContractCategory _contractCategory;
    private readonly int _maxContractsRevealed;
    
    public ContractDiscoveryEffect(string npcId, ContractCategory contractCategory, int maxContractsRevealed = 3)
    {
        _npcId = npcId;
        _contractCategory = contractCategory;
        _maxContractsRevealed = maxContractsRevealed;
    }

    public void Apply(EncounterState state)
    {
        // For now, this is a placeholder implementation
        // In a full implementation, this would:
        // 1. Access ContractRepository to get available contracts
        // 2. Filter contracts by category and player capabilities  
        // 3. Reveal contracts to player through game state or UI
        // 4. Log discovery message for player feedback
        
        string categoryName = _contractCategory.ToString().Replace("_", " ");
        
        // TODO: Implement contract discovery logic when ContractRepository access is available
        // This would integrate with the action system to reveal contracts based on:
        // - Player's social standing (can access contract category?)
        // - Player's current equipment (can potentially complete contracts?)
        // - Player's known information (understands contract requirements?)
    }
    
    public string GetDescriptionForPlayer()
    {
        string categoryName = _contractCategory.ToString().Replace("_", " ");
        return $"Discover available {categoryName} contracts from {_npcId}";
    }
}

/// <summary>
/// Data class for defining contract discovery effects in action templates
/// </summary>
public class ContractDiscoveryEffectData
{
    public string NPCId { get; set; } = "";
    public ContractCategory ContractCategory { get; set; } = ContractCategory.General;
    public int MaxContractsRevealed { get; set; } = 3;
    public bool RequiresSocialStanding { get; set; } = false;
    public SocialRequirement MinimumSocialStanding { get; set; } = SocialRequirement.Any;
    
    public ContractDiscoveryEffectData() { }
    
    public ContractDiscoveryEffectData(string npcId, ContractCategory category, int maxContracts = 3)
    {
        NPCId = npcId;
        ContractCategory = category;
        MaxContractsRevealed = maxContracts;
    }
}