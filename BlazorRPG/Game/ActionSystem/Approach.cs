public class Approach
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CardType { get; set; }
    public string Skill { get; set; }
    public int Difficulty { get; set; }
    public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();

    public Approach(string id, string name)
    {
        Id = id;
        Name = name;
    }
}