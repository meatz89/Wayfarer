public class ContractRepository
{
    private List<Contract> _contracts = new List<Contract>();

    public ContractRepository()
    {
        // Initialize with contracts from game state
        _contracts = GameWorld.AllContracts ?? new List<Contract>();
    }

    public List<Contract> GetAvailableContracts(int currentDay, TimeBlocks currentTimeBlock)
    {
        return _contracts
            .Where(c => c.IsAvailable(currentDay, currentTimeBlock))
            .ToList();
    }

    public Contract GetContract(string id)
    {
        return _contracts.FirstOrDefault(c => c.Id == id);
    }

    public void AddContract(Contract contract)
    {
        if (!_contracts.Any(c => c.Id == contract.Id))
        {
            _contracts.Add(contract);
            // Also add to the static list to ensure it's persisted
            if (!GameWorld.AllContracts.Any(c => c.Id == contract.Id))
            {
                GameWorld.AllContracts.Add(contract);
            }
        }
    }
}