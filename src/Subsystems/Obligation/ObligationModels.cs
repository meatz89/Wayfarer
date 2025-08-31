using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Result of attempting to deliver an obligation
    /// </summary>
    public class DeliveryResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DeliveryObligation DeliveredObligation { get; set; }
        public int TokensGranted { get; set; }
        public ConnectionType TokenType { get; set; }
        public string RecipientId { get; set; } = "";
    }

    /// <summary>
    /// Result of a displacement attempt
    /// </summary>
    public class DisplacementResult
    {
        public bool CanExecute { get; set; }
        public string ErrorMessage { get; set; } = "";
        public ObligationDisplacementPlan DisplacementPlan { get; set; }
        public Dictionary<ConnectionType, int> RequiredTokens { get; set; } = new Dictionary<ConnectionType, int>();
        public int TotalTokenCost { get; set; }
    }

    /// <summary>
    /// Result of adding an obligation to the queue
    /// </summary>
    public class ObligationAddResult
    {
        public bool Success { get; set; }
        public int Position { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DeliveryObligation AddedObligation { get; set; }
        public bool UsedLeverage { get; set; }
        public bool CausedDisplacement { get; set; }
        public List<string> DisplacedObligationIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of queue manipulation operations
    /// </summary>
    public class QueueManipulationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string OperationType { get; set; } = "";
        public int Position { get; set; }
        public DeliveryObligation AffectedObligation { get; set; }
        public Dictionary<ConnectionType, int> TokensCost { get; set; } = new Dictionary<ConnectionType, int>();
    }

    /// <summary>
    /// Metrics about obligation queue performance and statistics
    /// </summary>
    public class ObligationMetrics
    {
        public int ActiveObligationCount { get; set; }
        public int ActiveMeetingCount { get; set; }
        public int TotalQueueSize { get; set; }
        public int EmptyQueueSlots { get; set; }
        public double AverageDeadlineHours { get; set; }
        public int ExpiredObligationCount { get; set; }
        public int UrgentObligationCount { get; set; } // < 6 hours
        public int CriticalObligationCount { get; set; } // < 3 hours
        public int OverdueObligationCount { get; set; }
        
        // Time-based metrics
        public int TotalMinutesUntilNextDeadline { get; set; }
        public string NextDeadlineDescription { get; set; } = "";
        public DeliveryObligation MostUrgentObligation { get; set; }
        
        // Token leverage metrics
        public Dictionary<ConnectionType, int> TotalTokenLeverage { get; set; } = new Dictionary<ConnectionType, int>();
        public int PositionsGainedFromLeverage { get; set; }
        public int PositionsLostFromDebt { get; set; }
        
        // Queue efficiency metrics
        public bool IsQueueFull => EmptyQueueSlots == 0;
        public double QueueUtilizationPercentage => TotalQueueSize > 0 ? ((double)(TotalQueueSize - EmptyQueueSlots) / TotalQueueSize) * 100 : 0;
        public bool HasPosition1Available => Position1Status == Position1Status.Empty;
        public Position1Status Position1Status { get; set; } = Position1Status.Empty;
        
        // Delivery performance
        public int TotalDeliveredToday { get; set; }
        public int TotalSkippedToday { get; set; }
        public double DeliverySuccessRate { get; set; }
    }

    /// <summary>
    /// Status of queue position 1
    /// </summary>
    public enum Position1Status
    {
        Empty,
        CanDeliver,
        CannotDeliver,
        Blocked
    }

    /// <summary>
    /// Details about deadline tracking and expiration
    /// </summary>
    public class DeadlineTrackingInfo
    {
        public List<DeliveryObligation> ExpiringObligations { get; set; } = new List<DeliveryObligation>();
        public List<MeetingObligation> ExpiringMeetings { get; set; } = new List<MeetingObligation>();
        public int HoursElapsed { get; set; }
        public List<string> ExpiredObligationIds { get; set; } = new List<string>();
        public List<string> ExpiredMeetingIds { get; set; } = new List<string>();
        public bool HasExpiredItems => ExpiredObligationIds.Any() || ExpiredMeetingIds.Any();
        public int TotalExpiredCount => ExpiredObligationIds.Count + ExpiredMeetingIds.Count;
    }

    /// <summary>
    /// Types of triggers that cause automatic displacement
    /// </summary>
    public enum DisplacementTrigger
    {
        None,
        FailedNegotiation,
        ProudNPC,
        CriticalEmotionalWeight,
        StandingObligation,
        NPCDesperation
    }

    /// <summary>
    /// Information about leverage calculation for an obligation
    /// </summary>
    public class LeverageCalculation
    {
        public string NPCId { get; set; } = "";
        public string NPCName { get; set; } = "";
        public Dictionary<ConnectionType, int> AllTokens { get; set; } = new Dictionary<ConnectionType, int>();
        public int HighestPositiveToken { get; set; }
        public int WorstNegativeTokenPenalty { get; set; }
        public int BasePosition { get; set; }
        public int CalculatedPosition { get; set; }
        public int FinalPosition { get; set; }
        public bool HasActiveObligation { get; set; }
        public bool HasCommerceDebtOverride { get; set; }
        public LetterPositioningReason PositioningReason { get; set; }
        public int LeverageBoost { get; set; }
    }

    /// <summary>
    /// Physical space and satchel management information
    /// </summary>
    public class SatchelInfo
    {
        public List<Letter> CarriedLetters { get; set; } = new List<Letter>();
        public int TotalSize { get; set; }
        public int MaxCapacity { get; set; }
        public int RemainingSpace { get; set; }
        public bool IsFull => RemainingSpace <= 0;
        public double UtilizationPercentage => MaxCapacity > 0 ? ((double)TotalSize / MaxCapacity) * 100 : 0;
        public List<Letter> FragileLetters { get; set; } = new List<Letter>();
        public List<Letter> PerishableLetters { get; set; } = new List<Letter>();
        public List<Letter> ValuableLetters { get; set; } = new List<Letter>();
        public bool HasSpecialHandlingLetters => FragileLetters.Any() || PerishableLetters.Any() || ValuableLetters.Any();
    }

    /// <summary>
    /// Queue validation result
    /// </summary>
    public class QueueValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public List<string> ValidationWarnings { get; set; } = new List<string>();
        public bool HasErrors => ValidationErrors.Any();
        public bool HasWarnings => ValidationWarnings.Any();
    }

    /// <summary>
    /// Meeting management result
    /// </summary>
    public class MeetingResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public MeetingObligation AffectedMeeting { get; set; }
        public MeetingOperation Operation { get; set; }
        public string NPCId { get; set; } = "";
        public string NPCName { get; set; } = "";
    }

    /// <summary>
    /// Types of meeting operations
    /// </summary>
    public enum MeetingOperation
    {
        Add,
        Complete,
        Cancel,
        Expire
    }

    /// <summary>
    /// Statistics about letter generation and queue activity
    /// </summary>
    public class QueueActivity
    {
        public int LettersAddedToday { get; set; }
        public int LettersDeliveredToday { get; set; }
        public int LettersSkippedToday { get; set; }
        public int LettersExpiredToday { get; set; }
        public int MeetingsCompletedToday { get; set; }
        public int MeetingsExpiredToday { get; set; }
        public Dictionary<ConnectionType, int> TokensEarnedToday { get; set; } = new Dictionary<ConnectionType, int>();
        public Dictionary<ConnectionType, int> TokensSpentToday { get; set; } = new Dictionary<ConnectionType, int>();
        public int TotalDisplacementsToday { get; set; }
        public int TotalManipulationsToday { get; set; }
    }

    /// <summary>
    /// Information about queue position availability and blocking
    /// </summary>
    public class QueuePositionInfo
    {
        public int Position { get; set; }
        public bool IsOccupied { get; set; }
        public DeliveryObligation CurrentObligation { get; set; }
        public bool CanInsertHere { get; set; }
        public string BlockingReason { get; set; } = "";
        public Dictionary<ConnectionType, int> RequiredTokensToDisplace { get; set; } = new Dictionary<ConnectionType, int>();
    }

    /// <summary>
    /// Comprehensive queue state snapshot
    /// </summary>
    public class QueueSnapshot
    {
        public DateTime SnapshotTime { get; set; } = DateTime.UtcNow;
        public DeliveryObligation[] Queue { get; set; } = new DeliveryObligation[0];
        public List<MeetingObligation> Meetings { get; set; } = new List<MeetingObligation>();
        public ObligationMetrics Metrics { get; set; } = new ObligationMetrics();
        public List<QueuePositionInfo> PositionInfo { get; set; } = new List<QueuePositionInfo>();
        public SatchelInfo SatchelState { get; set; } = new SatchelInfo();
        public QueueActivity TodaysActivity { get; set; } = new QueueActivity();
    }
}