public class EncounterOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int FocusCost { get; set; }
    public SkillTypes Skill { get; set; }
    public int Difficulty { get; set; }
    public int LocationModifier { get; set; }
    public UniversalActionTypes ActionType { get; set; }
    public NegativeConsequenceTypes NegativeConsequenceType { get; set; }
    public List<string> Tags { get; set; }
    public bool IsDisabled { get; internal set; }
    public bool HasSkillCheck
    {
        get
        {
            return Skill != SkillTypes.None;
        }
    }
    public EncounterOption(string id, string name)
    {
        Id = id;
        Name = name;
        Tags = new List<string>();
    }

    
    public EncounterOption()
    {
        Tags = new List<string>();
        FocusCost = 1;
        ActionType = UniversalActionTypes.Recovery;
    }

}