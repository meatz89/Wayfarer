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
    public List<ConversationCardDTO> Cards { get; set; }

    /// <summary>
    /// Card deck definitions for conversation types
    /// </summary>
    public List<CardDeckDTO> CardDecks { get; set; }

    /// <summary>
    /// Conversation type definitions that reference card decks
    /// </summary>
    public List<ConversationTypeDefinitionDTO> ConversationTypes { get; set; }

    /// <summary>
    /// NPC definitions - uses existing NPCDTO
    /// </summary>
    public List<NPCDTO> Npcs { get; set; }

    /// <summary>
    /// Location definitions - uses existing LocationDTO
    /// </summary>
    public List<LocationDTO> Locations { get; set; }

    /// <summary>
    /// Location spot definitions - uses existing LocationSpotDTO
    /// </summary>
    public List<LocationSpotDTO> Spots { get; set; }

    /// <summary>
    /// Route definitions - uses existing RouteDTO
    /// </summary>
    public List<RouteDTO> Routes { get; set; }

    /// <summary>
    /// Observation definitions - uses DTO for consistency
    /// </summary>
    public List<ObservationDTO> Observations { get; set; }

    /// <summary>
    /// Investigation rewards - defines what observation cards are earned from location familiarity
    /// </summary>
    public List<ObservationRewardDTO> InvestigationRewards { get; set; }

    /// <summary>
    /// Letter templates - uses existing LetterTemplateDTO
    /// </summary>
    public List<LetterTemplateDTO> LetterTemplates { get; set; }

    /// <summary>
    /// Standing obligations - uses existing StandingObligationDTO
    /// </summary>
    public List<StandingObligationDTO> StandingObligations { get; set; }

    /// <summary>
    /// NPC goal cards - goal cards specific to NPCs (promises, connection tokens, etc)
    /// </summary>
    public List<NPCGoalCardDTO> NpcGoalCards { get; set; }

    /// <summary>
    /// NPC requests - bundles of request and promise cards for one-time requests
    /// </summary>
    public List<NPCRequestDTO> NpcRequests { get; set; }

    /// <summary>
    /// NPC-specific request cards organized by NPC ID
    /// </summary>
    public Dictionary<string, List<ConversationCardDTO>> NpcRequestCards { get; set; }

    /// <summary>
    /// Promise cards - special cards that can force queue positions or make commitments
    /// </summary>
    public List<ConversationCardDTO> PromiseCards { get; set; }

    /// <summary>
    /// Exchange cards - cards that offer trades between resources
    /// </summary>
    public List<ConversationCardDTO> ExchangeCards { get; set; }

    /// <summary>
    /// Path cards for travel system
    /// </summary>
    public List<PathCardDTO> PathCards { get; set; }

    /// <summary>
    /// Items - uses existing ItemDTO
    /// </summary>
    public List<ItemDTO> Items { get; set; }

    /// <summary>
    /// Location actions - uses LocationActionDTO for consistency
    /// </summary>
    public List<LocationActionDTO> LocationActions { get; set; }

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

    // THREE PARALLEL TACTICAL SYSTEMS - NO UNIFIED CLASSES
    public List<SocialEngagementTypeDTO> SocialEngagementTypes { get; set; } = new List<SocialEngagementTypeDTO>();
    public List<ConversationEngagementDeckDTO> ConversationEngagementDecks { get; set; } = new List<ConversationEngagementDeckDTO>();
    public List<MentalEngagementTypeDTO> MentalEngagementTypes { get; set; } = new List<MentalEngagementTypeDTO>();
    public List<MentalEngagementDeckDTO> MentalEngagementDecks { get; set; } = new List<MentalEngagementDeckDTO>();
    public List<PhysicalEngagementTypeDTO> PhysicalEngagementTypes { get; set; } = new List<PhysicalEngagementTypeDTO>();
    public List<PhysicalEngagementDeckDTO> PhysicalEngagementDecks { get; set; } = new List<PhysicalEngagementDeckDTO>();
}