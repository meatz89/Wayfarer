public class InputMechanics
{
    public AttentionCost AttentionCost { get; private set; }
    public SkillCheckRequirement SkillCheckRequirement { get; private set; }

    public InputMechanics(AttentionCost attentionCost, SkillCheckRequirement skillCheckRequirement)
    {
        AttentionCost = attentionCost;
        SkillCheckRequirement = skillCheckRequirement;
    }

    public object ToJsonObject()
    {
        return new
        {
            AttentionCost = AttentionCost.ToJsonObject(),
            SkillCheckRequirement = SkillCheckRequirement?.ToJsonObject()
        };
    }
}