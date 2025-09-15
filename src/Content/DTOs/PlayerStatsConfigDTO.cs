using System.Collections.Generic;

/// <summary>
/// DTO for player stats configuration package
/// </summary>
public class PlayerStatsConfigDTO
{
    public string PackageId { get; set; }
    public PackageMetadata Metadata { get; set; }
    public List<PlayerStatDefinitionDTO> Stats { get; set; }
    public StatProgressionDTO Progression { get; set; }
}