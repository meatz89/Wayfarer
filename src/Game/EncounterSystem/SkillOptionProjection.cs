public class SkillOptionProjection
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsUntrained { get; set; }
    public int EffectiveLevel { get; set; }
    public bool ChoiceSuccess { get; set; }
    public EffectProjection SuccessEffect { get; set; }
    public EffectProjection FailureEffect { get; set; }
}
