
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
                       s.ShouldResumeAtContext(locationId, npcId))
            .ToList();
    }

    /// <summary>
    /// Get all actions available at a location
    /// THREE-TIER TIMING MODEL: Creates ephemeral actions (Tier 3) fresh on every query
    /// Actions never stored, always rebuilt from ChoiceTemplates
    /// Called by UI when player enters location
    /// </summary>
    public List<LocationAction> GetActionsAtLocation(string locationId, Player player)
    {
        // Find active Scenes at this location
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Location (situation owns placement)
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Location?.Id == locationId)
            .ToList();

        List<LocationAction> allActions = new List<LocationAction>();

        foreach (Scene scene in scenes)
        {
            // Skip completed scenes
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;
            if (situation == null) continue;

            // Create actions fresh from ChoiceTemplates (ephemeral, not stored)
            foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
            {
                LocationAction action = new LocationAction
                {
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

                allActions.Add(action); // Add to return list only (NOT stored in GameWorld)
            }
        }

        return allActions;
    }

    // ==================== NPC CONTEXT ====================

    /// <summary>
    /// Get all actions available with an NPC
    /// THREE-TIER TIMING MODEL: Creates ephemeral actions (Tier 3) fresh on every query
    /// Actions never stored, always rebuilt from ChoiceTemplates
    /// Called by UI when player opens conversation with NPC
    /// </summary>
    public List<NPCAction> GetActionsForNPC(string npcId, Player player)
    {
        // Find active Scenes with this NPC
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Npc (situation owns placement)
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Npc?.ID == npcId)
            .ToList();

        List<NPCAction> allActions = new List<NPCAction>();

        foreach (Scene scene in scenes)
        {
            // Skip completed scenes
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;
            if (situation == null) continue;

            // Get NPC from situation (direct object reference)
            NPC npc = situation.Npc;

            // Create actions fresh from ChoiceTemplates (ephemeral, not stored)
            foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
            {
                NPCAction action = new NPCAction
                {
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

                allActions.Add(action); // Add to return list only (NOT stored in GameWorld)
            }
        }

        return allActions;
    }

    // ==================== ROUTE CONTEXT ====================

    /// <summary>
    /// Get all path cards available on a route
    /// THREE-TIER TIMING MODEL: Creates ephemeral path cards (Tier 3) fresh on every query
    /// Path cards never stored, always rebuilt from ChoiceTemplates
    /// Called by UI when player begins traveling route
    /// </summary>
    public List<PathCard> GetPathCardsForRoute(string routeId, Player player)
    {
        // Find active Scenes on this route
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Route (situation owns placement)
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Route?.Id == routeId)
            .ToList();

        List<PathCard> allPathCards = new List<PathCard>();

        foreach (Scene scene in scenes)
        {
            // Skip completed scenes
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;
            if (situation == null) continue;

            // Create path cards fresh from ChoiceTemplates (ephemeral, not stored)
            foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
            {
                PathCard pathCard = new PathCard
                {
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

                allPathCards.Add(pathCard); // Add to return list only (NOT stored in GameWorld)
            }
        }

        return allPathCards;
    }

    /// <summary>
    /// Get path cards for specific route segment (geographic specificity)
    /// THREE-TIER TIMING MODEL: Creates ephemeral path cards (Tier 3) fresh on every query
    /// Path cards never stored, always rebuilt from ChoiceTemplates
    /// Called by UI when player traveling at specific segment of route
    /// ARCHITECTURAL: Route segment situations enable geographic specificity (Tutorial A3 pattern)
    /// </summary>
    public List<PathCard> GetPathCardsForRouteSegment(string routeId, int segmentIndex, Player player)
    {
        // Find active Scenes on this route at this specific segment
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Route AND CurrentSituation.SegmentIndex
        List<Scene> scenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Route?.Id == routeId &&
                       s.CurrentSituation?.SegmentIndex == segmentIndex)
            .ToList();

        List<PathCard> allPathCards = new List<PathCard>();

        foreach (Scene scene in scenes)
        {
            // Skip completed scenes
            if (scene.IsComplete()) continue;

            // Get current Situation (direct object reference)
            Situation situation = scene.CurrentSituation;
            if (situation == null) continue;

            // Create path cards fresh from ChoiceTemplates (ephemeral, not stored)
            foreach (ChoiceTemplate choiceTemplate in situation.Template.ChoiceTemplates)
            {
                PathCard pathCard = new PathCard
                {
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

                allPathCards.Add(pathCard); // Add to return list only (NOT stored in GameWorld)
            }
        }

        return allPathCards;
    }

    // ==================== SHARED HELPERS ====================

    /// <summary>
    /// Build SceneSpawnContext from parent scene placement for placeholder resolution
    /// Used to resolve {NPCName}, {LocationName}, {PlayerName} in scene preview display names
    /// </summary>
    private SceneSpawnContext BuildContextFromParentScene(Scene parentScene, Player player)
    {
        // HIERARCHICAL PLACEMENT: Get placement from CurrentSituation (situation owns placement)
        return new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = parentScene.CurrentSituation?.Location,
            CurrentNPC = parentScene.CurrentSituation?.Npc,
            CurrentRoute = parentScene.CurrentSituation?.Route,
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

            // Hierarchical placement: Determine primary placement type from base filters
            // Precedence order: Location > NPC > Route (most specific to least specific)
            PlacementType? primaryPlacementType = null;
            if (template.BaseLocationFilter != null)
                primaryPlacementType = PlacementType.Location;
            else if (template.BaseNpcFilter != null)
                primaryPlacementType = PlacementType.NPC;
            else if (template.BaseRouteFilter != null)
                primaryPlacementType = PlacementType.Route;

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
                PlacementType = primaryPlacementType,
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
