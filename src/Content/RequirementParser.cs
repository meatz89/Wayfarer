/// <summary>
/// Parser for converting CompoundRequirement DTOs to domain models
/// Handles OR-based unlocking system for situations
/// Uses Explicit Property Principle: each requirement type maps to a named property
/// FAIL-FAST: Invalid content crashes immediately at parse-time with clear stack traces
/// </summary>
public static class RequirementParser
{
    /// <summary>
    /// Convert a CompoundRequirementDTO to a CompoundRequirement domain model
    /// GameWorld is REQUIRED for entity resolution - no null allowed
    /// </summary>
    public static CompoundRequirement ConvertDTOToCompoundRequirement(CompoundRequirementDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            return new CompoundRequirement();

        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = ParseOrPaths(dto.OrPaths, gameWorld)
        };

        return requirement;
    }

    private static List<OrPath> ParseOrPaths(List<OrPathDTO> dtos, GameWorld gameWorld)
    {
        if (dtos == null || !dtos.Any())
            return new List<OrPath>();

        List<OrPath> orPaths = new List<OrPath>();
        foreach (OrPathDTO dto in dtos)
        {
            OrPath path = ConvertDTOToOrPath(dto, gameWorld);
            orPaths.Add(path);
        }

        return orPaths;
    }

    private static OrPath ConvertDTOToOrPath(OrPathDTO dto, GameWorld gameWorld)
    {
        OrPath path = new OrPath
        {
            Label = dto.Label ?? GenerateDefaultLabel(dto),

            InsightRequired = dto.InsightRequired,
            RapportRequired = dto.RapportRequired,
            AuthorityRequired = dto.AuthorityRequired,
            DiplomacyRequired = dto.DiplomacyRequired,
            CunningRequired = dto.CunningRequired,

            ResolveRequired = dto.ResolveRequired,
            CoinsRequired = dto.CoinsRequired,

            SituationCountRequired = dto.SituationCountRequired,

            BondNpc = ResolveNPC(dto.BondNpcName, gameWorld),
            BondStrengthRequired = dto.BondStrengthRequired,

            ScaleType = ParseScaleType(dto.ScaleTypeName),
            ScaleValueRequired = dto.ScaleValueRequired,

            RequiredAchievement = ResolveAchievement(dto.RequiredAchievementName, gameWorld),
            RequiredState = ParseStateType(dto.RequiredStateName),
            RequiredItem = ResolveItem(dto.RequiredItemName, gameWorld)
        };

        return path;
    }

    /// <summary>
    /// Resolve NPC by name - FAIL-FAST if name provided but NPC not found
    /// </summary>
    private static NPC ResolveNPC(string npcName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(npcName))
            return null;

        return gameWorld.NPCs.First(n => n.Name == npcName);
    }

    /// <summary>
    /// Resolve Achievement by name - creates if not found (achievements are content definitions)
    /// </summary>
    private static Achievement ResolveAchievement(string achievementName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(achievementName))
            return null;

        Achievement achievement = gameWorld.Achievements.FirstOrDefault(a => a.Name == achievementName);
        if (achievement == null)
        {
            achievement = new Achievement { Name = achievementName };
            gameWorld.Achievements.Add(achievement);
        }
        return achievement;
    }

    /// <summary>
    /// Resolve Item by name - FAIL-FAST if name provided but Item not found
    /// </summary>
    private static Item ResolveItem(string itemName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        return gameWorld.Items.First(i => i.Name == itemName);
    }

    /// <summary>
    /// Parse ScaleType enum - FAIL-FAST on invalid values
    /// </summary>
    private static ScaleType? ParseScaleType(string scaleTypeName)
    {
        if (string.IsNullOrEmpty(scaleTypeName))
            return null;

        return Enum.Parse<ScaleType>(scaleTypeName, true);
    }

    /// <summary>
    /// Parse StateType enum - FAIL-FAST on invalid values
    /// </summary>
    private static StateType? ParseStateType(string stateTypeName)
    {
        if (string.IsNullOrEmpty(stateTypeName))
            return null;

        return Enum.Parse<StateType>(stateTypeName, true);
    }

    private static string GenerateDefaultLabel(OrPathDTO dto)
    {
        List<string> parts = new List<string>();

        if (dto.InsightRequired.HasValue) parts.Add($"Insight {dto.InsightRequired.Value}+");
        if (dto.RapportRequired.HasValue) parts.Add($"Rapport {dto.RapportRequired.Value}+");
        if (dto.AuthorityRequired.HasValue) parts.Add($"Authority {dto.AuthorityRequired.Value}+");
        if (dto.DiplomacyRequired.HasValue) parts.Add($"Diplomacy {dto.DiplomacyRequired.Value}+");
        if (dto.CunningRequired.HasValue) parts.Add($"Cunning {dto.CunningRequired.Value}+");

        if (dto.ResolveRequired.HasValue) parts.Add($"Resolve {dto.ResolveRequired.Value}+");
        if (dto.CoinsRequired.HasValue) parts.Add($"{dto.CoinsRequired.Value} coins");

        if (dto.SituationCountRequired.HasValue) parts.Add($"{dto.SituationCountRequired.Value} situations completed");

        if (!string.IsNullOrEmpty(dto.BondNpcName) && dto.BondStrengthRequired.HasValue)
            parts.Add($"Bond {dto.BondStrengthRequired.Value}+ with {dto.BondNpcName}");

        if (!string.IsNullOrEmpty(dto.ScaleTypeName) && dto.ScaleValueRequired.HasValue)
        {
            string direction = dto.ScaleValueRequired.Value >= 0 ? "+" : "";
            parts.Add($"{dto.ScaleTypeName} {direction}{dto.ScaleValueRequired.Value}");
        }

        if (!string.IsNullOrEmpty(dto.RequiredAchievementName)) parts.Add($"Achievement: {dto.RequiredAchievementName}");
        if (!string.IsNullOrEmpty(dto.RequiredStateName)) parts.Add($"State: {dto.RequiredStateName}");
        if (!string.IsNullOrEmpty(dto.RequiredItemName)) parts.Add($"Item: {dto.RequiredItemName}");

        return parts.Count > 0 ? string.Join(", ", parts) : "Unlabeled Path";
    }
}
