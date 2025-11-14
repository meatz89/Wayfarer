/// <summary>
/// RUNTIME PLAYABILITY VALIDATOR - Validates spawned Scene instances for actual player accessibility
///
/// DISTINCTION FROM SceneTemplateValidator:
/// - SceneTemplateValidator: Parse-time structural validation (templates have guaranteed paths)
/// - SpawnedScenePlayabilityValidator: Runtime playability validation (spawned instances are reachable)
///
/// CRITICAL: A scene can pass template validation but fail playability validation
/// Template has guaranteed path, but spawned instance might have:
/// - Marker resolution failures (references non-existent locations/NPCs)
/// - Placement at unreachable location
/// - Missing CurrentSituation
/// - All choices require unavailable resources
///
/// This validator catches SOFT LOCK conditions: Player completes previous scene,
/// next scene spawns successfully, but player cannot interact with it.
///
/// USAGE: Called after scene spawn (SpawnFacade, SceneInstanceFacade)
/// THROWS: InvalidOperationException with diagnostic info if scene unplayable
/// </summary>
public class SpawnedScenePlayabilityValidator
{
    private readonly GameWorld _gameWorld;

    public SpawnedScenePlayabilityValidator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Validate spawned scene for playability
    /// THROWS InvalidOperationException if scene unplayable (FAIL FAST)
    /// Returns void if playable (no news is good news)
    /// </summary>
    public void ValidatePlayability(Scene scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        List<string> errors = new List<string>();

        // RULE 1: Scene must have current situation
        if (scene.CurrentSituation == null)
        {
            errors.Add($"Scene '{scene.Id}' has no CurrentSituation - player cannot interact");
        }
        else
        {
            // RULE 2: Current situation must have choices
            if (scene.CurrentSituation.Template == null)
            {
                errors.Add($"Scene '{scene.Id}' current situation '{scene.CurrentSituation.Id}' has no Template - cannot generate choices");
            }
            else if (scene.CurrentSituation.Template.ChoiceTemplates == null || !scene.CurrentSituation.Template.ChoiceTemplates.Any())
            {
                errors.Add($"Scene '{scene.Id}' current situation '{scene.CurrentSituation.Id}' has no ChoiceTemplates - player has no options");
            }
            else
            {
                // RULE 3: At least one choice must be guaranteed accessible
                // For A-story: Must have guaranteed path (no requirements OR challenge with both success/failure spawns)
                if (scene.Category == StoryCategory.MainStory)
                {
                    bool hasGuaranteedPath = scene.CurrentSituation.Template.ChoiceTemplates.Any(IsGuaranteedAccessibleChoice);
                    if (!hasGuaranteedPath)
                    {
                        errors.Add($"A-story scene '{scene.Id}' (A{scene.MainStorySequence}) situation '{scene.CurrentSituation.Id}' " +
                                  $"has no guaranteed accessible path - SOFT LOCK RISK");
                    }
                }
            }

            // RULE 4: Required location must exist (if specified)
            ValidateRequiredLocation(scene.CurrentSituation, errors);

            // RULE 5: Required NPC must exist (if specified)
            ValidateRequiredNPC(scene.CurrentSituation, errors);
        }

        // RULE 6: Placement must be valid (location/NPC/route must exist)
        ValidatePlacement(scene, errors);

        // RULE 7: Marker resolution DELETED - markers no longer exist in new architecture
        // Entities use hierarchical placement via Situation.Location/Npc entity references

        if (errors.Any())
        {
            string errorSummary = string.Join("\n", errors);
            throw new InvalidOperationException(
                $"Scene '{scene.Id}' failed playability validation:\n{errorSummary}\n\n" +
                $"Scene Details:\n" +
                $"- TemplateId: {scene.TemplateId}\n" +
                $"- Category: {scene.Category}\n" +
                $"- MainStorySequence: {scene.MainStorySequence}\n" +
                $"- State: {scene.State}\n" +
                $"- CurrentSituationId: {scene.CurrentSituation?.Id}\n" +
                $"- CurrentSituation Location: {scene.CurrentSituation?.Location?.Id}\n" +
                $"- CurrentSituation Npc: {scene.CurrentSituation?.Npc?.ID}\n" +
                $"- CurrentSituation Route: {scene.CurrentSituation?.Route?.Id}\n" +
                $"- Situations Count: {scene.Situations.Count}\n" +
                $"This scene spawned successfully but is UNPLAYABLE. Player will be SOFT LOCKED."
            );
        }
    }

