
/// <summary>
/// SceneFacade - Query layer for Scene/Situation display
/// Implements query-time action instantiation (Tier 3 of three-tier timing model)
/// Responsibilities:
/// - Query active Scenes at placements
/// - Trigger Situation state transition (Dormant → Active)
/// - Instantiate ChoiceTemplates into actions (LocationAction/NPCAction/PathCard)
/// - Create provisional Scenes for actions with spawn rewards
/// - Return display models to UI
/// Does NOT execute actions (GameFacade handles execution)
/// </summary>
public class SceneFacade
{
    private readonly GameWorld _gameWorld;
    private readonly SceneInstanceFacade _sceneInstanceFacade;
    private readonly RewardApplicationService _rewardApplicationService;
    private readonly SituationCompletionHandler _situationCompletionHandler;

    public SceneFacade(GameWorld gameWorld, SceneInstanceFacade sceneInstanceFacade, RewardApplicationService rewardApplicationService, SituationCompletionHandler situationCompletionHandler)
    {
        _gameWorld = gameWorld;
        _sceneInstanceFacade = sceneInstanceFacade;
        _rewardApplicationService = rewardApplicationService;
        _situationCompletionHandler = situationCompletionHandler;
    }

    // ==================== LOCATION CONTEXT ====================

    /// <summary>
    /// Get resumable scenes at given context (location + optional NPC)
    /// Used for multi-situation scene resumption after navigation
    /// Returns scenes that should auto-activate when player navigates to their required context
    /// Example: Player completes Situation 1 at common_room, navigates to upper_floor,
    ///          this method finds the scene waiting at upper_floor and returns it for resumption
    /// ALL ACTIVE SCENES auto-activate when you enter their context (no "modal" vs "atmospheric" distinction)
    /// </summary>
    /// <param name="locationId">Location player is currently at</param>
    /// <param name="npcId">NPC player is currently interacting with (null if none)</param>
    /// <returns>List of scenes that should resume at this context</returns>
    public List<Scene> GetResumableScenesAtContext(string locationId, string npcId)
    {
        return _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.ShouldActivateAtContext(locationId, npcId))
            .ToList();
    }

    /// <summary>
    /// Get all actions available at a location
    /// TRIGGERS: Situation Dormant → Active transition
    /// CREATES: LocationActions from ChoiceTemplates
    /// CREATES: Provisional Scenes for actions with spawn rewards
    /// Called by UI when player enters location
    /// </summary>
    public List<LocationAction> GetActionsAtLocation(string locationId, Player player)
    {
        // Find active Scenes at this location
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.Location?.Id == locationId)
            .ToList();

        List<LocationAction> allActions = new List<LocationAction>();

        foreach (Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;

            if (situation == null) continue;

            // STATE TRANSITION: Deferred → Instantiated
            if (situation.InstantiationState == InstantiationState.Deferred)
            {
                ActivateSituationForLocation(situation, scene, player);
            }

            // Fetch already-instantiated actions
            List<LocationAction> situationActions = _gameWorld.LocationActions
                .Where(a => a.SituationId == situation.Id)
                .ToList();

            allActions.AddRange(situationActions);
        }

        return allActions;
    }

    /// <summary>
    /// Activate dormant Situation for Location context
    /// Instantiates ChoiceTemplates → LocationActions
    /// Creates provisional Scenes for actions with spawn rewards
    /// </summary>
    private void ActivateSituationForLocation(Situation situation, Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        // Instantiate actions from ChoiceTemplates
        foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
        {
            LocationAction action = new LocationAction
            {
                Id = $"{situation.Id}_action_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Name = choiceTemplate.ActionTextTemplate,
                Description = "",
                ChoiceTemplate = choiceTemplate,
                SituationId = situation.Id,

                // Legacy properties (empty - use ChoiceTemplate)
                RequiredProperties = new List<LocationPropertyType>(),
                OptionalProperties = new List<LocationPropertyType>(),
                ExcludedProperties = new List<LocationPropertyType>(),
                Costs = new ActionCosts(),
                Rewards = new ActionRewards(),
                TimeRequired = 0,
                Availability = new List<TimeBlocks>(),
                Priority = 100
            };

            // PERFECT INFORMATION: Generate scene previews from template metadata
            action.ScenePreviews = GenerateScenePreviews(choiceTemplate, scene, player);

            _gameWorld.LocationActions.Add(action);
        }
    }

    // ==================== NPC CONTEXT ====================

    /// <summary>
    /// Get all actions available with an NPC
    /// TRIGGERS: Situation Dormant → Active transition
    /// CREATES: NPCActions from ChoiceTemplates
    /// CREATES: Provisional Scenes for actions with spawn rewards
    /// Called by UI when player opens conversation with NPC
    /// </summary>
    public List<NPCAction> GetActionsForNPC(string npcId, Player player)
    {
        // Find active Scenes with this NPC
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.Npc?.ID == npcId)
            .ToList();

        List<NPCAction> allActions = new List<NPCAction>();

        foreach (Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;

            if (situation == null) continue;

            // STATE TRANSITION: Dormant → Active
            if (situation.InstantiationState == InstantiationState.Deferred)
            {
                ActivateSituationForNPC(situation, scene, player);
            }

            // Fetch already-instantiated actions
            List<NPCAction> situationActions = _gameWorld.NPCActions
                .Where(a => a.SituationId == situation.Id)
                .ToList();

            allActions.AddRange(situationActions);
        }

        return allActions;
    }

    /// <summary>
    /// Activate dormant Situation for NPC context
    /// Instantiates ChoiceTemplates → NPCActions
    /// Creates provisional Scenes for actions with spawn rewards
    /// </summary>
    private void ActivateSituationForNPC(Situation situation, Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        NPC npc = scene.Npc;

        foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
        {
            NPCAction action = new NPCAction
            {
                Id = $"{situation.Id}_npcaction_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Name = choiceTemplate.ActionTextTemplate,
                Description = "",
                NPCId = npc?.ID,
                ChoiceTemplate = choiceTemplate,
                SituationId = situation.Id,
                ActionType = DetermineNPCActionType(choiceTemplate),
                ChallengeId = choiceTemplate.ChallengeId,
                ChallengeType = choiceTemplate.ChallengeType
            };

            // PERFECT INFORMATION: Generate scene previews from template metadata
            action.ScenePreviews = GenerateScenePreviews(choiceTemplate, scene, player);

            _gameWorld.NPCActions.Add(action);
        }
    }

    // ==================== ROUTE CONTEXT ====================

    /// <summary>
    /// Get all path cards available on a route
    /// TRIGGERS: Situation Dormant → Active transition
    /// CREATES: PathCards from ChoiceTemplates
    /// CREATES: Provisional Scenes for actions with spawn rewards
    /// Called by UI when player begins traveling route
    /// </summary>
    public List<PathCard> GetPathCardsForRoute(string routeId, Player player)
    {
        // Find active Scenes on this route
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.Route?.Id == routeId)
            .ToList();

        List<PathCard> allPathCards = new List<PathCard>();

        foreach (Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;

            if (situation == null) continue;

            // STATE TRANSITION: Dormant → Active
            if (situation.InstantiationState == InstantiationState.Deferred)
            {
                ActivateSituationForRoute(situation, scene, player);
            }

            // Fetch already-instantiated path cards
            List<PathCard> situationCards = _gameWorld.PathCards
                .Where(pc => pc.SituationId == situation.Id)
                .ToList();

            allPathCards.AddRange(situationCards);
        }

        return allPathCards;
    }

    /// <summary>
    /// Activate dormant Situation for Route context
    /// Instantiates ChoiceTemplates → PathCards
    /// Creates provisional Scenes for actions with spawn rewards
    /// </summary>
    private void ActivateSituationForRoute(Situation situation, Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
        {
            PathCard pathCard = new PathCard
            {
                Id = $"{situation.Id}_pathcard_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Name = choiceTemplate.ActionTextTemplate,
                NarrativeText = "",
                ChoiceTemplate = choiceTemplate,

                // Legacy properties (default - use ChoiceTemplate)
                StartsRevealed = true,
                IsHidden = false,
                ExplorationThreshold = 0,
                IsOneTime = false,
                StaminaCost = 0,
                TravelTimeSegments = 0,
                StatRequirements = new Dictionary<string, int>()
            };

            // PERFECT INFORMATION: Generate scene previews from template metadata
            pathCard.ScenePreviews = GenerateScenePreviews(choiceTemplate, scene, player);

            _gameWorld.PathCards.Add(pathCard);
        }
    }

    // ==================== SHARED HELPERS ====================

    /// <summary>
    /// Build SceneSpawnContext from parent scene placement for placeholder resolution
    /// Used to resolve {NPCName}, {LocationName}, {PlayerName} in scene preview display names
    /// </summary>
    private SceneSpawnContext BuildContextFromParentScene(Scene parentScene, Player player)
    {
        return new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = parentScene.Location,
            CurrentNPC = parentScene.Npc,
            CurrentRoute = parentScene.Route,
            CurrentSituation = null
        };
    }

    /// <summary>
    /// Determine NPCActionType from ChoiceTemplate properties
    /// Migrated from SceneInstantiator (same logic, query-time execution)
    /// </summary>
    private NPCActionType DetermineNPCActionType(ChoiceTemplate template)
    {
        if (template.ActionType == ChoiceActionType.StartChallenge)
        {
            if (template.ChallengeType == TacticalSystemType.Social)
                return NPCActionType.StartConversation;
            else
                return NPCActionType.InitiateSituation;
        }
        else if (template.ActionType == ChoiceActionType.Navigate)
        {
            return NPCActionType.StartConversationTree;
        }
        else // ChoiceActionType.Instant
        {
            return NPCActionType.Instant;
        }
    }

    /// <summary>
    /// Generate scene previews from ChoiceTemplate reward spawns
    /// PERFECT INFORMATION: Shows player WHERE scenes will spawn and WHAT they contain
    /// Replaces provisional scene pattern with DTO generation from template metadata
    /// </summary>
    private List<ScenePreview> GenerateScenePreviews(ChoiceTemplate choiceTemplate, Scene parentScene, Player player)
    {
        List<ScenePreview> previews = new List<ScenePreview>();

        if (choiceTemplate.RewardTemplate == null || !choiceTemplate.RewardTemplate.ScenesToSpawn.Any())
            return previews; // No scenes to preview

        foreach (SceneSpawnReward spawnReward in choiceTemplate.RewardTemplate.ScenesToSpawn)
        {
            SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == spawnReward.SceneTemplateId);
            if (template == null)
            {
                Console.WriteLine($"[SceneFacade] WARNING: SceneTemplate '{spawnReward.SceneTemplateId}' not found for preview");
                continue;
            }

            // Get placement filter from template (spawned scene inherits template's filter - HIGHLANDER)
            PlacementFilter filter = template.PlacementFilter;

            // Collect unique challenge types from situation templates
            List<TacticalSystemType> challengeTypes = template.SituationTemplates
                .SelectMany(st => st.ChoiceTemplates)
                .Where(ct => ct.ChallengeType.HasValue)
                .Select(ct => ct.ChallengeType.Value)
                .Distinct()
                .ToList();

            // Use display name template directly (AI will generate complete text after resolution)
            string displayName = template.DisplayNameTemplate;

            ScenePreview preview = new ScenePreview
            {
                SceneTemplateId = template.Id,
                DisplayName = displayName,
                Tier = template.Tier,
                ResolvedPlacementId = null, // Cannot resolve at preview time (requires FindOrCreate)
                PlacementType = filter?.PlacementType,
                SituationCount = template.SituationTemplates.Count,
                ChallengeTypes = challengeTypes,
                ExpiresInDays = template.ExpirationDays,
                Archetype = template.Archetype
            };

            previews.Add(preview);
        }

        return previews;
    }
}
