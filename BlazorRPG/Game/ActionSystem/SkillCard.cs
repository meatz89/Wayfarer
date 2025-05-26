public class SkillCard
{
    public string Name { get; private set; }
    public SkillTypes SkillType { get; private set; }
    public SkillCategories Category { get; private set; }
    public int Level { get; private set; }
    public string Description { get; private set; }
    public bool IsExhausted { get; private set; }
    public string Id { get; internal set; }

    public SkillCard(string name, SkillTypes skillType, SkillCategories category, int level, string description)
    {
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
        // Base level
        int effectiveLevel = Level;

        // Add modifiers from state
        List<SkillModifier> modifiers = state.GetActiveModifiers(SkillType);
        foreach (SkillModifier modifier in modifiers)
        {
            effectiveLevel += modifier.ModifierValue;
        }

        // Check for location modifiers (already applied to choice difficulty)

        return Math.Max(0, effectiveLevel);
    }
}

