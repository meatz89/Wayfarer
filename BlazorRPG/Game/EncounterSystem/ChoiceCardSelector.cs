public class ChoiceCardSelector
{
    private readonly NarrativeChoiceRepository _narrativeChoiceRepository;

    public ChoiceCardSelector(NarrativeChoiceRepository choiceRepository)
    {
        _narrativeChoiceRepository = choiceRepository;
    }

    public List<EncounterOption> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get the current stage's options - no card filtering needed
        if (state.CurrentStageIndex < state.EncounterInfo.Stages.Count)
        {
            return state.EncounterInfo.Stages[state.CurrentStageIndex].Options;
        }

        return new List<EncounterOption>();
    }

    private List<EncounterOption> GetCurrentStageOptions(EncounterState state)
    {
        if (state.CurrentStageIndex < state.EncounterInfo.Stages.Count)
        {
            return state.EncounterInfo.Stages[state.CurrentStageIndex].Options;
        }

        return new List<EncounterOption>();
    }

    private List<EncounterOption> FilterOptionsByAvailableCards(List<EncounterOption> options, PlayerState playerState)
    {
        List<EncounterOption> availableOptions = new List<EncounterOption>();

        foreach (EncounterOption option in options)
        {
            // Check if player has a card that can be used for this skill type
            CardTypes requiredCardType = GetCardTypeForSkill(option.Skill);
            bool hasCardAvailable = playerState.HasNonExhaustedCardOfType(requiredCardType);

            if (hasCardAvailable || option.Skill == SkillTypes.None)
            {
                availableOptions.Add(option);
            }
        }

        // Always ensure at least one option is available
        if (availableOptions.Count == 0 && options.Count > 0)
        {
            // Add a "safe" option that doesn't require a skill check
            EncounterOption safeOption = new EncounterOption
            {
                Id = "safe_option",
                Name = "Cautious Approach",
                Description = "Take a safe but less effective approach",
                Skill = SkillTypes.None,
                Difficulty = 0,
                SuccessProgress = 1,
                FailureProgress = 0
            };

            availableOptions.Add(safeOption);
        }

        return availableOptions;
    }

    private CardTypes GetCardTypeForSkill(SkillTypes skill)
    {
        if (SkillCheckService.IsPhysicalSkill(skill))
            return CardTypes.Physical;

        if (SkillCheckService.IsIntellectualSkill(skill))
            return CardTypes.Intellectual;

        if (SkillCheckService.IsSocialSkill(skill))
            return CardTypes.Social;

        return CardTypes.Physical; // Default fallback
    }
}