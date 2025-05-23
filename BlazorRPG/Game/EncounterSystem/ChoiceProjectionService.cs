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

        // --- 2. Project Positive Effects Based on Tier and Template ---
        ProjectPositiveEffects(projection, choice, encounterState);

        // --- 3. Project Skill Check for Negative Consequence Mitigation ---
        if (choice.Skill != SkillTypes.None && choice.Difficulty > 0)
        {
            projection.HasSkillCheck = true;
            projection.BaseSkillLevel = playerState.GetSkillLevel(choice.Skill);
            projection.LocationModifierValue = GetLocationPropertyModifier(choice.Skill, location);
            projection.EffectiveSkillLevel = projection.BaseSkillLevel + projection.LocationModifierValue;
            projection.SkillCheckSuccess = projection.EffectiveSkillLevel >= choice.Difficulty;
        }
        else
        {
            projection.HasSkillCheck = false;
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
        // Get current stage tier (1-2 = Foundation, 3-4 = Development, 5 = Execution)
        int currentStage = encounterState.CurrentStageIndex + 1;
        EncounterTiers tier = GetEncounterTier(currentStage);

        switch (choice.ActionType)
        {
            case UniversalActionTypes.GenerationA:
                projection.AspectTokensGained[choice.PrimaryAspectType] = GetGenerationATokens(tier);
                break;

            case UniversalActionTypes.GenerationB:
                projection.AspectTokensGained[choice.PrimaryAspectType] = GetGenerationBTokens(tier);
                break;

            case UniversalActionTypes.ConversionA:
                if (projection.IsAffordableAspectTokens)
                    projection.ProgressGained = GetConversionAProgress(tier);
                break;

            case UniversalActionTypes.ConversionB:
                if (projection.IsAffordableAspectTokens)
                    projection.ProgressGained = GetConversionBProgress(tier);
                break;

            case UniversalActionTypes.Hybrid:
                projection.AspectTokensGained[choice.PrimaryAspectType] = GetHybridTokens(tier);
                projection.ProgressGained = GetHybridProgress(tier);
                break;

            case UniversalActionTypes.Recovery:
                projection.FocusPointsGained = 1; // Always +1 Focus, capped at starting maximum
                break;
        }

        // Apply immediate negative effects for failed skill checks
        if (!projection.SkillCheckSuccess)
        {
            ApplyImmediateNegativeEffects(projection, choice, encounterState);
        }
    }

    private static void ApplyImmediateNegativeEffects(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        NegativeConsequenceTypes effectiveNegative = choice.NegativeConsequenceType;

        // Handle Recovery cascading negatives
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            effectiveNegative = DetermineActualRecoveryNegative(encounterState);
        }

        switch (effectiveNegative)
        {
            case NegativeConsequenceTypes.GenerationReduction:
                // "This generation produces 1 fewer token" - reduce token gain
                if (choice.ActionType == UniversalActionTypes.GenerationA || choice.ActionType == UniversalActionTypes.GenerationB)
                {
                    var tokenType = choice.PrimaryAspectType;
                    if (projection.AspectTokensGained.ContainsKey(tokenType))
                    {
                        projection.AspectTokensGained[tokenType] = Math.Max(0, projection.AspectTokensGained[tokenType] - 1);
                    }
                }
                break;

            case NegativeConsequenceTypes.ConversionReduction:
                // "This conversion yields 1 less Progress" - reduce progress gain
                if (choice.ActionType == UniversalActionTypes.ConversionA || choice.ActionType == UniversalActionTypes.ConversionB)
                {
                    projection.ProgressGained = Math.Max(0, projection.ProgressGained - 1);
                }
                break;

            // Other negatives are handled in encounter resolution, not projection
            case NegativeConsequenceTypes.ProgressLoss:
            case NegativeConsequenceTypes.FocusLoss:
            case NegativeConsequenceTypes.ThresholdIncrease:
                // These are projected but not applied to the projection itself
                break;
        }
    }

    private static EncounterTiers GetEncounterTier(int stageNumber)
    {
        return stageNumber switch
        {
            1 or 2 => EncounterTiers.Foundation,
            3 or 4 => EncounterTiers.Development,
            5 => EncounterTiers.Execution,
            _ => EncounterTiers.Foundation
        };
    }

    // Tier-based value tables matching our design document
    private static int GetGenerationATokens(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 3,
        EncounterTiers.Development => 4,
        EncounterTiers.Execution => 4,
        _ => 3
    };

    private static int GetGenerationBTokens(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 2,
        EncounterTiers.Development => 6,
        EncounterTiers.Execution => 6,
        _ => 2
    };

    private static int GetConversionAProgress(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 2,
        EncounterTiers.Development => 4,
        EncounterTiers.Execution => 5,
        _ => 2
    };

    private static int GetConversionBProgress(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 4,
        EncounterTiers.Development => 6,
        EncounterTiers.Execution => 7,
        _ => 4
    };

    private static int GetHybridTokens(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 1,
        EncounterTiers.Development => 3,
        EncounterTiers.Execution => 1,
        _ => 1
    };

    private static int GetHybridProgress(EncounterTiers tier) => tier switch
    {
        EncounterTiers.Foundation => 1,
        EncounterTiers.Development => 2,
        EncounterTiers.Execution => 3,
        _ => 1
    };

    private static void ProjectEncounterOutcome(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        int totalStages = 5; // All encounters have exactly 5 stages
        bool isLastStage = (encounterState.CurrentStageIndex == totalStages - 1);
        projection.WillEncounterEnd = isLastStage;

        if (isLastStage)
        {
            int projectedProgress = encounterState.CurrentProgress + (projection.IsDisabled ? 0 : projection.ProgressGained);

            // Account for threshold increases
            int effectiveThresholdModifier = encounterState.OutcomeThresholdModifier;
            if (!projection.SkillCheckSuccess && choice.NegativeConsequenceType == NegativeConsequenceTypes.ThresholdIncrease)
            {
                effectiveThresholdModifier++;
            }

            // Standard thresholds: Basic (10), Good (14), Excellent (18)
            int basicThreshold = 10 + effectiveThresholdModifier;
            int goodThreshold = 14 + effectiveThresholdModifier;
            int excellentThreshold = 18 + effectiveThresholdModifier;

            if (projectedProgress >= excellentThreshold)
                projection.ProjectedOutcome = EncounterOutcomes.ExcellentSuccess;
            else if (projectedProgress >= goodThreshold)
                projection.ProjectedOutcome = EncounterOutcomes.GoodSuccess;
            else if (projectedProgress >= basicThreshold)
                projection.ProjectedOutcome = EncounterOutcomes.BasicSuccess;
            else
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
        }
        else
        {
            projection.ProjectedOutcome = EncounterOutcomes.None;
        }
    }

    private static string GetProjectedNegativeEffectText(
        EncounterOption choice,
        bool isSkillCheckProjectedToSucceed,
        EncounterState encounterState)
    {
        if (isSkillCheckProjectedToSucceed)
        {
            if (choice.NegativeConsequenceType == NegativeConsequenceTypes.None)
                return "This action has no negative consequence.";
            else
                return "Projected: Negative consequence will be AVOIDED.";
        }

        NegativeConsequenceTypes actualNegativeType = choice.NegativeConsequenceType;
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            actualNegativeType = DetermineActualRecoveryNegative(encounterState);
        }

        string riskPrefix = "RISK (if skill check fails): ";
        return actualNegativeType switch
        {
            NegativeConsequenceTypes.ProgressLoss => riskPrefix + "Lose 1 Progress Marker.",
            NegativeConsequenceTypes.FocusLoss => riskPrefix + "Lose 1 Focus Point.",
            NegativeConsequenceTypes.GenerationReduction => riskPrefix + "This generation produces 1 fewer token.",
            NegativeConsequenceTypes.ConversionReduction => riskPrefix + "This conversion yields 1 less Progress.",
            NegativeConsequenceTypes.ThresholdIncrease => riskPrefix + "Success thresholds increase by 1.",
            NegativeConsequenceTypes.None => "This action has no negative consequence.",
            _ => riskPrefix + "An unforeseen setback occurs."
        };
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
