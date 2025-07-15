using Wayfarer.Game.MainSystem;

public class ContractRepository
{
    private readonly GameWorld _gameWorld;

    public ContractRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;

        if (_gameWorld.WorldState.Contracts == null)
        {
            _gameWorld.WorldState.Contracts = new List<Contract>();
        }
        if (_gameWorld.WorldState.ActiveContracts == null)
        {
            _gameWorld.WorldState.ActiveContracts = new List<Contract>();
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
        return _gameWorld.WorldState.ActiveContracts?.ToList() ?? new List<Contract>();
    }

    // ===== QUERY METHODS FOR STATE INSPECTION =====
    // These methods provide state inspection capabilities for both production and testing

    /// <summary>
    /// Get detailed completion status for a specific contract
    /// </summary>
    public ContractCompletionResult GetContractStatus(string contractId)
    {
        Console.WriteLine($"[DEBUG] ContractRepository.GetContractStatus called for contract {contractId}");
        
        // Ensure ActiveContracts is initialized before accessing
        if (_gameWorld.WorldState.ActiveContracts == null)
        {
            _gameWorld.WorldState.ActiveContracts = new List<Contract>();
        }

        // First check active contracts
        Contract contract = _gameWorld.WorldState.ActiveContracts.FirstOrDefault(c => c != null && c.Id == contractId);
        Console.WriteLine($"[DEBUG] Contract {contractId} found in active contracts: {contract != null}");
        
        if (contract == null)
        {
            // Check if contract exists in completed contracts
            contract = GetCompletedContracts().FirstOrDefault(c => c != null && c.Id == contractId);
            Console.WriteLine($"[DEBUG] Contract {contractId} found in completed contracts: {contract != null}");
        }
        
        if (contract == null)
        {
            // Check if contract exists in world state but not active or completed
            contract = GetContract(contractId);
            Console.WriteLine($"[DEBUG] Contract {contractId} found in world state: {contract != null}");
            if (contract == null)
            {
                Console.WriteLine($"[DEBUG] Contract {contractId} not found anywhere");
                return new ContractCompletionResult
                {
                    ContractId = contractId,
                    Status = ContractStatus.NotFound,
                    ProgressPercentage = 0f
                };
            }

            Console.WriteLine($"[DEBUG] Contract {contractId} status: IsCompleted={contract.IsCompleted}, IsFailed={contract.IsFailed}");
            return new ContractCompletionResult
            {
                ContractId = contractId,
                Status = contract.IsCompleted ? ContractStatus.Completed :
                        contract.IsFailed ? ContractStatus.Failed : ContractStatus.NotActive,
                ProgressPercentage = contract.IsCompleted ? 1f : 0f,
                CompletedSteps = contract.CompletionSteps.Where(s => s.IsCompleted).ToList()
            };
        }

        // Calculate progress based on completed steps
        float progressPercentage = 0f;
        if (contract.CompletionSteps.Any())
        {
            int totalSteps = contract.CompletionSteps.Count(s => s.IsRequired);
            int completedSteps = contract.CompletionSteps.Count(s => s.IsRequired && s.IsCompleted);
            progressPercentage = totalSteps > 0 ? (float)completedSteps / totalSteps : 0f;
        }

        Console.WriteLine($"[DEBUG] Contract {contractId} final status: IsCompleted={contract.IsCompleted}, Steps={contract.CompletionSteps.Count}, CompletedSteps={contract.CompletionSteps.Count(s => s.IsCompleted)}");
        
        return new ContractCompletionResult
        {
            ContractId = contractId,
            Status = contract.IsCompleted ? ContractStatus.Completed :
                    contract.IsFailed ? ContractStatus.Failed : ContractStatus.Active,
            ProgressPercentage = progressPercentage,
            CompletedSteps = contract.CompletionSteps.Where(s => s.IsCompleted).ToList()
        };
    }

    /// <summary>
    /// Check if a specific contract is available to accept
    /// </summary>
    public bool IsContractAvailable(string contractId)
    {
        Contract contract = GetContract(contractId);
        if (contract == null) return false;

        bool isAlreadyActive = _gameWorld.WorldState.ActiveContracts?.Any(c => c.Id == contractId) ?? false;
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

    public void AddActiveContract(Contract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        List<Contract> activeContracts = _gameWorld.WorldState.ActiveContracts;
        if (!activeContracts.Any(c => c.Id == contract.Id))
        {
            activeContracts.Add(contract);
        }
    }

    public void RemoveActiveContract(Contract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        _gameWorld.WorldState.ActiveContracts.Remove(contract);
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