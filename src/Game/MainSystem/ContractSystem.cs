using System.Text;

public class ContractSystem
{
    private GameWorld gameWorld;
    private MessageSystem messageSystem;
    private ContractRepository contractRepository;

    public ContractSystem(GameWorld gameWorld, MessageSystem messageSystem, ContractRepository contractRepository)
    {
        this.gameWorld = gameWorld;
        this.messageSystem = messageSystem;
        this.contractRepository = contractRepository;
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

        if (!contract.CanComplete(player, gameWorld.WorldState.CurrentLocation.Id))
        {
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
        gameWorld.ActiveContracts.Remove(contract);

        // Add success message
        messageSystem.AddSystemMessage($"Contract '{contract.Description}' completed! Received {finalPayment} coins. {paymentMessage}");

        return true;
    }

    public List<Contract> GetActiveContracts()
    {
        return gameWorld.ActiveContracts?.ToList() ?? new List<Contract>();
    }

    public void CheckForFailedContracts()
    {
        List<Contract> failedContracts = gameWorld.ActiveContracts
            .Where(c => c.DueDay < gameWorld.CurrentDay && !c.IsCompleted)
            .ToList();

        foreach (Contract? contract in failedContracts)
        {
            contract.IsFailed = true;
            gameWorld.ActiveContracts.Remove(contract);

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
        return gameWorld.ActiveContracts
            .Where(c => c.DueDay == gameWorld.CurrentDay)
            .ToList();
    }

    public List<Contract> GetUrgentContracts(int daysThreshold = 1)
    {
        return contractRepository.GetActiveContracts()
            .Where(c => GetDaysRemaining(c) <= daysThreshold)
            .ToList();
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
        gameWorld.ActiveContracts.Add(contract);

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
            // New completion action format
            RequiredTransactions = new List<ContractTransaction>
            {
                new ContractTransaction
                {
                    ItemId = "herbs",
                    LocationId = "dusty_flagon",
                    TransactionType = TransactionType.Sell,
                    Quantity = 1
                }
            },
            RequiredDestinations = new List<string>(),
            RequiredNPCConversations = new List<string>(),
            RequiredLocationActions = new List<string>(),
            // Initialize completion tracking
            CompletedDestinations = new HashSet<string>(),
            CompletedTransactions = new List<ContractTransaction>(),
            CompletedNPCConversations = new HashSet<string>(),
            CompletedLocationActions = new HashSet<string>()
        };
    }
}