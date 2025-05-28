public class ChoiceConverter
{
    private PayloadRegistry payloadRegistry;

    public List<EncounterChoice> ConvertEncounterChoices(BeatResponse aiResponse, EncounterState state)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();

        foreach (EncounterChoice EncounterChoice in aiResponse.AvailableChoices)
        {
            EncounterChoice choice = new EncounterChoice();
            choice.ChoiceID = EncounterChoice.ChoiceID;
            choice.NarrativeText = EncounterChoice.NarrativeText;
            choice.FocusCost = EncounterChoice.FocusCost;

            // Convert each skill option
            choice.SkillOption = new List<SkillOption>();
            foreach (SkillOption aiSkillOption in EncounterChoice.SkillOption)
            {
                SkillOption option = ConvertSkillOption(aiSkillOption, state);
                choice.SkillOption.Add(option);
            }

            choices.Add(choice);
        }

        return choices;
    }

    private SkillOption ConvertSkillOption(SkillOption aiOption, EncounterState state)
    {
        SkillOption option = new SkillOption();
        option.RequiredSkillName = aiOption.SkillName;
        option.Difficulty = aiOption.SCD;

        // Calculate player's effective level with this skill
        SkillCard card = state.Player.FindCard(aiOption.SkillName);
        if (card != null && !card.IsExhausted)
        {
            option.EffectiveLevel = card.Level + state.GetNextCheckModifier();
            option.IsUntrained = false;
        }
        else
        {
            option.EffectiveLevel = 0;
            option.IsUntrained = true;
            option.Difficulty += 2; // Untrained penalty
        }

        // Calculate success chance
        option.SuccessChance = CalculateSuccessChance(option.EffectiveLevel, option.Difficulty);

        // Link to payloads
        option.SuccessPayload = aiOption.SuccessPayload.ID;
        option.FailurePayload = aiOption.FailurePayload.ID;

        return option;
    }

    private int CalculateSuccessChance(int effectiveLevel, string difficulty)
    {
        return 50; // Placeholder logic, replace with actual calculation based on game rules
    }
}