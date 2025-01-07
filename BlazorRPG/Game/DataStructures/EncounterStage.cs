public class EncounterStage
{
    public int Id { get; set; }
    public string Situation { get; set; }
    public List<EncounterChoice> Choices { get; set; } = new();

}
