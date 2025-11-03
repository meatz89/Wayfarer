using Wayfarer.Content;
using Wayfarer.GameState.Enums;
using Wayfarer.Subsystems.Consequence;

namespace Wayfarer.Subsystems.Scene;

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
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly RewardApplicationService _rewardApplicationService;

    public SceneFacade(GameWorld gameWorld, SceneInstantiator sceneInstantiator, RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld;
        _sceneInstantiator = sceneInstantiator;
        _rewardApplicationService = rewardApplicationService;
    }

    // ==================== LOCATION CONTEXT ====================

    /// <summary>
    /// Get modal scene at location (if one exists)
    /// Modal scenes take over the full screen on location entry (Sir Brante forced moment)
    /// Returns null if no modal scene at this location
    /// Called by UI when player enters location to check for modal takeover
    /// PLACEMENT-AGNOSTIC: Finds scenes placed at Location/NPC/Route that are present at this location
    /// </summary>
    public global::Scene GetModalSceneAtLocation(string locationId)
    {
        List<global::Scene> allActive = _gameWorld.Scenes.Where(s => s.State == SceneState.Active).ToList();
        Console.WriteLine($"[SceneFacade] GetModalSceneAtLocation('{locationId}')");
        Console.WriteLine($"  Total active scenes: {allActive.Count}");
        foreach (global::Scene s in allActive)
        {
            Console.WriteLine($"    - {s.Id}: {s.PlacementType}/{s.PlacementId}, Modal={s.PresentationMode == PresentationMode.Modal}");
        }

        return _gameWorld.Scenes
            .FirstOrDefault(s => s.State == SceneState.Active &&
                               s.PresentationMode == PresentationMode.Modal &&
                               IsSceneAtLocation(s, locationId));
    }

    /// <summary>
    /// Check if scene is present at this location (placement-agnostic)
    /// Handles three placement types:
    /// - Location: Direct placement at location
    /// - NPC: NPC is at this location
    /// - Route: Route departs from this location
    /// </summary>
    private bool IsSceneAtLocation(global::Scene scene, string locationId)
    {
        // Direct location placement
        if (scene.PlacementType == PlacementType.Location)
        {
            bool matches = scene.PlacementId == locationId;
            if (matches)
            {
                Console.WriteLine($"  [IsSceneAtLocation] Scene '{scene.Id}' MATCHES: Location placement '{scene.PlacementId}'");
            }
            else
            {
                Console.WriteLine($"  [IsSceneAtLocation] Scene '{scene.Id}' Location placement: '{scene.PlacementId}' (target: '{locationId}') - no match");
            }
            return matches;
        }

        // NPC placement - check if NPC is at this location
        if (scene.PlacementType == PlacementType.NPC)
        {
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == scene.PlacementId);
            string npcLocationId = npc?.Location?.Id;
            bool matches = npcLocationId == locationId;
            Console.WriteLine($"  [IsSceneAtLocation] Scene '{scene.Id}' NPC placement: NPC '{scene.PlacementId}' at location '{npcLocationId}' (target: '{locationId}') - {(matches ? "MATCH" : "no match")}");
            return matches;
        }

        // Route placement - check if route departs from this location
        if (scene.PlacementType == PlacementType.Route)
        {
            RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == scene.PlacementId);
            bool matches = route?.OriginLocationSpot == locationId;
            Console.WriteLine($"  [IsSceneAtLocation] Scene '{scene.Id}' Route placement: Route '{scene.PlacementId}' origin '{route?.OriginLocationSpot}' (target: '{locationId}') - {(matches ? "MATCH" : "no match")}");
            return matches;
        }

        Console.WriteLine($"  [IsSceneAtLocation] Scene '{scene.Id}' - Unknown placement type: {scene.PlacementType}");
        return false;
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
        List<global::Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.PlacementType == PlacementType.Location &&
                       s.PlacementId == locationId)
            .ToList();

        List<LocationAction> allActions = new List<LocationAction>();

        foreach (global::Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation from GameWorld.Situations using scene.CurrentSituationId
            Situation situation = _gameWorld.Situations
                .FirstOrDefault(s => s.Id == scene.CurrentSituationId);

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
    /// AutoAdvance situations execute rewards immediately
    /// </summary>
    private void ActivateSituationForLocation(Situation situation, global::Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        // AutoAdvance detection: Execute AutoProgressRewards immediately
        if (situation.IsAutoAdvance && situation.Template.AutoProgressRewards != null)
        {
            _rewardApplicationService.ApplyChoiceReward(situation.Template.AutoProgressRewards, situation);
            return; // No actions to instantiate for AutoAdvance situations
        }

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

            // Create provisional Scenes for SceneSpawnRewards
            CreateProvisionalScenesForAction(choiceTemplate, action, scene, player);

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
        List<global::Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.PlacementType == PlacementType.NPC &&
                       s.PlacementId == npcId)
            .ToList();

        List<NPCAction> allActions = new List<NPCAction>();

        foreach (global::Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation from GameWorld.Situations using scene.CurrentSituationId
            Situation situation = _gameWorld.Situations
                .FirstOrDefault(s => s.Id == scene.CurrentSituationId);

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
    /// AutoAdvance situations execute rewards immediately
    /// </summary>
    private void ActivateSituationForNPC(Situation situation, global::Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        // AutoAdvance detection: Execute AutoProgressRewards immediately
        if (situation.IsAutoAdvance && situation.Template.AutoProgressRewards != null)
        {
            _rewardApplicationService.ApplyChoiceReward(situation.Template.AutoProgressRewards, situation);
            return; // No actions to instantiate for AutoAdvance situations
        }

        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == scene.PlacementId);

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

            // Create provisional Scenes for SceneSpawnRewards
            CreateProvisionalScenesForAction(choiceTemplate, action, scene, player);

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
        List<global::Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.PlacementType == PlacementType.Route &&
                       s.PlacementId == routeId)
            .ToList();

        List<PathCard> allPathCards = new List<PathCard>();

        foreach (global::Scene scene in scenes)
        {
            // PHASE 1.3: Skip completed scenes (state machine method)
            if (scene.IsComplete()) continue;

            // Get current Situation from GameWorld.Situations using scene.CurrentSituationId
            Situation situation = _gameWorld.Situations
                .FirstOrDefault(s => s.Id == scene.CurrentSituationId);

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
    /// AutoAdvance situations execute rewards immediately
    /// </summary>
    private void ActivateSituationForRoute(Situation situation, global::Scene scene, Player player)
    {
        situation.InstantiationState = InstantiationState.Instantiated;

        // AutoAdvance detection: Execute AutoProgressRewards immediately
        if (situation.IsAutoAdvance && situation.Template.AutoProgressRewards != null)
        {
            _rewardApplicationService.ApplyChoiceReward(situation.Template.AutoProgressRewards, situation);
            return; // No actions to instantiate for AutoAdvance situations
        }

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

            // Create provisional Scenes for SceneSpawnRewards
            CreateProvisionalScenesForAction(choiceTemplate, pathCard, scene, player);

            _gameWorld.PathCards.Add(pathCard);
        }
    }

    // ==================== SHARED HELPERS ====================

    /// <summary>
    /// Create provisional Scenes for ChoiceTemplate with SceneSpawnRewards
    /// Shared across all three action types (Location/NPC/Route)
    /// Stores provisional global::SceneID on action for perfect information display
    /// </summary>
    private void CreateProvisionalScenesForAction<T>(
        ChoiceTemplate choiceTemplate,
        T action,
        global::Scene parentScene,
        Player player) where T : class
    {
        if (choiceTemplate.RewardTemplate?.ScenesToSpawn?.Count > 0)
        {
            foreach (SceneSpawnReward spawnReward in choiceTemplate.RewardTemplate.ScenesToSpawn)
            {
                SceneTemplate template = _gameWorld.SceneTemplates
                    .FirstOrDefault(t => t.Id == spawnReward.SceneTemplateId);

                if (template != null)
                {
                    global::Scene provisionalScene = _sceneInstantiator.CreateProvisionalScene(
                        template,
                        spawnReward,
                        BuildSpawnContext(parentScene, player)
                    );

                    // Store provisional global::SceneID on action (perfect information)
                    if (action is LocationAction locationAction)
                        locationAction.ProvisionalSceneId = provisionalScene.Id;
                    else if (action is NPCAction npcAction)
                        npcAction.ProvisionalSceneId = provisionalScene.Id;
                    else if (action is PathCard pathCard)
                        pathCard.SceneId = provisionalScene.Id;
                }
            }
        }
    }

    /// <summary>
    /// Build SceneSpawnContext from parent global::Sceneplacement
    /// </summary>
    private SceneSpawnContext BuildSpawnContext(global::Scene parentScene, Player player)
    {
        return new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = parentScene.PlacementType == PlacementType.Location ?
                _gameWorld.Venues.FirstOrDefault(v => v.Id == parentScene.PlacementId) : null,
            CurrentNPC = parentScene.PlacementType == PlacementType.NPC ?
                _gameWorld.NPCs.FirstOrDefault(n => n.ID == parentScene.PlacementId) : null,
            CurrentRoute = parentScene.PlacementType == PlacementType.Route ?
                _gameWorld.Routes.FirstOrDefault(r => r.Id == parentScene.PlacementId) : null,
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
}
