public class ActionCardDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public CardTypes Type { get; set; }
    public Skills Skill { get; set; }
    public int Level { get; set; }
    public int Cost { get; set; }
    public int Gain { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool IsExhausted { get; set; } = false;

    public ActionCardDefinition(string id, string name)
    {
        Id = id;
        Name = name;
    }
}