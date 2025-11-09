
/// <summary>
/// UNIVERSAL categorical property: Who is observing this interaction?
///
/// Derivation: Location properties (public, private, marketplace, court, secluded)
///
/// Scales:
/// - Social_maneuvering: Reputation impact of success/failure
/// - Negotiation: Face-saving costs (public deals more expensive)
/// - Reputation_challenge: Penalty severity
/// - Romance: Intimacy level available (can't be intimate in public)
///
/// Used by ALL social situation archetypes.
/// </summary>
public enum SocialStakes
{
Private,    // One-on-one, no witnesses, no reputation impact
Witnessed,  // Small group aware, minor reputation effects
Public      // Many observers, major reputation impact, face-saving required
}
