/// <summary>
/// Parser for converting CompoundRequirement DTOs to domain models
/// Handles OR-based unlocking system for situations
/// Uses Explicit Property Principle: each requirement type maps to a named property
/// See arc42/08_crosscutting_concepts.md §8.19
/// </summary>
public static class RequirementParser
{
    /// <summary>
    /// Convert a CompoundRequirementDTO to a CompoundRequirement domain model
    /// Uses null for entity references (NPCs, Achievements, Items) - for templates without GameWorld access
    /// </summary>
    public static CompoundRequirement ConvertDTOToCompoundRequirement(CompoundRequirementDTO dto)
    {
        return ConvertDTOToCompoundRequirement(dto, null);
    }

    /// <summary>
    /// Convert a CompoundRequirementDTO to a CompoundRequirement domain model
    /// Optionally uses GameWorld for entity lookups (NPCs, Achievements, Items)
    /// </summary>
    public static CompoundRequirement ConvertDTOToCompoundRequirement(CompoundRequirementDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            return new CompoundRequirement(); // Empty requirement = always unlocked

        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = ParseOrPaths(dto.OrPaths, gameWorld)
        };

        return requirement;
    }

    /// <summary>
    /// Parse a list of OR paths from DTOs
    /// Each path represents one complete way to unlock the situation
    /// </summary>
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

    /// <summary>
    /// Convert an OrPathDTO to an OrPath domain model
    /// Maps explicit DTO properties directly to domain properties
    /// Resolves string names to object references (NPCs, Achievements, Items)
    /// </summary>
    private static OrPath ConvertDTOToOrPath(OrPathDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            throw new InvalidOperationException("OrPath DTO cannot be null");

        OrPath path = new OrPath
        {
            // Label
            Label = dto.Label ?? GenerateDefaultLabel(dto),

            // Stat requirements (direct mapping)
            InsightRequired = dto.InsightRequired,
            RapportRequired = dto.RapportRequired,
            AuthorityRequired = dto.AuthorityRequired,
            DiplomacyRequired = dto.DiplomacyRequired,
            CunningRequired = dto.CunningRequired,

            // Resource requirements (direct mapping)
            ResolveRequired = dto.ResolveRequired,
            CoinsRequired = dto.CoinsRequired,

            // Progression requirements (direct mapping)
            SituationCountRequired = dto.SituationCountRequired,

            // Relationship requirements (resolve NPC by name)
            BondNpc = ResolveNPC(dto.BondNpcName, gameWorld),
            BondStrengthRequired = dto.BondStrengthRequired,

            // Scale requirements (parse enum from string)
            ScaleType = ParseScaleType(dto.ScaleTypeName),
            ScaleValueRequired = dto.ScaleValueRequired,

            // Boolean requirements (resolve by name)
            RequiredAchievement = ResolveAchievement(dto.RequiredAchievementName, gameWorld),
            RequiredState = ParseStateType(dto.RequiredStateName),
            RequiredItem = ResolveItem(dto.RequiredItemName, gameWorld)
        };

        return path;
    }

    /// <summary>
    /// Resolve NPC by name from GameWorld
    /// Returns null if name is empty, gameWorld is null, or NPC not found
    /// </summary>
    private static NPC ResolveNPC(string npcName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(npcName) || gameWorld == null)
            return null;

        NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.Name == npcName);
        if (npc == null)
        {
            Console.WriteLine($"[RequirementParser] WARNING: NPC '{npcName}' not found for bond requirement");
        }
        return npc;
    }

    /// <summary>
    /// Resolve Achievement by name from GameWorld
    /// Returns null if name is empty or gameWorld is null
    /// Creates achievement if not found (achievements are content definitions)
    /// </summary>
    private static Achievement ResolveAchievement(string achievementName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(achievementName) || gameWorld == null)
            return null;

        Achievement achievement = gameWorld.Achievements.FirstOrDefault(a => a.Name == achievementName);
        if (achievement == null)
        {
            // Create achievement if not found (achievements are content definitions)
            achievement = new Achievement { Name = achievementName };
            gameWorld.Achievements.Add(achievement);
            Console.WriteLine($"[RequirementParser] Created achievement '{achievementName}' for requirement");
        }
        return achievement;
    }

    /// <summary>
    /// Resolve Item by name from GameWorld
    /// Returns null if name is empty, gameWorld is null, or Item not found
    /// </summary>
    private static Item ResolveItem(string itemName, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(itemName) || gameWorld == null)
            return null;

        Item item = gameWorld.Items.FirstOrDefault(i => i.Name == itemName);
        if (item == null)
        {
            Console.WriteLine($"[RequirementParser] WARNING: Item '{itemName}' not found for requirement");
        }
        return item;
    }

    /// <summary>
    /// Parse ScaleType enum from string
    /// Returns null if name is empty or invalid
    /// </summary>
    private static ScaleType? ParseScaleType(string scaleTypeName)
    {
        if (string.IsNullOrEmpty(scaleTypeName))
            return null;

        if (Enum.TryParse<ScaleType>(scaleTypeName, true, out ScaleType scaleType))
            return scaleType;

        Console.WriteLine($"[RequirementParser] WARNING: Invalid ScaleType '{scaleTypeName}'");
        return null;
    }

    /// <summary>
    /// Parse StateType enum from string
    /// Returns null if name is empty or invalid
    /// </summary>
    private static StateType? ParseStateType(string stateTypeName)
    {
        if (string.IsNullOrEmpty(stateTypeName))
            return null;

        if (Enum.TryParse<StateType>(stateTypeName, true, out StateType stateType))
            return stateType;

        Console.WriteLine($"[RequirementParser] WARNING: Invalid StateType '{stateTypeName}'");
        return null;
    }

    /// <summary>
    /// Generate a default label if none provided in JSON
    /// Uses explicit property values to describe the requirement
    /// </summary>
    private static string GenerateDefaultLabel(OrPathDTO dto)
    {
        List<string> parts = new List<string>();

        // Stats
        if (dto.InsightRequired.HasValue) parts.Add($"Insight {dto.InsightRequired.Value}+");
        if (dto.RapportRequired.HasValue) parts.Add($"Rapport {dto.RapportRequired.Value}+");
        if (dto.AuthorityRequired.HasValue) parts.Add($"Authority {dto.AuthorityRequired.Value}+");
        if (dto.DiplomacyRequired.HasValue) parts.Add($"Diplomacy {dto.DiplomacyRequired.Value}+");
        if (dto.CunningRequired.HasValue) parts.Add($"Cunning {dto.CunningRequired.Value}+");

        // Resources
        if (dto.ResolveRequired.HasValue) parts.Add($"Resolve {dto.ResolveRequired.Value}+");
        if (dto.CoinsRequired.HasValue) parts.Add($"{dto.CoinsRequired.Value} coins");

        // Progression
        if (dto.SituationCountRequired.HasValue) parts.Add($"{dto.SituationCountRequired.Value} situations completed");

        // Relationship
        if (!string.IsNullOrEmpty(dto.BondNpcName) && dto.BondStrengthRequired.HasValue)
            parts.Add($"Bond {dto.BondStrengthRequired.Value}+ with {dto.BondNpcName}");

        // Scale
        if (!string.IsNullOrEmpty(dto.ScaleTypeName) && dto.ScaleValueRequired.HasValue)
        {
            string direction = dto.ScaleValueRequired.Value >= 0 ? "+" : "";
            parts.Add($"{dto.ScaleTypeName} {direction}{dto.ScaleValueRequired.Value}");
        }

        // Boolean requirements
        if (!string.IsNullOrEmpty(dto.RequiredAchievementName)) parts.Add($"Achievement: {dto.RequiredAchievementName}");
        if (!string.IsNullOrEmpty(dto.RequiredStateName)) parts.Add($"State: {dto.RequiredStateName}");
        if (!string.IsNullOrEmpty(dto.RequiredItemName)) parts.Add($"Item: {dto.RequiredItemName}");

        return parts.Count > 0 ? string.Join(", ", parts) : "Unlabeled Path";
    }
}
