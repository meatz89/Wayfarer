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
            Id = dto.Id,
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
            MainStorySequence = dto.MainStorySequence,
            SourceSituationId = dto.SourceSituationId
        };

        // =====================================================
        // SPAWN RULES PARSING
        // =====================================================
        if (dto.SpawnRules != null)
        {
            scene.SpawnRules = ParseSpawnRules(dto.SpawnRules);
        }

        // =====================================================
        // EMBEDDED SITUATIONS PARSING (Composition Pattern)
        // =====================================================
        // Scene OWNS Situations - parse embedded situations with per-situation entity resolution
        // CSS-STYLE INHERITANCE: Situation filters override scene base filters
        foreach (SituationDTO situationDto in dto.Situations)
        {
            // System 4: Resolve entities with hierarchical placement inheritance
            // Pattern: effectiveFilter = situationFilter ?? sceneBaseFilter
            Location resolvedLocation = null;
            NPC resolvedNpc = null;
            RouteOption resolvedRoute = null;

            // CSS-style fallback for location: situation override ?? scene base
            PlacementFilterDTO effectiveLocationFilter = situationDto.LocationFilter ?? dto.LocationFilter;
            if (effectiveLocationFilter != null)
            {
                string locationContext = $"Scene:{dto.Id}/Situation:{situationDto.Id}/Location";
                PlacementFilter locationFilter = SceneTemplateParser.ParsePlacementFilter(effectiveLocationFilter, locationContext);
                resolvedLocation = entityResolver.FindOrCreateLocation(locationFilter);
            }

            // CSS-style fallback for NPC: situation override ?? scene base
            PlacementFilterDTO effectiveNpcFilter = situationDto.NpcFilter ?? dto.NpcFilter;
            if (effectiveNpcFilter != null)
            {
                string npcContext = $"Scene:{dto.Id}/Situation:{situationDto.Id}/NPC";
                PlacementFilter npcFilter = SceneTemplateParser.ParsePlacementFilter(effectiveNpcFilter, npcContext);
                resolvedNpc = entityResolver.FindOrCreateNPC(npcFilter);
            }

            // CSS-style fallback for route: situation override ?? scene base
            PlacementFilterDTO effectiveRouteFilter = situationDto.RouteFilter ?? dto.RouteFilter;
            if (effectiveRouteFilter != null)
            {
                string routeContext = $"Scene:{dto.Id}/Situation:{situationDto.Id}/Route";
                PlacementFilter routeFilter = SceneTemplateParser.ParsePlacementFilter(effectiveRouteFilter, routeContext);
                resolvedRoute = entityResolver.FindOrCreateRoute(routeFilter);
            }

            // System 5: Situation Instantiation with pre-resolved objects
            Situation situation = SituationParser.ConvertDTOToSituation(
                situationDto,
                gameWorld,
                resolvedLocation,
                resolvedNpc,
                resolvedRoute);

            // CRITICAL: Set composition relationship (Situation → ParentScene)
            situation.ParentScene = scene;

            // CRITICAL: Resolve Template reference for lazy action instantiation
            // SituationParser sets TemplateId but not Template object
            if (!string.IsNullOrEmpty(situation.TemplateId))
            {
                situation.Template = template.SituationTemplates.FirstOrDefault(t => t.Id == situationDto.TemplateId);
                if (situation.Template == null)
                {
                    Console.WriteLine($"[SceneParser] WARNING: Situation '{situation.Id}' references TemplateId '{situationDto.TemplateId}' " +
                        $"but no such template found in SceneTemplate '{template.Id}'");
                }
            }

            scene.Situations.Add(situation);
        }

        // =====================================================
        // HIGHLANDER PATTERN B: CurrentSituation Resolution
        // =====================================================
        // Runtime state (object ONLY, NO ID) - resolved from CurrentSituationId if present
        if (!string.IsNullOrEmpty(dto.CurrentSituationId))
        {
            scene.CurrentSituation = scene.Situations.FirstOrDefault(s => s.Id == dto.CurrentSituationId);

            if (scene.CurrentSituation == null)
            {
                Console.WriteLine($"[SceneParser] WARNING: Scene '{dto.Id}' references CurrentSituationId '{dto.CurrentSituationId}' " +
                    $"but no such situation found in embedded Situations collection");
            }
        }

        // =====================================================
        // DERIVED PROPERTIES
        // =====================================================
        // SituationCount is computed from collection
        scene.SituationCount = scene.Situations.Count;

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
