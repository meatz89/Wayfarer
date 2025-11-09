namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Current demeanor/disposition of NPC toward player.
    /// Scales stat requirements and challenge difficulty: hostile NPCs are harder to persuade.
    /// Categorical property that translates to concrete thresholds at generation time.
    /// Derived from PersonalityType and current bond strength.
    /// </summary>
    public enum NPCDemeanor
    {
        /// <summary>
        /// NPC is warm, helpful, and predisposed to assist.
        /// Stat threshold multiplier: 0.6x
        /// Example: 5 base Rapport × 0.6 = 3 Rapport required
        /// Challenge difficulty: Easy
        /// </summary>
        Friendly,

        /// <summary>
        /// NPC is professional, businesslike, neither helpful nor hostile.
        /// Stat threshold multiplier: 1.0x (baseline)
        /// Example: 5 base Rapport × 1.0 = 5 Rapport required
        /// Challenge difficulty: Medium
        /// </summary>
        Neutral,

        /// <summary>
        /// NPC is cold, suspicious, or actively opposed.
        /// Stat threshold multiplier: 1.4x
        /// Example: 5 base Rapport × 1.4 = 7 Rapport required
        /// Challenge difficulty: Hard
        /// </summary>
        Hostile
    }
}
