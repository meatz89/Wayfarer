using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for player stat definitions from JSON packages
/// </summary>
public class PlayerStatDefinitionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConversationBenefit { get; set; }
    public string ObligationUnlock { get; set; }
    public string TravelUnlock { get; set; }
}

/// <summary>
/// DTO for stat progression rules from JSON packages
/// </summary>
public class StatProgressionDTO
{
    public List<int> XpThresholds { get; set; }
    public List<StatLevelBonusDTO> LevelBonuses { get; set; }
}

/// <summary>
/// DTO for individual stat level bonuses
/// </summary>
public class StatLevelBonusDTO
{
    public int Level { get; set; }
    public int SuccessBonus { get; set; }
    public string Effect { get; set; }
    public string Description { get; set; }
}