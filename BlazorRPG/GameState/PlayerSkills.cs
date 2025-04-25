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

    public int Warfare
    {
        get
        {
            return Skills[SkillTypes.Warfare].Level;
        }
    }

    public int Wilderness
    {
        get
        {
            return Skills[SkillTypes.Wilderness].Level;
        }
    }

    public int Scholarship
    {
        get
        {
            return Skills[SkillTypes.Scholarship].Level;
        }
    }

    public int Subterfuge
    {
        get
        {
            return Skills[SkillTypes.Subterfuge].Level;
        }
    }

    public int Diplomacy
    {
        get
        {
            return Skills[SkillTypes.Diplomacy].Level;
        }
    }

    public int BonusMaxHealth
    {
        get
        {
            return Warfare * 2;
        }
    }

    public int BonusMaxEnergy
    {
        get
        {
            return Wilderness * 2;
        }
    }

    public int BonusMaxConcentration
    {
        get
        {
            return Scholarship * 2;
        }
    }

    public int BonusMaxConfidence
    {
        get
        {
            return Diplomacy * 2;
        }
    }

    public int BonusReputation
    {
        get
        {
            return Subterfuge * 2;
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
}
