public class InputMechanics
{
    public FocusCost FocusCost { get; private set; }
    public SkillCheckRequirement SkillCheckRequirement { get; private set; }

    public InputMechanics(FocusCost focusCost, SkillCheckRequirement skillCheckRequirement)
    {
        FocusCost = focusCost;
        SkillCheckRequirement = skillCheckRequirement;
    }

    public object ToJsonObject()
    {
        return new
        {
            FocusCost = FocusCost.ToJsonObject(),
            SkillCheckRequirement = SkillCheckRequirement?.ToJsonObject()
        };
    }
}