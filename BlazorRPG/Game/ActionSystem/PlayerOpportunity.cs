public class PlayerOpportunity
{
    public string DefinitionId { get; set; }
    public int CurrentProgress { get; set; }
    public int CurrentStepIndex { get; set; } // For Sequential Opportunities
    public int DaysRemaining { get; set; }
    public List<Opportunitiestep> GeneratedSteps { get; set; } // For Sequential Opportunities
    public bool IsCompleted { get; set; }
}