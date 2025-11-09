using Wayfarer.Content.DTOs;

/// <summary>
/// Container for all game content in a package - uses DTOs consistently
/// </summary>
public class PackageContent
{
    /// <summary>
    /// Region definitions - top level geographic entities
    /// </summary>
    public List<RegionDTO> Regions { get; set; }

    /// <summary>
    /// District definitions - subdivisions of regions
    /// </summary>
    public List<DistrictDTO> Districts { get; set; }

    /// <summary>
    /// Hex map grid - spatial scaffolding for procedural world generation
    /// Defines terrain, danger levels, and location placement on hex grid
    /// Part of hex-based travel system
    /// NOTE: This is a single HexMapDTO (not a List) - one world grid per package
    /// </summary>
    public HexMapDTO HexMap { get; set; }

    /// <summary>
    /// Social card definitions - uses DTO for consistency
    /// </summary>
    public List<SocialCardDTO> SocialCards { get; set; }

    /// <summary>
    /// NPC definitions - uses existing NPCDTO
    /// </summary>
    public List<NPCDTO> Npcs { get; set; }

    /// <summary>
    /// Venue definitions - containers for locations
    /// </summary>
    public List<VenueDTO> Venues { get; set; }

    /// <summary>
    /// Venue location definitions - uses existing LocationDTO
    /// </summary>
    public List<LocationDTO> Locations { get; set; }

    /// <summary>
    /// Route definitions - uses existing RouteDTO
    /// </summary>
    public List<RouteDTO> Routes { get; set; }

    /// <summary>
    /// Observation definitions - uses DTO for consistency
    /// </summary>
    public List<ObservationDTO> Observations { get; set; }

    // ObligationRewards system eliminated - replaced by transparent resource competition

    /// <summary>
    /// Standing obligations - uses existing StandingObligationDTO
    /// </summary>
    public List<StandingObligationDTO> StandingObligations { get; set; }

    /// <summary>
    /// Situations - strategic layer entities that define UI actions (replaces inline NPC requests)
    /// Universal across all three tactical systems (Social/Mental/Physical)
    /// </summary>
    public List<SituationDTO> Situations { get; set; }

    /// <summary>
    /// Promise cards - special cards that can force queue positions or make commitments
    /// </summary>
    public List<SocialCardDTO> PromiseCards { get; set; }

    /// <summary>
    /// Exchange cards - cards that offer trades between resources
    /// </summary>
    public List<SocialCardDTO> ExchangeCards { get; set; }

    /// <summary>
    /// Path cards for travel system
    /// </summary>
    public List<PathCardDTO> PathCards { get; set; }

    /// <summary>
    /// Items - uses existing ItemDTO
    /// </summary>
    public List<ItemDTO> Items { get; set; }

    /// <summary>
    /// Location actions - location-specific actions matched by properties
    /// </summary>
    public List<LocationActionDTO> LocationActions { get; set; }

    /// <summary>
    /// Player actions - global actions available everywhere (e.g., check belongings)
    /// </summary>
    public List<PlayerActionDTO> PlayerActions { get; set; }

    /// <summary>
    /// Deck compositions - defines how many copies of each card in decks
    /// </summary>
    public DeckCompositionDTO DeckCompositions { get; set; }

    /// <summary>
    /// Exchange definitions - defines trade details for exchange cards
    /// </summary>
    public List<ExchangeDTO> Exchanges { get; set; }

    /// <summary>
    /// Path card collections for FixedPath segments
    /// </summary>
    public List<PathCardCollectionDTO> PathCardCollections { get; set; }

    /// <summary>
    /// Travel events for the normalized event system
    /// </summary>
    public List<TravelEventDTO> TravelEvents { get; set; }

    /// <summary>
    /// Event cards for the normalized event system (reuses PathCardDTO structure)
    /// </summary>
    public List<PathCardDTO> EventCards { get; set; }

    /// <summary>
    /// Dialogue templates for generating conversation text
    /// </summary>
    public DialogueTemplates DialogueTemplates { get; set; }

    /// <summary>
    /// Player stats configuration for progression system
    /// </summary>
    public PlayerStatsConfigDTO PlayerStatsConfig { get; set; }

