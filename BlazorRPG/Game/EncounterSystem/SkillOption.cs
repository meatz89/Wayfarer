public class SkillOption
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; }
    public int DifficultyLevel { get; set; }
    public string DifficultyLabel { get; set; } 
    public string RequiredSkillName { get; set; }
    public int EffectiveLevel { get; set; }
    public bool IsUntrained { get; set; }
    public int SuccessChance { get; set; }
}
