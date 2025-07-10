public class ContractRepository
{
    private readonly GameWorld _gameWorld;

    public ContractRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;

        if (_gameWorld.WorldState.Contracts == null)
        {
            // Initialize contracts collection if null
            System.Reflection.FieldInfo? contractsField = typeof(WorldState).GetField("Contracts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (contractsField != null)
            {
                contractsField.SetValue(_gameWorld.WorldState, new List<Contract>());
            }
        }
    }

    #region Read Methods

    public List<Contract> GetAvailableContracts(int currentDay, TimeBlocks currentTimeBlock)
    {
        return GetAllContracts()
            .Where(c => c.IsAvailable(currentDay, currentTimeBlock))
            .ToList();
    }

    public Contract GetContract(string id)
    {
        return GetAllContracts().FirstOrDefault(c => c.Id == id);
    }

    public List<Contract> GetAllContracts()
    {
        return _gameWorld.WorldState.Contracts ?? new List<Contract>();
    }

    public List<Contract> GetActiveContracts()
    {
        return _gameWorld.WorldState.ActiveContracts ?? new List<Contract>();
    }

    public List<Contract> GetCompletedContracts()
    {
        return _gameWorld.WorldState.CompletedContracts ?? new List<Contract>();
    }

    public List<Contract> GetFailedContracts()
    {
        return _gameWorld.WorldState.FailedContracts ?? new List<Contract>();
    }

    #endregion

    #region Write Methods

    public void AddContract(Contract contract)
    {
        List<Contract> contracts = GetAllContracts();
        if (!contracts.Any(c => c.Id == contract.Id))
        {
            contracts.Add(contract);
        }
        else
        {
            throw new InvalidOperationException($"Contract with ID '{contract.Id}' already exists.");
        }
    }

    public bool RemoveContract(string id)
    {
        List<Contract> contracts = GetAllContracts();
        Contract contract = contracts.FirstOrDefault(c => c.Id == id);
        if (contract != null)
        {
            return contracts.Remove(contract);
        }
        return false;
    }

    public void ActivateContract(string contractId)
    {
        Contract contract = GetContract(contractId);
        if (contract == null)
        {
            throw new InvalidOperationException($"Contract with ID '{contractId}' not found.");
        }

        List<Contract> activeContracts = _gameWorld.WorldState.ActiveContracts;
        if (!activeContracts.Any(c => c.Id == contractId))
        {
            activeContracts.Add(contract);
        }
    }

    public void CompleteContract(string contractId)
    {
        // Remove from active
        List<Contract> activeContracts = _gameWorld.WorldState.ActiveContracts;
        Contract contract = activeContracts.FirstOrDefault(c => c.Id == contractId);
        if (contract != null)
        {
            activeContracts.Remove(contract);
            _gameWorld.WorldState.CompletedContracts.Add(contract);
        }
    }

    public void FailContract(string contractId)
    {
        // Remove from active
        List<Contract> activeContracts = _gameWorld.WorldState.ActiveContracts;
        Contract contract = activeContracts.FirstOrDefault(c => c.Id == contractId);
        if (contract != null)
        {
            activeContracts.Remove(contract);
            _gameWorld.WorldState.FailedContracts.Add(contract);
        }
    }

    public void ClearAllContracts()
    {
        GetAllContracts().Clear();
        _gameWorld.WorldState.ActiveContracts.Clear();
        _gameWorld.WorldState.CompletedContracts.Clear();
        _gameWorld.WorldState.FailedContracts.Clear();
    }

    #endregion
}