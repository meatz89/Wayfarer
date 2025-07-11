using Wayfarer.Content;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Game.MainSystem;

/// <summary>
/// Service responsible for automatically detecting when basic actions complete contract requirements
/// Implements the core principle that contracts are completed through normal gameplay actions
/// </summary>
public class ContractProgressionService
{
    private readonly ContractRepository _contractRepository;
    private readonly ItemRepository _itemRepository;
    private readonly LocationRepository _locationRepository;

    public ContractProgressionService(
        ContractRepository contractRepository, 
        ItemRepository itemRepository,
        LocationRepository locationRepository)
    {
        _contractRepository = contractRepository;
        _itemRepository = itemRepository;
        _locationRepository = locationRepository;
    }

    /// <summary>
    /// Check if performing a location action progresses any active contracts
    /// </summary>
    public void CheckLocationActionProgression(LocationAction action, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;
            
            // Check if this action is required by the contract
            if (contract.RequiredLocationActions.Contains(action.ActionId))
            {
                if (!contract.CompletedLocationActions.Contains(action.ActionId))
                {
                    contract.CompletedLocationActions.Add(action.ActionId);
                    progressMade = true;
                }
            }
            
            // Check if action involves talking to required NPCs
            if (IsNPCConversationAction(action))
            {
                string npcId = ExtractNPCFromAction(action);
                if (!string.IsNullOrEmpty(npcId) && contract.RequiredNPCConversations.Contains(npcId))
                {
                    if (!contract.CompletedNPCConversations.Contains(npcId))
                    {
                        contract.CompletedNPCConversations.Add(npcId);
                        progressMade = true;
                    }
                }
            }
            
            // Check for contract completion
            if (progressMade && contract.IsFullyCompleted())
            {
                CompleteContract(contract);
            }
        }
    }

    /// <summary>
    /// Check if traveling to a location progresses any active contracts
    /// </summary>
    public void CheckTravelProgression(string destinationLocationId, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;
            
            // Check if this destination is required by the contract
            if (contract.RequiredDestinations.Contains(destinationLocationId))
            {
                if (!contract.CompletedDestinations.Contains(destinationLocationId))
                {
                    contract.CompletedDestinations.Add(destinationLocationId);
                    progressMade = true;
                }
            }
            
            
            // Check for contract completion
            if (progressMade && contract.IsFullyCompleted())
            {
                CompleteContract(contract);
            }
        }
    }

    /// <summary>
    /// Check if a market transaction progresses any active contracts
    /// </summary>
    public void CheckMarketProgression(string itemId, string locationId, TransactionType transactionType, 
                                     int quantity, int price, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;
            
            // Check transaction requirements
            foreach (ContractTransaction requirement in contract.RequiredTransactions)
            {
                if (requirement.Matches(itemId, locationId, transactionType, quantity, price))
                {
                    // Check if this transaction is already completed
                    bool alreadyCompleted = contract.CompletedTransactions.Any(completed =>
                        completed.ItemId == requirement.ItemId &&
                        completed.LocationId == requirement.LocationId &&
                        completed.TransactionType == requirement.TransactionType);
                    
                    if (!alreadyCompleted)
                    {
                        contract.CompletedTransactions.Add(new ContractTransaction(
                            itemId, locationId, transactionType, quantity));
                        progressMade = true;
                    }
                }
            }
            
            // Check for contract completion
            if (progressMade && contract.IsFullyCompleted())
            {
                CompleteContract(contract);
            }
        }
    }

    /// <summary>
    /// Check if gaining information progresses any active contracts
    /// </summary>
    public void CheckInformationProgression(Information information, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;
            
            // Check information requirements
            foreach (InformationRequirementData requirement in contract.RequiredInformation)
            {
                if (information.Type == requirement.RequiredType &&
                    information.Quality >= requirement.MinimumQuality &&
                    information.Freshness >= requirement.MinimumFreshness)
                {
                    // Information requirement satisfied
                    progressMade = true;
                    // Note: Information is tracked in player.KnownInformation, 
                    // contract validation will check this automatically
                }
            }
            
            // Check for contract completion
            if (progressMade && contract.IsFullyCompleted())
            {
                CompleteContract(contract);
            }
        }
    }

    /// <summary>
    /// Get progress information for a specific contract
    /// </summary>
    public ContractProgressInfo GetContractProgress(string contractId)
    {
        Contract contract = _contractRepository.GetContract(contractId);
        if (contract == null)
            return new ContractProgressInfo { ContractId = contractId, IsFound = false };
        
        return new ContractProgressInfo
        {
            ContractId = contractId,
            IsFound = true,
            ProgressPercentage = contract.CalculateProgress(),
            IsCompleted = contract.IsFullyCompleted(),
            CompletedDestinations = contract.CompletedDestinations.ToList(),
            RemainingDestinations = contract.RequiredDestinations.Except(contract.CompletedDestinations).ToList(),
            CompletedTransactions = contract.CompletedTransactions.ToList(),
            RemainingTransactions = contract.RequiredTransactions.Except(contract.CompletedTransactions).ToList(),
            CompletedNPCConversations = contract.CompletedNPCConversations.ToList(),
            RemainingNPCConversations = contract.RequiredNPCConversations.Except(contract.CompletedNPCConversations).ToList(),
            CompletedLocationActions = contract.CompletedLocationActions.ToList(),
            RemainingLocationActions = contract.RequiredLocationActions.Except(contract.CompletedLocationActions).ToList()
        };
    }

    /// <summary>
    /// Get progress information for all active contracts
    /// </summary>
    public List<ContractProgressInfo> GetAllActiveContractProgress()
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        return activeContracts.Select(contract => GetContractProgress(contract.Id)).ToList();
    }

    /// <summary>
    /// Check if having a conversation with an NPC progresses any active contracts
    /// </summary>
    public void CheckNPCConversationProgression(string npcId, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;
            
            // Check if this NPC conversation is required by the contract
            if (contract.RequiredNPCConversations.Contains(npcId))
            {
                if (!contract.CompletedNPCConversations.Contains(npcId))
                {
                    contract.CompletedNPCConversations.Add(npcId);
                    progressMade = true;
                }
            }
            
            // Check for contract completion
            if (progressMade && contract.IsFullyCompleted())
            {
                CompleteContract(contract);
            }
        }
    }

    private void CompleteContract(Contract contract)
    {
        // Mark contract as completed
        contract.IsCompleted = true;
        contract.CompletedDay = GetCurrentDay(); // TODO: Get from TimeManager
        
        // Move from active to completed
        _contractRepository.CompleteContract(contract.Id);
        
        // TODO: Apply contract completion rewards
        // TODO: Update player reputation
        // TODO: Trigger contract completion effects
    }

    private bool IsNPCConversationAction(LocationAction action)
    {
        // Check if action name/description indicates NPC conversation
        string actionName = action.Name.ToLower();
        return actionName.Contains("talk") || actionName.Contains("speak") || 
               actionName.Contains("discuss") || actionName.Contains("negotiate");
    }

    private string ExtractNPCFromAction(LocationAction action)
    {
        // Extract NPC ID from action based on location spot or action ID
        // This is a simplified implementation - could be enhanced with better NPC detection
        if (action.LocationSpotId.Contains("innkeeper"))
            return "innkeeper";
        if (action.LocationSpotId.Contains("trader") || action.LocationSpotId.Contains("merchant"))
            return "market_trader";
        if (action.LocationSpotId.Contains("guard"))
            return "town_guard";
        
        // Default to location spot ID for NPC identification
        return action.LocationSpotId;
    }

    private int GetCurrentDay()
    {
        // TODO: Integrate with TimeManager to get actual current day
        return 1;
    }
}

/// <summary>
/// Progress information for a contract
/// </summary>
public class ContractProgressInfo
{
    public string ContractId { get; set; } = "";
    public bool IsFound { get; set; }
    public float ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    
    // Destination progress
    public List<string> CompletedDestinations { get; set; } = new();
    public List<string> RemainingDestinations { get; set; } = new();
    
    // Transaction progress
    public List<ContractTransaction> CompletedTransactions { get; set; } = new();
    public List<ContractTransaction> RemainingTransactions { get; set; } = new();
    
    // NPC conversation progress
    public List<string> CompletedNPCConversations { get; set; } = new();
    public List<string> RemainingNPCConversations { get; set; } = new();
    
    // Location action progress
    public List<string> CompletedLocationActions { get; set; } = new();
    public List<string> RemainingLocationActions { get; set; } = new();
}