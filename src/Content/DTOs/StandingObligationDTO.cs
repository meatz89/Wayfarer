/// <summary>
/// Data Transfer Object for deserializing standing obligation data from JSON.
/// Maps to the structure in standing_obligations.json.
/// </summary>
public class StandingObligationDTO
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public string RelatedTokenType { get; set; }
    public List<string> BenefitEffects { get; set; } = new List<string>();
    public List<string> ConstraintEffects { get; set; } = new List<string>();

    // Threshold-based activation fields
    public string RelatedNPCId { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? DeactivationThreshold { get; set; }
    public bool IsThresholdBased { get; set; } = false;
    public bool ActivatesAboveThreshold { get; set; } = true;

    // Dynamic scaling fields (flat integers)
    public string ScalingType { get; set; } = "None";
    public int ScalingFactor { get; set; } = 1;
    public int BaseValue { get; set; } = 0;
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;
    public List<SteppedThreshold> SteppedThresholds { get; set; } = new List<SteppedThreshold>();
}