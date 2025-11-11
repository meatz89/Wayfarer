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

        // Parse stat definitions - configDto.Stats has inline init in DTO, but validate required data
        if (configDto.Stats == null)
            throw new InvalidDataException("PlayerStatsConfig missing required field 'Stats'");

        foreach (PlayerStatDefinitionDTO statDto in configDto.Stats)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(statDto.Id))
                throw new InvalidDataException("PlayerStatDefinition missing required field 'Id'");
            if (string.IsNullOrEmpty(statDto.Name))
                throw new InvalidDataException($"PlayerStatDefinition '{statDto.Id}' missing required field 'Name'");
            if (string.IsNullOrEmpty(statDto.Description))
                throw new InvalidDataException($"PlayerStatDefinition '{statDto.Id}' missing required field 'Description'");

            // Parse stat type from ID
            if (!Enum.TryParse<PlayerStatType>(statDto.Id, true, out PlayerStatType statType))
            {
                throw new InvalidDataException($"Invalid player stat ID: {statDto.Id}");
            }

            stats.Add(new PlayerStatDefinition
            {
                StatType = statType,
                Name = statDto.Name,
                Description = statDto.Description,
                ConversationBenefit = statDto.ConversationBenefit ?? "", // Optional - defaults to empty if missing
                ObligationUnlock = statDto.ObligationUnlock ?? "", // Optional - defaults to empty if missing
                TravelUnlock = statDto.TravelUnlock ?? "" // Optional - defaults to empty if missing
            });
        }

        // Parse progression rules - optional section
        StatProgression progression = null;
        if (configDto.Progression != null)
        {
            progression = new StatProgression();

            // Set XP thresholds - optional field
            if (configDto.Progression.XpThresholds != null)
            {
                progression.XpThresholds = new List<int>(configDto.Progression.XpThresholds);
            }

            // Parse level bonuses - optional field
            if (configDto.Progression.LevelBonuses != null)
            {
                progression.LevelBonuses = new List<StatLevelBonus>();
                foreach (StatLevelBonusDTO bonusDto in configDto.Progression.LevelBonuses)
                {
                    StatLevelBonus bonus = new StatLevelBonus
                    {
                        Level = bonusDto.Level,
                        SuccessBonus = bonusDto.SuccessBonus,
                        Description = bonusDto.Description ?? "" // Optional - defaults to empty if missing
                    };

                    // Parse special effects - optional field, only set if present
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
    public string ObligationUnlock { get; set; }
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

