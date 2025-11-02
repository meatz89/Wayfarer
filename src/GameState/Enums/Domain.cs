namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Domain classification for situations and locations.
    /// Determines which archetypes are common in specific areas and which stats are tested.
    /// Creates learnable patterns: Economic → Negotiation/Diplomacy, Authority → Confrontation/Authority, etc.
    /// </summary>
    public enum Domain
    {
        /// <summary>
        /// Merchant quarters, markets, guilds, trade centers.
        /// Common archetypes: Negotiation, Social Maneuvering
        /// Key stats: Diplomacy, Rapport
        /// </summary>
        Economic,

        /// <summary>
        /// Government buildings, guard posts, courts, official institutions.
        /// Common archetypes: Confrontation, Negotiation
        /// Key stats: Authority, Intimidation
        /// </summary>
        Authority,

        /// <summary>
        /// Libraries, laboratories, scholarly societies, research centers.
        /// Common archetypes: Investigation, Negotiation
        /// Key stats: Insight, Cunning
        /// </summary>
        Mental,

        /// <summary>
        /// Taverns, noble estates, social gatherings, public events.
        /// Common archetypes: Social Maneuvering, Negotiation
        /// Key stats: Rapport, Cunning
        /// </summary>
        Social,

        /// <summary>
        /// Wilderness, dangerous areas, physical challenges, survival situations.
        /// Common archetypes: Confrontation, Investigation, Crisis
        /// Key stats: Physical challenges (no stat gates typically)
        /// </summary>
        Physical
    }
}