    /// <summary>
    /// Stranger NPCs for practice conversations
    /// </summary>
    public List<StrangerNPCDTO> Strangers { get; set; }

    /// <summary>
    /// Listen draw counts per connection state for conversation mechanics
    /// </summary>
    public Dictionary<string, int> ListenDrawCounts { get; set; }

    /// <summary>
    /// Obligation cards - DELETED (wrong architecture, will be replaced with MentalCardDTO in Phase 1)
    /// </summary>
    // public List<Wayfarer.Content.DTOs.ObligationCardDTO> ObligationCards { get; set; }

    /// <summary>
    /// Obligation templates - strategic multi-phase activities that orchestrate tactical systems
    /// V3: Now using ObligationDTO with TacticalSystemType to spawn Social/Mental/Physical sessions
    /// </summary>
    public List<ObligationDTO> Obligations { get; set; }

    /// <summary>
    /// Travel scene definitions - challenges encountered during travel that require preparation (V2)
    /// </summary>
    public List<TravelSceneDTO> TravelScenes { get; set; }

    /// <summary>
    /// Mental cards for obligation system - parallel to conversation cards for mental tactical challenges
    /// </summary>
    public List<MentalCardDTO> MentalCards { get; set; }

    /// <summary>
    /// Physical cards for physical challenge system - parallel to mental cards for physical tactical challenges
    /// </summary>
    public List<PhysicalCardDTO> PhysicalCards { get; set; }

    // THREE PARALLEL TACTICAL SYSTEMS - Decks only (no Types, they're redundant)
    public List<SocialChallengeDeckDTO> SocialChallengeDecks { get; set; } = new List<SocialChallengeDeckDTO>();
    public List<MentalChallengeDeckDTO> MentalChallengeDecks { get; set; } = new List<MentalChallengeDeckDTO>();
    public List<PhysicalChallengeDeckDTO> PhysicalChallengeDecks { get; set; } = new List<PhysicalChallengeDeckDTO>();

    // NOTE: Old SceneDTO system removed - NEW Scene-Situation architecture
    // Scenes now spawn via Situation spawn rewards (SceneSpawnReward) instead of package-level definitions

    /// <summary>
    /// Conversation tree definitions - simple dialogue trees that can escalate to Social challenges
    /// </summary>
    public List<ConversationTreeDTO> ConversationTrees { get; set; } = new List<ConversationTreeDTO>();

    /// <summary>
    /// Observation scene definitions - scene investigation with multiple examination points
    /// </summary>
    public List<ObservationSceneDTO> ObservationScenes { get; set; } = new List<ObservationSceneDTO>();

    /// <summary>
    /// Emergency situation definitions - urgent situations demanding immediate response
    /// </summary>
    public List<EmergencySituationDTO> EmergencySituations { get; set; } = new List<EmergencySituationDTO>();

    // SCENE-SITUATION ARCHITECTURE (Sir Brante Integration)

    /// <summary>
    /// State definitions - metadata about temporary player conditions (Physical/Mental/Social)
    /// Defines blocked actions, enabled actions, clear conditions, and duration
    /// </summary>
    public List<StateDTO> States { get; set; } = new List<StateDTO>();

    /// <summary>
    /// Achievement definitions - milestone templates with grant conditions
    /// Tracks player accomplishments across categories (Combat/Social/Investigation/Economic/Political)
    /// </summary>
    public List<AchievementDTO> Achievements { get; set; } = new List<AchievementDTO>();

    /// <summary>
    /// Scene templates - immutable archetypes for procedural narrative spawning
    /// Defines multi-situation branching narratives with placement filters (Sir Brante pattern)
    /// </summary>
    public List<SceneTemplateDTO> SceneTemplates { get; set; } = new List<SceneTemplateDTO>();

    /// <summary>
    /// Scene instances - runtime instances spawned from templates with concrete placements
    /// Generated dynamically at spawn time, stored in dynamic packages
    /// DISTINCTION: SceneTemplates = reusable blueprints, Scenes = specific playthrough instances
    /// </summary>
    public List<SceneDTO> Scenes { get; set; } = new List<SceneDTO>();
}