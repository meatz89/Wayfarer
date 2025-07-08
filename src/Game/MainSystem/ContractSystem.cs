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
        
        if (!contract.CanComplete(player, gameWorld.CurrentLocation.Id))
        {
            return false;
        }

        // Remove required items from inventory
        foreach (string requiredItem in contract.RequiredItems)
        {
            player.Inventory.RemoveItem(requiredItem);
        }

        // Award payment
        player.ModifyCoins(contract.Payment);
        
        // Mark contract as completed
        contract.IsCompleted = true;
        
        // Remove from active contracts
        gameWorld.ActiveContracts.Remove(contract);
        
        // Add success message
        messageSystem.AddSystemMessage($"Contract '{contract.Description}' completed! Received {contract.Payment} coins.");
        
        return true;
    }

    public List<Contract> GetActiveContracts()
    {
        return gameWorld.ActiveContracts?.ToList() ?? new List<Contract>();
    }

    public void CheckForFailedContracts()
    {
        var failedContracts = gameWorld.ActiveContracts
            .Where(c => c.DueDay < gameWorld.CurrentDay && !c.IsCompleted)
            .ToList();

        foreach (var contract in failedContracts)
        {
            contract.IsFailed = true;
            gameWorld.ActiveContracts.Remove(contract);
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
        return gameWorld.ActiveContracts
            .Where(c => GetDaysRemaining(c) <= daysThreshold)
            .ToList();
    }
}