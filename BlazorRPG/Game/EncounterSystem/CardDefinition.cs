public class CardDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public CardTypes Type { get; set; }
    public SkillTypes Skill { get; set; }
    public int EnergyCost { get; set; }
    public int ConcentrationCost { get; set; }
    public int Level { get; set; } = 1;
    public int SkillBonus { get; set; } = 1;
    public List<string> Tags { get; set; } = new List<string>();
    public bool IsExhausted { get; set; } = false;

    public CardDefinition(string id, string name)
    {
        Id = id;
        Name = name;
    }
}