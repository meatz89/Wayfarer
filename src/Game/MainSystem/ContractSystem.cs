using System.Text;
using Wayfarer.Game.MainSystem;

public class ContractSystem
{
    private GameWorld gameWorld;
    private MessageSystem messageSystem;
    private ContractRepository contractRepository;
    private LocationRepository locationRepository;
    private ItemRepository itemRepository;
    private ContractGenerator contractGenerator;

    public ContractSystem(GameWorld gameWorld, MessageSystem messageSystem, ContractRepository contractRepository, LocationRepository locationRepository, ItemRepository itemRepository)
    {
        this.gameWorld = gameWorld;
        this.messageSystem = messageSystem;
        this.contractRepository = contractRepository;
        this.locationRepository = locationRepository;
        this.itemRepository = itemRepository;
        
        // Initialize contract generator with templates from repository
        List<Contract> contractTemplates = contractRepository.GetAllContracts();
        this.contractGenerator = new ContractGenerator(contractTemplates, contractRepository);
    }

    public string FormatActiveContracts(List<Contract> contracts)
    {
        StringBuilder sb = new StringBuilder();

        if (contracts == null || !contracts.Any())
            return "None";

        foreach (Contract? contract in contracts)
        {
            sb.AppendLine($"- {contract.Id}: {contract.Description}");
        }

        return sb.ToString();
    }

    public bool CompleteContract(Contract contract)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player has enough time blocks
        if (!gameWorld.TimeManager.ValidateTimeBlockAction(1))
        {
            messageSystem.AddSystemMessage("Not enough time blocks to complete contract");
            return false;
        }

        if (!contract.CanComplete(player, locationRepository.GetCurrentLocation().Id, itemRepository))
        {
            return false;
        }

        // Check if all required contract steps are completed
        if (!contract.IsFullyCompleted())
        {
            messageSystem.AddSystemMessage("Contract cannot be completed - some steps are still incomplete");
            return false;
        }

        // Consume time block for contract completion
        gameWorld.TimeManager.ConsumeTimeBlock(1);

        // NOTE: Items are no longer removed here - they are consumed during completion actions
        // (e.g., selling items for delivery contracts is handled by MarketManager)
        // This follows the "only check completion actions" principle

        // Natural market dynamics: urgent contracts pay more upfront
        // No arbitrary bonuses/penalties - reputation affects future opportunities
        int currentDay = gameWorld.CurrentDay;
        contract.CompletedDay = currentDay;
        int daysDifference = currentDay - contract.DueDay;

        // Payment is fixed by market demand when contract is offered
        int finalPayment = contract.Payment;
        string paymentMessage = "Contract completed";

        // Track reliability for future contract opportunities (not price modifiers)
        if (daysDifference <= 0) // On-time or early
        {
            player.Reputation += 1; // Builds trust for better contracts
            if (daysDifference < 0)
            {
                paymentMessage = "Early delivery - merchant impressed";
            }
        }
        else // Late delivery
        {
            player.Reputation -= daysDifference; // Affects future contract availability
            paymentMessage = "Late delivery - merchant disappointed";
        }

        // Award payment
        player.ModifyCoins(finalPayment);

        // Mark contract as completed
        contract.IsCompleted = true;

        // Remove from active contracts
        contractRepository.RemoveActiveContract(contract);

        // If this contract unlocks others, make them available
        if (contract.UnlocksContractIds.Any())
        {
            foreach (string contractId in contract.UnlocksContractIds)
            {
                Contract? unlockedContract = contractRepository.GetContract(contractId);
                if (unlockedContract != null)
                {
                    unlockedContract.StartDay = gameWorld.CurrentDay;
                    unlockedContract.DueDay = gameWorld.CurrentDay + 5; // Arbitrary due date
                    contractRepository.AddActiveContract(unlockedContract);
                    messageSystem.AddSystemMessage($"New contract unlocked: {unlockedContract.Description}");
                }
            }
        }

