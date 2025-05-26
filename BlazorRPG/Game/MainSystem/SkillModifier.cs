public class SkillModifier
{
    public SkillTypes TargetSkill { get; }
    public int ModifierValue { get; }
    public int Duration { get; private set; }

    public SkillModifier(SkillTypes targetSkill, int modifierValue, int duration)
    {
        TargetSkill = targetSkill;
        ModifierValue = modifierValue;
        Duration = duration;
    }

    public void DecrementDuration()
    {
        Duration = Math.Max(0, Duration - 1);
    }

    public bool HasExpired()
    {
        return Duration <= 0;
    }
}

