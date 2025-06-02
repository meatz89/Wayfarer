public class PlayerOpportunity
{
    public string DefinitionId { get; set; }
    public int CurrentProgress { get; set; }
    public int CurrentStepIndex { get; set; } // For Sequential opportunitys
    public int DaysRemaining { get; set; }
    public List<OpportunityStep> GeneratedSteps { get; set; } // For Sequential opportunitys
    public bool IsCompleted { get; set; }
}