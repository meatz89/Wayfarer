
public class NarrativeStage
{
    public int Id { get; set; }
    public string Situation { get; set; }
    public List<NarrativeChoice> Choices { get; set; } = new();
    
}
