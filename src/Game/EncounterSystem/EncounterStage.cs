public class EncounterStage
{
    public int StageNumber { get; set; }
    public string Description { get; set; }
    public List<EncounterChoice> Options { get; set; } = new List<EncounterChoice>();
    public bool IsCompleted { get; set; }
}
