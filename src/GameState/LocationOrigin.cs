/// <summary>
/// Explicit discriminator for location accessibility model.
///
/// CLEAN ARCHITECTURE: Replaces null-as-domain-meaning pattern.
/// Previously, Provenance == null meant "authored" which uses null as a domain concept.
/// This enum makes the domain concept explicit and type-safe.
///
/// ADR-012 Dual-Model Accessibility:
/// - Authored locations: ALWAYS accessible (TIER 1 No Soft-Locks)
/// - SceneCreated locations: Accessible only when scene grants access
/// </summary>
public enum LocationOrigin
{
    /// <summary>
    /// Location defined in base game content JSON.
    /// Always accessible per TIER 1 No Soft-Locks design pillar.
    /// Examples: Inn common room, market square, checkpoint.
    /// </summary>
    Authored,

    /// <summary>
    /// Location created dynamically by a scene during gameplay.
    /// Accessible only when an active scene's current situation is at this location.
    /// Examples: Private room (created after negotiating lodging), meeting chamber.
    /// </summary>
    SceneCreated
}
