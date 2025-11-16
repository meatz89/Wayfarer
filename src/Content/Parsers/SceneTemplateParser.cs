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

        // SCENE ARCHETYPE GENERATION: All scenes use sceneArchetypeId (HIGHLANDER: ONE path)
        // Scene archetype defines BOTH structure (how many situations) AND content (which situation types)
        // NO special handling for Standalone vs Multi-situation - catalogue handles all variation
        if (string.IsNullOrEmpty(dto.SceneArchetypeId))
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required 'sceneArchetypeId'. All scenes must reference a scene archetype.");

        Console.WriteLine($"[SceneArchetypeGeneration] Generating scene '{dto.Id}' using archetype '{dto.SceneArchetypeId}'");

        // CATEGORICAL PARSING: No entity resolution at parse time
        // Concrete binding happens at instantiation time via SceneInstantiator
        // All scenes use categorical properties for procedural placement
        string npcId = null;
        string locationId = null;

        Console.WriteLine($"[SceneGeneration] Categorical context: Tier={dto.Tier}, MainStorySequence={dto.MainStorySequence}");

        SceneArchetypeDefinition archetypeDefinition = _generationFacade.GenerateSceneFromArchetype(
            dto.SceneArchetypeId,
            dto.Tier,
            npcId,
            locationId,
            dto.MainStorySequence);

        List<SituationTemplate> situationTemplates = archetypeDefinition.SituationTemplates;
        SituationSpawnRules spawnRules = archetypeDefinition.SpawnRules;

        Console.WriteLine($"[SceneArchetypeGeneration] Generated {situationTemplates.Count} situations with pattern '{spawnRules.Pattern}'");

        // Extract dependent resources from archetype definition (self-contained pattern)
        // Catalogue generates resource specifications at parse time
        List<DependentLocationSpec> dependentLocations = archetypeDefinition.DependentLocations ?? new List<DependentLocationSpec>();
        List<DependentItemSpec> dependentItems = archetypeDefinition.DependentItems ?? new List<DependentItemSpec>();

        if (dependentLocations.Any() || dependentItems.Any())
        {
            Console.WriteLine($"[SceneArchetypeGeneration] Archetype generated {dependentLocations.Count} dependent locations and {dependentItems.Count} dependent items");
        }

        SceneTemplate template = new SceneTemplate
        {
            Id = dto.Id,
            Archetype = archetype,
            SceneArchetypeId = dto.SceneArchetypeId,
            DisplayNameTemplate = dto.DisplayNameTemplate,
            // Hierarchical placement: Parse three separate base filters for CSS-style inheritance
            BaseLocationFilter = ParsePlacementFilter(dto.BaseLocationFilter, dto.Id),
            BaseNpcFilter = ParsePlacementFilter(dto.BaseNpcFilter, dto.Id),
            BaseRouteFilter = ParsePlacementFilter(dto.BaseRouteFilter, dto.Id),
            SpawnConditions = SpawnConditionsParser.ParseSpawnConditions(dto.SpawnConditions),
            SituationTemplates = situationTemplates,
            SpawnRules = spawnRules,
            ExpirationDays = dto.ExpirationDays,
            IsStarter = dto.IsStarter,
            IntroNarrativeTemplate = dto.IntroNarrativeTemplate,
            Tier = dto.Tier,
            Category = category,
            MainStorySequence = mainStorySequence,
            PresentationMode = presentationMode,
            ProgressionMode = progressionMode,
            DependentLocations = dependentLocations,
            DependentItems = dependentItems
        };

        return template;
    }

    /// <summary>
    /// Parse PlacementFilter from DTO
    /// </summary>
    /// <param name="dto">PlacementFilter DTO from JSON</param>
    /// <param name="contextId">Context identifier for error messages (template ID or instance path)</param>
    public static PlacementFilter ParsePlacementFilter(PlacementFilterDTO dto, string contextId)
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
            // NPC filters
            PersonalityTypes = ParsePersonalityTypes(dto.PersonalityTypes, contextId),
            Professions = ParseProfessions(dto.Professions, contextId),
            RequiredRelationships = ParseNPCRelationships(dto.RequiredRelationships, contextId),
            MinTier = dto.MinTier,
            MaxTier = dto.MaxTier,
            MinBond = dto.MinBond,
            MaxBond = dto.MaxBond,
            NpcTags = dto.NpcTags,
            // Orthogonal categorical dimensions - NPC
            SocialStandings = ParseSocialStandings(dto.SocialStandings, contextId),
            StoryRoles = ParseStoryRoles(dto.StoryRoles, contextId),
            KnowledgeLevels = ParseKnowledgeLevels(dto.KnowledgeLevels, contextId),
            // Location filters
            LocationTypes = ParseLocationTypes(dto.LocationTypes, contextId),
            LocationProperties = ParseLocationProperties(dto.LocationProperties, contextId),
            IsPlayerAccessible = dto.IsPlayerAccessible,
            LocationTags = dto.LocationTags,
            // Orthogonal categorical dimensions - Location
            PrivacyLevels = ParsePrivacyLevels(dto.PrivacyLevels, contextId),
            SafetyLevels = ParseSafetyLevels(dto.SafetyLevels, contextId),
            ActivityLevels = ParseActivityLevels(dto.ActivityLevels, contextId),
            Purposes = ParsePurposes(dto.Purposes, contextId),
            DistrictId = dto.DistrictId,
            RegionId = dto.RegionId,
            // Route filters
            TerrainTypes = dto.TerrainTypes,
            RouteTier = dto.RouteTier,
            MinDifficulty = dto.MinDifficulty,
            MaxDifficulty = dto.MaxDifficulty,
            RouteTags = dto.RouteTags,
            SegmentIndex = dto.SegmentIndex, // Route segment placement for geographic specificity
            // Variety control
            ExcludeRecentlyUsed = dto.ExcludeRecentlyUsed,
            // Player state filters
            RequiredStates = ParseStateTypes(dto.RequiredStates, contextId, "RequiredStates"),
            ForbiddenStates = ParseStateTypes(dto.ForbiddenStates, contextId, "ForbiddenStates"),
            RequiredAchievements = dto.RequiredAchievements,
            ScaleRequirements = ParseScaleRequirements(dto.ScaleRequirements, contextId)
        };

        return filter;
    }

    /// <summary>
    /// Parse personality type strings to enum list
    /// </summary>
    private static List<PersonalityType> ParsePersonalityTypes(List<string> typeStrings, string contextId)
    {
        if (typeStrings == null || !typeStrings.Any())
            return new List<PersonalityType>();

        List<PersonalityType> types = new List<PersonalityType>();
        foreach (string typeString in typeStrings)
        {
            if (Enum.TryParse<PersonalityType>(typeString, true, out PersonalityType personalityType))
            {
                types.Add(personalityType);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid PersonalityType: '{typeString}'");
            }
        }

        return types;
    }

    /// <summary>
    /// Parse location property strings to enum list
    /// </summary>
    private static List<LocationPropertyType> ParseLocationProperties(List<string> propertyStrings, string contextId)
    {
        if (propertyStrings == null || !propertyStrings.Any())
            return new List<LocationPropertyType>();

        List<LocationPropertyType> properties = new List<LocationPropertyType>();
        foreach (string propertyString in propertyStrings)
        {
            if (Enum.TryParse<LocationPropertyType>(propertyString, true, out LocationPropertyType property))
            {
                properties.Add(property);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationPropertyType: '{propertyString}'");
            }
        }

        return properties;
    }

    /// <summary>
    /// Parse profession strings to enum list
    /// </summary>
    private static List<Professions> ParseProfessions(List<string> professionStrings, string contextId)
    {
        if (professionStrings == null || !professionStrings.Any())
            return new List<Professions>();

        List<Professions> professions = new List<Professions>();
        foreach (string professionString in professionStrings)
        {
            if (Enum.TryParse<Professions>(professionString, true, out Professions profession))
            {
                professions.Add(profession);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid Profession: '{professionString}'");
            }
        }

        return professions;
    }

    /// <summary>
    /// Parse NPC relationship strings to enum list
    /// </summary>
    private static List<NPCRelationship> ParseNPCRelationships(List<string> relationshipStrings, string contextId)
    {
        if (relationshipStrings == null || !relationshipStrings.Any())
            return new List<NPCRelationship>();

        List<NPCRelationship> relationships = new List<NPCRelationship>();
        foreach (string relationshipString in relationshipStrings)
        {
            if (Enum.TryParse<NPCRelationship>(relationshipString, true, out NPCRelationship relationship))
            {
                relationships.Add(relationship);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCRelationship: '{relationshipString}'");
            }
        }

        return relationships;
    }

    /// <summary>
    /// Parse social standing strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<NPCSocialStanding> ParseSocialStandings(List<string> standingStrings, string contextId)
    {
        if (standingStrings == null || !standingStrings.Any())
            return new List<NPCSocialStanding>();

        List<NPCSocialStanding> standings = new List<NPCSocialStanding>();
        foreach (string standingString in standingStrings)
        {
            if (Enum.TryParse<NPCSocialStanding>(standingString, true, out NPCSocialStanding standing))
            {
                standings.Add(standing);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCSocialStanding: '{standingString}'");
            }
        }

        return standings;
    }

    /// <summary>
    /// Parse story role strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<NPCStoryRole> ParseStoryRoles(List<string> roleStrings, string contextId)
    {
        if (roleStrings == null || !roleStrings.Any())
            return new List<NPCStoryRole>();

        List<NPCStoryRole> roles = new List<NPCStoryRole>();
        foreach (string roleString in roleStrings)
        {
            if (Enum.TryParse<NPCStoryRole>(roleString, true, out NPCStoryRole role))
            {
                roles.Add(role);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCStoryRole: '{roleString}'");
            }
        }

        return roles;
    }

    /// <summary>
    /// Parse knowledge level strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<NPCKnowledgeLevel> ParseKnowledgeLevels(List<string> levelStrings, string contextId)
    {
        if (levelStrings == null || !levelStrings.Any())
            return new List<NPCKnowledgeLevel>();

        List<NPCKnowledgeLevel> levels = new List<NPCKnowledgeLevel>();
        foreach (string levelString in levelStrings)
        {
            if (Enum.TryParse<NPCKnowledgeLevel>(levelString, true, out NPCKnowledgeLevel level))
            {
                levels.Add(level);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid NPCKnowledgeLevel: '{levelString}'");
            }
        }

        return levels;
    }

    /// <summary>
    /// Parse privacy level strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<LocationPrivacy> ParsePrivacyLevels(List<string> privacyStrings, string contextId)
    {
        if (privacyStrings == null || !privacyStrings.Any())
            return new List<LocationPrivacy>();

        List<LocationPrivacy> privacyLevels = new List<LocationPrivacy>();
        foreach (string privacyString in privacyStrings)
        {
            if (Enum.TryParse<LocationPrivacy>(privacyString, true, out LocationPrivacy privacy))
            {
                privacyLevels.Add(privacy);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationPrivacy: '{privacyString}'");
            }
        }

        return privacyLevels;
    }

    /// <summary>
    /// Parse safety level strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<LocationSafety> ParseSafetyLevels(List<string> safetyStrings, string contextId)
    {
        if (safetyStrings == null || !safetyStrings.Any())
            return new List<LocationSafety>();

        List<LocationSafety> safetyLevels = new List<LocationSafety>();
        foreach (string safetyString in safetyStrings)
        {
            if (Enum.TryParse<LocationSafety>(safetyString, true, out LocationSafety safety))
            {
                safetyLevels.Add(safety);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationSafety: '{safetyString}'");
            }
        }

        return safetyLevels;
    }

    /// <summary>
    /// Parse activity level strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<LocationActivity> ParseActivityLevels(List<string> activityStrings, string contextId)
    {
        if (activityStrings == null || !activityStrings.Any())
            return new List<LocationActivity>();

        List<LocationActivity> activityLevels = new List<LocationActivity>();
        foreach (string activityString in activityStrings)
        {
            if (Enum.TryParse<LocationActivity>(activityString, true, out LocationActivity activity))
            {
                activityLevels.Add(activity);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationActivity: '{activityString}'");
            }
        }

        return activityLevels;
    }

    /// <summary>
    /// Parse purpose strings to enum list (Orthogonal Categorical Dimension)
    /// </summary>
    private static List<LocationPurpose> ParsePurposes(List<string> purposeStrings, string contextId)
    {
        if (purposeStrings == null || !purposeStrings.Any())
            return new List<LocationPurpose>();

        List<LocationPurpose> purposes = new List<LocationPurpose>();
        foreach (string purposeString in purposeStrings)
        {
            if (Enum.TryParse<LocationPurpose>(purposeString, true, out LocationPurpose purpose))
            {
                purposes.Add(purpose);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationPurpose: '{purposeString}'");
            }
        }

        return purposes;
    }

    /// <summary>
    /// Parse location type strings to enum list
    /// </summary>
    private static List<LocationTypes> ParseLocationTypes(List<string> typeStrings, string contextId)
    {
        if (typeStrings == null || !typeStrings.Any())
            return new List<LocationTypes>();

        List<LocationTypes> types = new List<LocationTypes>();
        foreach (string typeString in typeStrings)
        {
            if (Enum.TryParse<LocationTypes>(typeString, true, out LocationTypes locationType))
            {
                types.Add(locationType);
            }
            else
            {
                throw new InvalidDataException($"PlacementFilter in '{contextId}' has invalid LocationType: '{typeString}'");
            }
        }

        return types;
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
        if (!string.IsNullOrEmpty(dto.ArchetypeId))
        {
            // PARSE-TIME ARCHETYPE GENERATION
            choiceTemplates = GenerateChoiceTemplatesFromArchetype(dto.ArchetypeId, contextId, dto.Id);
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
            GrantsLocationAccess = dto.GrantsLocationAccess,
            // Hierarchical placement override filters (CSS-style inheritance)
            LocationFilter = ParsePlacementFilter(dto.LocationFilter, contextId),
            NpcFilter = ParsePlacementFilter(dto.NpcFilter, contextId),
            RouteFilter = ParsePlacementFilter(dto.RouteFilter, contextId),
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
            RequirementFormula = RequirementParser.ConvertDTOToCompoundRequirement(dto.RequirementFormula),
            CostTemplate = ParseChoiceCost(dto.CostTemplate),
            RewardTemplate = ParseChoiceReward(dto.RewardTemplate),
            ActionType = actionType,
            ChallengeId = dto.ChallengeId,
            ChallengeType = challengeType,
            NavigationPayload = ParseNavigationPayload(dto.NavigationPayload)
        };

        return template;
    }

    /// <summary>
    /// Parse ChoiceCost from DTO
    /// </summary>
    private ChoiceCost ParseChoiceCost(ChoiceCostDTO dto)
    {
        if (dto == null)
            return new ChoiceCost(); // All costs default to 0

        return new ChoiceCost
        {
            Coins = dto.Coins,
            Resolve = dto.Resolve,
            TimeSegments = dto.TimeSegments,
            Health = dto.Health,
            Hunger = dto.Hunger,
            Stamina = dto.Stamina,
            Focus = dto.Focus
        };
    }

    /// <summary>
    /// Parse ChoiceReward from DTO
    /// </summary>
    private ChoiceReward ParseChoiceReward(ChoiceRewardDTO dto)
    {
        if (dto == null)
            return new ChoiceReward(); // No rewards

        return new ChoiceReward
        {
            Coins = dto.Coins,
            Resolve = dto.Resolve,
            TimeSegments = dto.TimeSegments,
            AdvanceToBlock = ParseTimeBlock(dto.AdvanceToBlock),
            AdvanceToDay = ParseDayAdvancement(dto.AdvanceToDay),
            Health = dto.Health,
            Hunger = dto.Hunger,
            Stamina = dto.Stamina,
            Focus = dto.Focus,
            Insight = dto.Insight,
            Rapport = dto.Rapport,
            Authority = dto.Authority,
            Diplomacy = dto.Diplomacy,
            Cunning = dto.Cunning,
            FullRecovery = dto.FullRecovery,
            BondChanges = ParseBondChanges(dto.BondChanges),
            ScaleShifts = ParseScaleShifts(dto.ScaleShifts),
            StateApplications = ParseStateApplications(dto.StateApplications),
            AchievementIds = dto.AchievementIds,
            ItemIds = dto.ItemIds,
            ItemsToRemove = dto.ItemsToRemove,
            ScenesToSpawn = ParseSceneSpawnRewards(dto.ScenesToSpawn)
        };
    }

    /// <summary>
    /// Parse BondChanges from DTOs
    /// </summary>
    private List<BondChange> ParseBondChanges(List<BondChangeDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<BondChange>();

        return dtos.Select(dto => new BondChange
        {
            NpcId = dto.NpcId,
            Delta = dto.Delta,
            Reason = dto.Reason
        }).ToList();
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
    /// </summary>
    private List<SceneSpawnReward> ParseSceneSpawnRewards(List<SceneSpawnRewardDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<SceneSpawnReward>();

        List<SceneSpawnReward> rewards = new List<SceneSpawnReward>();
        foreach (SceneSpawnRewardDTO dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.SceneTemplateId))
                throw new InvalidDataException("SceneSpawnReward missing required 'SceneTemplateId'");

            rewards.Add(new SceneSpawnReward
            {
                SceneTemplateId = dto.SceneTemplateId
            });
        }

        return rewards;
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
    private List<ChoiceTemplate> GenerateChoiceTemplatesFromArchetype(string archetypeId, string contextId, string situationTemplateId)
    {
        Console.WriteLine($"[Archetype Generation] Generating 4 choices for situation '{situationTemplateId}' using archetype '{archetypeId}'");

        // Fetch archetype definition from catalogue (PARSE-TIME ONLY)
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(archetypeId);

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // CHOICE 1: Stat-Gated (Primary OR Secondary stat)
        // Best outcome, free if stat requirement met
        ChoiceTemplate statGatedChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = CreateStatRequirement(archetype),
            CostTemplate = new ChoiceCost(), // Free
            RewardTemplate = new ChoiceReward(), // Will be defined in JSON or instantiated at spawn time
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(statGatedChoice);

        // CHOICE 2: Money
        // Guaranteed success, expensive
        ChoiceTemplate moneyChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            ActionTextTemplate = GenerateMoneyActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            CostTemplate = new ChoiceCost { Coins = archetype.CoinCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(moneyChoice);

        // CHOICE 3: Challenge
        // Variable outcome, risky
        ChoiceTemplate challengeChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements (but has resource cost)
            CostTemplate = new ChoiceCost { Resolve = archetype.ResolveCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null, // Will be set by spawn-time instantiation
            ChallengeType = archetype.ChallengeType
        };
        choices.Add(challengeChoice);

        // CHOICE 4: Fallback
        // Poor outcome, always available
        ChoiceTemplate fallbackChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            CostTemplate = new ChoiceCost { TimeSegments = archetype.FallbackTimeCost },
            RewardTemplate = new ChoiceReward(),
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
    /// Create compound requirement with OR logic for primary/secondary stat
    /// </summary>
    private CompoundRequirement CreateStatRequirement(SituationArchetype archetype)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        // Path 1: Primary stat meets threshold
        OrPath primaryPath = new OrPath
        {
            Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+",
            NumericRequirements = new List<NumericRequirement>
        {
            new NumericRequirement
            {
                Type = "PlayerStat",
                Context = archetype.PrimaryStat.ToString(),
                Threshold = archetype.StatThreshold,
                Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+"
            }
        }
        };
        requirement.OrPaths.Add(primaryPath);

        // Path 2: Secondary stat meets threshold (only if different from primary)
        if (archetype.SecondaryStat != archetype.PrimaryStat)
        {
            OrPath secondaryPath = new OrPath
            {
                Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+",
                NumericRequirements = new List<NumericRequirement>
            {
                new NumericRequirement
                {
                    Type = "PlayerStat",
                    Context = archetype.SecondaryStat.ToString(),
                    Threshold = archetype.StatThreshold,
                    Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+"
                }
            }
            };
            requirement.OrPaths.Add(secondaryPath);
        }

        return requirement;
    }

    /// <summary>
    /// Generate action text template for stat-gated choice
    /// </summary>
    private string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Assert authority and take command",
            "negotiation" => "Negotiate favorable terms",
            "investigation" => "Deduce the solution through analysis",
            "social_maneuvering" => "Read the social dynamics and navigate skillfully",
            "crisis" => "Take decisive action with expertise",
            _ => "Use your expertise"
        };
    }

    /// <summary>
    /// Generate action text template for money choice
    /// </summary>
    private string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Pay off the opposition",
            "negotiation" => "Pay the premium price",
            "investigation" => "Hire an expert or pay for information",
            "social_maneuvering" => "Offer a generous gift",
            "crisis" => "Pay for emergency solution",
            _ => "Pay to resolve"
        };
    }

    /// <summary>
    /// Generate action text template for challenge choice
    /// </summary>
    private string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Attempt a physical confrontation",
            "negotiation" => "Engage in complex debate",
            "investigation" => "Work through the puzzle systematically",
            "social_maneuvering" => "Make a bold social gambit",
            "crisis" => "Risk everything on a desperate gambit",
            _ => "Accept the challenge"
        };
    }

    /// <summary>
    /// Generate action text template for fallback choice
    /// </summary>
    private string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Back down and submit",
            "negotiation" => "Accept unfavorable terms",
            "investigation" => "Give up and move on",
            "social_maneuvering" => "Exit awkwardly",
            "crisis" => "Flee the situation",
            _ => "Accept poor outcome"
        };
    }

    /// <summary>
    /// Generate single SituationTemplate from situation archetype
    /// Called for Standalone scenes that need ONE situation with 4-choice pattern
    /// Returns SituationTemplate with choices generated from SituationArchetypeCatalog
    /// </summary>
    private SituationTemplate GenerateSingleSituationFromArchetype(string situationArchetypeId, string contextId, int tier)
    {
        string situationId = $"{contextId}_situation";

        Console.WriteLine($"[SingleSituationGeneration] Generating situation '{situationId}' from archetype '{situationArchetypeId}'");

        // Generate 4 choices from archetype catalogue
        List<ChoiceTemplate> choices = GenerateChoiceTemplatesFromArchetype(situationArchetypeId, contextId, situationId);

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
                Theme = situationArchetypeId,
                Context = "standalone_situation",
                Style = "balanced"
            }
        };

        Console.WriteLine($"[SingleSituationGeneration] Created SituationTemplate '{situationId}' with {choices.Count} choices");

        return template;
    }
}
