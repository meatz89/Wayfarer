using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for player stats system content from JSON packages
/// </summary>
public static class PlayerStatParser
{
    /// <summary>
    /// Parse player stats configuration from PlayerStatsConfigDTO
    /// </summary>
    public static (List<PlayerStatDefinition> stats, StatProgression progression) ParseStatsPackage(PlayerStatsConfigDTO configDto)
    {
        var stats = new List<PlayerStatDefinition>();

        // Parse stat definitions
        if (configDto.Stats != null)
        {
            foreach (var statDto in configDto.Stats)
            {
                stats.Add(ConvertDTOToStatDefinition(statDto));
            }
        }

        // Parse progression rules
        StatProgression progression = null;
        if (configDto.Progression != null)
        {
            progression = ConvertDTOToProgression(configDto.Progression);
        }

        return (stats, progression);
    }

    /// <summary>
    /// Parse player stats configuration from a JSON package (legacy method)
    /// </summary>
    public static (List<PlayerStatDefinition> stats, StatProgression progression) ParseStatsPackage(JsonElement root)
    {
        var stats = new List<PlayerStatDefinition>();
        StatProgression progression = null;

        // Parse content section
        if (root.TryGetProperty("content", out JsonElement content))
        {
            // Parse stat definitions
            if (content.TryGetProperty("stats", out JsonElement statsElement))
            {
                foreach (JsonElement statElement in statsElement.EnumerateArray())
                {
                    var statDto = JsonSerializer.Deserialize<PlayerStatDefinitionDTO>(statElement.GetRawText());
                    if (statDto != null)
                    {
                        stats.Add(ConvertDTOToStatDefinition(statDto));
                    }
                }
            }

            // Parse progression rules
            if (content.TryGetProperty("progression", out JsonElement progressionElement))
            {
                var progressionDto = JsonSerializer.Deserialize<StatProgressionDTO>(progressionElement.GetRawText());
                if (progressionDto != null)
                {
                    progression = ConvertDTOToProgression(progressionDto);
                }
            }
        }

        return (stats, progression);
    }

    /// <summary>
    /// Convert PlayerStatDefinitionDTO to PlayerStatDefinition domain model
    /// </summary>
    private static PlayerStatDefinition ConvertDTOToStatDefinition(PlayerStatDefinitionDTO dto)
    {
        // Parse stat type from ID
        if (!Enum.TryParse<PlayerStatType>(dto.Id, true, out PlayerStatType statType))
        {
            throw new ArgumentException($"Invalid player stat ID: {dto.Id}");
        }

        return new PlayerStatDefinition
        {
            StatType = statType,
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            ConversationBenefit = dto.ConversationBenefit ?? "",
            InvestigationUnlock = dto.InvestigationUnlock ?? "",
            TravelUnlock = dto.TravelUnlock ?? ""
        };
    }

    /// <summary>
    /// Convert StatProgressionDTO to StatProgression domain model
    /// </summary>
    private static StatProgression ConvertDTOToProgression(StatProgressionDTO dto)
    {
        var progression = new StatProgression();

        // Set XP thresholds
        if (dto.XpThresholds != null)
        {
            progression.XpThresholds = new List<int>(dto.XpThresholds);
        }

        // Parse level bonuses
        if (dto.LevelBonuses != null)
        {
            progression.LevelBonuses = new List<StatLevelBonus>();
            foreach (var bonusDto in dto.LevelBonuses)
            {
                var bonus = new StatLevelBonus
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
                        case "ignores_failure_listen":
                            bonus.IgnoresFailureListen = true;
                            break;
                    }
                }

                progression.LevelBonuses.Add(bonus);
            }
        }

        return progression;
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
    public bool IgnoresFailureListen { get; set; }
    public string Description { get; set; } = "";
}

