public class ChoiceProjectionService
{
    private readonly Player player;

    public ChoiceProjectionService(GameWorld gameWorld)
    {
        this.player = gameWorld.GetPlayer();
    }

    public ChoiceProjection ProjectChoice(EncounterChoice choice, EncounterState state)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Check if player can afford this choice
        projection.IsAffordable = state.FocusPoints >= choice.FocusCost;
        projection.IsAffordableFocus = projection.IsAffordable;

        // Process skill options
        SkillOptionProjection skillProjection = ProjectSkillOption(choice, state, player);
        projection.SkillOption = skillProjection;

        return projection;
    }

    private SkillOptionProjection ProjectSkillOption(EncounterChoice choice, EncounterState state, Player player)
    {
        var option = choice.SkillOption;

        SkillOptionProjection projection = new SkillOptionProjection();
        projection.SkillName = option.SkillName;
        projection.Difficulty = option.Difficulty;

        SkillCard card = FindCardByName(player.AvailableCards, option.SkillName);

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
            projection.EffectiveLevel = 1; // Base level for untrained
        }

        // Calculate success chance
        projection.ChoiceSuccess = DetermineChoiceSuccess(choice, projection, state, player);

        return projection;
    }

    private bool DetermineChoiceSuccess(EncounterChoice choice, SkillOptionProjection projection, EncounterState state, Player player)
    {
        SkillOption skillCheck = choice.SkillOption;

        // Find matching skill card
        SkillCard card = FindCardByName(player.AvailableCards, skillCheck.SkillName);
        bool isUntrained = (card == null || card.IsExhausted);

        int effectiveLevel = 0;
        int difficulty = GetDifficulty(projection.Difficulty);

        if (!isUntrained && card != null)
        {
            effectiveLevel = card.GetEffectiveLevel(state);
            card.Exhaust();
        }
        else
        {
            difficulty += 2; // +2 difficulty for untrained
        }

        // Apply modifier
        effectiveLevel += state.GetNextCheckModifier();

        var successChance = CalculateSuccessChance(effectiveLevel, difficulty);
        int random = new Random().Next(100);
        bool success = successChance >= random;

        return success;
    }

    private int GetDifficulty(string difficulty)
    {
        //Easy=2, Standard=3, Hard=4, Exceptional=5
        switch (difficulty)
        {
            case "Easy":
                return 2;
            case "Standard":
                return 3;
            case "Hard":
                return 4;
            case "Exceptional":
                return 5;
            default:
                return 3; // Default to Standard
        }
    }

    private int CalculateSuccessChance(int effectiveLevel, int difficulty)
    {
        int difference = effectiveLevel - difficulty;

        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 15; // Not impossible, but very unlikely
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

    private EffectProjection ProjectEffect(IMechanicalEffect effect, EncounterState state)
    {
        if (effect == null)
        {
            return new EffectProjection() { MechanicalDescription = "No mechanical Effect", NarrativeEffect = "No Narrative Effect" };
        }

        EffectProjection projection = new EffectProjection();
        projection.NarrativeEffect = effect.ToString();

        effect.Apply(state);

        projection.MechanicalDescription = effect.GetDescriptionForPlayer();

        return projection;
    }
}