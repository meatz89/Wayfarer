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
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'category'. Valid values: MainStory, SideStory, Encounter");
        }
        if (!Enum.TryParse<StoryCategory>(dto.Category, true, out StoryCategory category))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Category value: '{dto.Category}'. Valid values: MainStory, SideStory, Encounter");
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

        Console.WriteLine($"[SceneGeneration] Categorical context: Category={category}, MainStorySequence={dto.MainStorySequence}, Rhythm={rhythmPattern}");

        // HIGHLANDER: Branch on category, call appropriate method
        // MainStory scenes have a sequence number, SideStory/Encounter content does not
        SceneArchetypeDefinition archetypeDefinition;
        if (category == StoryCategory.MainStory)
        {
            // MainStory requires sequence (already validated at lines 67-80)
            archetypeDefinition = _generationFacade.GenerateMainStoryScene(
                sceneArchetypeType,
                contextNPC,
                contextLocation,
                dto.MainStorySequence.Value,
                rhythmPattern);
        }
        else
        {
            // SideStory and Encounter have no sequence
            archetypeDefinition = _generationFacade.GenerateSideContentScene(
                sceneArchetypeType,
                contextNPC,
                contextLocation,
                rhythmPattern);
        }

        List<SituationTemplate> situationTemplates = archetypeDefinition.SituationTemplates;
        SituationSpawnRules spawnRules = archetypeDefinition.SpawnRules;

        Console.WriteLine($"[SceneArchetypeGeneration] Generated {situationTemplates.Count} situations with pattern '{spawnRules.Pattern}'");


        PlacementFilter locationActivationFilter = PlacementFilterParser.Parse(dto.LocationActivationFilter, dto.Id, _gameWorld);

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
            Category = category,
            MainStorySequence = mainStorySequence,
            PresentationMode = presentationMode,
            ProgressionMode = progressionMode,
            IsStarter = isStarter,
            RhythmPattern = rhythmPattern
        };

        // HIGHLANDER: Explicit flow control for ALL scenes (arc42 §8.30)
        // NO SEQUENTIAL FALLBACK - all flow must be explicit
        EnrichSituationFlowControl(template);

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

        // HIGHLANDER: Validate mutual exclusivity at DTO level (arc42 §8.30)
        bool hasNextSituation = !string.IsNullOrEmpty(dto.NextSituationTemplateId);
        List<SceneSpawnReward> scenesToSpawn = ParseSceneSpawnRewards(dto.ScenesToSpawn);
        bool hasSceneSpawn = scenesToSpawn.Count > 0;

        if (hasNextSituation && dto.IsTerminal)
        {
            throw new InvalidDataException(
                $"Consequence has both NextSituationTemplateId='{dto.NextSituationTemplateId}' and IsTerminal=true. " +
                "These are mutually exclusive - a choice cannot both continue to next situation AND end the scene. (arc42 §8.30)");
        }

        if (hasNextSituation && hasSceneSpawn)
        {
            throw new InvalidDataException(
                $"Consequence has both NextSituationTemplateId='{dto.NextSituationTemplateId}' and ScenesToSpawn ({scenesToSpawn.Count} scenes). " +
                "These are mutually exclusive - a choice cannot both stay within scene AND spawn new scenes. (arc42 §8.30)");
        }

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
            ScenesToSpawn = scenesToSpawn,
            // Flow control (HIGHLANDER: all flow through choices)
            NextSituationTemplateId = dto.NextSituationTemplateId,
            IsTerminal = dto.IsTerminal
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
                // SIMPLIFIED (arc42 §8.28): RhythmPattern is THE ONLY context input
                RhythmPatternContext = ParseRhythmPattern(dto.RhythmPatternContext)
            };
            rewards.Add(reward);
        }

        return rewards;
    }

    /// <summary>
    /// Parse rhythm pattern from string.
    /// Returns null if string is null/empty.
    /// </summary>
    private RhythmPattern? ParseRhythmPattern(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        return value.ToLowerInvariant() switch
        {
            "building" => RhythmPattern.Building,
            "crisis" => RhythmPattern.Crisis,
            "mixed" => RhythmPattern.Mixed,
            _ => throw new InvalidOperationException(
                $"Unknown RhythmPattern '{value}' - valid values: Building, Crisis, Mixed")
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
            InitialSituationTemplateId = dto.InitialSituationTemplateId,
            // HIGHLANDER: Transitions REMOVED (see arc42 §8.30)
            // All flow control through Consequence.NextSituationTemplateId and IsTerminal
            CompletionCondition = dto.CompletionCondition
        };
    }

    // HIGHLANDER: ParseSituationTransitions method REMOVED (see arc42 §8.30)
    // All flow control through Consequence.NextSituationTemplateId and IsTerminal

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
    /// HIGHLANDER: Set explicit flow control on ALL choices in ALL situations (arc42 §8.30)
    /// NO SEQUENTIAL FALLBACK - every choice must specify where flow goes next.
    /// - Non-final situations: NextSituationTemplateId points to next situation
    /// - Final situations: IsTerminal = true (scene ends)
    /// </summary>
    private static void EnrichSituationFlowControl(SceneTemplate template)
    {
        if (template.SituationTemplates.Count == 0)
            return;

        for (int i = 0; i < template.SituationTemplates.Count; i++)
        {
            SituationTemplate situation = template.SituationTemplates[i];
            bool isFinalSituation = (i == template.SituationTemplates.Count - 1);

            foreach (ChoiceTemplate choice in situation.ChoiceTemplates)
            {
                EnrichChoiceFlowControl(choice.Consequence, situation, template, i, isFinalSituation);
                EnrichChoiceFlowControl(choice.OnSuccessConsequence, situation, template, i, isFinalSituation);
                EnrichChoiceFlowControl(choice.OnFailureConsequence, situation, template, i, isFinalSituation);
            }
        }

        Console.WriteLine($"[FlowControl Enrichment] Enriched {template.SituationTemplates.Count} situations in scene '{template.Id}'");
    }

    /// <summary>
    /// Set flow control on a single consequence.
    /// MUTUAL EXCLUSIVITY: If consequence already has ScenesToSpawn, that implies scene transition - set IsTerminal.
    /// Otherwise: non-final → NextSituationTemplateId, final → IsTerminal.
    /// </summary>
    private static void EnrichChoiceFlowControl(Consequence consequence, SituationTemplate situation, SceneTemplate template, int situationIndex, bool isFinalSituation)
    {
        if (consequence == null)
            return;

        // Skip if flow control already explicitly set
        if (!string.IsNullOrEmpty(consequence.NextSituationTemplateId) || consequence.IsTerminal)
            return;

        // If consequence spawns scenes, it's implicitly terminal (leaving current scene)
        if (consequence.ScenesToSpawn.Count > 0)
        {
            consequence.IsTerminal = true;
            return;
        }

        if (isFinalSituation)
        {
            // Final situation - scene ends
            consequence.IsTerminal = true;
        }
        else
        {
            // Non-final situation - point to next situation in sequence
            SituationTemplate nextSituation = template.SituationTemplates[situationIndex + 1];
            consequence.NextSituationTemplateId = nextSituation.Id;
        }
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
            Console.WriteLine($"[MainStory Enrichment] Choice '{choice.Id}' ScenesToSpawn before: {choice.Consequence.ScenesToSpawn.Count}");
            bool alreadyHasMainStorySpawn = choice.Consequence.ScenesToSpawn.Any(s => s.SpawnNextMainStoryScene);
            if (!alreadyHasMainStorySpawn)
            {
                choice.Consequence.ScenesToSpawn.Add(new SceneSpawnReward { SpawnNextMainStoryScene = true });
            }
            // HIGHLANDER: Mark final situation choices as terminal (arc42 §8.30)
            choice.Consequence.IsTerminal = true;
            Console.WriteLine($"[MainStory Enrichment] Choice '{choice.Id}' ScenesToSpawn after: {choice.Consequence.ScenesToSpawn.Count}, IsTerminal: {choice.Consequence.IsTerminal}");
        }

        Console.WriteLine($"[MainStory Enrichment] Enriched {finalSituation.ChoiceTemplates.Count} choices in final situation '{finalSituation.Id}' for scene '{template.Id}'");
    }
}
