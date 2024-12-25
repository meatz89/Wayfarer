public class Narrative
{
    public BasicActionTypes ActionType { get; set; }
    public List<NarrativeStage> Stages { get; set; } = new();
}