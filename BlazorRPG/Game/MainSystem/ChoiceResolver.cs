public class ChoiceResolver
{
    public BeatOutcome ResolveChoice(PlayerChoiceSelection selection, EncounterState state)
    {
        ChoiceTemplateLibrary templateLibrary = new ChoiceTemplateLibrary();

        // Deduct focus cost
        EncounterChoice selectedChoice = selection.Choice;
        state.FocusPoints -= selectedChoice.FocusCost;

        // Find and exhaust skill card
        SkillOption selectedSkillOption = selection.SelectedOption;
        SkillCard usedCard = FindAndExhaustCard(selectedSkillOption.SkillName, state);

        // Perform skill check
        SkillCheckResolver skillCheckResolver = new SkillCheckResolver();
        SkillCheckResult checkResult = skillCheckResolver.Resolve(
            selectedSkillOption,
            usedCard,
            state
        );

        // Apply appropriate effect
        string effectID = checkResult.IsSuccess ?
            selectedSkillOption.SuccessEffect.ID :
            selectedSkillOption.FailureEffect.ID;

        EffectProcessor effectProcessor = new EffectProcessor(templateLibrary, state);
        effectProcessor.ApplyEffect(effectID, state);

        return new BeatOutcome
        {
            CheckResult = checkResult,
            EffectApplied = effectID,
            NewFlags = state.FlagManager.GetRecentlySetFlags(),
            IsEncounterComplete = state.IsEncounterComplete
        };
    }

    private SkillCard FindAndExhaustCard(string skillName, EncounterState state)
    {
        throw new NotImplementedException();
    }
}