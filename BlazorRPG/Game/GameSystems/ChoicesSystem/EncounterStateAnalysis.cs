public class EncounterStateAnalysis
{
    public bool IsCritical { get; set; }          // Player is in danger of losing
    public bool HasEscapeRoute { get; set; }      // Player has built up resources to escape
    public bool IsWinnable { get; set; }          // Victory is still possible
    public bool RequiresEscalation { get; set; }  // Need to force a conclusion
    public int SafeChoicesRemaining { get; set; } // How many more "safe" choices we can offer
    public bool MustProvideSafeChoice { get; set; } // Must give player at least one safe option

    public static EncounterStateAnalysis Analyze(EncounterStateValues values, int stageNumber, PlayerState player)
    {
        return new EncounterStateAnalysis
        {
            // Critical if low outcome, high pressure, or low energy across the board
            IsCritical = values.Pressure >= 7 || values.Outcome <= 3 ||
                        (player.PhysicalEnergy <= 2 && player.SocialEnergy <= 2 && player.FocusEnergy <= 2),

            // Escape routes through high Insight (pressure reduction) or Resonance (efficient gains)
            HasEscapeRoute = values.Insight >= 6 || values.Resonance >= 7,

            // Multiple victory conditions
            IsWinnable = values.Outcome >= 7 ||
                        (values.Resonance >= 8 && values.Outcome >= 5) ||
                        (values.Insight >= 6 && values.Pressure <= 4),

            // Force escalation in later stages if outcome is too low
            RequiresEscalation = stageNumber >= 3 && values.Outcome < 7,

            // Limit safe choices based on stage number
            SafeChoicesRemaining = Math.Max(0, 4 - stageNumber),

            // Must provide safe choice if player is in really bad shape
            MustProvideSafeChoice = player.Health <= 3 || values.Pressure >= 8
        };
    }
}