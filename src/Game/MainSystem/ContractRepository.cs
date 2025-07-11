using Wayfarer.Game.MainSystem;

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
        return _gameWorld.ActiveContracts?.ToList() ?? new List<Contract>();
    }

    // ===== QUERY METHODS FOR STATE INSPECTION =====
    // These methods provide state inspection capabilities for both production and testing

    /// <summary>
    /// Get detailed completion status for a specific contract
    /// </summary>
    public ContractCompletionResult GetContractStatus(string contractId)
    {
        Contract contract = _gameWorld.ActiveContracts?.FirstOrDefault(c => c.Id == contractId);
        if (contract == null)
        {
            // Check if contract exists in world state but not active
            contract = GetContract(contractId);
            if (contract == null)
            {
                return new ContractCompletionResult 
                { 
                    ContractId = contractId, 
                    Status = ContractStatus.NotFound,
                    ProgressPercentage = 0f
                };
            }
            
            return new ContractCompletionResult 
            { 
                ContractId = contractId, 
                Status = contract.IsCompleted ? ContractStatus.Completed : 
                        contract.IsFailed ? ContractStatus.Failed : ContractStatus.NotActive,
                ProgressPercentage = contract.CalculateProgress(),
                CompletedTransactions = contract.CompletedTransactions.ToList(),
                CompletedDestinations = contract.CompletedDestinations.ToList(),
                CompletedNPCConversations = contract.CompletedNPCConversations.ToList(),
                CompletedLocationActions = contract.CompletedLocationActions.ToList()
            };
        }

        return new ContractCompletionResult 
        { 
            ContractId = contractId, 
            Status = contract.IsCompleted ? ContractStatus.Completed : 
                    contract.IsFailed ? ContractStatus.Failed : ContractStatus.Active,
            ProgressPercentage = contract.CalculateProgress(),
            CompletedTransactions = contract.CompletedTransactions.ToList(),
            CompletedDestinations = contract.CompletedDestinations.ToList(),
            CompletedNPCConversations = contract.CompletedNPCConversations.ToList(),
            CompletedLocationActions = contract.CompletedLocationActions.ToList()
        };
    }

    /// <summary>
    /// Check if a specific contract is available to accept
    /// </summary>
    public bool IsContractAvailable(string contractId)
    {
        Contract contract = GetContract(contractId);
        if (contract == null) return false;
        
        bool isAlreadyActive = _gameWorld.ActiveContracts?.Any(c => c.Id == contractId) ?? false;
        if (isAlreadyActive) return false;
        
        return contract.IsAvailable(_gameWorld.CurrentDay, _gameWorld.WorldState.CurrentTimeBlock);
    }

    /// <summary>
    /// Get contracts that are expiring soon
    /// </summary>
    public List<Contract> GetUrgentContracts(int daysThreshold = 1)
    {
        int currentDay = _gameWorld.CurrentDay;
        return GetActiveContracts()
            .Where(c => (c.DueDay - currentDay) <= daysThreshold && (c.DueDay - currentDay) >= 0)
            .ToList();
    }

    /// <summary>
    /// Get contracts expiring today
    /// </summary>
    public List<Contract> GetContractsExpiringToday()
    {
        return GetActiveContracts()
            .Where(c => c.DueDay == _gameWorld.CurrentDay)
            .ToList();
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