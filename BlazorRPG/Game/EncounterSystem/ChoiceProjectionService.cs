using System.Text;

public static class ChoiceProjectionService
{
    public static ChoiceProjection CreateUniversalChoiceProjection(
        EncounterOption choice,
        EncounterState encounterState,
        PlayerState playerState,
        Location location)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // --- 1. Determine Affordability (Focus Points & Aspect Tokens) ---
        projection.IsAffordableFocus = encounterState.FocusPoints >= choice.FocusCost;

        projection.IsAffordableAspectTokens = true;
        if (choice.RequiresTokens())
        {
            foreach (var costEntry in choice.TokenCosts)
            {
                if (encounterState.AspectTokens.GetTokenCount(costEntry.Key) < costEntry.Value)
                {
                    projection.IsAffordableAspectTokens = false;
                    break;
                }
            }
        }

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
            projection.ProgressGained = 0;
            projection.AspectTokensGained.Clear();
        }
        else
        {
            // Project token generation
            if (choice.TokenGeneration != null)
            {
                foreach (var tokenGen in choice.TokenGeneration)
                {
                    projection.AspectTokensGained[tokenGen.Key] = tokenGen.Value;
                }
            }

            // Project progress generation
            if (choice.RequiresTokens() && !projection.IsAffordableAspectTokens)
            {
                projection.ProgressGained = 0; // Can't convert without tokens
            }
            else
            {
                projection.ProgressGained = choice.SuccessProgress;
            }

            projection.FocusPointsGained = 0;
        }
    }

    private static void ProjectEncounterOutcome(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        int totalStages = encounterState.EncounterInfo.Stages.Count;
        bool isLastStage = (encounterState.CurrentStageIndex == totalStages - 1);
        int projectedProgressAfterThisChoice = encounterState.CurrentProgress + projection.ProgressGained;

        int effectiveOutcomeThresholdModifier = encounterState.OutcomeThresholdModifier;
        if (choice.ActionType == UniversalActionTypes.Recovery &&
            choice.FocusCost == 0 &&
            !projection.SkillCheckSuccess)
        {
            NegativeConsequenceTypes recoveryNegative = DetermineActualRecoveryNegative(encounterState);
            if (recoveryNegative == NegativeConsequenceTypes.ThresholdIncrease)
            {
                effectiveOutcomeThresholdModifier++;
            }
        }

        projection.WillEncounterEnd = isLastStage;
        if (isLastStage)
        {
            int successThreshold = encounterState.EncounterInfo.SuccessThreshold;
            int progressNeededForSuccess = successThreshold + effectiveOutcomeThresholdModifier;
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
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            actualNegativeType = DetermineActualRecoveryNegative(encounterState);
        }

        if (actualNegativeType == NegativeConsequenceTypes.None)
        {
            return "This action has no inherent negative consequence.";
        }

        string penaltyPrefix = "RISK (if skill check fails): ";
        return actualNegativeType switch
        {
            NegativeConsequenceTypes.ProgressLoss => penaltyPrefix + "Lose 1 Progress Marker.",
            NegativeConsequenceTypes.FocusLoss => penaltyPrefix + "Lose 1 Focus Point from your encounter pool.",
            NegativeConsequenceTypes.TokenDisruption => choice.ActionType == UniversalActionTypes.Recovery
                ? penaltyPrefix + GetRecoveryTokenDisruptionText(encounterState)
                : penaltyPrefix + "This generation produces 1 fewer token.",
            NegativeConsequenceTypes.ThresholdIncrease => penaltyPrefix + "Final success thresholds for this encounter will increase by 1.",
            _ => penaltyPrefix + "An unforeseen setback occurs."
        };
    }

    private static string GetRecoveryTokenDisruptionText(EncounterState encounterState)
    {
        int totalTokens = encounterState.AspectTokens.GetAllTokenCounts().Values.Sum();
        return totalTokens >= 2 ? "Discard 2 random Aspect Tokens from your pool." : "Discard 1 random Aspect Token from your pool.";
    }

    private static NegativeConsequenceTypes DetermineActualRecoveryNegative(EncounterState encounterState)
    {
        if (encounterState.CurrentProgress > 0)
            return NegativeConsequenceTypes.ProgressLoss;
        else if (encounterState.AspectTokens.GetAllTokenCounts().Values.Sum() >= 2)
            return NegativeConsequenceTypes.TokenDisruption;
        else
            return NegativeConsequenceTypes.ThresholdIncrease;
    }

    private static string GenerateFormattedOutcomeSummary(ChoiceProjection projection, PlayerState playerState, EncounterOption choice, EncounterState encounterState)
    {
        StringBuilder summary = new StringBuilder();

        // Cost
        summary.Append($"Cost: {choice.FocusCost} Focus");
        if (!projection.IsAffordableFocus) summary.Append(" (CANNOT AFFORD)");

        if (choice.RequiresTokens())
        {
            summary.Append(" + ");
            List<string> costStrings = new List<string>();
            foreach (var cost in choice.TokenCosts)
            {
                costStrings.Add($"{cost.Value} {cost.Key}");
            }
            summary.Append(string.Join(", ", costStrings));
            if (!projection.IsAffordableAspectTokens) summary.Append(" (INSUFFICIENT TOKENS)");
        }
        summary.AppendLine();

        // Positive Effects
        summary.Append("Effect: ");
        List<string> positiveEffects = new List<string>();

        if (projection.FocusPointsGained > 0)
            positiveEffects.Add($"+{projection.FocusPointsGained} Focus");

        foreach (var gain in projection.AspectTokensGained)
        {
            if (gain.Value > 0)
                positiveEffects.Add($"+{gain.Value} {gain.Key}");
        }

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
