public class AIResponseProcessor
{
    private readonly PayloadRegistry payloadRegistry;

    public AIResponseProcessor(PayloadRegistry payloadRegistry)
    {
        this.payloadRegistry = payloadRegistry;
    }

    public void ProcessChoice(AIChoice choice, string skillOptionName, EncounterState state)
    {
        state.FocusPoints -= choice.FocusCost;

        SkillOption selectedOption = null;
        foreach (SkillOption option in choice.SkillOptions)
        {
            if (option.SkillName.Equals(skillOptionName, StringComparison.OrdinalIgnoreCase))
            {
                selectedOption = option;
                break;
            }
        }

        if (selectedOption == null)
        {
            throw new InvalidOperationException($"Skill option {skillOptionName} not found in choice {choice.ChoiceID}");
        }

        SkillCard card = FindCardByName(state.Player.PlayerHandCards, selectedOption.SkillName);
        bool isUntrained = (card == null || card.IsExhausted);

        int effectiveLevel = 0;
        int difficulty = selectedOption.SCD;

        if (!isUntrained && card != null)
        {
            effectiveLevel = card.GetEffectiveLevel(state);
            card.Exhaust();
        }
        else
        {
            effectiveLevel = 0;
            difficulty += 2;
        }

        effectiveLevel += state.GetNextCheckModifier();

        bool success = effectiveLevel >= difficulty;

        if (success)
        {
            ApplyPayload(selectedOption.SuccessPayload.MechanicalEffectID, state);
        }
        else
        {
            ApplyPayload(selectedOption.FailurePayload.MechanicalEffectID, state);
        }

        if (choice.FocusCost == 0)
        {
            state.ConsecutiveRecoveryCount++;
        }
        else
        {
            state.ConsecutiveRecoveryCount = 0;
        }

        state.ProcessModifiers();
        state.CheckGoalCompletion();
        state.AdvanceDuration(1);
    }

    private void ApplyPayload(string payloadID, EncounterState state)
    {
        if (payloadRegistry.HasEffect(payloadID))
        {
            IMechanicalEffect effect = payloadRegistry.GetEffect(payloadID);
            effect.Apply(state);
        }
    }

    private SkillCard FindCardByName(List<SkillCard> cards, string name)
    {
        foreach (SkillCard card in cards)
        {
            if (card.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !card.IsExhausted)
            {
                return card;
            }
        }
        return null;
    }
}

