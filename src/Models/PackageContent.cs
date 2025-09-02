using System.Collections.Generic;

/// <summary>
/// Container for all game content in a package - uses DTOs consistently
/// </summary>
public class PackageContent
{
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
    /// Letter templates - uses existing LetterTemplateDTO
    /// </summary>
    public List<LetterTemplateDTO> LetterTemplates { get; set; }
    
    /// <summary>
    /// Standing obligations - uses existing StandingObligationDTO
    /// </summary>
    public List<StandingObligationDTO> StandingObligations { get; set; }
    
    /// <summary>
    /// Travel cards - uses existing TravelCard
    /// </summary>
    public List<TravelCard> TravelCards { get; set; }
    
    /// <summary>
    /// Items - uses existing ItemDTO
    /// </summary>
    public List<ItemDTO> Items { get; set; }
}