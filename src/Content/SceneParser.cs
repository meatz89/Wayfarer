/// <summary>
/// Parser for converting SceneDTO to Scene domain model
/// HIGHLANDER: JSON → PackageLoader → Parser → Entity (single instantiation path)
/// </summary>
public static class SceneParser
{
    /// <summary>
    /// Convert a SceneDTO to a Scene domain model (System 5: Scene Instantiation)
    /// Resolves template reference and parses embedded situations
    /// ARCHITECTURAL CHANGE: Entity resolution happens per-situation (not per-scene)
    /// Each situation has its own placement resolved from its own PlacementFilter
    /// </summary>
    public static Scene ConvertDTOToScene(
        SceneDTO dto,
        GameWorld gameWorld,
        EntityResolver entityResolver)
    {
        // =====================================================
        // VALIDATION: Required Fields
        // =====================================================
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("Scene DTO missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.TemplateId))
            throw new InvalidDataException($"Scene '{dto.Id}' missing required field 'TemplateId'");
        if (string.IsNullOrEmpty(dto.State))
            throw new InvalidDataException($"Scene '{dto.Id}' missing required field 'State'");

        // =====================================================
        // ENUM PARSING
        // =====================================================

        if (!Enum.TryParse<SceneState>(dto.State, true, out SceneState state))
        {
            throw new InvalidDataException($"Scene '{dto.Id}' has invalid State value: '{dto.State}'");
        }

        // Parse optional enums with defaults
        PresentationMode presentationMode = PresentationMode.Atmospheric;
        if (!string.IsNullOrEmpty(dto.PresentationMode))
        {
            if (!Enum.TryParse<PresentationMode>(dto.PresentationMode, true, out presentationMode))
            {
                throw new InvalidDataException($"Scene '{dto.Id}' has invalid PresentationMode value: '{dto.PresentationMode}'");
            }
        }

        ProgressionMode progressionMode = ProgressionMode.Breathe;
        if (!string.IsNullOrEmpty(dto.ProgressionMode))
        {
            if (!Enum.TryParse<ProgressionMode>(dto.ProgressionMode, true, out progressionMode))
            {
                throw new InvalidDataException($"Scene '{dto.Id}' has invalid ProgressionMode value: '{dto.ProgressionMode}'");
            }
        }

        SpawnPattern archetype = SpawnPattern.Standalone;
        if (!string.IsNullOrEmpty(dto.Archetype))
        {
            if (!Enum.TryParse<SpawnPattern>(dto.Archetype, true, out archetype))
            {
                throw new InvalidDataException($"Scene '{dto.Id}' has invalid Archetype value: '{dto.Archetype}'");
            }
        }

        StoryCategory category = StoryCategory.SideStory;
        if (!string.IsNullOrEmpty(dto.Category))
        {
            if (!Enum.TryParse<StoryCategory>(dto.Category, true, out category))
            {
                throw new InvalidDataException($"Scene '{dto.Id}' has invalid Category value: '{dto.Category}'");
            }
        }

        // =====================================================
        // HIGHLANDER PATTERN A: Template Reference Resolution
        // =====================================================
        // ID from JSON (persistence), object resolved at parse time (runtime navigation)
        SceneTemplate template = gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == dto.TemplateId);
        if (template == null)
        {
            throw new InvalidDataException(
                $"Scene '{dto.Id}' references non-existent SceneTemplate '{dto.TemplateId}'. " +
                $"Available templates: {string.Join(", ", gameWorld.SceneTemplates.Select(t => t.Id))}");
        }

        // =====================================================
        // ENTITY CONSTRUCTION (SCENE HAS NO PLACEMENT)
        // =====================================================
        // ARCHITECTURAL CHANGE: Placement moved to Situation level
        // Scene is narrative container with no specific location/NPC/route
        Scene scene = new Scene
        {
            TemplateId = dto.TemplateId,
            Template = template,
            State = state,
            ExpiresOnDay = dto.ExpiresOnDay,
            Archetype = archetype,
            DisplayName = dto.DisplayName ?? "",
            IntroNarrative = dto.IntroNarrative ?? "",
            PresentationMode = presentationMode,
            ProgressionMode = progressionMode,
            Category = category,
            MainStorySequence = dto.MainStorySequence
        };

        // Resolve SourceSituation object from SourceSituationId (parse-time translation)
        if (!string.IsNullOrEmpty(dto.SourceSituationId))
        {
            scene.SourceSituation = gameWorld.Scenes
                .SelectMany(s => s.Situations)
                .FirstOrDefault(s => s.Template?.Id == dto.SourceSituationId);
        }

        // =====================================================
        // SPAWN RULES PARSING
        // =====================================================
        if (dto.SpawnRules != null)
        {
            scene.SpawnRules = ParseSpawnRules(dto.SpawnRules);
        }

        // =====================================================
        // ACTIVATION FILTERS PARSING (TRIGGER CONDITIONS)
        // =====================================================
        // Parse activation filters that determine WHEN scene activates (Deferred → Active)
        // Separate from situation placement filters (which determine WHERE situations happen)
        // Evaluated BEFORE entity resolution using categorical matching only
        // Scenes activate via LOCATION ONLY (player enters location matching filter)
        PlacementFilter locationActivationFilter = null;

        if (dto.LocationActivationFilter != null)
        {
            string locationActivationContext = $"Scene:{dto.DisplayName}/LocationActivation";
            locationActivationFilter = SceneTemplateParser.ParsePlacementFilter(
                dto.LocationActivationFilter, locationActivationContext);
        }

        // Store activation filter on Scene (not resolved, just stored for activation check)
        // Scenes activate via LOCATION ONLY (player enters location matching filter)
        scene.LocationActivationFilter = locationActivationFilter;

        // =====================================================
        // EMBEDDED SITUATIONS PARSING (Composition Pattern)
        // =====================================================
        // Scene OWNS Situations - parse filters (NO entity resolution at parse time)
        // THREE-TIER TIMING: Filters stored here (Tier 1), entities resolved at activation (Tier 2)
        // Each situation MUST specify explicit categorical filters - NO inheritance, NO fallback
        foreach (SituationDTO situationDto in dto.Situations)
        {
            // Parse PlacementFilters (explicit per-situation)
            PlacementFilter locationFilter = null;
            PlacementFilter npcFilter = null;
            PlacementFilter routeFilter = null;
            int segmentIndex = 0; // Route segment placement for geographic specificity

            // Parse location filter (MUST be explicit on situation)
            if (situationDto.LocationFilter != null)
            {
                string locationContext = $"Scene:{dto.DisplayName}/Situation:{situationDto.Name}/Location";
                locationFilter = SceneTemplateParser.ParsePlacementFilter(situationDto.LocationFilter, locationContext);
            }

            // Parse NPC filter (MUST be explicit on situation)
            if (situationDto.NpcFilter != null)
            {
                string npcContext = $"Scene:{dto.DisplayName}/Situation:{situationDto.Name}/NPC";
                npcFilter = SceneTemplateParser.ParsePlacementFilter(situationDto.NpcFilter, npcContext);
            }

            // Parse route filter (MUST be explicit on situation)
            if (situationDto.RouteFilter != null)
            {
                string routeContext = $"Scene:{dto.DisplayName}/Situation:{situationDto.Name}/Route";
                routeFilter = SceneTemplateParser.ParsePlacementFilter(situationDto.RouteFilter, routeContext);
                segmentIndex = routeFilter.SegmentIndex; // Capture segment placement from filter
            }

            // System 5: Situation Instantiation with NULL entity references (deferred resolution)
            Situation situation = SituationParser.ConvertDTOToSituation(
                situationDto,
                gameWorld);

            // Store PlacementFilters for activation-time resolution (THREE-TIER TIMING)
            situation.LocationFilter = locationFilter;
            situation.NpcFilter = npcFilter;
            situation.RouteFilter = routeFilter;

            // CRITICAL: Set composition relationship (Situation → ParentScene)
            situation.ParentScene = scene;

            // Assign route segment placement from filter (geographic specificity)
            situation.SegmentIndex = segmentIndex;

            // CRITICAL: Resolve Template reference for lazy action instantiation
            // SituationParser sets TemplateId but not Template object
            if (!string.IsNullOrEmpty(situation.TemplateId))
            {
                // DEBUG: Log what we're searching for and what's available
                Console.WriteLine($"[SceneParser] Looking for SituationTemplate '{situationDto.TemplateId}' in SceneTemplate '{template.Id}'");
                Console.WriteLine($"[SceneParser] Available SituationTemplates: {string.Join(", ", template.SituationTemplates.Select(t => $"'{t.Id}'"))}");

                situation.Template = template.SituationTemplates.FirstOrDefault(t => t.Id == situationDto.TemplateId);

                // FAIL-FAST: Throw immediately instead of continuing with null
                // Violating "LET IT CRASH" and "PLAYABILITY OVER COMPILATION" is forbidden
                if (situation.Template == null)
                {
                    throw new InvalidDataException(
                        $"[SceneParser] CRITICAL: Situation '{situation.Name}' references TemplateId '{situationDto.TemplateId}' " +
                        $"but no such template found in SceneTemplate '{template.Id}'. " +
                        $"Available templates: {string.Join(", ", template.SituationTemplates.Select(t => $"'{t.Id}'"))}. " +
                        $"This indicates Parser-JSON-Entity Triangle violation: JSON contains misaligned TemplateId.");
                }
            }

            scene.Situations.Add(situation);
        }

        // =====================================================
        // NEW ARCHITECTURE: CurrentSituationIndex from CurrentSituationId
        // ===================================================================
        // Find index of situation matching CurrentSituationId from DTO - HIGHLANDER: Use Template.Id
        if (!string.IsNullOrEmpty(dto.CurrentSituationId))
        {
            int index = scene.Situations.FindIndex(s => s.Template?.Id == dto.CurrentSituationId);
            if (index >= 0)
            {
                scene.CurrentSituationIndex = index;
            }
            else
            {
                Console.WriteLine($"[SceneParser] WARNING: Scene '{dto.DisplayName}' references CurrentSituationId '{dto.CurrentSituationId}' " +
                    $"but no such situation found in embedded Situations collection. Defaulting to index 0.");
                scene.CurrentSituationIndex = 0;
            }
        }

        // =====================================================
        // DERIVED PROPERTIES
        // =====================================================
        // SituationCount is computed from collection
        scene.SituationCount = scene.Situations.Count;

        // =====================================================
        // PROCEDURAL CONTENT TRACING: Record authored scene spawn
        // =====================================================
        if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
        {
            SceneSpawnNode sceneNode = gameWorld.ProceduralTracer.RecordSceneSpawn(
                scene,
                scene.TemplateId,
                false, // isProcedurallyGenerated = false (authored content from JSON)
                SpawnTriggerType.Initial,
                gameWorld.CurrentDay,
                gameWorld.CurrentTimeBlock
            );

            // Record all embedded situations as children of this scene
            foreach (Situation situation in scene.Situations)
            {
                gameWorld.ProceduralTracer.RecordSituationSpawn(
                    situation,
                    sceneNode,
                    SituationSpawnTriggerType.InitialScene
                );
            }
        }

        return scene;
    }

