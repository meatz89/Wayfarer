public class SkillProgress
{
    public SkillTypes SkillType { get; set; }
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int XPToNextLevel => Level * 100;
}

public class PlayerSkills
{
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<SkillProgress> Skills { get; set; } = new List<SkillProgress>();

    public PlayerSkills()
    {
        foreach (SkillTypes type in Enum.GetValues(typeof(SkillTypes)))
            Skills.Add(new SkillProgress { SkillType = type });
    }

    public List<SkillTypes> GetAllSkills()
    {
        return Skills.Select(s => s.SkillType).ToList();
    }

    private SkillProgress GetSkillProgress(SkillTypes skillType)
    {
        return Skills.FirstOrDefault(s => s.SkillType == skillType);
    }

    public int GetLevelForSkill(SkillTypes skillType)
    {
        return GetSkillProgress(skillType)?.Level ?? 1;
    }

    public int GetXPForSkill(SkillTypes skillType)
    {
        return GetSkillProgress(skillType)?.XP ?? 0;
    }

    public int GetXPToNextForSkill(SkillTypes skillType)
    {
        return GetSkillProgress(skillType)?.XPToNextLevel ?? 100;
    }

    public void SetSkillXP(SkillTypes skillType, int XP)
    {
        SkillProgress progress = GetSkillProgress(skillType);
        if (progress != null)
        {
            progress.XP = XP;
        }
    }

    public void AddLevelBonus(SkillTypes skillType, int level)
    {
        SkillProgress progress = GetSkillProgress(skillType);
        if (progress != null)
        {
            progress.Level += level;
        }
    }

    public PlayerSkills Clone()
    {
        PlayerSkills clone = new PlayerSkills();

        // Copy each skill level
        foreach (SkillTypes skillType in Enum.GetValues<SkillTypes>())
        {
            int level = this.GetLevelForSkill(skillType);
            for (int i = 0; i < level; i++)
            {
                clone.AddLevelBonus(skillType, 1);
            }
        }

        return clone;
    }
}
