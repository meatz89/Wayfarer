using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Parser for player stats system content from JSON packages
/// </summary>
public static class PlayerStatParser
{
    /// <summary>
    /// Parse player stats configuration from PlayerStatsConfigDTO
    /// </summary>
    public static PlayerStatsParseResult ParseStatsPackage(PlayerStatsConfigDTO configDto)
    {
        List<PlayerStatDefinition> stats = new List<PlayerStatDefinition>();

        // Parse stat definitions
        if (configDto.Stats != null)
        {
            foreach (PlayerStatDefinitionDTO statDto in configDto.Stats)
            {
                // Parse stat type from ID
                if (!Enum.TryParse<PlayerStatType>(statDto.Id, true, out PlayerStatType statType))
                {
                    throw new ArgumentException($"Invalid player stat ID: {statDto.Id}");
                }

                stats.Add(new PlayerStatDefinition
                {
                    StatType = statType,
                    Name = statDto.Name ?? "",
                    Description = statDto.Description ?? "",
                    ConversationBenefit = statDto.ConversationBenefit ?? "",
                    InvestigationUnlock = statDto.InvestigationUnlock ?? "",
                    TravelUnlock = statDto.TravelUnlock ?? ""
                });
            }
        }

        // Parse progression rules
        StatProgression progression = null;
        if (configDto.Progression != null)
        {
            progression = new StatProgression();

            // Set XP thresholds
            if (configDto.Progression.XpThresholds != null)
            {
                progression.XpThresholds = new List<int>(configDto.Progression.XpThresholds);
            }

            // Parse level bonuses
            if (configDto.Progression.LevelBonuses != null)
            {
                progression.LevelBonuses = new List<StatLevelBonus>();
                foreach (StatLevelBonusDTO bonusDto in configDto.Progression.LevelBonuses)
                {
                    StatLevelBonus bonus = new StatLevelBonus
                    {
                        Level = bonusDto.Level,
                        SuccessBonus = bonusDto.SuccessBonus,
                        Description = bonusDto.Description ?? ""
                    };

                    // Parse special effects
                    if (!string.IsNullOrEmpty(bonusDto.Effect))
                    {
                        switch (bonusDto.Effect.ToLower())
                        {
                            case "gains_thought_persistence":
                                bonus.GainsThoughtPersistence = true;
                                break;
                        }
                    }

                    progression.LevelBonuses.Add(bonus);
                }
            }
        }

        return new PlayerStatsParseResult(stats, progression);
    }



}

/// <summary>
/// Domain model for player stat definitions
/// </summary>
public class PlayerStatDefinition
{
    public PlayerStatType StatType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConversationBenefit { get; set; }
    public string InvestigationUnlock { get; set; }
    public string TravelUnlock { get; set; }
}

/// <summary>
/// Domain model for stat progression rules
/// </summary>
public class StatProgression
{
    public List<int> XpThresholds { get; set; } = new();
    public List<StatLevelBonus> LevelBonuses { get; set; } = new();
}

/// <summary>
/// Domain model for stat level bonuses
/// </summary>
public class StatLevelBonus
{
    public int Level { get; set; }
    public int SuccessBonus { get; set; }
    public bool GainsThoughtPersistence { get; set; }
    public string Description { get; set; }
}

