public class CardDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Tier { get; set; }
    public EffectTypes EffectType { get; set; }
    public int EffectValue { get; set; }
    public int OptimalApproachPosition { get; set; }
    public int OptimalFocusPosition { get; set; }
    public EnvironmentalPropertyEffect StrategicEffect { get; set; }
    public List<SkillRequirement> UnlockRequirements { get; set; } = new List<SkillRequirement>();
    public bool IsBlocked { get; private set; }

    public CardDefinition()
    {

    }
}

