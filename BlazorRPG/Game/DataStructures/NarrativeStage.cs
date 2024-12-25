public class NarrativeStage
{
    public int Id { get; set; }
    public string Situation { get; set; }
    public List<Choice> Choices { get; set; } = new();
}
