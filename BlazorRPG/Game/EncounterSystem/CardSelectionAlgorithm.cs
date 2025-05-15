public class CardSelectionAlgorithm
{
    private readonly ChoiceRepository _cardRepository;

    public CardSelectionAlgorithm(ChoiceRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public List<CardDefinition> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get all available cards
        List<CardDefinition> allCards = _cardRepository.GetForEncounter(state);

        List<CardDefinition> playableCards = FilterPlayableCards(allCards, state);

        // Calculate card viability scores
        Dictionary<CardDefinition, CardViabilityScore> cardScores = new Dictionary<CardDefinition, CardViabilityScore>();
        foreach (CardDefinition card in playableCards)
        {
            CardViabilityScore score = CalculateCardViability(
                card,
                state,
                playerState
            );
            cardScores[card] = score;
        }

        // Select 4 strategically diverse cards based on the viability scores
        return SelectStrategicCardHand(playableCards, cardScores);
    }

    private List<CardDefinition> FilterPlayableCards(List<CardDefinition> allCards, EncounterState state)
    {
        List<CardDefinition> playableCards = new List<CardDefinition>();

        foreach (CardDefinition card in allCards)
        {
            // Tier 1 cards are always playable
            if (card.Level == 1)
            {
                playableCards.Add(card);
                continue;
            }
        }

        return playableCards;
    }

    private CardViabilityScore CalculateCardViability(
        CardDefinition card,
        EncounterState state,
        PlayerState playerState)
    {
        CardViabilityScore score = new CardViabilityScore();

        // For Tier 1 cards, position doesn't matter as much
        if (card.Level == 1)
        {
            score.PositionalScore = 0;
            score.IsPlayable = true;
        }
        else
        {
            // Apply tier modifiers
            score.PositionalScore -= card.Level; // Higher tier cards get an advantage

            // Card is already confirmed playable
            score.IsPlayable = true;
        }

        // Strategic synergy with environmental properties
        score.EnvironmentalSynergy = CalculateEnvironmentalSynergy(card, state);

        // Check if skills make this card more effective
        score.SkillBonus = CalculateSkillBonus(card, playerState);

        // Final score calculation (lower is better)
        score.TotalScore = score.PositionalScore - score.SituationalValue -
                          score.EnvironmentalSynergy - score.SkillBonus;

        return score;
    }
    
    private int CalculateEnvironmentalSynergy(CardDefinition card, EncounterState state)
    {
        int synergy = 0;

        return synergy;
    }

    private int CalculateSkillBonus(CardDefinition card, PlayerState playerState)
    {
        int bonus = 0;

        return bonus;
    }

    private List<CardDefinition> SelectStrategicCardHand(
        List<CardDefinition> viableCards,
        Dictionary<CardDefinition, CardViabilityScore> cardScores)
    {
        List<CardDefinition> result = new List<CardDefinition>();

        if (viableCards.Any())
        {
            CardDefinition bestCard = viableCards
                .OrderBy(c =>
                {
                    return cardScores[c].TotalScore;
                })
                .First();

            result.Add(bestCard);
        }

        // If we still need cards, add highest scoring remaining cards
        while (result.Count < 4 && viableCards.Any(c =>
        {
            return !result.Contains(c);
        }))
        {
            CardDefinition nextCard = viableCards
                .Where(c =>
                {
                    return !result.Contains(c);
                })
                .OrderBy(c =>
                {
                    return cardScores[c].TotalScore;
                })
                .First();

            result.Add(nextCard);
        }

        return result;
    }
}
