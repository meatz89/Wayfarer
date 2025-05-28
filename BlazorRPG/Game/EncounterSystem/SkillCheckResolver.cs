public class SkillCheckResolver
{
    public SkillCheckResult ResolveCheck(SkillOption option, EncounterState state)
    {
        SkillCard card = FindSkillCard(option.RequiredSkillName, state.Player);

        int effectiveLevel = 0;
        int modifiedDifficulty = option.Difficulty.Length;
        bool isUntrained = false;

        if (card != null && !card.IsExhausted)
        {
            effectiveLevel = card.Level;
            card.Exhaust();
        }
        else
        {
            isUntrained = true;
            modifiedDifficulty += 2; // Untrained penalty
        }

        // Apply any temporary modifiers
        effectiveLevel += state.GetTemporarySkillModifier(option.RequiredSkillName);

        bool success = effectiveLevel >= modifiedDifficulty;

        return new SkillCheckResult
        {
            SkillName = option.RequiredSkillName,
            PlayerLevel = effectiveLevel,
            RequiredLevel = modifiedDifficulty,
            IsSuccess = success,
            IsUntrained = isUntrained,
            SuccessChance = CalculateSuccessChance(effectiveLevel, modifiedDifficulty)
        };
    }

    private SkillCard FindSkillCard(string skillName, Player player)
    {
        List<SkillCard> cards = player.GetAllAvailableCards();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].Name == skillName && !cards[i].IsExhausted)
            {
                return cards[i];
            }
        }
        return null;
    }

    private int CalculateSuccessChance(int level, int difficulty)
    {
        int difference = level - difficulty;
        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5;
    }
}