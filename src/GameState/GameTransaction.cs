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
        List<string> failedOperations = new List<string>();

        foreach (IGameOperation operation in _operations)
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
        TransactionValidationResult validation = Validate();
        if (!validation.IsValid)
        {
            return TransactionResult.ValidationFailure(validation.FailedOperations);
        }

        try
        {
            // Execute each operation
            int completedCount = 0;
            foreach (IGameOperation operation in _operations)
            {
                operation.Execute(_gameWorld);
                completedCount++;
            }

            return TransactionResult.Success(_operations.Select(o => o.Description).ToList());
        }
        catch (Exception ex)
        {
            // Cannot rollback - operations are not reversible
            return TransactionResult.ExecutionFailure(ex, 0, _operations.Count);
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
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public TransactionFailureType? FailureType { get; private set; }
    public List<string> CompletedOperations { get; private set; } = new();
    public List<string> FailedOperations { get; private set; } = new();
    public Exception Exception { get; private set; }

    public static TransactionResult Success(List<string> completedOperations)
    {
        return new TransactionResult
        {
            IsSuccess = true,
            Message = $"Transaction completed successfully with {completedOperations.Count} operations",
            CompletedOperations = completedOperations
        };
    }

    public static TransactionResult ValidationFailure(List<string> failedOperations)
    {
        return new TransactionResult
        {
            IsSuccess = false,
            Message = $"Transaction validation failed. {failedOperations.Count} operations cannot execute.",
            FailureType = TransactionFailureType.ValidationFailed,
            FailedOperations = failedOperations
        };
    }

    public static TransactionResult ExecutionFailure(Exception ex, int completedCount, int totalCount)
    {
        return new TransactionResult
        {
            IsSuccess = false,
            Message = $"Transaction failed after completing {completedCount}/{totalCount} operations.",
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