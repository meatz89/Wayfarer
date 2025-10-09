using System.Collections.Generic;

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
    /// Card definitions - uses DTO for consistency
    /// </summary>
    public List<SocialCardDTO> Cards { get; set; }

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

    /// <summary>
    /// Investigation rewards - defines what observation cards are earned from Venue familiarity
    /// </summary>
    public List<ObservationRewardDTO> InvestigationRewards { get; set; }

    /// <summary>
    /// Standing obligations - uses existing StandingObligationDTO
    /// </summary>
    public List<StandingObligationDTO> StandingObligations { get; set; }

    /// <summary>
    /// Goals - strategic layer entities that define UI actions (replaces inline NPC requests)
    /// Universal across all three tactical systems (Social/Mental/Physical)
    /// </summary>
    public List<GoalDTO> Goals { get; set; }

    /// <summary>
    /// NPC goal cards - goal cards specific to NPCs (promises, connection tokens, etc)
    /// DEPRECATED: Being replaced by Goal system
    /// </summary>
    public List<GoalCardDTO> NpcGoalCards { get; set; }

    /// <summary>
    /// NPC requests - bundles of request and promise cards for one-time requests
    /// DEPRECATED: Being replaced by Goal system
    /// </summary>
    public List<NPCRequestDTO> NpcRequests { get; set; }

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
    /// Venue actions - uses LocationActionDTO for consistency
    /// </summary>
    public List<VenueActionDTO> LocationActions { get; set; }

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
    /// Investigation cards - DELETED (wrong architecture, will be replaced with MentalCardDTO in Phase 1)
    /// </summary>
    // public List<Wayfarer.Content.DTOs.InvestigationCardDTO> InvestigationCards { get; set; }

    /// <summary>
    /// Investigation templates - strategic multi-phase activities that orchestrate tactical systems
    /// V3: Now using InvestigationDTO with TacticalSystemType to spawn Social/Mental/Physical sessions
    /// </summary>
    public List<InvestigationDTO> Investigations { get; set; }

    /// <summary>
    /// Knowledge definitions - structured discoveries that connect investigations and enhance conversations
    /// Serves as connective tissue: unlocks investigations, unlocks goals, grants observation cards
    /// </summary>
    public List<KnowledgeDTO> Knowledge { get; set; }

    /// <summary>
    /// Travel obstacle definitions - challenges encountered during travel that require preparation (V2)
    /// </summary>
    public List<TravelObstacleDTO> TravelObstacles { get; set; }

    /// <summary>
    /// Mental cards for investigation system - parallel to conversation cards for mental tactical challenges
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
}