        // Add success message
        messageSystem.AddSystemMessage($"Contract '{contract.Description}' completed! Received {finalPayment} coins. {paymentMessage}");

        return true;
    }

    public List<Contract> GetActiveContracts()
    {
        return contractRepository.GetActiveContracts();
    }

    public void CheckForFailedContracts()
    {
        List<Contract> failedContracts = contractRepository.GetActiveContracts()
            .Where(c => c.DueDay < gameWorld.CurrentDay && !c.IsCompleted)
            .ToList();

        foreach (Contract? contract in failedContracts)
        {
            contract.IsFailed = true;
            contractRepository.RemoveActiveContract(contract);

            // Natural consequences: failed contracts affect future opportunities
            Player player = gameWorld.GetPlayer();
            player.Reputation -= 3; // Affects future contract availability, not arbitrary penalty

            messageSystem.AddSystemMessage($"Contract '{contract.Description}' failed! {contract.FailurePenalty}");
        }
    }

    public int GetDaysRemaining(Contract contract)
    {
        return Math.Max(0, contract.DueDay - gameWorld.CurrentDay);
    }

    public List<Contract> GetContractsExpiringToday()
    {
        return contractRepository.GetActiveContracts()
            .Where(c => c.DueDay == gameWorld.CurrentDay)
            .ToList();
    }

    public List<Contract> GetUrgentContracts(int daysThreshold = 1)
    {
        return contractRepository.GetActiveContracts()
            .Where(c => GetDaysRemaining(c) <= daysThreshold)
            .ToList();
    }

    /// <summary>
    /// Check all active contracts for step completion based on player's current action
    /// Should be called after player performs actions like traveling, trading, etc.
    /// </summary>
    public void CheckContractStepCompletion(Player player, string currentLocationId, object actionContext = null)
    {
        List<Contract> activeContracts = contractRepository.GetActiveContracts();
        
        foreach (Contract contract in activeContracts)
        {
            bool anyStepCompleted = contract.CheckStepCompletion(player, currentLocationId, actionContext, itemRepository);
            
            if (anyStepCompleted)
            {
                // Notify player about progress
                int completedSteps = contract.CompletionSteps.Count(s => s.IsCompleted);
                int totalSteps = contract.CompletionSteps.Count(s => s.IsRequired);
                
                if (contract.IsFullyCompleted())
                {
                    messageSystem.AddSystemMessage($"Contract '{contract.Description}' - All steps completed! You can now complete the contract.");
                }
                else
                {
                    messageSystem.AddSystemMessage($"Contract '{contract.Description}' - Step completed ({completedSteps}/{totalSteps})");
                }
            }
        }
    }

    public bool AcceptContract(Contract contract)
    {
        // Check if player has enough time blocks
        if (!gameWorld.TimeManager.ValidateTimeBlockAction(1))
        {
            messageSystem.AddSystemMessage("Not enough time blocks to accept contract");
            return false;
        }

        // Check if contract is available at current time
        if (!contract.IsAvailable(gameWorld.CurrentDay, gameWorld.CurrentTimeBlock))
        {
            messageSystem.AddSystemMessage("Contract not available at this time");
            return false;
        }

        // Consume time block for contract acceptance
        gameWorld.TimeManager.ConsumeTimeBlock(1);

        // Add to active contracts
        contractRepository.AddActiveContract(contract);

        messageSystem.AddSystemMessage($"Accepted contract: {contract.Description}");
        return true;
    }

    public Contract GenerateContract()
    {
        // Generate difficulty-scaled contract based on current day
        int currentDay = gameWorld.CurrentDay;
        int difficultyScaling = Math.Max(1, currentDay / 5); // Increases every 5 days

        int baseDuration = 5 - Math.Min(2, difficultyScaling); // Shorter deadlines as game progresses
        int basePayment = 10 + (difficultyScaling * 5); // Higher rewards as game progresses

        return new Contract
        {
            Id = $"generated_{currentDay}_{Guid.NewGuid().ToString("N")[0..8]}",
            Description = $"Generated delivery contract (Day {currentDay})",
            StartDay = currentDay,
            DueDay = currentDay + baseDuration,
            Payment = basePayment,
            FailurePenalty = "Loss of reputation",
            IsCompleted = false,
            IsFailed = false,
        };
    }

    // === RENEWABLE CONTRACT GENERATION SYSTEM ===

    /// <summary>
    /// Refresh daily contracts by generating new contracts from NPCs and removing expired ones
    /// </summary>
    public void RefreshDailyContracts()
    {
        int currentDay = gameWorld.CurrentDay;
        
        // Remove expired contracts that haven't been accepted
        RemoveExpiredContracts();
        
        // Generate new contracts from NPCs
        GenerateContractsFromNPCs(currentDay);
        
        messageSystem.AddSystemMessage($"Contract offerings refreshed for day {currentDay}");
    }

    /// <summary>
    /// Generate renewable contracts from all NPCs based on their contract categories
    /// </summary>
    private void GenerateContractsFromNPCs(int currentDay)
    {
        List<NPC> npcs = gameWorld.WorldState.NPCs ?? new List<NPC>();
        
        foreach (NPC npc in npcs)
        {
            if (npc.ContractCategories != null && npc.ContractCategories.Any())
            {
                List<Contract> newContracts = contractGenerator.GenerateRenewableContracts(npc, currentDay);
                
                foreach (Contract contract in newContracts)
                {
                    try
                    {
                        contractRepository.AddContract(contract);
                    }
                    catch (InvalidOperationException)
                    {
                        // Contract already exists - skip (prevents duplicates)
                        continue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Remove contracts that have expired and were never accepted
    /// </summary>
    private void RemoveExpiredContracts()
    {
        int currentDay = gameWorld.CurrentDay;
        List<Contract> allContracts = contractRepository.GetAllContracts();
        
        List<Contract> expiredContracts = allContracts
            .Where(c => !c.IsCompleted && !c.IsFailed && c.DueDay < currentDay)
            .Where(c => !contractRepository.GetActiveContracts().Any(ac => ac.Id == c.Id))
            .ToList();

        foreach (Contract expiredContract in expiredContracts)
        {
            contractRepository.RemoveContract(expiredContract.Id);
        }
    }

    /// <summary>
    /// Get available contracts from a specific NPC
    /// </summary>
    public List<Contract> GetAvailableContractsFromNPC(string npcId)
    {
        int currentDay = gameWorld.CurrentDay;
        TimeBlocks currentTimeBlock = gameWorld.WorldState.CurrentTimeBlock;
        
        return contractRepository.GetAvailableContracts(currentDay, currentTimeBlock)
            .Where(contract => contract.Id.StartsWith(npcId))
            .ToList();
    }

    /// <summary>
    /// Check if contract generation is working properly (for testing/debugging)
    /// </summary>
    public ContractGenerationStatus GetContractGenerationStatus()
    {
        List<NPC> npcsWithContracts = gameWorld.WorldState.NPCs?
            .Where(npc => npc.ContractCategories?.Any() == true)
            .ToList() ?? new List<NPC>();

        List<Contract> availableContracts = contractRepository.GetAvailableContracts(
            gameWorld.CurrentDay, 
            gameWorld.WorldState.CurrentTimeBlock);

        return new ContractGenerationStatus
        {
            NPCsWithContractCategories = npcsWithContracts.Count,
            TotalAvailableContracts = availableContracts.Count,
            ContractTemplatesLoaded = contractGenerator.GetContractTemplates().Count,
            LastRefreshDay = gameWorld.CurrentDay
        };
    }
}

/// <summary>
/// Status information about contract generation system
/// </summary>
public class ContractGenerationStatus
{
    public int NPCsWithContractCategories { get; set; }
    public int TotalAvailableContracts { get; set; }
    public int ContractTemplatesLoaded { get; set; }
    public int LastRefreshDay { get; set; }
}