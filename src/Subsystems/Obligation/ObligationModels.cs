/// <summary>
/// Result of attempting to deliver an obligation
/// </summary>
public class DeliveryResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int TokensGranted { get; set; }
    public ConnectionType TokenType { get; set; }
    public string RecipientId { get; set; }
}

/// <summary>
/// Result of a displacement attempt
/// </summary>
public class DisplacementResult
{
    public bool CanExecute { get; set; }
    public string ErrorMessage { get; set; }
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
    public string ErrorMessage { get; set; }
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
    public string ErrorMessage { get; set; }
    public string OperationType { get; set; }
    public int Position { get; set; }
    public Dictionary<ConnectionType, int> TokensCost { get; set; } = new Dictionary<ConnectionType, int>();
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
    public List<MeetingObligation> ExpiringMeetings { get; set; } = new List<MeetingObligation>();
    public int SegmentsElapsed { get; set; }
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
    CriticalEmotionalFocus,
    StandingObligation,
    NPCDesperation
}

/// <summary>
/// Information about leverage calculation for an obligation
/// </summary>
public class LeverageCalculation
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public Dictionary<ConnectionType, int> AllTokens { get; set; } = new Dictionary<ConnectionType, int>();
    public int HighestPositiveToken { get; set; }
    public int WorstNegativeTokenPenalty { get; set; }
    public int BasePosition { get; set; }
    public int CalculatedPosition { get; set; }
    public int FinalPosition { get; set; }
    public bool HasActiveObligation { get; set; }
    public bool HasDiplomacyDebtOverride { get; set; }
    public int LeverageBoost { get; set; }
}

/// <summary>
/// Physical space and satchel management information
/// </summary>
public class SatchelInfo
{
    public int TotalSize { get; set; }
    public int MaxCapacity { get; set; }
    public int RemainingSpace { get; set; }
    public bool IsFull => RemainingSpace <= 0;
    public double UtilizationPercentage => MaxCapacity > 0 ? ((double)TotalSize / MaxCapacity) * 100 : 0;
}

/// <summary>
/// Meeting management result
/// </summary>
public class MeetingResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public MeetingObligation AffectedMeeting { get; set; }
    public MeetingOperation Operation { get; set; }
    public string NPCId { get; set; }
    public string NPCName { get; set; }
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