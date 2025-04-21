public class CardDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Tier { get; set; }
    public EffectTypes EffectType { get; set; }
    public int EffectValue { get; set; }
    public ApproachTags Approach { get; set; }
    public int OptimalApproachPosition { get; set; }
    public FocusTags Focus { get; set; }
    public int OptimalFocusPosition { get; set; }
    public List<TagModification> TagModifications { get; set; }
    public EnvironmentalPropertyEffect StrategicEffect { get; set; }
    public List<SkillRequirement> UnlockRequirements { get; set; } = new List<SkillRequirement>();
    public bool IsBlocked { get; private set; }

    public string GetDetails()
    {
        string desc = $"Tier {Tier} (Requires: {Approach} {OptimalApproachPosition}, {Focus} {OptimalFocusPosition})";
        return desc;
    }

    public override string ToString()
    {
        string desc = $"{Name} - Tier {Tier} (Requires: {Approach} {OptimalApproachPosition}, {Focus} {OptimalFocusPosition})";
        return desc;
    }

    public object GetName()
    {
        string name = $"{Name} ({Approach} - {Focus})";
        return name;
    }

    public CardDefinition()
    {

    }
}

