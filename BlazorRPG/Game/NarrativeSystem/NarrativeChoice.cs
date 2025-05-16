public class NarrativeChoice
{

    public NarrativeChoice(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Skills Skill { get; set; }
    public List<string> Tags { get; set; }
    public int Difficulty { get; set; }
    public int Reward { get; set; }
    public bool IsDisabled { get; set; } = false;
}