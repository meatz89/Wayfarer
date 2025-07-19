public class SkillCheckResult
{
    public SkillTypes Skill { get; }
    public int EffectiveLevel { get; }
    public int Difficulty { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsUntrained { get; set; }
    public string SkillName { get; set; }
    public int PlayerLevel { get; set; }
    public int RequiredLevel { get; set; }
    public int SuccessChance { get; set; }
}