public class SkillOption
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; }
    public int DifficultyLevel { get; set; }
    public string DifficultyString { get; set; } // "EASY", "STANDARD", "HARD", "EXCEPTIONAL"
    public int SCD { get; set; } // The actual numerical difficulty
    public IMechanicalEffect SuccessEffect { get; set; }
    public IMechanicalEffect FailureEffect { get; set; }
    public EffectEntry SuccessEffectEntry { get; set; }
    public EffectEntry FailureEffectEntry { get; set; }

    public string RequiredSkillName { get; set; }
    public int EffectiveLevel { get; set; }
    public bool IsUntrained { get; set; }
    public int SuccessChance { get; set; }
}
