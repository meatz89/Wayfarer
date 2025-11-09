namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Quality tier of service being provided.
    /// Scales costs and requirements: higher quality = higher cost, better experience.
    /// Categorical property that translates to concrete numerical values at generation time.
    /// </summary>
    public enum ServiceQuality
    {
        /// <summary>
        /// Bare minimum service (cheap, minimal comfort).
        /// Cost multiplier: 0.6x
        /// Example: 5 coins base × 0.6 = 3 coins
        /// </summary>
        Basic,

        /// <summary>
        /// Average service quality (standard pricing, adequate comfort).
        /// Cost multiplier: 1.0x (baseline)
        /// Example: 5 coins base × 1.0 = 5 coins
        /// </summary>
        Standard,

        /// <summary>
        /// Above-average service (expensive, good comfort).
        /// Cost multiplier: 1.6x
        /// Example: 5 coins base × 1.6 = 8 coins
        /// </summary>
        Premium,

        /// <summary>
        /// Exceptional service (very expensive, excellent comfort).
        /// Cost multiplier: 2.4x
        /// Example: 5 coins base × 2.4 = 12 coins
        /// </summary>
        Luxury
    }
}
