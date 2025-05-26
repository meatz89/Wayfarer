public class SkillOptionProjection
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; }
    public int SCD { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsUntrained { get; set; }
    public int EffectiveLevel { get; set; }
    public int SuccessChance { get; set; }
    public PayloadProjection SuccessPayload { get; set; }
    public PayloadProjection FailurePayload { get; set; }
}
