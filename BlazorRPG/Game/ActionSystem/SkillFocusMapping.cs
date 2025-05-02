public class SkillFocusMapping
{
    public SkillFocusMapping(FocusTags focusTag, SkillTypes skillType, float multiplier)
    {
        SkillType = skillType;
        FocusTag = focusTag;
        Multiplier = multiplier;
    }

    public SkillTypes SkillType { get; set; }
    public FocusTags FocusTag { get; set; }
    public float Multiplier { get; set; }
}

