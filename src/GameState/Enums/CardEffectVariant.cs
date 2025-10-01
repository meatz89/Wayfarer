/// <summary>
/// Defines which effect formula variant to use from CardEffectCatalog.
/// Used to select between Strike/Setup/Specialist patterns at Foundation tier,
/// and Base/Signature/Advanced patterns at higher tiers.
/// </summary>
public enum CardEffectVariant
{
    /// <summary>
    /// Default base effect for the stat/depth combination.
    /// At Foundation (depth 1-2): Maps to Strike pattern (+2 Momentum, +1 Initiative, +stat flavor)
    /// At Standard+ (depth 3+): Uses first variant in catalog
    /// </summary>
    Base,

    /// <summary>
    /// Foundation Type A: Strike pattern (+2 Momentum, +1 Initiative, +stat flavor)
    /// Most common Foundation card type - simultaneously productive and enabling.
    /// Inspired by Steamworld Quest's Strike cards.
    /// </summary>
    Strike,

    /// <summary>
    /// Foundation Type B: Setup pattern (+1 Momentum, +2 Initiative, +stat flavor)
    /// Tactical Foundation cards focused on enabling future plays.
    /// </summary>
    Setup,

    /// <summary>
    /// Foundation Type C: Specialist pattern (pure stat specialty effect)
    /// Rare Foundation cards that focus purely on stat identity.
    /// Example: Insight draws 2 cards, Authority gives +3 Momentum.
    /// </summary>
    Specialist,

    /// <summary>
    /// Signature card variant - requires tokens, powerful effects.
    /// Has statement requirements based on tier.
    /// </summary>
    Signature,

    /// <summary>
    /// Advanced variant with enhanced effects or compound effects.
    /// Used for mid-tier cards with multiple resource generation.
    /// </summary>
    Advanced
}
