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
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            projection.FocusPointsGained = 1; 
            projection.ProgressGained = 0;
            projection.AspectTokensGained.Clear();
        }
        else
        {
            if (choice.RequiresTokens() && !projection.IsAffordableAspectTokens)
            {
                if (choice.ActionType == UniversalActionTypes.ConversionA || choice.ActionType == UniversalActionTypes.ConversionB)
                {
                    projection.ProgressGained = 0;
                }
            }
        }

        // --- 3. Project Skill Check for Negative Consequence Mitigation ---
        if (projection.HasSkillCheck)
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
        int totalStages = encounterState.EncounterInfo.Stages.Count;
        bool isLastStage = (encounterState.CurrentStageIndex == totalStages - 1); 
        int projectedProgressAfterThisChoice = encounterState.CurrentProgress + (projection.IsDisabled ? 0 : projection.ProgressGained);

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
                ? EncounterOutcomes.Success 
                : EncounterOutcomes.Failure; 
        }
        else
        {
            projection.ProjectedOutcome = EncounterOutcomes.None;
        }

        // --- 6. Generate Comprehensive Formatted Outcome Summary Text ---
        projection.FormattedOutcomeSummary = GenerateFormattedOutcomeSummary(projection, playerState, choice);

        return projection;
    }

    private static string GetProjectedNegativeEffectText(
        EncounterOption choice,
        bool isSkillCheckProjectedToSucceed,
        EncounterState encounterState)
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

        string penaltyPrefix = "RISK (if skill check fails or N/A): ";
        switch (actualNegativeType)
        {
            case NegativeConsequenceTypes.ProgressLoss:
                return penaltyPrefix + "Lose 1 Progress Marker.";
            case NegativeConsequenceTypes.FocusLoss:
                return penaltyPrefix + "Lose 1 Focus Point from your encounter pool.";
            case NegativeConsequenceTypes.TokenDisruption:
                return penaltyPrefix + "Discard 1 random Aspect Token from your Pool.";
            case NegativeConsequenceTypes.FutureCostIncrease:
                return penaltyPrefix + "Your next choice this encounter will cost +1 Focus Point.";
            case NegativeConsequenceTypes.ThresholdIncrease:
                return penaltyPrefix + "Final success thresholds for this encounter will increase by 1.";
            default:
                return penaltyPrefix + "An unforeseen setback occurs.";
        }
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

    private static bool WouldRecoveryNegativeIncreaseThreshold(NegativeConsequenceTypes originalNegTypeForRecovery, EncounterState encounterState)
    {
        return DetermineActualRecoveryNegative(encounterState) == NegativeConsequenceTypes.ThresholdIncrease;
    }


    private static string GenerateFormattedOutcomeSummary(ChoiceProjection projection, PlayerState playerState, EncounterOption choice)
    {
        StringBuilder summary = new StringBuilder();

        // Cost
        summary.Append($"Cost: {projection.FocusCost} Focus. ");
        if (!projection.IsAffordableFocus) summary.Append("(CANNOT AFFORD) ");

        if (choice.RequiresTokens())
        {
            summary.Append("Requires: ");
            List<string> costStrings = new List<string>();
            foreach (var cost in projection.AspectTokenCosts)
            {
                costStrings.Add($"{cost.Value} {cost.Key}");
            }
            summary.Append(string.Join(", ", costStrings) + ". ");
            if (!projection.IsAffordableAspectTokens) summary.Append("(INSUFFICIENT TOKENS) ");
        }
        summary.AppendLine();

        // Positive Effects
        summary.Append("Effect: ");
        List<string> positiveEffects = new List<string>();
        if (projection.FocusPointsGained > 0) positiveEffects.Add($"Gain {projection.FocusPointsGained} Focus Point");

        foreach (var gain in projection.AspectTokensGained)
        {
            if (gain.Value > 0) positiveEffects.Add($"Gain {gain.Value} {gain.Key}");
        }
        if (projection.ProgressGained > 0)
        {
            if (choice.RequiresTokens() && !projection.IsAffordableAspectTokens)
            {
                if (choice.ActionType == UniversalActionTypes.ConversionA || choice.ActionType == UniversalActionTypes.ConversionB)
                    positiveEffects.Add($"Gain 0 Progress (cannot afford tokens)");
                else 
                    positiveEffects.Add($"Gain {projection.ProgressGained} Progress"); 
            }
            else
            {
                positiveEffects.Add($"Gain {projection.ProgressGained} Progress");
            }
        }
        if (!positiveEffects.Any()) positiveEffects.Add("No direct change in resources or progress.");
        summary.Append(string.Join(", ", positiveEffects).TrimEnd(',', ' '));
        summary.AppendLine(".");

        // Skill Check & Negative
        if (projection.HasSkillCheck)
        {
            summary.Append($"Skill Check: {choice.Skill} ({projection.EffectiveSkillLevel}) vs Difficulty {choice.Difficulty}. ");
            summary.Append(projection.SkillCheckSuccess ? "Likely SUCCESS. " : "Likely FAILURE. ");
        }

        // Final Outcome if encounter ends
        if (projection.WillEncounterEnd)
        {
            summary.AppendLine($"Encounter will end. Projected Outcome: {projection.ProjectedOutcome}.");
        }

        return summary.ToString().Trim();
    }

    // --- Helper methods from user's original code (ensure they are present) ---
    public static int GetLocationPropertyModifier(SkillTypes skill, Location location)
    {
        int modifier = 0;
        if (IsIntellectualSkill(skill))
        {
            if (location.Population == Population.Crowded) modifier -= 1;
            if (location.Illumination == Illumination.Dark) modifier -= 1;
            if (location.Population == Population.Scholarly) modifier += 1;
        }
        else if (IsSocialSkill(skill))
        {
            if (location.Population == Population.Quiet) modifier -= 1;
            if (location.Atmosphere == Atmosphere.Tense) modifier -= 1;
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

    public static bool IsPhysicalSkill(SkillTypes skill) => skill == SkillTypes.Strength || skill == SkillTypes.Endurance || skill == SkillTypes.Precision || skill == SkillTypes.Agility;
    public static bool IsIntellectualSkill(SkillTypes skill) => skill == SkillTypes.Analysis || skill == SkillTypes.Observation || skill == SkillTypes.Knowledge || skill == SkillTypes.Planning;
    public static bool IsSocialSkill(SkillTypes skill) => skill == SkillTypes.Charm || skill == SkillTypes.Persuasion || skill == SkillTypes.Deception || skill == SkillTypes.Intimidation;

}
