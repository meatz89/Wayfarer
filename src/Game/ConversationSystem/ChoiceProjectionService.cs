public class ChoiceProjectionService
{
    private static readonly Random _random = new Random();
    private readonly Player player;

    public ChoiceProjectionService(GameWorld gameWorld)
    {
        this.player = gameWorld.GetPlayer();
    }

    public ChoiceProjection ProjectChoice(ConversationChoice choice, ConversationState state)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Check if player can afford this choice
        projection.IsAffordable = state.FocusPoints >= choice.AttentionCost;
        projection.IsAffordableFocus = projection.IsAffordable;

        // Process skill options
        SkillOptionProjection skillProjection = ProjectSkillOption(choice, state, player);
        projection.SkillOption = skillProjection;

        return projection;
    }

    private SkillOptionProjection ProjectSkillOption(ConversationChoice choice, ConversationState state, Player player)
    {
        SkillOptionProjection projection = new SkillOptionProjection();
        if (!choice.RequiresSkillCheck)
        {
            projection.SkillName = "None";
            projection.IsAvailable = true;
            projection.IsUntrained = false;
            projection.EffectiveLevel = 1;
            projection.ChoiceSuccess = true;
            return projection;
        }

        SkillOption option = choice.SkillOption;

        projection.SkillName = option.SkillName;
        projection.Difficulty = option.Difficulty;

        // Skill cards removed - using letter queue and token system
        projection.IsAvailable = true;
        projection.IsUntrained = false;
        projection.EffectiveLevel = 1;

        // Calculate success chance
        projection.ChoiceSuccess = DetermineChoiceSuccess(choice, projection, state, player);

        return projection;
    }

    private bool DetermineChoiceSuccess(ConversationChoice choice, SkillOptionProjection projection, ConversationState state, Player player)
    {
        SkillOption skillCheck = choice.SkillOption;

        // Skill cards removed - using relationship tokens for checks
        int effectiveLevel = 1;
        int difficulty = GetDifficulty(projection.Difficulty);

        // Apply modifier - removed for conversation system

        int successChance = CalculateSuccessChance(effectiveLevel, difficulty);
        int random = _random.Next(100); // Use shared Random instance
        bool success = successChance >= random;

        return success;
    }

    private int GetDifficulty(string difficulty)
    {
        //Easy=0, Standard=1, Hard=2, Exceptional=3
        switch (difficulty)
        {
            case "Easy":
                return 0;
            case "Standard":
                return 1;
            case "Hard":
                return 2;
            case "Exceptional":
                return 3;
            default:
                return 1; // Default to Standard
        }
    }

    private int CalculateSuccessChance(int effectiveLevel, int difficulty)
    {
        int difference = effectiveLevel - difficulty;

        if (difference >= 2) return 95;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 40;
        return 30; // Not impossible, but very unlikely
    }


    // Skill cards removed - conversation system uses tokens and relationships

    private EffectProjection ProjectEffect(IMechanicalEffect effect, ConversationState state)
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