/// <summary>
/// Builds challenge view models for Mental and Physical situations at locations.
/// Extracted from LocationFacade for file size compliance (COMPOSITION pattern).
/// Queries GameWorld for active scenes and builds UI-ready view models.
/// </summary>
public class LocationChallengeBuilder
{
    private readonly GameWorld _gameWorld;
    private readonly DifficultyCalculationService _difficultyService;
    private readonly ItemRepository _itemRepository;

    public LocationChallengeBuilder(
        GameWorld gameWorld,
        DifficultyCalculationService difficultyService,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _difficultyService = difficultyService;
        _itemRepository = itemRepository;
    }

    public ChallengeBuildResult BuildMentalChallenges(Location spot)
    {
        return BuildChallengesBySystemType(spot, TacticalSystemType.Mental, "mental", "Exposure");
    }

    public ChallengeBuildResult BuildPhysicalChallenges(Location spot)
    {
        return BuildChallengesBySystemType(spot, TacticalSystemType.Physical, "physical", "Danger");
    }

    private ChallengeBuildResult BuildChallengesBySystemType(
        Location spot, TacticalSystemType systemType, string systemTypeStr, string difficultyLabel)
    {
        List<SituationCardViewModel> ambientSituations = new List<SituationCardViewModel>();
        List<SceneWithSituationsViewModel> sceneGroups = new List<SceneWithSituationsViewModel>();

        // SCENE-SITUATION ARCHITECTURE: Query active Scenes at this location, get Situations from Scene.Situations
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Location (situation owns placement)
        List<Scene> scenesAtLocation = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Location != null &&
                       s.CurrentSituation.Location == spot)
            .ToList();

        // Get all situations from scenes at this location (direct object ownership)
        List<Situation> allVisibleSituations = scenesAtLocation
            .SelectMany(scene => scene.Situations)
            .ToList();

        // Filter to this system type only - ALL intensity levels included
        // Player state does NOT filter situation visibility (Challenge and Consequence Philosophy)
        // Learning comes from seeing choices they can't afford, not hidden situations
        List<Situation> systemSituations = allVisibleSituations
            .Where(g => g.SystemType == systemType)
            .Where(g => g.IsAvailable && !g.IsCompleted)
            .ToList();

        // Group situations by scene (ambient situations have no scene parent)
        // DOMAIN COLLECTION PRINCIPLE: Use List, not Dictionary
        List<(Scene Scene, List<Situation> Situations)> situationsByScene = new List<(Scene, List<Situation>)>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in systemSituations)
        {
            // Check if this situation belongs to an scene
            Scene parentScene = FindParentScene(spot, situation);

            if (parentScene != null)
            {
                // Find existing entry by object equality
                int existingIndex = -1;
                for (int i = 0; i < situationsByScene.Count; i++)
                {
                    if (situationsByScene[i].Scene == parentScene)
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    situationsByScene[existingIndex].Situations.Add(situation);
                }
                else
                {
                    situationsByScene.Add((parentScene, new List<Situation> { situation }));
                }
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach ((Scene scene, List<Situation> sceneSituations) in situationsByScene)
        {
            sceneGroups.Add(BuildSceneWithSituations(scene, sceneSituations, systemTypeStr, difficultyLabel));
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentScene(Location spot, Situation situation)
    {
        // Query GameWorld.Scenes by placement, check if situation matches
        // HIERARCHICAL PLACEMENT: Check if any situation in scene is at this location
        return _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .Where(s => s.Situations.Any(sit => sit.Location == spot))
            .FirstOrDefault(s => s.Situations.Contains(situation));
    }

    public ChallengeBuildResult GroupSituationsByScene(
        NPC npc, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        List<SituationCardViewModel> ambientSituations = new List<SituationCardViewModel>();
        List<SceneWithSituationsViewModel> sceneGroups = new List<SceneWithSituationsViewModel>();

        // Group situations by scene (ambient situations have no scene parent)
        // DOMAIN COLLECTION PRINCIPLE: Use List, not Dictionary
        List<(Scene Scene, List<Situation> Situations)> situationsByScene = new List<(Scene, List<Situation>)>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in situations)
        {
            // Check if this situation belongs to an scene from this NPC
            Scene parentScene = FindParentSceneForNPC(npc, situation);

            if (parentScene != null)
            {
                // Find existing entry by object equality
                int existingIndex = -1;
                for (int i = 0; i < situationsByScene.Count; i++)
                {
                    if (situationsByScene[i].Scene == parentScene)
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    situationsByScene[existingIndex].Situations.Add(situation);
                }
                else
                {
                    situationsByScene.Add((parentScene, new List<Situation> { situation }));
                }
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach ((Scene scene, List<Situation> sceneSituations) in situationsByScene)
        {
            sceneGroups.Add(BuildSceneWithSituations(scene, sceneSituations, systemTypeStr, difficultyLabel));
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentSceneForNPC(NPC npc, Situation situation)
    {
        // Query GameWorld.Scenes by placement type, check if situation matches
        // HIERARCHICAL PLACEMENT: Check if any situation in scene has this NPC
        // HIGHLANDER: Object equality, no ID comparison
        return _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .Where(s => s.Situations.Any(sit => sit.Npc == npc))
            .FirstOrDefault(s => s.Situations.Contains(situation));
    }

    private SceneWithSituationsViewModel BuildSceneWithSituations(Scene scene, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        return new SceneWithSituationsViewModel
        {
            Scene = scene,
            Name = scene.DisplayName,
            Description = scene.IntroNarrative,
            Intensity = 0,  // Intensity removed from Scene - defaulting to 0
            Contexts = new List<string>(),  // Contexts removed from Scene
            ContextsDisplay = "",  // Contexts removed from Scene
            Situations = situations.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList()
        };
    }

    public SituationCardViewModel BuildSituationCard(Situation situation, string systemType, string difficultyLabel)
    {
        int baseDifficulty = GetBaseDifficultyForSituation(situation);
        DifficultyResult difficultyResult = _difficultyService.CalculateDifficulty(situation, baseDifficulty, _itemRepository);

        return new SituationCardViewModel
        {
            Situation = situation,
            Name = situation.Name,
            Description = situation.Description,
            SystemType = systemType,
            Type = situation.Type.ToString(),  // Copy from domain entity (Normal/Crisis)
            Difficulty = difficultyResult.FinalDifficulty,
            DifficultyLabel = difficultyLabel,
            Obligation = situation.Obligation,
            IsIntroAction = situation.Obligation != null,
            // HIGHLANDER: EntryCost uses negative values for costs
            FocusCost = situation.EntryCost.Focus < 0 ? -situation.EntryCost.Focus : 0,
            StaminaCost = situation.EntryCost.Stamina < 0 ? -situation.EntryCost.Stamina : 0
        };
    }

    private int GetBaseDifficultyForSituation(Situation situation)
    {
        switch (situation.SystemType)
        {
            case TacticalSystemType.Social:
                SocialChallengeDeck socialDeck = situation.Deck as SocialChallengeDeck;
                return socialDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Mental:
                MentalChallengeDeck mentalDeck = situation.Deck as MentalChallengeDeck;
                return mentalDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Physical:
                PhysicalChallengeDeck physicalDeck = situation.Deck as PhysicalChallengeDeck;
                return physicalDeck?.DangerThreshold ?? 10;

            default:
                return 10;
        }
    }
}
