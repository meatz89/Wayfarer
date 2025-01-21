public class EncounterHistory
{
    public string TaskToSolve { get; set; }
    public string InitialSituation { get; set; }
    public List<Narrative> Narratives = new();
    public string ResultSituation { get; set; }

}

public class Narrative
{
    public Roles Role { get; set; }
    public string Text { get; set; }
}