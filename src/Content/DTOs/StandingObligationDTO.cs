using System.Collections.Generic;

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
}