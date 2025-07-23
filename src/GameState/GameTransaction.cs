using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState;

/// <summary>
/// Represents a transaction containing multiple game operations that execute atomically
/// </summary>
public class GameTransaction
{
    private readonly List<IGameOperation> _operations = new();
    private readonly List<IGameOperation> _completed = new();
    private readonly GameWorld _gameWorld;
    
    public GameTransaction(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }
    
    /// <summary>
    /// Adds an operation to the transaction
    /// </summary>
    public GameTransaction AddOperation(IGameOperation operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));
        _operations.Add(operation);
        return this;
    }
    
    /// <summary>
    /// Validates whether all operations in the transaction can execute
    /// </summary>
    public TransactionValidationResult Validate()
    {
        var failedOperations = new List<string>();
        
        foreach (var operation in _operations)
        {
            if (!operation.CanExecute(_gameWorld))
            {
                failedOperations.Add(operation.Description);
            }
        }
        
        return new TransactionValidationResult
        {
            IsValid = !failedOperations.Any(),
            FailedOperations = failedOperations
        };
    }
    
    /// <summary>
    /// Executes all operations in the transaction atomically
    /// </summary>
    public TransactionResult Execute()
    {
        // First validate all operations
        var validation = Validate();
        if (!validation.IsValid)
        {
            return TransactionResult.ValidationFailure(validation.FailedOperations);
        }
        
        try
        {
            // Execute each operation
            foreach (var operation in _operations)
            {
                operation.Execute(_gameWorld);
                _completed.Add(operation);
            }
            
            return TransactionResult.Success(_operations.Select(o => o.Description).ToList());
        }
        catch (Exception ex)
        {
            // Rollback completed operations in reverse order
            foreach (var operation in _completed.AsEnumerable().Reverse())
            {
                try
                {
                    operation.Rollback(_gameWorld);
                }
                catch (Exception rollbackEx)
                {
                    // Log rollback failure but continue rolling back other operations
                    Console.WriteLine($"Rollback failed for operation '{operation.Description}': {rollbackEx.Message}");
                }
            }
            
            return TransactionResult.ExecutionFailure(ex, _completed.Count, _operations.Count);
        }
    }
    
    /// <summary>
    /// Gets the number of operations in this transaction
    /// </summary>
    public int OperationCount => _operations.Count;
    
    /// <summary>
    /// Gets descriptions of all operations in the transaction
    /// </summary>
    public IReadOnlyList<string> GetOperationDescriptions()
    {
        return _operations.Select(o => o.Description).ToList();
    }
}

/// <summary>
/// Result of transaction validation
/// </summary>
public class TransactionValidationResult
{
    public bool IsValid { get; set; }
    public List<string> FailedOperations { get; set; } = new();
}

/// <summary>
/// Result of a transaction execution
/// </summary>
public class TransactionResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public TransactionFailureType? FailureType { get; private set; }
    public List<string> CompletedOperations { get; private set; } = new();
    public List<string> FailedOperations { get; private set; } = new();
    public Exception Exception { get; private set; }
    
    public static TransactionResult Success(List<string> completedOperations)
    {
        return new TransactionResult
        {
            Success = true,
            Message = $"Transaction completed successfully with {completedOperations.Count} operations",
            CompletedOperations = completedOperations
        };
    }
    
    public static TransactionResult ValidationFailure(List<string> failedOperations)
    {
        return new TransactionResult
        {
            Success = false,
            Message = $"Transaction validation failed. {failedOperations.Count} operations cannot execute.",
            FailureType = TransactionFailureType.ValidationFailed,
            FailedOperations = failedOperations
        };
    }
    
    public static TransactionResult ExecutionFailure(Exception ex, int completedCount, int totalCount)
    {
        return new TransactionResult
        {
            Success = false,
            Message = $"Transaction failed after completing {completedCount}/{totalCount} operations. All changes have been rolled back.",
            FailureType = TransactionFailureType.ExecutionFailed,
            Exception = ex
        };
    }
}

/// <summary>
/// Types of transaction failures
/// </summary>
public enum TransactionFailureType
{
    ValidationFailed,
    ExecutionFailed
}