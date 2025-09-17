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
    /// NPC-specific progression cards organized by NPC ID
    /// </summary>
    public Dictionary<string, List<ConversationCardDTO>> NpcProgressionCards { get; set; }

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
}