    /// <summary>
    /// Check if choice is guaranteed accessible (no requirements or both success/failure paths exist)
    /// Replicates SceneTemplateValidator.IsGuaranteedSuccessChoice logic
    /// </summary>
    private bool IsGuaranteedAccessibleChoice(ChoiceTemplate choice)
    {
        bool hasNoRequirements = choice.RequirementFormula == null ||
                                 choice.RequirementFormula.OrPaths == null ||
                                 !choice.RequirementFormula.OrPaths.Any();

        // Instant choices with no requirements are always accessible
        if (choice.ActionType == ChoiceActionType.Instant && hasNoRequirements)
        {
            return true;
        }

        // Navigation choices with no requirements are always accessible
        if (choice.ActionType == ChoiceActionType.Navigate && hasNoRequirements)
        {
            return true;
        }

        // Challenge choices with no requirements AND both success/failure paths are guaranteed progression
        if (choice.ActionType == ChoiceActionType.StartChallenge && hasNoRequirements)
        {
            bool successSpawns = choice.RewardTemplate?.ScenesToSpawn?.Any() == true;
            bool failureSpawns = choice.OnFailureReward?.ScenesToSpawn?.Any() == true;
            return successSpawns && failureSpawns;
        }

        return false;
    }

    /// <summary>
    /// Validate required location exists in GameWorld
    /// Hierarchical placement: Situation has direct object reference (not string ID)
    /// </summary>
    private void ValidateRequiredLocation(Situation situation, List<string> errors)
    {
        if (situation.Location != null)
        {
            // Verify the location reference is still valid in GameWorld
            if (!_gameWorld.Locations.Contains(situation.Location))
            {
                errors.Add($"Situation '{situation.Id}' references location '{situation.Location.Id}' which is no longer in GameWorld");
            }
        }
    }

    /// <summary>
    /// Validate required NPC exists in GameWorld
    /// Hierarchical placement: Situation has direct object reference (not string ID)
    /// </summary>
    private void ValidateRequiredNPC(Situation situation, List<string> errors)
    {
        if (situation.Npc != null)
        {
            // Verify the NPC reference is still valid in GameWorld
            if (!_gameWorld.NPCs.Contains(situation.Npc))
            {
                errors.Add($"Situation '{situation.Id}' references NPC '{situation.Npc.ID}' which is no longer in GameWorld");
            }
        }
    }

    /// <summary>
    /// Validate situation placements reference existing entities
    /// ARCHITECTURAL CHANGE: Placement is per-situation (not per-scene)
    /// Checks that each situation has valid placement references
    /// </summary>
    private void ValidatePlacement(Scene scene, List<string> errors)
    {
        // Scene must have at least one situation with placement
        bool hasPlacement = scene.Situations.Any(sit =>
            sit.Location != null || sit.Npc != null || sit.Route != null);

        if (!hasPlacement)
        {
            errors.Add($"Scene '{scene.Id}' has no situations with placement (all situations have null Location/Npc/Route) - player cannot find it");
            return;
        }

        // Validate each situation's placement
        foreach (Situation situation in scene.Situations)
        {
            // Validate Location placement if present
            if (situation.Location != null)
            {
                Location location = _gameWorld.GetLocation(situation.Location.Id);
                if (location == null)
                {
                    errors.Add($"Situation '{situation.Id}' references Location '{situation.Location.Id}' which does not exist in GameWorld");
                }
            }

            // Validate NPC placement if present
            if (situation.Npc != null)
            {
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == situation.Npc.ID);
                if (npc == null)
                {
                    errors.Add($"Situation '{situation.Id}' references NPC '{situation.Npc.ID}' which does not exist in GameWorld");
                }
            }

            // Validate Route placement if present
            if (situation.Route != null)
            {
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == situation.Route.Id);
                if (route == null)
                {
                    errors.Add($"Situation '{situation.Id}' references Route '{situation.Route.Id}' which does not exist in GameWorld");
                }
            }
        }
    }
}
