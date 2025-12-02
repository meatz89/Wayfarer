/// <summary>
/// Loads Scene-Situation architecture components: states, achievements, templates, scenes, screen expansions.
/// COMPOSITION OVER INHERITANCE: Extracted from PackageLoader for single responsibility.
/// </summary>
public class SceneSituationSystemLoader
{
    private readonly GameWorld _gameWorld;
    private readonly SceneGenerationFacade _sceneGenerationFacade;

    public SceneSituationSystemLoader(GameWorld gameWorld, SceneGenerationFacade sceneGenerationFacade)
    {
        _gameWorld = gameWorld;
        _sceneGenerationFacade = sceneGenerationFacade;
    }

    /// <summary>
    /// Load all scene/situation content from a package.
    /// Called by PackageLoader during package loading.
    /// </summary>
    public void LoadSceneSituationContent(
        List<StateDTO> stateDtos,
        List<AchievementDTO> achievementDtos,
        List<SceneTemplateDTO> sceneTemplateDtos,
        List<SceneDTO> sceneDtos,
        List<ConversationTreeDTO> conversationTreeDtos,
        List<ObservationSceneDTO> observationSceneDtos,
        List<EmergencySituationDTO> emergencySituationDtos,
        List<SituationDTO> situationDtos,
        PackageLoadResult result,
        bool allowSkeletons)
    {
        LoadStates(stateDtos, allowSkeletons);
        LoadAchievements(achievementDtos, allowSkeletons);
        LoadSceneTemplates(sceneTemplateDtos, result, allowSkeletons);
        LoadScenes(sceneDtos, result, allowSkeletons);
        LoadConversationTrees(conversationTreeDtos, allowSkeletons);
        LoadObservationScenes(observationSceneDtos, allowSkeletons);
        LoadEmergencySituations(emergencySituationDtos, allowSkeletons);
        LoadSituations(situationDtos, allowSkeletons);
    }

    private void LoadStates(List<StateDTO> stateDtos, bool allowSkeletons)
    {
        if (stateDtos == null) return;

        List<State> states = StateParser.ParseStates(stateDtos);
        foreach (State state in states)
        {
            _gameWorld.States.Add(state);
        }
    }

    private void LoadAchievements(List<AchievementDTO> achievementDtos, bool allowSkeletons)
    {
        if (achievementDtos == null) return;

        List<Achievement> achievements = AchievementParser.ParseAchievements(achievementDtos);
        foreach (Achievement achievement in achievements)
        {
            _gameWorld.Achievements.Add(achievement);
        }
    }

    private void LoadSceneTemplates(List<SceneTemplateDTO> sceneTemplateDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (sceneTemplateDtos == null) return;

        SceneTemplateParser parser = new SceneTemplateParser(_gameWorld, _sceneGenerationFacade);
        foreach (SceneTemplateDTO dto in sceneTemplateDtos)
        {
            SceneTemplate template = parser.ParseSceneTemplate(dto);
            _gameWorld.SceneTemplates.Add(template);
            result.SceneTemplatesAdded.Add(template);
        }

        if (sceneTemplateDtos.Count > 0)
        {
            Console.WriteLine($"[SceneSituationSystemLoader] Loaded {sceneTemplateDtos.Count} SceneTemplates");
        }
    }

    private void LoadScenes(List<SceneDTO> sceneDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (sceneDtos == null) return;

        foreach (SceneDTO dto in sceneDtos)
        {
            Scene scene = SceneParser.ConvertDTOToScene(dto, _gameWorld);
            _gameWorld.Scenes.Add(scene);
            result.ScenesAdded.Add(scene);
        }

        if (sceneDtos.Count > 0)
        {
            Console.WriteLine($"[SceneSituationSystemLoader] Loaded {sceneDtos.Count} Scene instances");
        }
    }

    private void LoadConversationTrees(List<ConversationTreeDTO> conversationTrees, bool allowSkeletons)
    {
        if (conversationTrees == null) return;

        EntityResolver entityResolver = new EntityResolver(_gameWorld);
        foreach (ConversationTreeDTO dto in conversationTrees)
        {
            ConversationTree tree = ConversationTreeParser.Parse(dto, entityResolver);
            _gameWorld.ConversationTrees.Add(tree);
        }
    }

    private void LoadObservationScenes(List<ObservationSceneDTO> observationScenes, bool allowSkeletons)
    {
        if (observationScenes == null) return;

        EntityResolver entityResolver = new EntityResolver(_gameWorld);
        foreach (ObservationSceneDTO dto in observationScenes)
        {
            ObservationScene scene = ObservationSceneParser.Parse(dto, entityResolver);
            _gameWorld.ObservationScenes.Add(scene);
        }
    }

    private void LoadEmergencySituations(List<EmergencySituationDTO> emergencySituations, bool allowSkeletons)
    {
        if (emergencySituations == null) return;

        foreach (EmergencySituationDTO dto in emergencySituations)
        {
            EmergencySituation emergency = EmergencyParser.Parse(dto, _gameWorld);
            _gameWorld.EmergencySituations.Add(emergency);
        }
    }

    private void LoadSituations(List<SituationDTO> situationDtos, bool allowSkeletons)
    {
        // ARCHITECTURAL CONSTRAINT: Standalone situations not supported
        // All situations must be owned by Scenes (created by SceneInstantiator)
        if (situationDtos != null && situationDtos.Any())
        {
            Console.WriteLine($"[SceneSituationSystemLoader] WARNING: Package contains {situationDtos.Count} standalone situations - these are IGNORED. Situations must be part of SceneTemplates.");
        }
    }
}
