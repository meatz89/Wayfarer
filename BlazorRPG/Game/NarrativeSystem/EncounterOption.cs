public class EncounterOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillTypes Skill { get; set; }
    public int Difficulty { get; set; }
    public int SuccessProgress { get; set; }
    public int FailureProgress { get; set; }
    public bool IsDisabled { get; set; } = false;

    public EncounterOption()
    {
        
    }

    public EncounterOption(string id, string name)
    {
        Id = id;
        Name = name;
    }
}