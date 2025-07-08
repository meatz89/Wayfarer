using System.Text;

public class ContractSystem
{
    private GameWorld gameWorld;
    private MessageSystem messageSystem;

    public ContractSystem(GameWorld gameWorld, MessageSystem messageSystem)
    {
        this.gameWorld = gameWorld;
        this.messageSystem = messageSystem;
    }

    public string FormatActiveContracts(List<Contract> contracts)
    {
        StringBuilder sb = new StringBuilder();

        if (contracts == null || !contracts.Any())
            return "None";

        foreach (Contract? contract in contracts)
        {
            sb.AppendLine($"- {contract.Id}: {contract.Description} (at {contract.DestinationLocation})");
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

        // Remove required items from inventory
        foreach (string requiredItem in contract.RequiredItems)
        {
            player.Inventory.RemoveItem(requiredItem);
        }

        // Calculate payment with early/late delivery adjustments
        int currentDay = gameWorld.WorldState.CurrentDay;
        contract.CompletedDay = currentDay;
        int daysDifference = currentDay - contract.DueDay;
        
        int finalPayment = contract.Payment;
        string paymentMessage;
        
        if (daysDifference < 0) // Early delivery
        {
            int daysEarly = Math.Abs(daysDifference);
            int bonus = (int)(contract.Payment * 0.2f * daysEarly); // 20% bonus per day early
            finalPayment += bonus;
            paymentMessage = $"Early delivery bonus: +{bonus} coins";
            
            // Reputation bonus for early delivery
            player.Reputation += daysEarly;
        }
        else if (daysDifference > 0) // Late delivery
        {
            int daysLate = daysDifference;
            int penalty = (int)(contract.Payment * 0.5f * daysLate); // 50% penalty per day late
            finalPayment = Math.Max(1, finalPayment - penalty); // Minimum 1 coin
            paymentMessage = $"Late delivery penalty: -{penalty} coins";
            
            // Reputation penalty for late delivery
            player.Reputation -= daysLate * 2;
        }
        else // On-time delivery
        {
            paymentMessage = "On-time delivery";
            player.Reputation += 1; // Small reputation bonus for reliability
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
        var failedContracts = gameWorld.ActiveContracts
            .Where(c => c.DueDay < gameWorld.WorldState.CurrentDay && !c.IsCompleted)
            .ToList();

        foreach (var contract in failedContracts)
        {
            contract.IsFailed = true;
            gameWorld.ActiveContracts.Remove(contract);
            
            // Apply reputation penalty for failed contracts
            Player player = gameWorld.GetPlayer();
            player.Reputation -= 5; // Significant reputation penalty
            
            messageSystem.AddSystemMessage($"Contract '{contract.Description}' failed! {contract.FailurePenalty} (-5 reputation)");
        }
    }

    public int GetDaysRemaining(Contract contract)
    {
        return Math.Max(0, contract.DueDay - gameWorld.WorldState.CurrentDay);
    }

    public List<Contract> GetContractsExpiringToday()
    {
        return gameWorld.ActiveContracts
            .Where(c => c.DueDay == gameWorld.WorldState.CurrentDay)
            .ToList();
    }

    public List<Contract> GetUrgentContracts(int daysThreshold = 1)
    {
        return gameWorld.ActiveContracts
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
        if (!contract.IsAvailable(gameWorld.WorldState.CurrentDay, gameWorld.WorldState.CurrentTimeWindow))
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
        int currentDay = gameWorld.WorldState.CurrentDay;
        int difficultyScaling = Math.Max(1, currentDay / 5); // Increases every 5 days
        
        int baseDuration = 5 - Math.Min(2, difficultyScaling); // Shorter deadlines as game progresses
        int basePayment = 10 + (difficultyScaling * 5); // Higher rewards as game progresses
        
        return new Contract
        {
            Id = $"generated_{currentDay}_{Guid.NewGuid().ToString("N")[0..8]}",
            Description = $"Generated delivery contract (Day {currentDay})",
            RequiredItems = new List<string> { "herbs" }, // Simple for now
            DestinationLocation = "dusty_flagon",
            StartDay = currentDay,
            DueDay = currentDay + baseDuration,
            Payment = basePayment,
            FailurePenalty = "Loss of reputation",
            IsCompleted = false,
            IsFailed = false
        };
    }
}