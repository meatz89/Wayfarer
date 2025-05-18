public class PlayerCommission
{
    public string DefinitionId { get; set; }
    public int CurrentProgress { get; set; }
    public int CurrentStepIndex { get; set; } // For Sequential commissions
    public int DaysRemaining { get; set; }
    public List<CommissionStep> GeneratedSteps { get; set; } // For Sequential commissions
    public bool IsCompleted { get; set; }
}