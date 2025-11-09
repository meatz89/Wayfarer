namespace Wayfarer.GameState.Enums;

/// <summary>
/// UNIVERSAL categorical property: Relationship temperature between player and NPC
///
/// Derivation: NPC.Bond value (low = Cold, mid = Warm, high = Passionate for love/hate extremes)
///
/// Scales:
/// - Social_maneuvering rewards: Warm (2x rapport), Cold (0.5x rapport)
/// - Negotiation rapport bonuses: Warm gives discounts
/// - Crisis empathy: Warm NPCs help more willingly
/// - Romance: Passionate enables intimate options
///
/// Used by ALL relationship-based situation archetypes.
/// </summary>
public enum EmotionalTone
{
    Cold,       // Professional, transactional, distant (low Bond 0-7)
    Warm,       // Friendly, trusting, comfortable (mid Bond 8-14)
    Passionate  // Intense emotions: love OR hate (high Bond 15+ or very negative)
}
