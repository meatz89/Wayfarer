/// <summary>
/// Parser for SceneTemplate definitions - converts DTOs to domain models
/// Handles recursive parsing of embedded SituationTemplates and ChoiceTemplates
/// Uses SceneGenerationFacade for clean isolation from generation subsystem
/// </summary>
public class SceneTemplateParser
{
    private readonly GameWorld _gameWorld;
    private readonly SceneGenerationFacade _generationFacade;

    public SceneTemplateParser(GameWorld gameWorld, SceneGenerationFacade generationFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _generationFacade = generationFacade ?? throw new ArgumentNullException(nameof(generationFacade));
    }

    /// <summary>
    /// Parse a SceneTemplateDTO to SceneTemplate domain entity
    /// </summary>
    public SceneTemplate ParseSceneTemplate(SceneTemplateDTO dto)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("SceneTemplate missing required field 'Id'");

        Console.WriteLine($"[SceneTemplateParser] Parsing SceneTemplate: {dto.Id}");

        if (string.IsNullOrEmpty(dto.Archetype))
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'Archetype'");

        // Parse archetype enum
        if (!Enum.TryParse<SpawnPattern>(dto.Archetype, true, out SpawnPattern archetype))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Archetype value: '{dto.Archetype}'");
        }

        // FAIL-FAST: Tier is REQUIRED (no silent defaults)
        if (!dto.Tier.HasValue)
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'tier'. Must be 0-4.");
        }
        int tier = dto.Tier.Value;
        if (tier < 0 || tier > 4)
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid tier={tier}. Must be 0-4.");
        }

        // Parse PresentationMode (defaults to Atmospheric if not specified)
        PresentationMode presentationMode = PresentationMode.Atmospheric;
        if (!string.IsNullOrEmpty(dto.PresentationMode))
        {
            if (!Enum.TryParse<PresentationMode>(dto.PresentationMode, true, out presentationMode))
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid PresentationMode value: '{dto.PresentationMode}'. Must be 'Atmospheric' or 'Modal'.");
            }
        }

        // Parse ProgressionMode (defaults to Breathe if not specified)
        ProgressionMode progressionMode = ProgressionMode.Breathe;
        if (!string.IsNullOrEmpty(dto.ProgressionMode))
        {
            if (!Enum.TryParse<ProgressionMode>(dto.ProgressionMode, true, out progressionMode))
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid ProgressionMode value: '{dto.ProgressionMode}'. Must be 'Breathe' or 'Cascade'.");
            }
        }

        // Parse StoryCategory (defaults to SideStory if not specified)
        StoryCategory category = StoryCategory.SideStory;
        if (!string.IsNullOrEmpty(dto.Category))
        {
            if (!Enum.TryParse<StoryCategory>(dto.Category, true, out category))
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Category value: '{dto.Category}'. Valid values: MainStory, SideStory, Service");
            }
        }

        // Validate MainStorySequence constraints
        int? mainStorySequence = dto.MainStorySequence;
        if (mainStorySequence.HasValue)
        {
            if (category != StoryCategory.MainStory)
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has MainStorySequence={mainStorySequence} but Category is '{category}'. MainStorySequence requires Category='MainStory'.");
            }

            if (mainStorySequence.Value < 1 || mainStorySequence.Value > 99)
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid MainStorySequence={mainStorySequence}. Must be between 1 and 99.");
            }
        }

        // SCENE ARCHETYPE GENERATION: All scenes use sceneArchetype (HIGHLANDER: ONE path)
        // Scene archetype defines BOTH structure (how many situations) AND content (which situation types)
        // NO special handling for Standalone vs Multi-situation - catalogue handles all variation
        // PRINCIPLE: SceneArchetype is a TYPE discriminator, not an ID (arc42 §8.3)
        SceneArchetypeType sceneArchetypeType;

        if (!string.IsNullOrEmpty(dto.SceneArchetype))
        {
            // EXPLICIT ARCHETYPE: Parse SceneArchetypeType enum with fail-fast validation
            if (!Enum.TryParse<SceneArchetypeType>(dto.SceneArchetype, true, out sceneArchetypeType))
            {
                throw new InvalidDataException(
                    $"SceneTemplate '{dto.Id}' has invalid SceneArchetype: '{dto.SceneArchetype}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<SceneArchetypeType>())}");
            }
        }
        else if (!string.IsNullOrEmpty(dto.ArchetypeCategory))
        {
            // CATEGORICAL ARCHETYPE: Resolve via Catalogue at PARSE TIME (CATALOGUE PATTERN)
            // Used by procedural generation - ArchetypeCategory + ExcludedArchetypes → specific archetype
            int sequence = dto.MainStorySequence ?? 1;
            sceneArchetypeType = SceneArchetypeCatalog.ResolveFromCategory(
                dto.ArchetypeCategory,
                dto.ExcludedArchetypes,
                sequence);

            Console.WriteLine($"[SceneArchetypeGeneration] Resolved category '{dto.ArchetypeCategory}' → archetype '{sceneArchetypeType}'");
        }
        else
        {
            throw new InvalidDataException(
                $"SceneTemplate '{dto.Id}' missing required archetype. " +
                $"Provide either 'sceneArchetype' (explicit) or 'archetypeCategory' (categorical resolution).");
        }

        Console.WriteLine($"[SceneArchetypeGeneration] Generating scene '{dto.Id}' using archetype '{sceneArchetypeType}'");

        // CATEGORICAL PARSING: No entity resolution at parse time
        // Concrete binding happens at instantiation time via SceneInstantiator
        // All scenes use categorical properties for procedural placement
        // HIGHLANDER: Pass null objects, not null strings
        NPC contextNPC = null;
        Location contextLocation = null;

        Console.WriteLine($"[SceneGeneration] Categorical context: Tier={tier}, MainStorySequence={dto.MainStorySequence}");

        SceneArchetypeDefinition archetypeDefinition = _generationFacade.GenerateSceneFromArchetype(
            sceneArchetypeType,
            tier,
            contextNPC,
            contextLocation,
            dto.MainStorySequence);

        List<SituationTemplate> situationTemplates = archetypeDefinition.SituationTemplates;
        SituationSpawnRules spawnRules = archetypeDefinition.SpawnRules;

        Console.WriteLine($"[SceneArchetypeGeneration] Generated {situationTemplates.Count} situations with pattern '{spawnRules.Pattern}'");


        PlacementFilter locationActivationFilter = ParsePlacementFilter(dto.LocationActivationFilter, dto.Id, _gameWorld);

        // FAIL-FAST VALIDATION: Detect silent JSON deserialization failures
        // If JSON field names don't match DTO properties (e.g. 'baseLocationFilter' vs 'locationActivationFilter'),
        // System.Text.Json silently leaves properties as null instead of throwing exceptions
        // For MainStory scenes (critical path content), require LocationActivationFilter
        // This catches JSON structure mismatches at parse time instead of runtime
        if (category == StoryCategory.MainStory)
        {
            if (locationActivationFilter == null)
            {
                throw new InvalidOperationException(
                    $"SceneTemplate '{dto.Id}' is MainStory but has NO LocationActivationFilter. " +
                    $"This indicates JSON field name mismatch. " +
                    $"Verify JSON uses correct field name: 'locationActivationFilter' (not 'baseLocationFilter'). " +
                    $"MainStory scenes require LocationActivationFilter to determine when they activate.");
            }
        }

        SceneTemplate template = new SceneTemplate
        {
            Id = dto.Id,
            Archetype = archetype,
            SceneArchetype = sceneArchetypeType,
            DisplayNameTemplate = dto.DisplayNameTemplate,
            // Activation filter: Parse trigger for scene activation (Deferred → Active)
            // Scenes activate via LOCATION ONLY (player enters location matching filter)
            // Separate from situation placement filters (each situation has explicit filters)
            LocationActivationFilter = locationActivationFilter,
            SpawnConditions = SpawnConditionsParser.ParseSpawnConditions(dto.SpawnConditions),
            SituationTemplates = situationTemplates,
            SpawnRules = spawnRules,
            ExpirationDays = dto.ExpirationDays,
            IntroNarrativeTemplate = dto.IntroNarrativeTemplate,
            Tier = tier,
            Category = category,
            MainStorySequence = mainStorySequence,
            PresentationMode = presentationMode,
            ProgressionMode = progressionMode,
            IsStarter = dto.IsStarter
        };

        // A-STORY ENRICHMENT: Per CONTENT_ARCHITECTURE.md §8
        // "ALL final situation choices receive spawn reward for next A-scene"
        // HIGHLANDER: ONE enrichment path for ALL MainStory scenes
        if (template.Category == StoryCategory.MainStory)
        {
            EnrichMainStoryFinalChoices(template);
        }

        return template;
    }

    /// <summary>
    /// Parse PlacementFilter from DTO
    /// </summary>
    /// <param name="dto">PlacementFilter DTO from JSON</param>
    /// <param name="contextId">Context identifier for error messages (template ID or instance path)</param>
    public static PlacementFilter ParsePlacementFilter(PlacementFilterDTO dto, string contextId, GameWorld gameWorld = null)
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
            MinTier = dto.MinTier,
            MaxTier = dto.MaxTier,
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
            DistrictId = dto.DistrictId,
            RegionId = dto.RegionId,
            // Route filters - SINGULAR (orthogonal)
            Terrain = ParseTerrainType(dto.Terrain, contextId),
            Structure = ParseStructureType(dto.Structure, contextId),
            RouteTier = dto.RouteTier,
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
    /// Parse selection strategy string to enum
    /// </summary>
    private static PlacementSelectionStrategy ParseSelectionStrategy(string strategyString, string contextId)
    {
        if (string.IsNullOrEmpty(strategyString))
            return PlacementSelectionStrategy.Random; // Default

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

    /// <summary>
    /// Parse embedded SituationTemplates
    /// </summary>
    private List<SituationTemplate> ParseSituationTemplates(List<SituationTemplateDTO> dtos, string contextId, SpawnPattern archetype)
    {
        if (dtos == null || !dtos.Any())
            throw new InvalidDataException($"SceneTemplate '{contextId}' must have at least one SituationTemplate");

        List<SituationTemplate> templates = new List<SituationTemplate>();
        foreach (SituationTemplateDTO dto in dtos)
        {
            templates.Add(ParseSituationTemplate(dto, contextId, archetype));
        }

        return templates;
    }

    /// <summary>
    /// Parse a single SituationTemplate
    /// </summary>
    private SituationTemplate ParseSituationTemplate(SituationTemplateDTO dto, string contextId, SpawnPattern archetype)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException($"SituationTemplate in SceneTemplate '{contextId}' missing required 'Id'");

        // Parse SituationType (defaults to Normal if not specified for backward compatibility)
        SituationType situationType = SituationType.Normal;
        if (!string.IsNullOrEmpty(dto.Type))
        {
            if (!Enum.TryParse<SituationType>(dto.Type, true, out situationType))
            {
                throw new InvalidDataException($"SituationTemplate '{dto.Id}' in SceneTemplate '{contextId}' has invalid Type value: '{dto.Type}'. Must be 'Normal' or 'Crisis'.");
            }
        }

        // ARCHETYPE-BASED GENERATION: archetypeId generates 4 ChoiceTemplates from catalogue
        List<ChoiceTemplate> choiceTemplates;
        if (dto.ArchetypeId != null)
        {
            // PARSE-TIME ARCHETYPE GENERATION
            choiceTemplates = GenerateChoiceTemplatesFromArchetype(dto.ArchetypeId.Value, contextId, dto.Id);
        }
        else
        {
            throw new InvalidDataException($"SituationTemplate '{dto.Id}' in SceneTemplate '{contextId}' must have 'archetypeId' for archetype-driven choice generation. All situations require player choices.");
        }

        SituationTemplate template = new SituationTemplate
        {
            Id = dto.Id,
            Type = situationType,
            NarrativeTemplate = dto.NarrativeTemplate,
            ChoiceTemplates = choiceTemplates,
            Priority = dto.Priority,
            // Explicit placement filters - no inheritance, each situation specifies its own
            LocationFilter = ParsePlacementFilter(dto.LocationFilter, contextId, _gameWorld),
            NpcFilter = ParsePlacementFilter(dto.NpcFilter, contextId, _gameWorld),
            RouteFilter = ParsePlacementFilter(dto.RouteFilter, contextId, _gameWorld),
            NarrativeHints = ParseNarrativeHints(dto.NarrativeHints)
        };

        return template;
    }

    /// <summary>
    /// Parse embedded ChoiceTemplates
    /// </summary>
    private List<ChoiceTemplate> ParseChoiceTemplates(List<ChoiceTemplateDTO> dtos, string contextId, string situationTemplateId, SpawnPattern archetype)
    {
        // Normal scenes require 2-4 choices (Sir Brante pattern)
        if (dtos == null || !dtos.Any())
            throw new InvalidDataException($"SituationTemplate '{situationTemplateId}' in SceneTemplate '{contextId}' must have at least 2 ChoiceTemplates (Sir Brante pattern: 2-4 choices)");

        if (dtos.Count < 2 || dtos.Count > 4)
            throw new InvalidDataException($"SituationTemplate '{situationTemplateId}' in SceneTemplate '{contextId}' has {dtos.Count} choices. Must have 2-4 choices (Sir Brante pattern)");

        List<ChoiceTemplate> templates = new List<ChoiceTemplate>();
        foreach (ChoiceTemplateDTO dto in dtos)
        {
            templates.Add(ParseChoiceTemplate(dto, contextId, situationTemplateId));
        }

        return templates;
    }

    /// <summary>
    /// Parse a single ChoiceTemplate
    /// </summary>
    private ChoiceTemplate ParseChoiceTemplate(ChoiceTemplateDTO dto, string contextId, string situationTemplateId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException($"ChoiceTemplate in SituationTemplate '{situationTemplateId}' (SceneTemplate '{contextId}') missing required 'Id'");

        // Parse ActionType enum
        if (!Enum.TryParse<ChoiceActionType>(dto.ActionType, true, out ChoiceActionType actionType))
        {
            throw new InvalidDataException($"ChoiceTemplate '{dto.Id}' has invalid ActionType: '{dto.ActionType}'");
        }

        // Parse ChallengeType if present
        TacticalSystemType? challengeType = null;
        if (!string.IsNullOrEmpty(dto.ChallengeType))
        {
            if (Enum.TryParse<TacticalSystemType>(dto.ChallengeType, true, out TacticalSystemType parsedType))
            {
                challengeType = parsedType;
            }
            else
            {
                throw new InvalidDataException($"ChoiceTemplate '{dto.Id}' has invalid ChallengeType: '{dto.ChallengeType}'");
            }
        }

        // Parse PathType (defaults to Fallback if not specified)
        ChoicePathType pathType = ChoicePathType.Fallback;
        if (!string.IsNullOrEmpty(dto.PathType))
        {
            if (!Enum.TryParse<ChoicePathType>(dto.PathType, true, out pathType))
            {
                throw new InvalidDataException($"ChoiceTemplate '{dto.Id}' has invalid PathType: '{dto.PathType}'. Valid values: InstantSuccess, Challenge, Fallback");
            }
        }

        ChoiceTemplate template = new ChoiceTemplate
        {
            Id = dto.Id,
            PathType = pathType,
            ActionTextTemplate = dto.ActionTextTemplate,
            RequirementFormula = RequirementParser.ConvertDTOToCompoundRequirement(dto.RequirementFormula, _gameWorld),
            Consequence = ParseConsequence(dto.Consequence),
            OnSuccessConsequence = ParseConsequence(dto.OnSuccessConsequence),
            OnFailureConsequence = ParseConsequence(dto.OnFailureConsequence),
            ActionType = actionType,
            ChallengeId = dto.ChallengeId,
            ChallengeType = challengeType,
            NavigationPayload = ParseNavigationPayload(dto.NavigationPayload)
        };

        return template;
    }

    /// <summary>
    /// Parse unified Consequence from DTO
    /// DESIGN: Negative values = costs, positive values = rewards
    /// Example: Coins = -5 means pay 5 coins, Coins = 10 means gain 10 coins
    /// </summary>
    private Consequence ParseConsequence(ConsequenceDTO dto)
    {
        if (dto == null)
            return new Consequence(); // No effects

        return new Consequence
        {
            // Resource changes (negative = cost, positive = reward)
            Coins = dto.Coins,
            Resolve = dto.Resolve,
            TimeSegments = dto.TimeSegments,
            Health = dto.Health,
            Hunger = dto.Hunger,
            Stamina = dto.Stamina,
            Focus = dto.Focus,
            // Five Stats
            Insight = dto.Insight,
            Rapport = dto.Rapport,
            Authority = dto.Authority,
            Diplomacy = dto.Diplomacy,
            Cunning = dto.Cunning,
            // Time advancement
            AdvanceToBlock = ParseTimeBlock(dto.AdvanceToBlock),
            AdvanceToDay = ParseDayAdvancement(dto.AdvanceToDay),
            FullRecovery = dto.FullRecovery,
            // Relationships
            BondChanges = ParseBondChanges(dto.BondChanges),
            ScaleShifts = ParseScaleShifts(dto.ScaleShifts),
            StateApplications = ParseStateApplications(dto.StateApplications),
            // Progression
            Achievements = ParseAchievements(dto.AchievementIds),
            Items = ParseItems(dto.ItemIds),
            ItemsToRemove = ParseItemsToRemove(dto.ItemsToRemove),
            ScenesToSpawn = ParseSceneSpawnRewards(dto.ScenesToSpawn)
        };
    }

    /// <summary>
    /// Parse BondChanges from DTOs
    /// Resolves NPC object references from NpcId strings
    /// </summary>
    private List<BondChange> ParseBondChanges(List<BondChangeDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<BondChange>();

        List<BondChange> bondChanges = new List<BondChange>();
        foreach (BondChangeDTO dto in dtos)
        {
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.NpcId);
            if (npc == null)
            {
                Console.WriteLine($"[SceneTemplateParser.ParseBondChanges] WARNING: NPC '{dto.NpcId}' not found for BondChange");
                continue; // Skip invalid bond change
            }

            bondChanges.Add(new BondChange
            {
                Npc = npc, // Object reference, NO ID
                Delta = dto.Delta,
                Reason = dto.Reason
            });
        }

        return bondChanges;
    }

    /// <summary>
    /// Parse ScaleShifts from DTOs
    /// </summary>
    private List<ScaleShift> ParseScaleShifts(List<ScaleShiftDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<ScaleShift>();

        List<ScaleShift> shifts = new List<ScaleShift>();
        foreach (ScaleShiftDTO dto in dtos)
        {
            if (!Enum.TryParse<ScaleType>(dto.ScaleType, true, out ScaleType scaleType))
            {
                throw new InvalidDataException($"ScaleShift has invalid ScaleType: '{dto.ScaleType}'");
            }

            shifts.Add(new ScaleShift
            {
                ScaleType = scaleType,
                Delta = dto.Delta,
                Reason = dto.Reason
            });
        }

        return shifts;
    }

    /// <summary>
    /// Parse StateApplications from DTOs
    /// </summary>
    private List<StateApplication> ParseStateApplications(List<StateApplicationDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<StateApplication>();

        List<StateApplication> applications = new List<StateApplication>();
        foreach (StateApplicationDTO dto in dtos)
        {
            if (!Enum.TryParse<StateType>(dto.StateType, true, out StateType stateType))
            {
                throw new InvalidDataException($"StateApplication has invalid StateType: '{dto.StateType}'");
            }

            applications.Add(new StateApplication
            {
                StateType = stateType,
                Apply = dto.Apply,
                Reason = dto.Reason
            });
        }

        return applications;
    }

    /// <summary>
    /// Parse SceneSpawnRewards from DTOs
    /// NO ID STRINGS - uses SpawnNextMainStoryScene boolean flag
    /// </summary>
    private List<SceneSpawnReward> ParseSceneSpawnRewards(List<SceneSpawnRewardDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<SceneSpawnReward>();

        List<SceneSpawnReward> rewards = new List<SceneSpawnReward>();
        foreach (SceneSpawnRewardDTO dto in dtos)
        {
            rewards.Add(new SceneSpawnReward
            {
                SpawnNextMainStoryScene = dto.SpawnNextMainStoryScene
            });
        }

        return rewards;
    }

    /// <summary>
    /// Parse Items from item ID strings
    /// Resolves Item object references from string IDs at parse-time
    /// HIGHLANDER: Returns object references, not IDs
    /// </summary>
    private List<Item> ParseItems(List<string> itemIds)
    {
        if (itemIds == null || !itemIds.Any())
            return new List<Item>();

        List<Item> items = new List<Item>();
        foreach (string itemId in itemIds)
        {
            Item item = _gameWorld.Items.FirstOrDefault(i => i.Name == itemId);
            if (item == null)
            {
                Console.WriteLine($"[SceneTemplateParser.ParseItems] WARNING: Item '{itemId}' not found");
                continue; // Skip invalid item
            }

            items.Add(item); // Object reference, NO ID
        }

        return items;
    }

    /// <summary>
    /// Parse Achievements from achievement ID strings
    /// Resolves Achievement object references from string IDs at parse-time
    /// HIGHLANDER: Returns object references, not IDs
    /// </summary>
    private List<Achievement> ParseAchievements(List<string> achievementIds)
    {
        if (achievementIds == null || !achievementIds.Any())
            return new List<Achievement>();

        List<Achievement> achievements = new List<Achievement>();
        foreach (string achievementId in achievementIds)
        {
            Achievement achievement = _gameWorld.Achievements.FirstOrDefault(a => a.Name == achievementId);
            if (achievement == null)
            {
                Console.WriteLine($"[SceneTemplateParser.ParseAchievements] WARNING: Achievement '{achievementId}' not found");
                continue; // Skip invalid achievement
            }

            achievements.Add(achievement); // Object reference, NO ID
        }

        return achievements;
    }

    /// <summary>
    /// Parse Items to Remove from item ID strings
    /// Resolves Item object references from string IDs at parse-time
    /// HIGHLANDER: Returns object references, not IDs
    /// </summary>
    private List<Item> ParseItemsToRemove(List<string> itemIds)
    {
        if (itemIds == null || !itemIds.Any())
            return new List<Item>();

        List<Item> items = new List<Item>();
        foreach (string itemId in itemIds)
        {
            Item item = _gameWorld.Items.FirstOrDefault(i => i.Name == itemId);
            if (item == null)
            {
                Console.WriteLine($"[SceneTemplateParser.ParseItemsToRemove] WARNING: Item '{itemId}' not found");
                continue; // Skip invalid item
            }

            items.Add(item); // Object reference, NO ID
        }

        return items;
    }

    /// <summary>
    /// Parse NavigationPayload from DTO
    /// Resolves Destination Location object from ID
    /// </summary>
    private NavigationPayload ParseNavigationPayload(NavigationPayloadDTO dto)
    {
        if (dto == null)
            return null;

        // Resolve destination location from DestinationId
        Location destination = null;
        if (!string.IsNullOrEmpty(dto.DestinationId))
        {
            destination = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationId);
            if (destination == null)
                throw new InvalidOperationException($"NavigationPayload references unknown Location: '{dto.DestinationId}'");
        }

        return new NavigationPayload
        {
            Destination = destination,
            AutoTriggerScene = dto.AutoTriggerScene
        };
    }

    /// <summary>
    /// Parse NarrativeHints from DTO
    /// </summary>
    private NarrativeHints ParseNarrativeHints(NarrativeHintsDTO dto)
    {
        if (dto == null)
            return null;

        return new NarrativeHints
        {
            Tone = dto.Tone,
            Theme = dto.Theme,
            Context = dto.Context,
            Style = dto.Style
        };
    }

    /// <summary>
    /// Parse SituationSpawnRules from DTO
    /// </summary>
    private SituationSpawnRules ParseSpawnRules(SituationSpawnRulesDTO dto, string contextId)
    {
        if (dto == null)
            return null; // Optional - some SceneTemplates may not have complex spawn rules

        if (!Enum.TryParse<SpawnPattern>(dto.Pattern, true, out SpawnPattern pattern))
        {
            throw new InvalidDataException($"SceneTemplate '{contextId}' SpawnRules has invalid Pattern: '{dto.Pattern}'");
        }

        return new SituationSpawnRules
        {
            Pattern = pattern,
            InitialSituationId = dto.InitialSituationId,
            Transitions = ParseSituationTransitions(dto.Transitions, contextId),
            CompletionCondition = dto.CompletionCondition
        };
    }

    /// <summary>
    /// Parse SituationTransitions from DTOs
    /// </summary>
    private List<SituationTransition> ParseSituationTransitions(List<SituationTransitionDTO> dtos, string contextId)
    {
        if (dtos == null || !dtos.Any())
            return new List<SituationTransition>();

        List<SituationTransition> transitions = new List<SituationTransition>();
        foreach (SituationTransitionDTO dto in dtos)
        {
            if (!Enum.TryParse<TransitionCondition>(dto.Condition, true, out TransitionCondition condition))
            {
                throw new InvalidDataException($"SceneTemplate '{contextId}' SituationTransition has invalid Condition: '{dto.Condition}'");
            }

            transitions.Add(new SituationTransition
            {
                SourceSituationId = dto.SourceSituationId,
                DestinationSituationId = dto.DestinationSituationId,
                Condition = condition,
                SpecificChoiceId = dto.SpecificChoiceId
            });
        }

        return transitions;
    }

    /// <summary>
    /// Parse TimeBlock enum from string
    /// </summary>
    private TimeBlocks? ParseTimeBlock(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<TimeBlocks>(value, ignoreCase: true, out TimeBlocks result))
            return result;

        throw new InvalidDataException($"Invalid TimeBlock value: '{value}'. Valid values: Morning, Midday, Afternoon, Evening");
    }

    /// <summary>
    /// Parse DayAdvancement enum from string
    /// </summary>
    private DayAdvancement? ParseDayAdvancement(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<DayAdvancement>(value, ignoreCase: true, out DayAdvancement result))
            return result;

        throw new InvalidDataException($"Invalid DayAdvancement value: '{value}'. Valid values: CurrentDay, NextDay");
    }

    /// <summary>
    /// Generate 4 ChoiceTemplates from archetype structure (parse-time only)
    /// Called ONLY at parse time when SituationTemplate has archetypeId
    /// Creates the 4-choice pattern: stat-gated, money, challenge, fallback
    /// </summary>
    private List<ChoiceTemplate> GenerateChoiceTemplatesFromArchetype(SituationArchetypeType archetypeType, string contextId, string situationTemplateId)
    {
        Console.WriteLine($"[Archetype Generation] Generating 4 choices for situation '{situationTemplateId}' using archetype '{archetypeType}'");

        // Fetch archetype definition from catalogue (PARSE-TIME ONLY)
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(archetypeType);

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // CHOICE 1: Stat-Gated (Primary OR Secondary stat)
        // Best outcome, free if stat requirement met
        ChoiceTemplate statGatedChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = SituationArchetypeCatalog.CreateStatRequirement(archetype, archetype.StatThreshold),
            Consequence = new Consequence(), // Free - no costs, rewards defined later
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(statGatedChoice);

        // CHOICE 2: Money
        // Guaranteed success, expensive (negative Coins = cost)
        ChoiceTemplate moneyChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            ActionTextTemplate = GenerateMoneyActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            Consequence = new Consequence { Coins = -archetype.CoinCost }, // Negative = cost
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(moneyChoice);

        // CHOICE 3: Challenge
        // Variable outcome, risky (negative Resolve = cost)
        ChoiceTemplate challengeChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements (but has resource cost)
            Consequence = new Consequence { Resolve = -archetype.ResolveCost }, // Negative = cost
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null, // Will be set by spawn-time instantiation
            ChallengeType = archetype.ChallengeType
        };
        choices.Add(challengeChoice);

        // CHOICE 4: Fallback
        // Poor outcome, always available (TimeSegments is always a cost, so positive)
        ChoiceTemplate fallbackChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            Consequence = new Consequence { TimeSegments = archetype.FallbackTimeCost }, // Time is always positive (passes)
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(fallbackChoice);

        // VERIFICATION LOGGING - Prove 4 choices generated with correct properties
        Console.WriteLine($"[Archetype Generation] Generated {choices.Count} choices:");
        Console.WriteLine($"  [Choice 1] Stat-Gated: Requires {archetype.PrimaryStat}/{archetype.SecondaryStat} {archetype.StatThreshold}+, Costs 0, Type={statGatedChoice.ActionType}");
        Console.WriteLine($"  [Choice 2] Money: No requirements, Costs {archetype.CoinCost} coins, Type={moneyChoice.ActionType}");
        Console.WriteLine($"  [Choice 3] Challenge: No requirements, Costs {archetype.ResolveCost} Resolve, Type={challengeChoice.ActionType}, ChallengeType={archetype.ChallengeType}");
        Console.WriteLine($"  [Choice 4] Fallback: No requirements, Costs {archetype.FallbackTimeCost} time segments, Type={fallbackChoice.ActionType}");

        return choices;
    }


    /// <summary>
    /// Generate action text template for stat-gated choice
    /// </summary>
    private string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Assert authority and take command",
            SituationArchetypeType.Negotiation => "Negotiate favorable terms",
            SituationArchetypeType.Investigation => "Deduce the solution through analysis",
            SituationArchetypeType.SocialManeuvering => "Read the social dynamics and navigate skillfully",
            SituationArchetypeType.Crisis => "Take decisive action with expertise",
            _ => "Use your expertise"
        };
    }

    /// <summary>
    /// Generate action text template for money choice
    /// </summary>
    private string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Pay off the opposition",
            SituationArchetypeType.Negotiation => "Pay the premium price",
            SituationArchetypeType.Investigation => "Hire an expert or pay for information",
            SituationArchetypeType.SocialManeuvering => "Offer a generous gift",
            SituationArchetypeType.Crisis => "Pay for emergency solution",
            _ => "Pay to resolve"
        };
    }

    /// <summary>
    /// Generate action text template for challenge choice
    /// </summary>
    private string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Attempt a physical confrontation",
            SituationArchetypeType.Negotiation => "Engage in complex debate",
            SituationArchetypeType.Investigation => "Work through the puzzle systematically",
            SituationArchetypeType.SocialManeuvering => "Make a bold social gambit",
            SituationArchetypeType.Crisis => "Risk everything on a desperate gambit",
            _ => "Accept the challenge"
        };
    }

    /// <summary>
    /// Generate action text template for fallback choice
    /// </summary>
    private string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Back down and submit",
            SituationArchetypeType.Negotiation => "Accept unfavorable terms",
            SituationArchetypeType.Investigation => "Give up and move on",
            SituationArchetypeType.SocialManeuvering => "Exit awkwardly",
            SituationArchetypeType.Crisis => "Flee the situation",
            _ => "Accept poor outcome"
        };
    }

    /// <summary>
    /// Enrich MainStory final situation choices with SpawnNextMainStoryScene
    /// Per CONTENT_ARCHITECTURE.md §8: "ALL final situation choices receive spawn reward"
    /// HIGHLANDER: ONE enrichment path for ALL MainStory scenes
    /// </summary>
    private static void EnrichMainStoryFinalChoices(SceneTemplate template)
    {
        if (template.SituationTemplates.Count == 0)
            return;

        SituationTemplate finalSituation = template.SituationTemplates[template.SituationTemplates.Count - 1];

        foreach (ChoiceTemplate choice in finalSituation.ChoiceTemplates)
        {
            Console.WriteLine($"[MainStory Enrichment] Choice '{choice.Id}' Consequence HashCode: {choice.Consequence.GetHashCode()}, ScenesToSpawn before: {choice.Consequence.ScenesToSpawn.Count}");
            bool alreadyHasMainStorySpawn = choice.Consequence.ScenesToSpawn.Any(s => s.SpawnNextMainStoryScene);
            if (!alreadyHasMainStorySpawn)
            {
                choice.Consequence.ScenesToSpawn.Add(new SceneSpawnReward { SpawnNextMainStoryScene = true });
            }
            Console.WriteLine($"[MainStory Enrichment] Choice '{choice.Id}' ScenesToSpawn after: {choice.Consequence.ScenesToSpawn.Count}");
        }

        Console.WriteLine($"[MainStory Enrichment] Enriched {finalSituation.ChoiceTemplates.Count} choices in final situation '{finalSituation.Id}' for scene '{template.Id}'");
    }

    /// <summary>
    /// Generate single SituationTemplate from situation archetype
    /// Called for Standalone scenes that need ONE situation with 4-choice pattern
    /// Returns SituationTemplate with choices generated from SituationArchetypeCatalog
    /// </summary>
    private SituationTemplate GenerateSingleSituationFromArchetype(SituationArchetypeType situationArchetypeType, string contextId, int tier)
    {
        string situationId = $"{contextId}_situation";

        Console.WriteLine($"[SingleSituationGeneration] Generating situation '{situationId}' from archetype '{situationArchetypeType}'");

        // Generate 4 choices from archetype catalogue
        List<ChoiceTemplate> choices = GenerateChoiceTemplatesFromArchetype(situationArchetypeType, contextId, situationId);

        // Create situation template with generated choices
        SituationTemplate template = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            NarrativeTemplate = null, // AI generates from hints
            ChoiceTemplates = choices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "neutral",
                Theme = situationArchetypeType.ToString(),
                Context = "standalone_situation",
                Style = "balanced"
            }
        };

        Console.WriteLine($"[SingleSituationGeneration] Created SituationTemplate '{situationId}' with {choices.Count} choices");

        return template;
    }
}
