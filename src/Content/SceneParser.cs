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
    /// THREE-TIER TIMING: Filters stored here (Tier 1), entities resolved at activation (Tier 2)
    /// </summary>
    public static Scene ConvertDTOToScene(
        SceneDTO dto,
        GameWorld gameWorld)
    {
        // =====================================================
        // VALIDATION: Required Fields
        // =====================================================
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("Scene DTO missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.SceneArchetype))
            throw new InvalidDataException($"Scene '{dto.Id}' missing required field 'SceneArchetype'");
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
        SceneTemplate template = gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == dto.SceneArchetype);
        if (template == null)
        {
            throw new InvalidDataException(
                $"Scene '{dto.Id}' references non-existent SceneTemplate '{dto.SceneArchetype}'. " +
                $"Available templates: {string.Join(", ", gameWorld.SceneTemplates.Select(t => t.Id))}");
        }

        // =====================================================
        // ENTITY CONSTRUCTION (SCENE HAS NO PLACEMENT)
        // =====================================================
        // ARCHITECTURAL CHANGE: Placement moved to Situation level
        // Scene is narrative container with no specific location/NPC/route
        Scene scene = new Scene
        {
            TemplateId = dto.SceneArchetype,
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
            locationActivationFilter = PlacementFilterParser.Parse(
                dto.LocationActivationFilter, locationActivationContext, gameWorld);
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
                locationFilter = PlacementFilterParser.Parse(situationDto.LocationFilter, locationContext, gameWorld);
            }

            // Parse NPC filter (MUST be explicit on situation)
            if (situationDto.NpcFilter != null)
            {
                string npcContext = $"Scene:{dto.DisplayName}/Situation:{situationDto.Name}/NPC";
                npcFilter = PlacementFilterParser.Parse(situationDto.NpcFilter, npcContext, gameWorld);
            }

            // Parse route filter (MUST be explicit on situation)
            if (situationDto.RouteFilter != null)
            {
                string routeContext = $"Scene:{dto.DisplayName}/Situation:{situationDto.Name}/Route";
                routeFilter = PlacementFilterParser.Parse(situationDto.RouteFilter, routeContext, gameWorld);
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

                // Copy Intensity from template for runtime filtering
                // CATALOGUE PATTERN: Intensity set at parse-time from archetype, copied here at spawn-time
                situation.Intensity = situation.Template.Intensity;
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

        // NOTE: Procedural content tracing REMOVED from parse-time
        // Parse-time happens during GameWorldInitializer BEFORE DI is available
        // Tracing of authored content (static JSON) belongs in activation/runtime, not parse-time
        // See: ARCH SMELL in todo list - SceneParser tracing architecture

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
