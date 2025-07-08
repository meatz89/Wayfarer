public class SkillModifier
{
    public SkillTypes TargetSkill { get; }
    public int ModifierValue { get; }
    public int RemainingDuration { get; private set; }

    public SkillModifier(SkillTypes targetSkill, int modifierValue, int duration)
    {
        TargetSkill = targetSkill;
        ModifierValue = modifierValue;
        RemainingDuration = duration;
    }

    public void DecrementDuration()
    {
        if (RemainingDuration > 0)
        {
            RemainingDuration--;
        }
    }

    public bool HasExpired()
    {
        return RemainingDuration <= 0;
    }
}