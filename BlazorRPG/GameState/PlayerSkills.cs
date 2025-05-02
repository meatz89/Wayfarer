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
    public readonly Dictionary<SkillTypes, SkillProgress> Skills = new();

    public int Endurance
    {
        get
        {
            return Skills[SkillTypes.Endurance].Level;
        }
    }

    public int Charm
    {
        get
        {
            return Skills[SkillTypes.Charm].Level;
        }
    }

    public int Observation
    {
        get
        {
            return Skills[SkillTypes.Insight].Level;
        }
    }

    public int Finesse
    {
        get
        {
            return Skills[SkillTypes.Finesse].Level;
        }
    }

    public int Diplomacy
    {
        get
        {
            return Skills[SkillTypes.Diplomacy].Level;
        }
    }

    public int Insight
    {
        get
        {
            return Skills[SkillTypes.Lore].Level;
        }
    }
    public int BonusMaxHealth
    {
        get
        {
            return Skills[SkillTypes.Endurance].Level +
                Skills[SkillTypes.Charm].Level +
                Skills[SkillTypes.Insight].Level;
        }
    }

    public int BonusMaxEnergy
    {
        get
        {
            return Skills[SkillTypes.Endurance].Level +
                Skills[SkillTypes.Finesse].Level +
                Skills[SkillTypes.Diplomacy].Level;
        }
    }

    public int BonusMaxConcentration
    {
        get
        {
            return Skills[SkillTypes.Finesse].Level +
                Skills[SkillTypes.Insight].Level +
                Skills[SkillTypes.Lore].Level;
        }
    }

    public int BonusMaxConfidence
    {
        get
        {
            return Skills[SkillTypes.Diplomacy].Level +
                Skills[SkillTypes.Charm].Level +
                Skills[SkillTypes.Lore].Level;
        }
    }

    public PlayerSkills()
    {
        foreach (SkillTypes type in Enum.GetValues(typeof(SkillTypes)))
            Skills[type] = new SkillProgress();
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

    internal void AddLevelBonus(SkillTypes skillType, int level)
    {
        Skills[skillType].Level += level;
    }
}
