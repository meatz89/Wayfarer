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
            // Critical if low advantage, high tension, or low energy across the board
            IsCritical = values.Tension >= 7 || values.Advantage <= 3 ||
                        (player.PhysicalEnergy <= 2 && player.SocialEnergy <= 2 && player.FocusEnergy <= 2),

            // Escape routes through high Understanding (tension reduction) or Connection (efficient gains)
            HasEscapeRoute = values.Understanding >= 6 || values.Connection >= 7,

            // Multiple victory conditions
            IsWinnable = values.Advantage >= 7 ||
                        (values.Connection >= 8 && values.Advantage >= 5) ||
                        (values.Understanding >= 6 && values.Tension <= 4),

            // Force escalation in later stages if advantage is too low
            RequiresEscalation = stageNumber >= 3 && values.Advantage < 7,

            // Limit safe choices based on stage number
            SafeChoicesRemaining = Math.Max(0, 4 - stageNumber),

            // Must provide safe choice if player is in really bad shape
            MustProvideSafeChoice = player.Health <= 3 || values.Tension >= 8
        };
    }
}