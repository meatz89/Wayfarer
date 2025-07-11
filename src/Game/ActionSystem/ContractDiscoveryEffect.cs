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
    private readonly ContractRepository _contractRepository;
    private readonly ContractValidationService _contractValidationService;
    
    public ContractDiscoveryEffect(string npcId, ContractCategory contractCategory, int maxContractsRevealed,
                                  ContractRepository contractRepository, ContractValidationService contractValidationService)
    {
        _npcId = npcId;
        _contractCategory = contractCategory;
        _maxContractsRevealed = maxContractsRevealed;
        _contractRepository = contractRepository;
        _contractValidationService = contractValidationService;
    }

    public void Apply(EncounterState state)
    {
        Player player = state.Player;
        
        // Get all available contracts matching the category (simplified - no time filtering for now)
        List<Contract> availableContracts = _contractRepository.GetAllContracts()
            .Where(contract => 
                !contract.IsCompleted && 
                !contract.IsFailed &&
                (_contractCategory == ContractCategory.General || contract.Category == _contractCategory))
            .ToList();

        // For encounter-based contract discovery, we'll validate based on player properties only
        // Location-specific validation will be handled when the player attempts to accept contracts
        List<Contract> accessibleContracts = new List<Contract>();
        foreach (Contract contract in availableContracts)
        {
            // Basic validation without location context (encounters may be location-independent)
            ContractAccessResult accessResult = _contractValidationService.ValidateContractAccess(contract, player, "");
            if (accessResult.CanAccept)
            {
                accessibleContracts.Add(contract);
            }
        }

        // Limit to maximum contracts revealed
        List<Contract> revealedContracts = accessibleContracts.Take(_maxContractsRevealed).ToList();

        // Add contracts to player's known contracts (if not already known)
        foreach (Contract contract in revealedContracts)
        {
            player.DiscoverContract(contract.Id);
        }

        // Note: Message logging will be handled by the UI layer when contracts are discovered
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