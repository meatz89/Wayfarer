using Wayfarer.GameState;
public static class SceneTemplateValidator
{
    public static ValidationResult Validate(SceneTemplate template)
    {
        List<ValidationError> errors = new();

        ValidateStructure(template, errors);
        ValidateDependencies(template, errors);

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
}

public record ValidationResult(bool IsValid, List<ValidationError> Errors);
public record ValidationError(string Code, string Message);
