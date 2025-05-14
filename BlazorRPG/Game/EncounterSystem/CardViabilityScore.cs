public class CardViabilityScore
{
    public int PositionalScore { get; set; } // Lower is better
    public int SituationalValue { get; set; } // Higher is better
    public int EnvironmentalSynergy { get; set; } // Higher is better
    public int SkillBonus { get; set; } // Higher is better
    public int TotalScore { get; set; } // Lower is better
    public bool IsPlayable { get; set; }
}