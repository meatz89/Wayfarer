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

    // RULE 7: Marker resolution map must resolve all required markers
    ValidateMarkerResolution(scene, errors);

    if (errors.Any())
    {
        string errorSummary = string.Join("\n", errors);
        throw new InvalidOperationException(
            $"Scene '{scene.Id}' failed playability validation:\n{errorSummary}\n\n" +
            $"Scene Details:\n" +
            $"- TemplateId: {scene.TemplateId}\n" +
            $"- Category: {scene.Category}\n" +
            $"- MainStorySequence: {scene.MainStorySequence}\n" +
            $"- PlacementType: {scene.PlacementType}\n" +
            $"- PlacementId: {scene.PlacementId}\n" +
            $"- State: {scene.State}\n" +
            $"- CurrentSituationId: {scene.CurrentSituationId}\n" +
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
/// </summary>
private void ValidateRequiredLocation(Situation situation, List<string> errors)
{
    string requiredLocationId = situation.ResolvedRequiredLocationId ?? situation.Template?.RequiredLocationId;

    if (!string.IsNullOrEmpty(requiredLocationId))
    {
        Location location = _gameWorld.GetLocation(requiredLocationId);
        if (location == null)
        {
            errors.Add($"Situation '{situation.Id}' requires location '{requiredLocationId}' which does not exist in GameWorld");
        }
    }
}

/// <summary>
/// Validate required NPC exists in GameWorld
/// </summary>
private void ValidateRequiredNPC(Situation situation, List<string> errors)
{
    string requiredNpcId = situation.ResolvedRequiredNpcId ?? situation.Template?.RequiredNpcId;

    if (!string.IsNullOrEmpty(requiredNpcId))
    {
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == requiredNpcId);
        if (npc == null)
        {
            errors.Add($"Situation '{situation.Id}' requires NPC '{requiredNpcId}' which does not exist in GameWorld");
        }
    }
}

/// <summary>
/// Validate scene placement references existing entity
/// </summary>
private void ValidatePlacement(Scene scene, List<string> errors)
{
    if (string.IsNullOrEmpty(scene.PlacementId))
    {
        errors.Add($"Scene '{scene.Id}' has no PlacementId - player cannot find it");
        return;
    }

    bool placementExists = scene.PlacementType switch
    {
        PlacementType.Location => _gameWorld.GetLocation(scene.PlacementId) != null,
        PlacementType.NPC => _gameWorld.NPCs.Any(n => n.ID == scene.PlacementId),
        PlacementType.Route => _gameWorld.Routes.Any(r => r.Id == scene.PlacementId),
        _ => false
    };

    if (!placementExists)
    {
        errors.Add($"Scene '{scene.Id}' placed at {scene.PlacementType} '{scene.PlacementId}' which does not exist in GameWorld");
    }
}

/// <summary>
/// Validate marker resolution map resolves all "generated:" markers in situations
/// </summary>
private void ValidateMarkerResolution(Scene scene, List<string> errors)
{
    if (scene.MarkerResolutionMap == null || scene.MarkerResolutionMap.Count == 0)
    {
        // No markers to resolve - valid if scene has no dependent resources
        if ((scene.CreatedLocationIds?.Any() == true) || (scene.CreatedItemIds?.Any() == true))
        {
            errors.Add($"Scene '{scene.Id}' created dependent resources but has no MarkerResolutionMap");
        }
        return;
    }

    // Check all situations for unresolved markers
    foreach (Situation situation in scene.Situations)
    {
        // Check RequiredLocationId
        string locationId = situation.Template?.RequiredLocationId;
        if (!string.IsNullOrEmpty(locationId) && locationId.StartsWith("generated:"))
        {
            if (!scene.MarkerResolutionMap.ContainsKey(locationId))
            {
                errors.Add($"Situation '{situation.Id}' references marker '{locationId}' which is not in MarkerResolutionMap");
            }
        }

        // Check RequiredNpcId
        string npcId = situation.Template?.RequiredNpcId;
        if (!string.IsNullOrEmpty(npcId) && npcId.StartsWith("generated:"))
        {
            if (!scene.MarkerResolutionMap.ContainsKey(npcId))
            {
                errors.Add($"Situation '{situation.Id}' references marker '{npcId}' which is not in MarkerResolutionMap");
            }
        }

        // Check choice rewards for markers
        if (situation.Template?.ChoiceTemplates != null)
        {
            foreach (ChoiceTemplate choice in situation.Template.ChoiceTemplates)
            {
                ValidateRewardMarkers(choice.RewardTemplate, situation.Id, choice.Id, scene.MarkerResolutionMap, errors);
                ValidateRewardMarkers(choice.OnFailureReward, situation.Id, choice.Id, scene.MarkerResolutionMap, errors);
            }
        }
    }
}

/// <summary>
/// Validate reward template markers are in resolution map
/// </summary>
private void ValidateRewardMarkers(RewardTemplate reward, string situationId, string choiceId, Dictionary<string, string> markerMap, List<string> errors)
{
    if (reward == null)
        return;

    // Check LocationsToUnlock
    if (reward.LocationsToUnlock != null)
    {
        foreach (string locationId in reward.LocationsToUnlock)
        {
            if (!string.IsNullOrEmpty(locationId) && locationId.StartsWith("generated:"))
            {
                if (!markerMap.ContainsKey(locationId))
                {
                    errors.Add($"Situation '{situationId}' choice '{choiceId}' reward references marker '{locationId}' which is not in MarkerResolutionMap");
                }
            }
        }
    }

    // Check ItemsToGrant
    if (reward.ItemsToGrant != null)
    {
        foreach (string itemId in reward.ItemsToGrant)
        {
            if (!string.IsNullOrEmpty(itemId) && itemId.StartsWith("generated:"))
            {
                if (!markerMap.ContainsKey(itemId))
                {
                    errors.Add($"Situation '{situationId}' choice '{choiceId}' reward references marker '{itemId}' which is not in MarkerResolutionMap");
                }
            }
        }
    }
}
}
