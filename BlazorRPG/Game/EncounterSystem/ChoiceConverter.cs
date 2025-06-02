public class ChoiceConverter
{
    public List<EncounterChoice> ConvertEncounterChoices(AIResponse aiResponse, EncounterState state)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();

        foreach (EncounterChoice encounterChoice in aiResponse.Choices)
        {
            EncounterChoice choice = new EncounterChoice();
            choice.ChoiceID = encounterChoice.ChoiceID;
            choice.NarrativeText = encounterChoice.NarrativeText;
            choice.FocusCost = encounterChoice.FocusCost;

            SkillOption option = ConvertSkillOption(encounterChoice.SkillOption, state);
            choice.SkillOption = option;

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
        option.SuccessChance = CalculateSuccessChance(option.EffectiveLevel, option.DifficultyString);

        // Link to effects
        option.SuccessEffect = aiOption.SuccessEffect;
        option.FailureEffect = aiOption.FailureEffect;

        return option;
    }

    private int CalculateSuccessChance(int effectiveLevel, string difficulty)
    {
        return 50; // Placeholder logic, replace with actual calculation based on game rules
    }
}