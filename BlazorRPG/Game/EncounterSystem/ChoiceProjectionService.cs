public static class ChoiceProjectionService
{
    public static ChoiceProjection CreateUniversalChoiceProjection(
        EncounterOption choice,
        int currentProgress,
        int progressThreshold,
        int currentTurn,
        PlayerState playerState,
        Location location,
        EncounterState encounterState)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Set Focus cost
        projection.FocusCost = choice.FocusCost;

        // Apply positive effects (always happen)
        ApplyPositiveEffects(projection, choice);

        // Determine skill check success for negative consequence mitigation
        if (choice.Skill != SkillTypes.None)
        {
            projection.SkillCheckSuccess = DetermineSkillCheckSuccess(choice, playerState, location);
        }
        else
        {
            projection.SkillCheckSuccess = true; // No skill check means no negative consequence
        }

        // Set negative consequence type
        projection.NegativeConsequenceType = choice.NegativeConsequenceType;

        // Calculate total progress gained (from tokens converted + direct progress)
        CalculateProgressGained(projection, choice, encounterState);

        // Determine if encounter will end
        bool willComplete = (currentProgress + projection.ProgressGained) >= progressThreshold;
        projection.EncounterWillEnd = willComplete;

        // Set projected outcome
        if (willComplete)
        {
            projection.ProjectedOutcome = DetermineCompletionOutcome(currentProgress + projection.ProgressGained, progressThreshold);
        }

        // Set description
        SetProjectionDescription(projection, choice, playerState);

        return projection;
    }

    private static void ApplyPositiveEffects(ChoiceProjection projection, EncounterOption choice)
    {
        // Token generation effects
        foreach (KeyValuePair<AspectTokenTypes, int> tokenGen in choice.TokenGeneration)
        {
            projection.SetTokenGain(tokenGen.Key, tokenGen.Value);
        }

        // Token conversion effects (if this is a conversion choice)
        if (choice.RequiresTokens())
        {
            projection.IsConversionChoice = true;
            foreach (KeyValuePair<AspectTokenTypes, int> tokenCost in choice.TokenCosts)
            {
                projection.SetTokenCost(tokenCost.Key, tokenCost.Value);
            }
        }
    }

    private static bool DetermineSkillCheckSuccess(EncounterOption choice, PlayerState playerState, Location location)
    {
        int skillLevel = playerState.GetSkillLevel(choice.Skill);
        int locationModifier = ChoiceProjectionService.GetLocationPropertyModifier(choice.Skill, location);
        int effectiveSkill = skillLevel + locationModifier;

        return effectiveSkill >= choice.Difficulty;
    }

    private static void CalculateProgressGained(ChoiceProjection projection, EncounterOption choice, EncounterState encounterState)
    {
        int totalProgress = 0;

        // Direct progress from the choice
        totalProgress += choice.SuccessProgress;

        // Progress from token conversion (if applicable)
        if (choice.RequiresTokens())
        {
            totalProgress += CalculateConversionProgress(choice, encounterState);
        }

        projection.ProgressGained = totalProgress;
    }

    private static int CalculateConversionProgress(EncounterOption choice, EncounterState encounterState)
    {
        // Calculate progress based on conversion ratios from the universal templates
        int totalTokensSpent = choice.TokenCosts.Values.Sum();

        return choice.ActionType switch
        {
            UniversalActionType.BasicConversion => totalTokensSpent + 1, // 2 tokens → 3 progress
            UniversalActionType.SpecializedConversion => totalTokensSpent, // 2 tokens → 2 progress + bonus
            UniversalActionType.PremiumConversion => totalTokensSpent + 1, // 3 tokens → 4 progress
            _ => 0
        };
    }

    private static EncounterOutcomes DetermineCompletionOutcome(int finalProgress, int threshold)
    {
        if (finalProgress >= threshold + 4) return EncounterOutcomes.Success;
        if (finalProgress >= threshold) return EncounterOutcomes.Partial;
        return EncounterOutcomes.Failure;
    }

    private static void SetProjectionDescription(ChoiceProjection projection, EncounterOption choice, PlayerState playerState)
    {
        if (choice.Skill == SkillTypes.None)
        {
            projection.Description = "Safe approach guarantees positive outcome";
        }
        else
        {
            string skillName = choice.Skill.ToString();
            int skillLevel = playerState.GetSkillLevel(choice.Skill);

            if (projection.SkillCheckSuccess)
            {
                projection.Description = $"Success! Your {skillName} skill ({skillLevel}) meets the difficulty ({choice.Difficulty})";
            }
            else
            {
                projection.Description = $"Risk! Your {skillName} skill ({skillLevel}) may not meet the difficulty ({choice.Difficulty})";
            }
        }
    }

    public static AspectTokenTypes MapSkillToToken(SkillTypes skill, CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => MapPhysicalSkillToToken(skill),
            CardTypes.Social => MapSocialSkillToToken(skill),
            CardTypes.Intellectual => MapIntellectualSkillToToken(skill),
            _ => AspectTokenTypes.Force // Default fallback
        };
    }

    private static AspectTokenTypes MapPhysicalSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Strength => AspectTokenTypes.Force,
            SkillTypes.Agility => AspectTokenTypes.Flow,
            SkillTypes.Precision => AspectTokenTypes.Focus,
            SkillTypes.Endurance => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Force
        };
    }

    private static AspectTokenTypes MapSocialSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Intimidation => AspectTokenTypes.Force,
            SkillTypes.Charm => AspectTokenTypes.Flow,
            SkillTypes.Persuasion => AspectTokenTypes.Focus,
            SkillTypes.Deception => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Flow
        };
    }

    private static AspectTokenTypes MapIntellectualSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Analysis => AspectTokenTypes.Force,
            SkillTypes.Observation => AspectTokenTypes.Flow,
            SkillTypes.Knowledge => AspectTokenTypes.Focus,
            SkillTypes.Planning => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Focus
        };
    }


    public static int GetLocationPropertyModifier(SkillTypes skill, Location location)
    {
        int modifier = 0;

        // Apply modifiers based on location properties
        if (IsIntellectualSkill(skill))
        {
            // Intellectual skills are affected by Population and Illumination
            if (location.Population == Population.Crowded)
                modifier -= 1; // Hard to concentrate in crowds

            if (location.Illumination == Illumination.Dark)
                modifier -= 1; // Hard to analyze in darkness

            if (location.Population == Population.Scholarly)
                modifier += 1; // Easier to think in scholarly environment
        }
        else if (IsSocialSkill(skill))
        {
            // Social skills are affected by Population and Atmosphere
            if (location.Population == Population.Quiet)
                modifier -= 1; // Fewer social opportunities

            if (location.Atmosphere == Atmosphere.Tense)
                modifier -= 1; // Harder to be persuasive in tense atmosphere

            if (location.Population == Population.Crowded)
                modifier += 1; // More social opportunities
        }
        else if (IsPhysicalSkill(skill))
        {
            // Physical skills are affected by Physical property and Illumination
            if (location.Physical == Physical.Confined)
                modifier -= 1; // Limited physical movement

            if (location.Illumination == Illumination.Dark)
                modifier -= 1; // Hard to see for precise movements

            if (location.Physical == Physical.Expansive)
                modifier += 1; // Room to move freely
        }

        return modifier;
    }

    public static bool IsPhysicalSkill(SkillTypes skill)
    {
        return skill == SkillTypes.Strength ||
               skill == SkillTypes.Endurance ||
               skill == SkillTypes.Precision ||
               skill == SkillTypes.Agility;
    }

    public static bool IsIntellectualSkill(SkillTypes skill)
    {
        return skill == SkillTypes.Analysis ||
               skill == SkillTypes.Observation ||
               skill == SkillTypes.Knowledge ||
               skill == SkillTypes.Planning;
    }

    public static bool IsSocialSkill(SkillTypes skill)
    {
        return skill == SkillTypes.Charm ||
               skill == SkillTypes.Persuasion ||
               skill == SkillTypes.Deception ||
               skill == SkillTypes.Intimidation;
    }
}