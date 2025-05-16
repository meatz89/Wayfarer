
public class NarrativeChoice
{

    public NarrativeChoice(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; internal set; }
    public string Name { get; internal set; }
    public string Description { get; internal set; }
    public Skills Skill { get; internal set; }
    public List<string> Tags { get; internal set; }
    public int Difficulty { get; internal set; }
    public int Reward { get; internal set; }
    public bool IsDisabled { get; internal set; } = false;
}