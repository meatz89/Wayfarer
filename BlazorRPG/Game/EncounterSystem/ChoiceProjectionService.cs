using System.Text;

public static class ChoiceProjectionService
{
    public static ChoiceProjection CreateUniversalChoiceProjection(
        EncounterOption choice,
        EncounterState encounterState,
        Player playerState,
        Location location)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // --- 1. Determine Affordability (Focus Points & Aspect Tokens) ---
        projection.IsAffordableFocus = encounterState.FocusPoints >= choice.FocusCost;

        projection.IsAffordableAspectTokens = true;

        // --- 2. Project Positive Effects ---
        ProjectPositiveEffects(projection, choice, encounterState);

        // --- 3. Project Skill Check for Negative Consequence Mitigation ---
        if (choice.HasSkillCheck)
        {
            projection.BaseSkillLevel = playerState.GetSkillLevel(choice.Skill);
            projection.LocationModifierValue = GetLocationPropertyModifier(choice.Skill, location);
            projection.EffectiveSkillLevel = projection.BaseSkillLevel + projection.LocationModifierValue;
            projection.SkillCheckSuccess = projection.EffectiveSkillLevel >= choice.Difficulty;
        }
        else
        {
            projection.SkillCheckSuccess = (choice.NegativeConsequenceType == NegativeConsequenceTypes.None);
        }

        // --- 4. Describe the Projected Mechanical Negative Effect ---
        projection.MechanicalDescription = GetProjectedNegativeEffectText(
            choice,
            projection.SkillCheckSuccess,
            encounterState
        );

        // --- 5. Project Encounter End & Outcome ---
        ProjectEncounterOutcome(projection, choice, encounterState);

        // --- 6. Generate Comprehensive Formatted Outcome Summary Text ---
        projection.FormattedOutcomeSummary = GenerateFormattedOutcomeSummary(projection, playerState, choice, encounterState);

        return projection;
    }

    private static void ProjectPositiveEffects(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            projection.FocusPointsGained = 1;
        }
    }

    private static void ProjectEncounterOutcome(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        int totalStages = encounterState.MaxDuration;
        bool isLastStage = (encounterState.CurrentStageIndex == totalStages - 1);
        int projectedProgressAfterThisChoice = encounterState.CurrentProgress + projection.ProgressGained;

        projection.WillEncounterEnd = isLastStage;
        if (isLastStage)
        {
            int successThreshold = 10;
            int progressNeededForSuccess = successThreshold;
            projection.ProjectedOutcome =
                projectedProgressAfterThisChoice >= progressNeededForSuccess
                ? EncounterOutcomes.BasicSuccess
                : EncounterOutcomes.Failure;
        }
        else
        {
            projection.ProjectedOutcome = EncounterOutcomes.None;
        }
    }

    private static string GetProjectedNegativeEffectText(EncounterOption choice, bool isSkillCheckProjectedToSucceed, EncounterState encounterState)
    {
        if (isSkillCheckProjectedToSucceed)
        {
            if (choice.NegativeConsequenceType == NegativeConsequenceTypes.None && !choice.HasSkillCheck)
                return "This action has no inherent negative consequence.";
            else
                return "Projected: Negative consequence will be AVOIDED.";
        }

        NegativeConsequenceTypes actualNegativeType = choice.NegativeConsequenceType;

        if (actualNegativeType == NegativeConsequenceTypes.None)
        {
            return "This action has no inherent negative consequence.";
        }

        string penaltyPrefix = "RISK (if skill check fails): ";
        return actualNegativeType switch
        {
            NegativeConsequenceTypes.ProgressLoss => penaltyPrefix + "Lose 1 Progress Marker.",
            NegativeConsequenceTypes.FocusLoss => penaltyPrefix + "Lose 1 Focus Point from your encounter pool.",
            NegativeConsequenceTypes.ThresholdIncrease => penaltyPrefix + "Final success thresholds for this encounter will increase by 1.",
            _ => penaltyPrefix + "An unforeseen setback occurs."
        };
    }

    private static string GenerateFormattedOutcomeSummary(ChoiceProjection projection, Player playerState, EncounterOption choice, EncounterState encounterState)
    {
        StringBuilder summary = new StringBuilder();

        // Cost
        summary.Append($"Cost: {choice.FocusCost} Focus");
        if (!projection.IsAffordableFocus) summary.Append(" (CANNOT AFFORD)");

        // Positive Effects
        summary.Append("Effect: ");
        List<string> positiveEffects = new List<string>();

        if (projection.FocusPointsGained > 0)
            positiveEffects.Add($"+{projection.FocusPointsGained} Focus");

        if (projection.ProgressGained > 0)
            positiveEffects.Add($"+{projection.ProgressGained} Progress");

        if (!positiveEffects.Any())
            positiveEffects.Add("No direct resource change");

        summary.AppendLine(string.Join(", ", positiveEffects));

        // Skill Check & Risk
        if (projection.HasSkillCheck)
        {
            summary.AppendLine($"Skill Check: {choice.Skill} ({projection.EffectiveSkillLevel}) vs {choice.Difficulty} - {(projection.SkillCheckSuccess ? "SUCCESS" : "FAILURE")} projected");
        }

        summary.AppendLine(projection.MechanicalDescription);

        // Final Outcome
        if (projection.WillEncounterEnd)
        {
            summary.AppendLine($"Encounter End: {projection.ProjectedOutcome}");
        }

        return summary.ToString().Trim();
    }

    public static int GetLocationPropertyModifier(SkillTypes skill, Location location)
    {
        int modifier = 0;

        if (IsIntellectualSkill(skill))
        {
            if (location.Population == Population.Crowded) modifier -= 1;
            if (location.Illumination == Illumination.Dark) modifier -= 1;
            if (location.Population == Population.Quiet) modifier += 1;
        }
        else if (IsSocialSkill(skill))
        {
            if (location.Population == Population.Quiet) modifier -= 1;
            if (location.Atmosphere == Atmosphere.Formal) modifier += 1;
            if (location.Population == Population.Crowded) modifier += 1;
        }
        else if (IsPhysicalSkill(skill))
        {
            if (location.Physical == Physical.Confined) modifier -= 1;
            if (location.Illumination == Illumination.Dark) modifier -= 1;
            if (location.Physical == Physical.Expansive) modifier += 1;
        }

        return modifier;
    }

    public static bool IsPhysicalSkill(SkillTypes skill) =>
        skill == SkillTypes.Strength || skill == SkillTypes.Endurance ||
        skill == SkillTypes.Precision || skill == SkillTypes.Agility;

    public static bool IsIntellectualSkill(SkillTypes skill) =>
        skill == SkillTypes.Analysis || skill == SkillTypes.Observation ||
        skill == SkillTypes.Knowledge || skill == SkillTypes.Planning;

    public static bool IsSocialSkill(SkillTypes skill) =>
        skill == SkillTypes.Charm || skill == SkillTypes.Persuasion ||
        skill == SkillTypes.Deception || skill == SkillTypes.Intimidation;
}
