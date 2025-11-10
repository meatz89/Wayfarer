public static class SceneTemplateValidator
{
public static ValidationResult Validate(SceneTemplate template)
{
    List<ValidationError> errors = new();

    ValidateStructure(template, errors);
    ValidateDependencies(template, errors);
    ValidateAStoryConsistency(template, errors);

    return new ValidationResult(!errors.Any(), errors);
}

private static void ValidateStructure(SceneTemplate template, List<ValidationError> errors)
{
    if (string.IsNullOrEmpty(template.Id))
    {
        errors.Add(new ValidationError("STRUCT_001", "Template.Id is required"));
    }

    if (template.SituationTemplates == null || !template.SituationTemplates.Any())
    {
        errors.Add(new ValidationError("STRUCT_002", "Template must have at least one situation"));
    }

    foreach (var situation in template.SituationTemplates ?? Enumerable.Empty<SituationTemplate>())
    {
        if (string.IsNullOrEmpty(situation.Id))
        {
            errors.Add(new ValidationError("STRUCT_003", "Situation.Id is required"));
        }

        if (situation.ChoiceTemplates != null && situation.ChoiceTemplates.Count > 4)
        {
            errors.Add(new ValidationError("STRUCT_004", $"Situation {situation.Id} has {situation.ChoiceTemplates.Count} choices (max 4)"));
        }
    }
}

private static void ValidateDependencies(SceneTemplate template, List<ValidationError> errors)
{
    if (template.SituationTemplates == null || template.SpawnRules == null)
    {
        return;
    }

    var situationIds = template.SituationTemplates.Select(s => s.Id).ToHashSet();

    foreach (var transition in template.SpawnRules.Transitions ?? Enumerable.Empty<SituationTransition>())
    {
        if (!situationIds.Contains(transition.SourceSituationId))
        {
            errors.Add(new ValidationError("DEP_001", $"Transition references unknown source: {transition.SourceSituationId}"));
        }

        if (!situationIds.Contains(transition.DestinationSituationId))
        {
            errors.Add(new ValidationError("DEP_002", $"Transition references unknown destination: {transition.DestinationSituationId}"));
        }
    }

    if (!string.IsNullOrEmpty(template.SpawnRules.InitialSituationId) &&
        !situationIds.Contains(template.SpawnRules.InitialSituationId))
    {
        errors.Add(new ValidationError("DEP_003", $"InitialSituationId '{template.SpawnRules.InitialSituationId}' not found in situation list"));
    }
}

private static void ValidateAStoryConsistency(SceneTemplate template, List<ValidationError> errors)
{
    if (template.Category != StoryCategory.MainStory)
    {
        return;
    }

    if (!template.MainStorySequence.HasValue)
    {
        errors.Add(new ValidationError("ASTORY_001", $"Template '{template.Id}' has Category=MainStory but no MainStorySequence"));
        return;
    }

    // All A-story templates (authored and procedural) follow same validation rules
    ValidateAStoryTemplate(template, errors);
}

private static void ValidateAStoryTemplate(SceneTemplate template, List<ValidationError> errors)
{
    if (template.SituationTemplates == null || !template.SituationTemplates.Any())
    {
        errors.Add(new ValidationError("ASTORY_002", $"A-story template '{template.Id}' (A{template.MainStorySequence}) has no situations"));
        return;
    }

    foreach (var situation in template.SituationTemplates)
    {
        if (situation.ChoiceTemplates == null || !situation.ChoiceTemplates.Any())
        {
            errors.Add(new ValidationError("ASTORY_003", $"A-story situation '{situation.Id}' in '{template.Id}' has no choices"));
            continue;
        }

        bool hasGuaranteedPath = situation.ChoiceTemplates.Any(IsGuaranteedSuccessChoice);
        if (!hasGuaranteedPath)
        {
            errors.Add(new ValidationError("ASTORY_004",
                $"A-story situation '{situation.Id}' in '{template.Id}' (A{template.MainStorySequence}) lacks guaranteed success path. " +
                $"Every A-story situation must have at least one choice with no requirements OR a challenge choice that spawns scenes on both success and failure."));
        }
    }

    ValidateFinalSituationAdvancement(template, errors);
}

private static bool IsGuaranteedSuccessChoice(ChoiceTemplate choice)
{
    bool hasNoRequirements = choice.RequirementFormula == null ||
                             choice.RequirementFormula.OrPaths == null ||
                             !choice.RequirementFormula.OrPaths.Any();

    if (choice.ActionType == ChoiceActionType.Instant && hasNoRequirements)
    {
        return true;
    }

    if (choice.ActionType == ChoiceActionType.StartChallenge && hasNoRequirements)
    {
        bool successSpawns = choice.RewardTemplate?.ScenesToSpawn?.Any() == true;
        bool failureSpawns = choice.OnFailureReward?.ScenesToSpawn?.Any() == true;
        return successSpawns && failureSpawns;
    }

    return false;
}

private static void ValidateFinalSituationAdvancement(SceneTemplate template, List<ValidationError> errors)
{
    if (template.SituationTemplates == null || !template.SituationTemplates.Any())
    {
        return;
    }

    var situationIds = template.SituationTemplates.Select(s => s.Id).ToHashSet();
    var finalSituations = template.SituationTemplates.Where(s => IsFinalSituation(s, template.SpawnRules)).ToList();

    foreach (var finalSituation in finalSituations)
    {
        if (finalSituation.ChoiceTemplates == null || !finalSituation.ChoiceTemplates.Any())
        {
            continue;
        }

        var spawnedScenes = finalSituation.ChoiceTemplates
            .SelectMany(c => c.RewardTemplate?.ScenesToSpawn ?? new List<SceneSpawnReward>())
            .ToList();

        if (spawnedScenes.Any())
        {
            var uniqueSceneTemplates = spawnedScenes.Select(s => s.SceneTemplateId).Distinct().ToList();
            if (uniqueSceneTemplates.Count > 1)
            {
                errors.Add(new ValidationError("ASTORY_008",
                    $"Final A-story situation '{finalSituation.Id}' in '{template.Id}' spawns different scenes across choices. " +
                    $"All choices in final situation must spawn the SAME next A-scene (for guaranteed progression)."));
            }
        }
    }
}

private static bool IsFinalSituation(SituationTemplate situation, SituationSpawnRules rules)
{
    if (rules?.Transitions == null || !rules.Transitions.Any())
    {
        return true;
    }

    return !rules.Transitions.Any(t => t.SourceSituationId == situation.Id);
}

public static ValidationResult ValidateAStoryChain(List<SceneTemplate> allTemplates)
{
    List<ValidationError> errors = new();

    var aStoryTemplates = allTemplates
        .Where(t => t.Category == StoryCategory.MainStory && t.MainStorySequence.HasValue)
        .OrderBy(t => t.MainStorySequence.Value)
        .ToList();

    if (!aStoryTemplates.Any())
    {
        return new ValidationResult(true, errors);
    }

    int minSequence = aStoryTemplates.Min(t => t.MainStorySequence!.Value);
    int maxSequence = aStoryTemplates.Max(t => t.MainStorySequence!.Value);

    if (minSequence != 1)
    {
        errors.Add(new ValidationError("ACHAIN_003", $"Authored A-story must start at sequence 1, but starts at A{minSequence}."));
    }

    for (int expectedSeq = minSequence; expectedSeq <= maxSequence; expectedSeq++)
    {
        var template = aStoryTemplates.FirstOrDefault(t => t.MainStorySequence == expectedSeq);
        if (template == null)
        {
            errors.Add(new ValidationError("ACHAIN_001", $"Missing A-story sequence A{expectedSeq}. Authored A-story must have complete sequence with no gaps (found A{minSequence}-A{maxSequence})."));
        }
    }

    var duplicateSequences = aStoryTemplates
        .GroupBy(t => t.MainStorySequence)
        .Where(g => g.Count() > 1)
        .ToList();

    foreach (var group in duplicateSequences)
    {
        var templateIds = string.Join(", ", group.Select(t => t.Id));
        errors.Add(new ValidationError("ACHAIN_002", $"Duplicate A-story sequence A{group.Key} in templates: {templateIds}"));
    }

    return new ValidationResult(!errors.Any(), errors);
}
}

public record ValidationResult(bool IsValid, List<ValidationError> Errors);
public record ValidationError(string Code, string Message);
