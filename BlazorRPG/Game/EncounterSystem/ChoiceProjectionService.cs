public class ChoiceProjectionService
{
    private TemplateLibrary templateLibrary;
    private readonly Player player;

    public ChoiceProjectionService(TemplateLibrary templateLibrary, Player player)
    {
        this.templateLibrary = templateLibrary;
        this.player = player;
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
        projection.Difficulty = option.DifficultyString;
        projection.SCD = option.SCD;

        SkillCard card = FindCardByName(player.AvailableCards, option.SkillName);

        // TODO 
        SkillCheckResolver resolver = new SkillCheckResolver();
        SkillCheckResult skillCheckResult = resolver.Resolve(option, state);

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

        // Project effects
        projection.SuccessEffect = ProjectEffect(option.SuccessEffect, state);
        projection.FailureEffect = ProjectEffect(option.FailureEffect, state);

        return projection;
    }

    private EffectProjection ProjectEffect(IMechanicalEffect effect, EncounterState state)
    {
        EffectProjection projection = new EffectProjection();
        projection.NarrativeEffect = effect.ToString();

        effect.Apply(state); 

        // Get mechanical effect from registry
        if (templateLibrary.GetEffect(effect) != null)
        {
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
