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

        // Ensure at least one momentum and one pressure card
        EnsureCardTypeBalance(playableCards, cardScores);

        // Select 4 strategically diverse cards based on the viability scores
        return SelectStrategicCardHand(playableCards, cardScores);
    }

    private List<CardDefinition> FilterPlayableCards(List<CardDefinition> allCards, EncounterState state)
    {
        List<CardDefinition> playableCards = new List<CardDefinition>();

        foreach (CardDefinition card in allCards)
        {
            // Tier 1 cards are always playable
            if (card.Tier == 1)
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
        if (card.Tier == 1)
        {
            score.PositionalScore = 0;
            score.IsPlayable = true;
        }
        else
        {
            // Apply tier modifiers
            score.PositionalScore -= card.Tier; // Higher tier cards get an advantage

            // Card is already confirmed playable
            score.IsPlayable = true;
        }

        // Situational value based on encounter state
        score.SituationalValue = CalculateSituationalValue(card, state);

        // Strategic synergy with environmental properties
        score.EnvironmentalSynergy = CalculateEnvironmentalSynergy(card, state);

        // Check if skills make this card more effective
        score.SkillBonus = CalculateSkillBonus(card, playerState);

        // Final score calculation (lower is better)
        score.TotalScore = score.PositionalScore - score.SituationalValue -
                          score.EnvironmentalSynergy - score.SkillBonus;

        return score;
    }

    private int CalculateSituationalValue(CardDefinition card, EncounterState state)
    {
        int value = 0;

        // Momentum-building cards become more valuable as encounter nears end
        if (card.EffectType == EffectTypes.Momentum &&
            state.CurrentTurn >= state.EncounterInfo.MaxTurns - 2 &&
            state.Momentum < state.EncounterInfo.StandardThreshold)
        {
            value += 3;
        }

        // Pressure-reducing cards become more valuable as pressure rises
        if (card.EffectType == EffectTypes.Pressure &&
            state.Pressure >= state.EncounterInfo.MaxPressure * 0.6)
        {
            value += 3;
        }

        return value;
    }

    private int CalculateEnvironmentalSynergy(CardDefinition card, EncounterState state)
    {
        int synergy = 0;

        // Check if card's strategic effect is relevant to the current environment
        if (card.StrategicEffect != null)
        {
            List<StrategicTag> environmentTags = state.ActiveTags
                .Where(t =>
                {
                    return t is StrategicTag;
                })
                .Cast<StrategicTag>()
                .ToList();

            foreach (StrategicTag tag in environmentTags)
            {
                if (card.StrategicEffect.IsActive(tag))
                {
                    // Card has direct synergy with this environment
                    synergy += 2;
                }
            }
        }

        return synergy;
    }

    private int CalculateSkillBonus(CardDefinition card, PlayerState playerState)
    {
        int bonus = 0;

        // Check if player has skills that directly enhance this card
        foreach (SkillRequirement req in card.UnlockRequirements)
        {
            int skillLevel = playerState.Skills.GetLevelForSkill(req.SkillType);
            if (skillLevel >= req.RequiredLevel)
            {
                bonus += skillLevel - req.RequiredLevel + 1;
            }
        }

        return bonus;
    }

    private void EnsureCardTypeBalance(List<CardDefinition> viableCards, Dictionary<CardDefinition, CardViabilityScore> cardScores)
    {
        bool hasMomentumCard = viableCards.Any(c =>
        {
            return c.EffectType == EffectTypes.Momentum;
        });
        bool hasPressureCard = viableCards.Any(c =>
        {
            return c.EffectType == EffectTypes.Pressure;
        });

        if (!hasMomentumCard || !hasPressureCard)
        {
            // Add basic cards of the missing type
            List<CardDefinition> allCards = _cardRepository.GetAll();

            if (!hasMomentumCard)
            {
                CardDefinition basicMomentumCard = allCards.FirstOrDefault(c =>
                {
                    return c.EffectType == EffectTypes.Momentum && c.Tier == 1;
                });

                if (basicMomentumCard != null)
                {
                    viableCards.Add(basicMomentumCard);
                    cardScores[basicMomentumCard] = new CardViabilityScore { IsPlayable = true, TotalScore = 20 };
                }
            }

            if (!hasPressureCard)
            {
                CardDefinition basicPressureCard = allCards.FirstOrDefault(c =>
                {
                    return c.EffectType == EffectTypes.Pressure && c.Tier == 1;
                });

                if (basicPressureCard != null)
                {
                    viableCards.Add(basicPressureCard);
                    cardScores[basicPressureCard] = new CardViabilityScore { IsPlayable = true, TotalScore = 20 };
                }
            }
        }
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
