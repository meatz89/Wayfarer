/// <summary>
/// DTO for danger effects in Mental and Physical cards
/// </summary>
public class DangerEffectDTO
{
    public int HealthDamage { get; set; } = 0;
    public int StaminaDamage { get; set; } = 0;
    public int ExposureIncrease { get; set; } = 0;
    public int DangerIncrease { get; set; } = 0;
    public bool FalseClue { get; set; } = false;
}
