public class EncounterStage
{
    public string Situation { get; set; }
    public List<Choice> Choices { get; set; } = new List<Choice>();
    public EncounterState EncounterState { get; internal set; }
}
