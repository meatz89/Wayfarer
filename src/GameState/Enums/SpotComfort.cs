namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Comfort level of the physical space where service is provided.
    /// Scales restoration amounts: higher comfort = better recovery.
    /// Categorical property that translates to concrete restoration values at generation time.
    /// </summary>
    public enum SpotComfort
    {
        /// <summary>
        /// Minimal comfort (hard bed, cold room, basic amenities).
        /// Restoration multiplier: 1x (baseline)
        /// Example: 10 base health × 1 = 10 health restored
        /// </summary>
        Basic,

        /// <summary>
        /// Adequate comfort (decent bed, warm room, standard amenities).
        /// Restoration multiplier: 2x
        /// Example: 10 base health × 2 = 20 health restored
        /// </summary>
        Standard,

        /// <summary>
        /// High comfort (soft bed, pleasant room, quality amenities).
        /// Restoration multiplier: 3x
        /// Example: 10 base health × 3 = 30 health restored
        /// </summary>
        Premium
    }
}
