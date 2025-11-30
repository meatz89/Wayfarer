/// <summary>
/// Strongly-typed enum for scene archetype IDs.
/// Enables compile-time validation instead of runtime string matching.
/// Used by SceneArchetypeCatalog to generate scene structures.
/// HIGHLANDER: ONE enum for ALL scene archetypes (A-story, B-story, C-story)
/// </summary>
public enum SceneArchetypeType
{
    // ==================== SERVICE PATTERNS (4) ====================
    // Reusable patterns for transactional C-story content

    /// <summary>
    /// Inn lodging scene: negotiate → rest → depart
    /// </summary>
    InnLodging,

    /// <summary>
    /// Consequence reflection scene: single-situation acknowledgment
    /// </summary>
    ConsequenceReflection,

    /// <summary>
    /// Delivery contract scene: accept → travel → deliver
    /// </summary>
    DeliveryContract,

    /// <summary>
    /// Route segment travel scene: travel between locations
    /// </summary>
    RouteSegmentTravel,

    // ==================== NARRATIVE PATTERNS (9) ====================
    // Reusable patterns for A-story and B-story narrative content

    /// <summary>
    /// Player seeks audience with authority figure or important NPC.
    /// Pattern: negotiate_access → audience
    /// </summary>
    SeekAudience,

    /// <summary>
    /// Player investigates a specific location for clues.
    /// Pattern: search → analyze → conclude
    /// </summary>
    InvestigateLocation,

    /// <summary>
    /// Player gathers testimony from witnesses or informants.
    /// Pattern: approach → interview
    /// </summary>
    GatherTestimony,

    /// <summary>
    /// Player confronts the antagonist directly.
    /// Pattern: accuse → resolve
    /// </summary>
    ConfrontAntagonist,

    /// <summary>
    /// Player meets a member of an order or faction.
    /// Pattern: contact → negotiate → revelation
    /// </summary>
    MeetOrderMember,

    /// <summary>
    /// Player discovers a significant artifact.
    /// Pattern: locate → acquire
    /// </summary>
    DiscoverArtifact,

    /// <summary>
    /// Player uncovers evidence of conspiracy.
    /// Pattern: suspect → proof → expose → consequence
    /// </summary>
    UncoverConspiracy,

    /// <summary>
    /// Player faces an urgent decision with time pressure.
    /// Pattern: crisis → decision
    /// </summary>
    UrgentDecision,

    /// <summary>
    /// Player faces a moral crossroads with no clear right answer.
    /// Pattern: dilemma → choice → consequence
    /// </summary>
    MoralCrossroads,

    // ==================== PEACEFUL PATTERNS (3) ====================
    // Recovery-focused patterns for exhausted players (Resolve less than 3)
    // All choices positive, no requirements, stat grants only

    /// <summary>
    /// Quiet meditation and mental recovery scene.
    /// Pattern: settle → reflect (all positive outcomes)
    /// </summary>
    QuietReflection,

    /// <summary>
    /// Casual social interaction with locals.
    /// Pattern: encounter → converse (all positive outcomes)
    /// </summary>
    CasualEncounter,

    /// <summary>
    /// Scholarly study and knowledge gathering.
    /// Pattern: browse → study (all positive outcomes)
    /// </summary>
    ScholarlyPursuit
}
