/// <summary>
/// Parser for PlacementFilter - converts DTOs to domain models
/// Extracts categorical properties and validates enum values
/// FAIL-FAST: Invalid enums throw immediately at parse time
/// </summary>
public static class PlacementFilterParser
{
    /// <summary>
    /// Parse PlacementFilter from DTO
    /// </summary>
    /// <param name="dto">PlacementFilter DTO from JSON</param>
    /// <param name="contextId">Context identifier for error messages (template ID or instance path)</param>
    public static PlacementFilter Parse(PlacementFilterDTO dto, string contextId, GameWorld gameWorld = null)
    {
        if (dto == null)
            return null; // Optional - some SceneTemplates may not have filters

        // Validate PlacementType
        if (string.IsNullOrEmpty(dto.PlacementType))
            throw new InvalidDataException($"PlacementFilter in '{contextId}' missing required 'PlacementType' field");

        if (!Enum.TryParse<PlacementType>(dto.PlacementType, true, out PlacementType placementType))
        {
            throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid PlacementType: '{dto.PlacementType}'");
        }

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = placementType,
            // System control
            SelectionStrategy = ParseSelectionStrategy(dto.SelectionStrategy, contextId),
            // NPC filters - SINGULAR properties
            PersonalityType = ParsePersonalityType(dto.PersonalityType, contextId),
            Profession = ParseProfession(dto.Profession, contextId),
            RequiredRelationship = ParseNPCRelationship(dto.RequiredRelationship, contextId),
            MinBond = dto.MinBond,
            MaxBond = dto.MaxBond,
            NpcTags = dto.NpcTags,
            // Orthogonal categorical dimensions - NPC - SINGULAR
            SocialStanding = ParseSocialStanding(dto.SocialStanding, contextId),
            StoryRole = ParseStoryRole(dto.StoryRole, contextId),
            KnowledgeLevel = ParseKnowledgeLevel(dto.KnowledgeLevel, contextId),
            // Location filters - SINGULAR properties (orthogonal)
            LocationRole = ParseLocationRole(dto.Role, contextId),
            IsPlayerAccessible = dto.IsPlayerAccessible,
            // Orthogonal categorical dimensions - Location - SINGULAR
            Privacy = ParsePrivacy(dto.Privacy, contextId),
            Safety = ParseSafety(dto.Safety, contextId),
            Activity = ParseActivity(dto.Activity, contextId),
            Purpose = ParsePurpose(dto.Purpose, contextId),
            DistrictName = dto.DistrictName,
            RegionName = dto.RegionName,
            // Route filters - SINGULAR (orthogonal)
            Terrain = ParseTerrainType(dto.Terrain, contextId),
            Structure = ParseStructureType(dto.Structure, contextId),
            MinDifficulty = dto.MinDifficulty,
            MaxDifficulty = dto.MaxDifficulty,
            RouteTags = dto.RouteTags,
            SegmentIndex = dto.SegmentIndex, // Route segment placement for geographic specificity
            // Variety control
            ExcludeRecentlyUsed = dto.ExcludeRecentlyUsed,
            // Player state filters (still lists - player can have multiple states)
            RequiredStates = ParseStateTypes(dto.RequiredStates, contextId, "RequiredStates"),
            ForbiddenStates = ParseStateTypes(dto.ForbiddenStates, contextId, "ForbiddenStates"),
            RequiredAchievements = ParseAchievements(dto.RequiredAchievements, contextId, gameWorld),
            ScaleRequirements = ParseScaleRequirements(dto.ScaleRequirements, contextId)
        };

