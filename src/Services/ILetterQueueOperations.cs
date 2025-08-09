using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wayfarer.Services
{
    public interface ILetterQueueOperations
    {
        // Query Operations - O(1) time, O(1) space
        Letter[] GetQueueSnapshot();
        QueueOperationCost GetOperationCost(QueueOperationType operation, int position1, int? position2 = null);
        bool CanPerformOperation(QueueOperationType operation, int position1, int? position2 = null);
        
        // Mutation Operations - O(n) time worst case, O(1) space
        Task<QueueOperationResult> TryMorningSwapAsync(int position1, int position2);
        Task<QueueOperationResult> TryPriorityMoveAsync(int fromPosition, Dictionary<ConnectionType, int> payment);
        Task<QueueOperationResult> TryExtendDeadlineAsync(int position, Dictionary<ConnectionType, int> payment);
        Task<QueueOperationResult> DeliverFromPosition1Async();
        Task<QueueOperationResult> TrySkipDeliverAsync(int position, Dictionary<ConnectionType, int> payment);
        Task<QueueOperationResult> TryReorderAsync(int fromPosition, int toPosition);
    }

    public enum QueueOperationType
    {
        MorningSwap = 1,
        PriorityMove = 2,
        ExtendDeadline = 3,
        Deliver = 4,
        SkipDeliver = 5,
        Reorder = 6
    }

    public record QueueOperationCost(
        Dictionary<ConnectionType, int> TokenCosts,
        bool RequiresMorningTime,
        bool RequiresPosition1,
        string[] ValidationErrors
    );

    public record QueueOperationResult(
        bool Success,
        string FailureReason,
        Dictionary<ConnectionType, int> TokensSpent,
        Letter[] UpdatedQueue
    );
}