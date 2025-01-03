public class Choice
{
    public int Index { get; set; }
    public string Description { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Outcomes { get; set; } = new();
}