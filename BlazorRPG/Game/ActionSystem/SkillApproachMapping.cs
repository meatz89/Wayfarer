public class SkillApproachMapping
{
    public SkillApproachMapping(ApproachTags approachTag, SkillTypes skillType, float multiplier)
    {
        SkillType = skillType;
        ApproachTag = approachTag;
        Multiplier = multiplier;
    }

    public SkillTypes SkillType { get; set; }
    public ApproachTags ApproachTag { get; set; }
    public float Multiplier { get; set; }
}