public class ChoiceProjectionService
{
    private PayloadRegistry _payloadRegistry;

    public ChoiceProjectionService(PayloadRegistry payloadRegistry, Player player)
    {
        _payloadRegistry = payloadRegistry;
    }

    public ChoiceProjection ProjectChoice(EncounterChoice choice, EncounterState state)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Check if player can afford this choice
        projection.IsAffordable = state.FocusPoints >= choice.FocusCost;
        projection.IsAffordableFocus = projection.IsAffordable;

        // Process skill options
        SkillOptionProjection skillProjection = ProjectSkillOption(choice.SkillOption, state, player);
        projection.SkillOption = skillProjection;

        return projection;
    }

    private SkillOptionProjection ProjectSkillOption(SkillOption option, EncounterState state, Player player)
    {
        SkillOptionProjection projection = new SkillOptionProjection();
        projection.SkillName = option.SkillName;
        projection.Difficulty = option.Difficulty;
        projection.SCD = option.SCD;

        SkillCard card = FindCardByName(player.AvailableCards, option.SkillName);

        // TODO 
        SkillCheckResolver resolver = new SkillCheckResolver(_payloadRegistry);
        SkillCheckResult skillCheckResult = resolver.Resolve(option, card, state);

        if (card != null && !card.IsExhausted)
        {
            // Player has the card and it's not exhausted
            projection.IsAvailable = true;
            projection.IsUntrained = false;
            projection.EffectiveLevel = card.GetEffectiveLevel(state);
        }
        else
        {
            // Untrained attempt
            projection.IsAvailable = true; // Still available, but untrained
            projection.IsUntrained = true;
            projection.EffectiveLevel = 0; // Base level for untrained
            projection.SCD = option.SCD + 2; // +2 difficulty for untrained
        }

        // Calculate success chance
        projection.SuccessChance = CalculateSuccessChance(projection.EffectiveLevel, projection.SCD);

        // Project payloads
        projection.SuccessPayload = ProjectPayload(option.SuccessPayload, state);
        projection.FailurePayload = ProjectPayload(option.FailurePayload, state);

        return projection;
    }

    private PayloadProjection ProjectPayload(AIPayload payload, EncounterState state)
    {
        PayloadProjection projection = new PayloadProjection();
        projection.NarrativeEffect = payload.NarrativeEffect;

        // TODO
        PayloadProcessor payloadProcessor = new PayloadProcessor(_payloadRegistry, state);
        payloadProcessor.Apply(payload.MechanicalEffectID, state);

        // Get mechanical effect from registry
        if (_payloadRegistry.HasEffect(payload.MechanicalEffectID))
        {
            IMechanicalEffect effect = _payloadRegistry.GetEffect(payload.MechanicalEffectID);
            projection.MechanicalDescription = effect.GetDescriptionForPlayer();
        }
        else
        {
            projection.MechanicalDescription = "Unknown effect";
        }

        return projection;
    }

    private int CalculateSuccessChance(int effectiveLevel, int difficulty)
    {
        int difference = effectiveLevel - difficulty;

        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5; // Not impossible, but very unlikely
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
