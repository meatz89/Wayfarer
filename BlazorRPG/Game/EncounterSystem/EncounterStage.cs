public class EncounterStage
{
    public int StageNumber { get; set; }
    public string Description { get; set; }
    public List<AiChoice> Options { get; set; } = new List<AiChoice>();
    public bool IsCompleted { get; set; }
}
