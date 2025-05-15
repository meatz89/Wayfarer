public class ApproachOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CardTypes RequiredCardType { get; set; }
    public string Skill { get; set; }
    public int Difficulty { get; set; }
    public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();

    public ApproachOption(string id, string name)
    {
        Id = id;
        Name = name;
    }
}