    /// <summary>
    /// Parse SituationSpawnRulesDTO to SituationSpawnRules domain entity
    /// </summary>
    private static SituationSpawnRules ParseSpawnRules(SituationSpawnRulesDTO dto)
    {
        if (!Enum.TryParse<SpawnPattern>(dto.Pattern, true, out SpawnPattern pattern))
        {
            throw new InvalidDataException($"Invalid SituationSpawnPattern value: '{dto.Pattern}'");
        }

        return new SituationSpawnRules
        {
            Pattern = pattern,
            InitialSituationId = dto.InitialSituationId,
            Transitions = dto.Transitions?.Select(ParseTransition).ToList() ?? new List<SituationTransition>()
        };
    }

    /// <summary>
    /// Parse SituationTransitionDTO to SituationTransition domain entity
    /// </summary>
    private static SituationTransition ParseTransition(SituationTransitionDTO dto)
    {
        if (!Enum.TryParse<TransitionCondition>(dto.Condition, true, out TransitionCondition condition))
        {
            throw new InvalidDataException($"Invalid TransitionCondition value: '{dto.Condition}'");
        }

        return new SituationTransition
        {
            SourceSituationId = dto.SourceSituationId,
            DestinationSituationId = dto.DestinationSituationId,
            Condition = condition,
            SpecificChoiceId = dto.SpecificChoiceId
        };
    }
}
