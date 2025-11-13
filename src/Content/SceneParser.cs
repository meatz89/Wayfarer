/// <summary>
/// Parser for converting SceneDTO to Scene domain model
/// HIGHLANDER: JSON → PackageLoader → Parser → Entity (single instantiation path)
/// </summary>
public static class SceneParser
{
    /// <summary>
    /// Convert a SceneDTO to a Scene domain model (System 5: Scene Instantiation)
    /// Receives pre-resolved entity objects from EntityResolver (System 4)
    /// Resolves template reference and parses embedded situations
    /// </summary>
    public static Scene ConvertDTOToScene(
        SceneDTO dto,
        GameWorld gameWorld,
        Location resolvedLocation,
        NPC resolvedNpc,
        RouteOption resolvedRoute)
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
        // ENTITY CONSTRUCTION (System 5: Direct Object References)
        // =====================================================
        Scene scene = new Scene
        {
            Id = dto.Id,
            TemplateId = dto.TemplateId,
            Template = template,
            Location = resolvedLocation,
            Npc = resolvedNpc,
            Route = resolvedRoute,
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
        // Scene OWNS Situations - parse embedded situations and add to collection
        foreach (SituationDTO situationDto in dto.Situations)
        {
            Situation situation = SituationParser.ConvertDTOToSituation(situationDto, gameWorld);

            // CRITICAL: Set composition relationship (Situation → ParentScene)
            // Required for GetPlacementId() which queries ParentScene.Location/Npc/Route objects
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
