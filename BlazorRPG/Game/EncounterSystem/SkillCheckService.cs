public static class SkillCheckService
{
    public static ChoiceProjection CreateChoiceProjection(
        EncounterOption choice,
        int currentProgress,
        int progressThreshold,
        int currentTurn,
        PlayerState playerState,
        Location location)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // For "safe" options with no skill check
        if (choice.Skill == SkillTypes.None)
        {
            projection.ProgressGained = choice.SuccessProgress;
            projection.Description = "Safe approach guarantees minimal progress";

            // Check if encounter will complete
            bool willCompleteSafe = (currentProgress + projection.ProgressGained) >= progressThreshold;
            projection.EncounterWillEnd = willCompleteSafe;

            if (willCompleteSafe)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Partial;
            }

            return projection;
        }

        // Calculate the effective skill level
        int skillLevel = playerState.GetSkillLevel(choice.Skill);
        int effectiveSkill = skillLevel;

        // Determine if the skill check succeeds
        bool success = effectiveSkill >= choice.Difficulty;

        // Set progress based on success or failure
        projection.ProgressGained = success ? choice.SuccessProgress : choice.FailureProgress;

        // Set descriptive text
        if (success)
        {
            projection.Description = $"Success! You meet the {choice.Difficulty} difficulty with your skill of {effectiveSkill}";
        }
        else
        {
            projection.Description = $"Failure. Your skill of {effectiveSkill} doesn't meet the {choice.Difficulty} difficulty";
        }

        // Calculate if this will complete the encounter
        bool willComplete = (currentProgress + projection.ProgressGained) >= progressThreshold;
        projection.EncounterWillEnd = willComplete;

        // Determine outcome if encounter will end
        if (willComplete)
        {
            projection.ProjectedOutcome = success ? EncounterOutcomes.Success : EncounterOutcomes.Partial;
        }

        return projection;
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