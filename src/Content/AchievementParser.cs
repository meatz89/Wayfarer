/// <summary>
/// Parser for converting AchievementDTO to Achievement domain model
/// </summary>
public static class AchievementParser
{
    /// <summary>
    /// Convert an AchievementDTO to an Achievement domain model
    /// </summary>
    public static Achievement ConvertDTOToAchievement(AchievementDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Achievement DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Category))
            throw new InvalidOperationException($"Achievement {dto.Id} missing required 'Category' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Achievement {dto.Id} missing required 'Name' field");

        // Parse category
        if (!Enum.TryParse<AchievementCategory>(dto.Category, true, out AchievementCategory category))
        {
            throw new InvalidOperationException($"Achievement {dto.Id} has invalid Category value: '{dto.Category}'. Must be valid AchievementCategory enum value.");
        }

        // Parse grant conditions from Dictionary to strongly-typed structure
        AchievementGrantConditions grantConditions = ParseGrantConditions(dto.GrantConditions);

        Achievement achievement = new Achievement
        {
            Id = dto.Id,
            Category = category,
            Name = dto.Name,
            Description = dto.Description ?? "",
            Icon = dto.Icon ?? "",
            GrantConditions = grantConditions
        };

        return achievement;
    }

    /// <summary>
    /// Parse grant conditions from DTO Dictionary to strongly-typed AchievementGrantConditions
    /// Replaces generic Dictionary pattern with concrete properties
    /// </summary>
    private static AchievementGrantConditions ParseGrantConditions(Dictionary<string, int> conditions)
    {
        if (conditions == null || !conditions.Any())
            return new AchievementGrantConditions();

        AchievementGrantConditions grantConditions = new AchievementGrantConditions();

        foreach (KeyValuePair<string, int> condition in conditions)
        {
            switch (condition.Key.ToLowerInvariant())
            {
                case "bondstrengthwithanynpc":
                    grantConditions.BondStrengthWithAnyNpc = condition.Value;
                    break;
                case "completedobligations":
                    grantConditions.CompletedObligations = condition.Value;
                    break;
                case "completedsocialsituations":
                    grantConditions.CompletedSocialSituations = condition.Value;
                    break;
                case "completedmentalsituations":
                    grantConditions.CompletedMentalSituations = condition.Value;
                    break;
                case "completedphysicalsituations":
                    grantConditions.CompletedPhysicalSituations = condition.Value;
                    break;
                case "coinsearned":
                    grantConditions.CoinsEarned = condition.Value;
                    break;
                case "moralityscale":
                    grantConditions.MoralityScale = condition.Value;
                    break;
                case "lawfulnessscale":
                    grantConditions.LawfulnessScale = condition.Value;
                    break;
                case "methodscale":
                    grantConditions.MethodScale = condition.Value;
                    break;
                case "cautionscale":
                    grantConditions.CautionScale = condition.Value;
                    break;
                case "transparencyscale":
                    grantConditions.TransparencyScale = condition.Value;
                    break;
                case "famescale":
                    grantConditions.FameScale = condition.Value;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown grant condition key: '{condition.Key}'");
            }
        }

        return grantConditions;
    }

    /// <summary>
    /// Parse all achievements from AchievementDTO list
    /// </summary>
    public static List<Achievement> ParseAchievements(List<AchievementDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<Achievement>();

        List<Achievement> achievements = new List<Achievement>();
        foreach (AchievementDTO dto in dtos)
        {
            try
            {
                Achievement achievement = ConvertDTOToAchievement(dto);
                achievements.Add(achievement);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse achievement '{dto?.Id}': {ex.Message}", ex);
            }
        }

        return achievements;
    }
}