        return filter;
    }

    /// <summary>
    /// Parse single personality type string to nullable enum
    /// </summary>
    private static PersonalityType? ParsePersonalityType(string typeString, string contextId)
    {
        if (string.IsNullOrEmpty(typeString))
            return null;

        if (Enum.TryParse<PersonalityType>(typeString, true, out PersonalityType personalityType))
            return personalityType;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid PersonalityType: '{typeString}'");
    }

    /// <summary>
    /// Parse single profession string to nullable enum
    /// </summary>
    private static Professions? ParseProfession(string professionString, string contextId)
    {
        if (string.IsNullOrEmpty(professionString))
            return null;

        if (Enum.TryParse<Professions>(professionString, true, out Professions profession))
            return profession;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid Profession: '{professionString}'");
    }

    /// <summary>
    /// Parse single NPC relationship string to nullable enum
    /// </summary>
    private static NPCRelationship? ParseNPCRelationship(string relationshipString, string contextId)
    {
        if (string.IsNullOrEmpty(relationshipString))
            return null;

        if (Enum.TryParse<NPCRelationship>(relationshipString, true, out NPCRelationship relationship))
            return relationship;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCRelationship: '{relationshipString}'");
    }

    /// <summary>
    /// Parse single social standing string to nullable enum
    /// </summary>
    private static NPCSocialStanding? ParseSocialStanding(string standingString, string contextId)
    {
        if (string.IsNullOrEmpty(standingString))
            return null;

        if (Enum.TryParse<NPCSocialStanding>(standingString, true, out NPCSocialStanding standing))
            return standing;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCSocialStanding: '{standingString}'");
    }

    /// <summary>
    /// Parse single story role string to nullable enum
    /// </summary>
    private static NPCStoryRole? ParseStoryRole(string roleString, string contextId)
    {
        if (string.IsNullOrEmpty(roleString))
            return null;

        if (Enum.TryParse<NPCStoryRole>(roleString, true, out NPCStoryRole role))
            return role;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCStoryRole: '{roleString}'");
    }

    /// <summary>
    /// Parse single knowledge level string to nullable enum
    /// </summary>
    private static NPCKnowledgeLevel? ParseKnowledgeLevel(string levelString, string contextId)
    {
        if (string.IsNullOrEmpty(levelString))
            return null;

        if (Enum.TryParse<NPCKnowledgeLevel>(levelString, true, out NPCKnowledgeLevel level))
            return level;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCKnowledgeLevel: '{levelString}'");
    }

    /// <summary>
    /// Parse single privacy string to nullable enum
    /// </summary>
    private static LocationPrivacy? ParsePrivacy(string privacyString, string contextId)
    {
        if (string.IsNullOrEmpty(privacyString))
            return null;

        if (Enum.TryParse<LocationPrivacy>(privacyString, true, out LocationPrivacy privacy))
            return privacy;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationPrivacy: '{privacyString}'");
    }

    /// <summary>
    /// Parse single safety string to nullable enum
    /// </summary>
    private static LocationSafety? ParseSafety(string safetyString, string contextId)
    {
        if (string.IsNullOrEmpty(safetyString))
            return null;

        if (Enum.TryParse<LocationSafety>(safetyString, true, out LocationSafety safety))
            return safety;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationSafety: '{safetyString}'");
    }

    /// <summary>
    /// Parse single activity string to nullable enum
    /// </summary>
    private static LocationActivity? ParseActivity(string activityString, string contextId)
    {
        if (string.IsNullOrEmpty(activityString))
            return null;

        if (Enum.TryParse<LocationActivity>(activityString, true, out LocationActivity activity))
            return activity;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationActivity: '{activityString}'");
    }

    /// <summary>
    /// Parse single purpose string to nullable enum
    /// </summary>
    private static LocationPurpose? ParsePurpose(string purposeString, string contextId)
    {
        if (string.IsNullOrEmpty(purposeString))
            return null;

        if (Enum.TryParse<LocationPurpose>(purposeString, true, out LocationPurpose purpose))
            return purpose;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationPurpose: '{purposeString}'");
    }

    /// <summary>
    /// Parse single location role string to nullable enum
    /// </summary>
    private static LocationRole? ParseLocationRole(string roleString, string contextId)
    {
        if (string.IsNullOrEmpty(roleString))
            return null;

        if (Enum.TryParse<LocationRole>(roleString, true, out LocationRole locationRole))
            return locationRole;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationRole: '{roleString}'");
    }

    /// <summary>
    /// Parse single terrain type string to nullable enum
    /// </summary>
    private static TerrainType? ParseTerrainType(string terrainString, string contextId)
    {
        if (string.IsNullOrEmpty(terrainString))
            return null;

        if (Enum.TryParse<TerrainType>(terrainString, true, out TerrainType terrainType))
            return terrainType;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid TerrainType: '{terrainString}'");
    }

    /// <summary>
    /// Parse single structure type string to nullable enum
    /// </summary>
    private static StructureType? ParseStructureType(string structureString, string contextId)
    {
        if (string.IsNullOrEmpty(structureString))
            return null;

        if (Enum.TryParse<StructureType>(structureString, true, out StructureType structureType))
            return structureType;

        throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid StructureType: '{structureString}'");
    }

    /// <summary>
    /// Parse selection strategy string to enum.
    /// DDR-007: Default to First (deterministic) - Random removed from enum.
    /// </summary>
    private static PlacementSelectionStrategy ParseSelectionStrategy(string strategyString, string contextId)
    {
        if (string.IsNullOrEmpty(strategyString))
            return PlacementSelectionStrategy.First; // DDR-007: Deterministic default

        if (Enum.TryParse<PlacementSelectionStrategy>(strategyString, true, out PlacementSelectionStrategy strategy))
        {
            return strategy;
        }
        else
        {
            throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid SelectionStrategy: '{strategyString}'");
        }
    }

    /// <summary>
    /// Parse state type strings to enum list
    /// </summary>
    private static List<StateType> ParseStateTypes(List<string> stateStrings, string contextId, string fieldName)
    {
        if (stateStrings == null || !stateStrings.Any())
            return new List<StateType>();

        List<StateType> states = new List<StateType>();
        foreach (string stateString in stateStrings)
        {
            if (Enum.TryParse<StateType>(stateString, true, out StateType stateType))
            {
                states.Add(stateType);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}'.{fieldName} has invalid StateType: '{stateString}'");
            }
        }

        return states;
    }

    /// <summary>
    /// Parse scale requirements from DTOs
    /// </summary>
    private static List<ScaleRequirement> ParseScaleRequirements(List<ScaleRequirementDTO> dtos, string contextId)
    {
        if (dtos == null || !dtos.Any())
            return new List<ScaleRequirement>();

        List<ScaleRequirement> requirements = new List<ScaleRequirement>();
        foreach (ScaleRequirementDTO dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.ScaleType))
                throw new InvalidDataException($"PlacementFilter in '{contextId}' ScaleRequirement missing 'ScaleType'");

            if (!Enum.TryParse<ScaleType>(dto.ScaleType, true, out ScaleType scaleType))
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' ScaleRequirement has invalid ScaleType: '{dto.ScaleType}'");
            }

            requirements.Add(new ScaleRequirement
            {
                ScaleType = scaleType,
                MinValue = dto.MinValue,
                MaxValue = dto.MaxValue
            });
        }

        return requirements;
    }

    /// <summary>
    /// Parse achievement name strings to Achievement object list
    /// Resolves achievement strings to Achievement objects at parse-time
    /// </summary>
    private static List<Achievement> ParseAchievements(List<string> achievementNames, string contextId, GameWorld gameWorld)
    {
        if (achievementNames == null || !achievementNames.Any())
            return new List<Achievement>();

        if (gameWorld == null)
            return new List<Achievement>(); // Can't resolve without GameWorld

        List<Achievement> achievements = new List<Achievement>();
        foreach (string achievementName in achievementNames)
        {
            Achievement achievement = gameWorld.Achievements.FirstOrDefault(a => a.Name == achievementName);
            if (achievement == null)
            {
                achievement = new Achievement { Name = achievementName };
                gameWorld.Achievements.Add(achievement);
            }

            achievements.Add(achievement);
        }

        return achievements;
    }
}
