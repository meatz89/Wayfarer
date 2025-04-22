public class SkillProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int XPToNextLevel => Level * 100;
}

public class PlayerSkills
{
    public readonly Dictionary<SkillTypes, SkillProgress> Skills = new();

    public int Warfare => Skills[SkillTypes.Warfare].Level;
    public int Wilderness => Skills[SkillTypes.Wilderness].Level;
    public int Scholarship => Skills[SkillTypes.Scholarship].Level;
    public int Subterfuge => Skills[SkillTypes.Subterfuge].Level;
    public int Diplomacy => Skills[SkillTypes.Diplomacy].Level;

    public int BonusMaxHealth => Warfare * 2;
    public int BonusMaxEnergy => Wilderness * 2;
    public int BonusMaxConcentration => Scholarship * 2;
    public int BonusMaxConfidence => Subterfuge * 2;
    public int BonusReputation => Diplomacy * 2;

    public PlayerSkills()
    {
        foreach (SkillTypes type in Enum.GetValues(typeof(SkillTypes)))
            Skills[type] = new SkillProgress();
    }

    public int GetLevelForSkill(SkillTypes skillType) => Skills[skillType].Level;
    public int GetXPForSkill(SkillTypes skillType) => Skills[skillType].XP;
    public int GetXPToNextForSkill(SkillTypes skillType) => Skills[skillType].XPToNextLevel;
}
