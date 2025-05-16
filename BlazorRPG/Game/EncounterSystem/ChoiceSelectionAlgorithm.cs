public class ChoiceSelectionAlgorithm
{
    private readonly NarrativeChoiceRepository _narrativeChoiceRepository;

    public ChoiceSelectionAlgorithm(NarrativeChoiceRepository cardRepository)
    {
        _narrativeChoiceRepository = cardRepository;
    }

    public List<NarrativeChoice> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get all available cards
        List<NarrativeChoice> allCards = _narrativeChoiceRepository.GetForEncounter(state);

        List<NarrativeChoice> playableCards = FilterPlayableCards(allCards, state);

        // Calculate card viability scores
        Dictionary<NarrativeChoice, CardViabilityScore> cardScores = new Dictionary<NarrativeChoice, CardViabilityScore>();
        foreach (NarrativeChoice card in playableCards)
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

    private List<NarrativeChoice> FilterPlayableCards(List<NarrativeChoice> allCards, EncounterState state)
    {
        List<NarrativeChoice> playableCards = new List<NarrativeChoice>();

        foreach (NarrativeChoice card in allCards)
        {
            // Tier 1 cards are always playable
            if (card.Difficulty == 1)
            {
                playableCards.Add(card);
                continue;
            }
        }

        return playableCards;
    }

    private CardViabilityScore CalculateCardViability(
        NarrativeChoice card,
        EncounterState state,
        PlayerState playerState)
    {
        CardViabilityScore score = new CardViabilityScore();

        // For Tier 1 cards, position doesn't matter as much
        if (card.Difficulty == 1)
        {
            score.PositionalScore = 0;
            score.IsPlayable = true;
        }
        else
        {
            // Apply tier modifiers
            score.PositionalScore -= card.Difficulty; // Higher tier cards get an advantage

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
    
    private int CalculateEnvironmentalSynergy(NarrativeChoice card, EncounterState state)
    {
        int synergy = 0;

        return synergy;
    }

    private int CalculateSkillBonus(NarrativeChoice card, PlayerState playerState)
    {
        int bonus = 0;

        return bonus;
    }

    private List<NarrativeChoice> SelectStrategicCardHand(
        List<NarrativeChoice> viableCards,
        Dictionary<NarrativeChoice, CardViabilityScore> cardScores)
    {
        List<NarrativeChoice> result = new List<NarrativeChoice>();

        if (viableCards.Any())
        {
            NarrativeChoice bestCard = viableCards
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
            NarrativeChoice nextCard = viableCards
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
