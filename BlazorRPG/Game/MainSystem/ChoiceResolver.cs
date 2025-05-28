public class ChoiceResolver
{
    public BeatOutcome ResolveChoice(PlayerChoiceSelection selection, EncounterState state)
    {
        PayloadRegistry payloadRegistry = new PayloadRegistry();

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

        // Apply appropriate payload
        string payloadID = checkResult.IsSuccess ?
            selectedSkillOption.SuccessPayload.MechanicalEffectID :
            selectedSkillOption.FailurePayload.MechanicalEffectID;

        PayloadProcessor payloadProcessor = new PayloadProcessor(payloadRegistry, state);
        payloadProcessor.Apply(payloadID, state);

        return new BeatOutcome
        {
            CheckResult = checkResult,
            PayloadApplied = payloadID,
            NewFlags = state.FlagManager.GetRecentlySetFlags(),
            IsEncounterComplete = state.IsEncounterComplete
        };
    }

    private SkillCard FindAndExhaustCard(string skillName, EncounterState state)
    {
        throw new NotImplementedException();
    }
}