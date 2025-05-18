public class EncounterStage
{
    public int StageNumber { get; set; }
    public string Description { get; set; }
    public List<EncounterOption> Options { get; set; } = new List<EncounterOption>();
    public bool IsCompleted { get; set; }
}
