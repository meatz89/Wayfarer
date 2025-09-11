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
}