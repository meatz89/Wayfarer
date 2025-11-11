public class SkillProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int XPToNextLevel => Level * 100;
}

public class PlayerSkills
{
    public Dictionary<SkillTypes, SkillProgress> Skills = new();

    public PlayerSkills()
    {
        foreach (SkillTypes type in Enum.GetValues(typeof(SkillTypes)))
            Skills[type] = new SkillProgress();
    }

    public List<SkillTypes> GetAllSkills()
    {
        return Skills.Keys.ToList();
    }

    public int GetLevelForSkill(SkillTypes skillType)
    {
        return Skills[skillType].Level;
    }

    public int GetXPForSkill(SkillTypes skillType)
    {
        return Skills[skillType].XP;
    }

    public int GetXPToNextForSkill(SkillTypes skillType)
    {
        return Skills[skillType].XPToNextLevel;
    }

    public void SetSkillXP(SkillTypes skillType, int XP)
    {
        Skills[skillType].XP = XP;
    }

    public void AddLevelBonus(SkillTypes skillType, int level)
    {
        Skills[skillType].Level += level;
    }

    public PlayerSkills Clone()
    {
        PlayerSkills clone = new PlayerSkills();

        // Assuming PlayerSkills has some public collection of skill levels
        // that needs to be copied

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
