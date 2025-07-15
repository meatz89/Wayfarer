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
    private readonly GameWorld _gameWorld;

    public ContractProgressionService(
        ContractRepository contractRepository,
        ItemRepository itemRepository,
        LocationRepository locationRepository,
        GameWorld gameWorld)
    {
        _contractRepository = contractRepository;
        _itemRepository = itemRepository;
        _locationRepository = locationRepository;
        _gameWorld = gameWorld;
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

            // NEW: Check ContractStep system first
            if (contract.CompletionSteps.Any())
            {
                LocationActionContext locationActionContext = new LocationActionContext
                {
                    ActionId = action.ActionId,
                    LocationId = action.LocationId,
                    LocationSpotId = action.LocationSpotId
                };

                progressMade = contract.CheckStepCompletion(player, action.LocationId, locationActionContext);

                // Also check for NPC conversations embedded in location actions
                if (IsNPCConversationAction(action))
                {
                    string npcId = ExtractNPCFromAction(action);
                    if (!string.IsNullOrEmpty(npcId))
                    {
                        ConversationContext conversationContext = new ConversationContext
                        {
                            NPCId = npcId,
                            LocationId = action.LocationId
                        };

                        if (contract.CheckStepCompletion(player, action.LocationId, conversationContext))
                        {
                            progressMade = true;
                        }
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

            // NEW: Check ContractStep system first
            if (contract.CompletionSteps.Any())
            {
                progressMade = contract.CheckStepCompletion(player, destinationLocationId);
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

            // NEW: Check ContractStep system first
            if (contract.CompletionSteps.Any())
            {
                TransactionContext transactionContext = new TransactionContext
                {
                    ItemId = itemId,
                    LocationId = locationId,
                    TransactionType = transactionType,
                    Quantity = quantity,
                    Price = price
                };

                progressMade = contract.CheckStepCompletion(player, locationId, transactionContext);
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
                    information.Quality >= requirement.MinimumQuality)
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

        ContractProgressInfo progressInfo = new ContractProgressInfo
        {
            ContractId = contractId,
            IsFound = true,
            IsCompleted = contract.IsFullyCompleted()
        };

        // NEW: Use ContractStep system if available
        if (contract.CompletionSteps.Any())
        {
            progressInfo.CompletionSteps = contract.GetCompletionRequirements();
            progressInfo.RemainingSteps = contract.GetRemainingRequirements();
        }

        return progressInfo;
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
    public void CheckNPCConversationProgression(string npcId, string locationId, Player player)
    {
        List<Contract> activeContracts = _contractRepository.GetActiveContracts();

        foreach (Contract contract in activeContracts)
        {
            bool progressMade = false;

            // NEW: Check ContractStep system first
            if (contract.CompletionSteps.Any())
            {
                ConversationContext conversationContext = new ConversationContext
                {
                    NPCId = npcId,
                    LocationId = locationId
                };

                progressMade = contract.CheckStepCompletion(player, locationId, conversationContext);
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
        contract.CompletedDay = _gameWorld.TimeManager.GetCurrentDay();

        // Move from active to completed
        _contractRepository.CompleteContract(contract.Id);

        // Apply contract completion rewards
        Player player = _gameWorld.GetPlayer();
        if (contract.Payment > 0)
        {
            player.ModifyCoins(contract.Payment);
        }

        // Update player reputation based on contract completion
        // Early completion provides reputation bonus
        int daysEarly = contract.DueDay - contract.CompletedDay;
        if (daysEarly > 0)
        {
            player.Reputation += daysEarly; // 1 reputation per day early
        }

        // Trigger contract completion effects - using a message system if available
        // Note: Since GameWorld doesn't have MessageSystem, we'll skip the messages for now
        // This could be enhanced by injecting MessageSystem separately
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
        // Integrate with TimeManager to get actual current day
        return _gameWorld.TimeManager.GetCurrentDay();
    }
}

/// <summary>
/// Progress information for a contract
/// </summary>
public class ContractProgressInfo
{
    public string ContractId { get; set; } = "";
    public bool IsFound { get; set; }
    public bool IsCompleted { get; set; }

    // NEW: ContractStep system progress
    public List<ContractStepRequirement> CompletionSteps { get; set; } = new();
    public List<ContractStepRequirement> RemainingSteps { get; set; } = new();
}