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

        // FAIL-FAST: PresentationMode is REQUIRED (no silent defaults)
        if (string.IsNullOrEmpty(dto.PresentationMode))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'presentationMode'. Must be 'Atmospheric' or 'Modal'.");
        }
        if (!Enum.TryParse<PresentationMode>(dto.PresentationMode, true, out PresentationMode presentationMode))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid PresentationMode value: '{dto.PresentationMode}'. Must be 'Atmospheric' or 'Modal'.");
        }

        // FAIL-FAST: ProgressionMode is REQUIRED (no silent defaults)
        if (string.IsNullOrEmpty(dto.ProgressionMode))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'progressionMode'. Must be 'Breathe' or 'Cascade'.");
        }
        if (!Enum.TryParse<ProgressionMode>(dto.ProgressionMode, true, out ProgressionMode progressionMode))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid ProgressionMode value: '{dto.ProgressionMode}'. Must be 'Breathe' or 'Cascade'.");
        }

        // FAIL-FAST: Category is REQUIRED (no silent defaults)
        if (string.IsNullOrEmpty(dto.Category))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'category'. Valid values: MainStory, SideStory, Service");
        }
        if (!Enum.TryParse<StoryCategory>(dto.Category, true, out StoryCategory category))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Category value: '{dto.Category}'. Valid values: MainStory, SideStory, Service");
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

        // FAIL-FAST: RhythmPattern is REQUIRED (no silent defaults)
        // MUST parse BEFORE GenerateSceneFromArchetype since rhythm affects choice generation
        // Sir Brante rhythm classification determines choice generation pattern
        // See arc42/08_crosscutting_concepts.md §8.26
        if (string.IsNullOrEmpty(dto.RhythmPattern))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'rhythmPattern'. Must be 'Building', 'Crisis', or 'Mixed'.");
        }
        if (!Enum.TryParse<RhythmPattern>(dto.RhythmPattern, true, out RhythmPattern rhythmPattern))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid RhythmPattern value: '{dto.RhythmPattern}'. Must be 'Building', 'Crisis', or 'Mixed'.");
        }

        // CATEGORICAL PARSING: No entity resolution at parse time
        // Concrete binding happens at instantiation time via SceneInstantiator
        // All scenes use categorical properties for procedural placement
        // HIGHLANDER: Pass null objects, not null strings
        NPC contextNPC = null;
        Location contextLocation = null;

        Console.WriteLine($"[SceneGeneration] Categorical context: Tier={tier}, MainStorySequence={dto.MainStorySequence}, Rhythm={rhythmPattern}");

        SceneArchetypeDefinition archetypeDefinition = _generationFacade.GenerateSceneFromArchetype(
            sceneArchetypeType,
            tier,
            contextNPC,
            contextLocation,
            dto.MainStorySequence,
            rhythmPattern);

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

        // FAIL-FAST: IsStarter is REQUIRED (no silent defaults)
        if (!dto.IsStarter.HasValue)
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'isStarter'. Must be true or false.");
        }
        bool isStarter = dto.IsStarter.Value;

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
            IsStarter = isStarter,
            RhythmPattern = rhythmPattern
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
    /// CONTEXT INJECTION: Parses categorical inputs for selection logic
    /// HIGHLANDER: Same selection logic processes authored and procedural
    /// </summary>
    private List<SceneSpawnReward> ParseSceneSpawnRewards(List<SceneSpawnRewardDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<SceneSpawnReward>();

        List<SceneSpawnReward> rewards = new List<SceneSpawnReward>();
        foreach (SceneSpawnRewardDTO dto in dtos)
        {
            SceneSpawnReward reward = new SceneSpawnReward
            {
                SpawnNextMainStoryScene = dto.SpawnNextMainStoryScene,
                // CATEGORICAL INPUTS - flow through same selection logic as procedural
                LocationSafetyContext = ParseLocationSafety(dto.LocationSafetyContext),
                LocationPurposeContext = ParseLocationPurpose(dto.LocationPurposeContext),
                RhythmPhaseContext = ParseRhythmPhase(dto.RhythmPhaseContext),
                TierContext = dto.TierContext
            };
            rewards.Add(reward);
        }

        return rewards;
    }

    /// <summary>
    /// Parse location safety from string.
    /// Returns null if string is null/empty.
    /// </summary>
    private LocationSafety? ParseLocationSafety(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return value.ToLowerInvariant() switch
        {
            "safe" => LocationSafety.Safe,
            "risky" => LocationSafety.Risky,
            "dangerous" => LocationSafety.Dangerous,
            _ => throw new InvalidOperationException(
                $"Unknown LocationSafety '{value}' - valid values: Safe, Risky, Dangerous")
        };
    }

    /// <summary>
    /// Parse location purpose from string.
    /// Returns null if string is null/empty.
    /// </summary>
    private LocationPurpose? ParseLocationPurpose(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return value.ToLowerInvariant() switch
        {
            "commerce" => LocationPurpose.Commerce,
            "governance" => LocationPurpose.Governance,
            "worship" => LocationPurpose.Worship,
            "dwelling" => LocationPurpose.Dwelling,
            "civic" => LocationPurpose.Civic,
            "production" => LocationPurpose.Production,
            "hospitality" => LocationPurpose.Hospitality,
            "utility" => LocationPurpose.Utility,
            _ => throw new InvalidOperationException(
                $"Unknown LocationPurpose '{value}' - valid values: Commerce, Governance, Worship, Dwelling, Civic, Production, Hospitality, Utility")
        };
    }

    /// <summary>
    /// Parse rhythm phase from string.
    /// Returns null if string is null/empty.
    /// </summary>
    private RhythmPhase? ParseRhythmPhase(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return value.ToLowerInvariant() switch
        {
            "accumulation" => RhythmPhase.Accumulation,
            "test" => RhythmPhase.Test,
            "recovery" => RhythmPhase.Recovery,
            _ => throw new InvalidOperationException(
                $"Unknown RhythmPhase '{value}' - valid values: Accumulation, Test, Recovery")
        };
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
}
