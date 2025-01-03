public class Choice
{
    public int Index { get; set; }
    public string Description { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; } = new();
    public List<Outcome> Rewards { get; internal set; }
}