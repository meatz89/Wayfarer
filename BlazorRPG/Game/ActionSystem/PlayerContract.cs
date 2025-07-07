public class PlayerContract
{
    public string DefinitionId { get; set; }
    public int CurrentProgress { get; set; }
    public int CurrentStepIndex { get; set; } // For Sequential Contracts
    public int DaysRemaining { get; set; }
    public List<ContractStep> GeneratedSteps { get; set; } // For Sequential Contracts
    public bool IsCompleted { get; set; }
}