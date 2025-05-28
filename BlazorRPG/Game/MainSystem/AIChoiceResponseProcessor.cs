public class EncounterChoiceResponseProcessor
{
    private PayloadRegistry _payloadRegistry;

    public EncounterChoiceResponseProcessor(PayloadRegistry payloadRegistry, ILogger<EncounterChoiceResponseProcessor> logger)
    {
        _payloadRegistry = payloadRegistry;
    }

    public BeatOutcome ProcessChoice(EncounterChoice choice, string skillOptionName, EncounterState state, Player player)
    {
        // First, deduct Focus cost
        state.SpendFocusPoints(choice.FocusCost);

        // Find the selected skill option
        SkillOption selectedOption = choice.SkillOption;

        // Find the skill card
        SkillCard card = FindCardByName(player.AvailableCards, selectedOption.SkillName);
        bool isUntrained = (card == null || card.IsExhausted);

        // Perform skill check
        int effectiveLevel = 0;
        int difficulty = selectedOption.SCD;

        if (!isUntrained && card != null)
        {
            // Using a skill card
            effectiveLevel = card.GetEffectiveLevel(state);
            card.Exhaust(); // Exhaust the card
        }
        else
        {
            // Untrained attempt
            effectiveLevel = 0;
            difficulty += 2; // +2 difficulty for untrained
        }

        // Add any next check modifier
        effectiveLevel += state.GetNextCheckModifier();

        // Determine success
        bool success = effectiveLevel >= difficulty;

        // Apply appropriate payload
        if (success)
        {
            ApplyPayload(selectedOption.SuccessPayload.MechanicalEffectID, state);
        }
        else
        {
            ApplyPayload(selectedOption.FailurePayload.MechanicalEffectID, state);
        }

        // If this was a recovery action (0 Focus cost), increment consecutive recovery count
        if (choice.FocusCost == 0)
        {
            state.ConsecutiveRecoveryCount++;
        }
        else
        {
            // Reset consecutive recovery count for non-recovery actions
            state.ConsecutiveRecoveryCount = 0;
        }

        // Process skill modifiers
        state.ProcessModifiers();

        // Check if goal has been achieved
        state.CheckGoalCompletion();

        // Advance duration - basic duration advance for any action
        state.AdvanceDuration(1);

        // Calculate progress based on choice and state
        int progressGained = CalculateProgressGained(choice, selectedOption, success);

        // Create outcome
        PlayerChoiceSelection playerSelection = new PlayerChoiceSelection
        {
            Choice = choice,
            SelectedOption = selectedOption,
        };

        ChoiceResolver choiceResolver = new ChoiceResolver();
        BeatOutcome beatOutcome = choiceResolver.ResolveChoice(playerSelection, state);

        return beatOutcome;
    }

    private int CalculateProgressGained(EncounterChoice choice, SkillOption option, bool success)
    {
        // Base progress calculation - can be expanded based on payload types
        if (success)
        {
            return 2; // Default progress for successful action
        }
        return 1; // Minimal progress for failed action
    }

    private EncounterOutcomes DetermineOutcome(EncounterState state, int progressGained)
    {
        if (!state.IsEncounterComplete)
        {
            return EncounterOutcomes.None;
        }

        int projectedTotalProgress = state.CurrentProgress + progressGained;
        int successThreshold = 10; // Basic success threshold

        return projectedTotalProgress >= successThreshold
            ? EncounterOutcomes.Success
            : EncounterOutcomes.Failure;
    }

    private void ApplyPayload(string payloadID, EncounterState state)
    {
        if (_payloadRegistry.HasEffect(payloadID))
        {
            IMechanicalEffect effect = _payloadRegistry.GetEffect(payloadID);
            effect.Apply(state);
        }
        else
        {
            _logger.LogWarning("Payload ID not found: {PayloadID}", payloadID);
        }
    }

    private string GetMechanicalDescriptionForPayload(AIPayload payload)
    {
        if (_payloadRegistry.HasEffect(payload.MechanicalEffectID))
        {
            IMechanicalEffect effect = _payloadRegistry.GetEffect(payload.MechanicalEffectID);
            return effect.GetDescriptionForPlayer();
        }
        return "Unknown effect";
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
