using System.Collections.Generic;

/// <summary>
/// Result of a queue displacement attempt
/// </summary>
public class QueueDisplacementResult
{
    public bool CanExecute { get; set; }
    public string ErrorMessage { get; set; }
    public ObligationDisplacementPlan DisplacementPlan { get; set; }
}

/// <summary>
/// Complete plan for displacing an obligation in the queue
/// </summary>
public class ObligationDisplacementPlan
{
    public DeliveryObligation ObligationToMove { get; set; }
    public int OriginalPosition { get; set; }
    public int TargetPosition { get; set; }
    public List<ObligationDisplacement> Displacements { get; set; } = new List<ObligationDisplacement>();
    public int TotalTokenCost { get; set; }
}

/// <summary>
/// Details of a single obligation being displaced
/// </summary>
public class ObligationDisplacement
{
    public DeliveryObligation DisplacedObligation { get; set; }
    public int OriginalPosition { get; set; }
    public int NewPosition { get; set; }
    public int TokenCost { get; set; }
}

/// <summary>
/// Preview information for the UI
/// </summary>
public class QueueDisplacementPreview
{
    public bool CanExecute { get; set; }
    public string ErrorMessage { get; set; }
    public int TotalTokenCost { get; set; }
    public List<DisplacementDetail> DisplacementDetails { get; set; } = new List<DisplacementDetail>();
}

/// <summary>
/// UI-friendly details about a displacement
/// </summary>
public class DisplacementDetail
{
    public string NPCName { get; set; }
    public ConnectionType TokenType { get; set; }
    public int TokenCost { get; set; }
    public int FromPosition { get; set; }
    public int ToPosition { get; set; }
}