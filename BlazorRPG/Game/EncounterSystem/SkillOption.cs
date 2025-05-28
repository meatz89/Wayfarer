public class SkillOption
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; } // "EASY", "STANDARD", "HARD", "EXCEPTIONAL"
    public int SCD { get; set; } // The actual numerical difficulty
    public PayloadEntry SuccessPayload { get; set; }
    public PayloadEntry FailurePayload { get; set; }

    public string RequiredSkillName { get; set; }
    public int EffectiveLevel { get; set; }
    public bool IsUntrained { get; set; }
    public int SuccessChance { get; set; }
}
