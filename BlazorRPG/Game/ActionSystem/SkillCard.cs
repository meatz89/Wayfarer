public class SkillCard
{
    public string Id { get; }
    public string Name { get; }
    public SkillTypes SkillType { get; }
    public SkillCategories Category { get; }
    public int Level { get; }
    public string Description { get; }
    public bool IsExhausted { get; private set; }

    public SkillCard(string name, SkillTypes skillType, SkillCategories category, int level, string description)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        SkillType = skillType;
        Category = category;
        Level = level;
        Description = description;
        IsExhausted = false;
    }

    public void Exhaust()
    {
        IsExhausted = true;
    }

    public void Refresh()
    {
        IsExhausted = false;
    }

    public int GetEffectiveLevel(EncounterState state)
    {
        int baseLevel = Level;
        List<SkillModifier> modifiers = state.GetActiveModifiers(SkillType);
        foreach (SkillModifier modifier in modifiers)
        {
            baseLevel += modifier.ModifierValue;
        }
        return Math.Max(0, baseLevel);
    }
}
