public class SkillProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int XPToNextLevel
    {
        get
        {
            return Level * 100;
        }
    }
}

public class PlayerSkills
{
    public readonly Dictionary<Skills, SkillProgress> Skills = new();

    public PlayerSkills()
    {
        foreach (Skills type in Enum.GetValues(typeof(Skills)))
            Skills[type] = new SkillProgress();
    }

    public int GetLevelForSkill(Skills skillType)
    {
        return Skills[skillType].Level;
    }

    public int GetXPForSkill(Skills skillType)
    {
        return Skills[skillType].XP;
    }

    public int GetXPToNextForSkill(Skills skillType)
    {
        return Skills[skillType].XPToNextLevel;
    }

    public void SetSkillXP(Skills skillType, int XP)
    {
        Skills[skillType].XP = XP;
    }

    public void AddLevelBonus(Skills skillType, int level)
    {
        Skills[skillType].Level += level;
    }

    public PlayerSkills Clone()
    {
        PlayerSkills clone = new PlayerSkills();

        // Assuming PlayerSkills has some internal collection of skill levels
        // that needs to be copied

        // Copy each skill level
        foreach (Skills skillType in Enum.GetValues<Skills>())
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
