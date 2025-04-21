public class CardSelectionAlgorithm
{
    private readonly CardRepository _cardRepository;

    public CardSelectionAlgorithm(CardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public List<CardDefinition> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get all available cards
        List<CardDefinition> allCards = _cardRepository.GetForEncounter(state);

        // FIRST FILTER: Remove cards that don't meet minimum approach/focus requirements
        List<CardDefinition> playableCards = FilterPlayableCards(allCards, state);

        // Calculate positional advantages from skills
        Dictionary<ApproachTags, int> approachAdvantages = CalculateSkillApproachBonuses(playerState);
        Dictionary<FocusTags, int> focusAdvantages = CalculateSkillFocusBonuses(playerState);

        // Calculate environmental influences
        Dictionary<ApproachTags, int> environmentalBonuses = CalculateEnvironmentalBonuses(state);

        // Calculate card viability scores
        Dictionary<CardDefinition, CardViabilityScore> cardScores = new Dictionary<CardDefinition, CardViabilityScore>();
        foreach (CardDefinition card in playableCards)
        {
            CardViabilityScore score = CalculateCardViability(
                card,
                state,
                playerState,
                approachAdvantages,
                focusAdvantages,
                environmentalBonuses
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
            // Get current position values
            int currentApproachValue = state.EncounterTagSystem.GetApproachTagValue(card.Approach);
            int currentFocusValue = state.EncounterTagSystem.GetFocusTagValue(card.Focus);

            // Tier 1 cards are always playable
            if (card.Tier == 1)
            {
                playableCards.Add(card);
                continue;
            }

            // CHECK REQUIREMENTS: Card is only playable if we meet BOTH minimum values
            if (currentApproachValue >= card.OptimalApproachPosition &&
                currentFocusValue >= card.OptimalFocusPosition)
            {
                playableCards.Add(card);
            }
        }

        return playableCards;
    }

    private CardViabilityScore CalculateCardViability(
        CardDefinition card,
        EncounterState state,
        PlayerState playerState,
        Dictionary<ApproachTags, int> approachAdvantages,
        Dictionary<FocusTags, int> focusAdvantages,
        Dictionary<ApproachTags, int> environmentalBonuses)
    {
        CardViabilityScore score = new CardViabilityScore();

        // Get current approach/focus values
        int approachValue = state.EncounterTagSystem.GetApproachTagValue(card.Approach);
        int focusValue = state.EncounterTagSystem.GetFocusTagValue(card.Focus);

        // For scoring purposes, we can add skill advantages
        if (approachAdvantages.TryGetValue(card.Approach, out int approachBonus))
            approachValue += approachBonus;

        if (focusAdvantages.TryGetValue(card.Focus, out int focusBonus))
            focusValue += focusBonus;

        // For Tier 1 cards, position doesn't matter as much
        if (card.Tier == 1)
        {
            score.PositionalScore = 0;
            score.IsPlayable = true;
        }
        else
        {
            // For higher tier cards, calculate positional advantage (not requirements)
            int approachAdvantage = approachValue - card.OptimalApproachPosition;
            int focusAdvantage = focusValue - card.OptimalFocusPosition;

            // Calculate how far beyond requirements we are (positive is good)
            score.PositionalScore = -(approachAdvantage + focusAdvantage);

            // Environmental bonus improves position score
            if (environmentalBonuses.TryGetValue(card.Approach, out int envBonus))
                score.PositionalScore -= envBonus;

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

    private Dictionary<ApproachTags, int> CalculateSkillApproachBonuses(PlayerState playerState)
    {
        Dictionary<ApproachTags, int> bonuses = new Dictionary<ApproachTags, int>();

        // Warfare skill → Dominance approach
        bonuses[ApproachTags.Dominance] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Warfare) / 2;

        // Diplomacy skill → Rapport approach
        bonuses[ApproachTags.Rapport] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Diplomacy) / 2;

        // Scholarship skill → Analysis approach
        bonuses[ApproachTags.Analysis] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Scholarship) / 2;

        // Wilderness skill → Precision approach
        bonuses[ApproachTags.Precision] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Wilderness) / 2;

        // Subterfuge skill → Concealment approach
        bonuses[ApproachTags.Concealment] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Subterfuge) / 2;

        return bonuses;
    }

    private Dictionary<FocusTags, int> CalculateSkillFocusBonuses(PlayerState playerState)
    {
        Dictionary<FocusTags, int> bonuses = new Dictionary<FocusTags, int>();

        // Create skill-focus mappings
        bonuses[FocusTags.Physical] = (playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Warfare) +
                                     playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Wilderness)) / 3;

        bonuses[FocusTags.Information] = (playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Scholarship) +
                                        playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Subterfuge)) / 3;

        bonuses[FocusTags.Relationship] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Diplomacy) / 2;

        bonuses[FocusTags.Environment] = playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Wilderness) / 2;

        bonuses[FocusTags.Resource] = (playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Subterfuge) +
                                     playerState.PlayerSkills.GetLevelForSkill(SkillTypes.Diplomacy)) / 4;

        return bonuses;
    }

    private Dictionary<ApproachTags, int> CalculateEnvironmentalBonuses(EncounterState state)
    {
        Dictionary<ApproachTags, int> bonuses = new Dictionary<ApproachTags, int>();

        // Extract active environmental properties
        List<IEnvironmentalProperty> properties = state.ActiveTags
            .Where(t => t is StrategicTag)
            .Select(t => ((StrategicTag)t).EnvironmentalProperty)
            .ToList();

        // Define which approaches are favored by each environmental property
        foreach (IEnvironmentalProperty property in properties)
        {
            if (property is Illumination illumination)
            {
                if (illumination.Equals(Illumination.Bright))
                {
                    AddOrIncrease(bonuses, ApproachTags.Dominance, 1);
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                }
                else if (illumination.Equals(Illumination.Shadowy))
                {
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                    AddOrIncrease(bonuses, ApproachTags.Concealment, 1);
                }
                else if (illumination.Equals(Illumination.Dark))
                {
                    AddOrIncrease(bonuses, ApproachTags.Concealment, 2);
                }
            }
            else if (property is Population population)
            {
                if (population.Equals(Population.Crowded))
                {
                    AddOrIncrease(bonuses, ApproachTags.Rapport, 2);
                    AddOrIncrease(bonuses, ApproachTags.Dominance, 1);
                }
                else if (population.Equals(Population.Quiet))
                {
                    AddOrIncrease(bonuses, ApproachTags.Analysis, 1);
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                }
                else if (population.Equals(Population.Scholarly))
                {
                    AddOrIncrease(bonuses, ApproachTags.Analysis, 2);
                }
            }
            else if (property is Physical physical)
            {
                if (physical.Equals(Physical.Confined))
                {
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                    AddOrIncrease(bonuses, ApproachTags.Concealment, 1);
                }
                else if (physical.Equals(Physical.Expansive))
                {
                    AddOrIncrease(bonuses, ApproachTags.Dominance, 1);
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                }
                else if (physical.Equals(Physical.Hazardous))
                {
                    AddOrIncrease(bonuses, ApproachTags.Analysis, 1);
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                }
            }
            else if (property is Atmosphere atmosphere)
            {
                if (atmosphere.Equals(Atmosphere.Rough))
                {
                    AddOrIncrease(bonuses, ApproachTags.Dominance, 2);
                }
                else if (atmosphere.Equals(Atmosphere.Calm))
                {
                    AddOrIncrease(bonuses, ApproachTags.Rapport, 1);
                    AddOrIncrease(bonuses, ApproachTags.Precision, 1);
                }
                else if (atmosphere.Equals(Atmosphere.Chaotic))
                {
                    AddOrIncrease(bonuses, ApproachTags.Analysis, 1);
                    AddOrIncrease(bonuses, ApproachTags.Concealment, 1);
                }
            }
        }

        return bonuses;
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
                .Where(t => t is StrategicTag)
                .Cast<StrategicTag>()
                .ToList();

            foreach (StrategicTag tag in environmentTags)
            {
                if (card.StrategicEffect.IsActive(tag))
                {
                    // Card has direct synergy with this environment
                    synergy += 2;

                    // Additional synergy if the effect scales with an approach the player has developed
                    int approachValue = state.EncounterTagSystem.GetApproachTagValue(card.StrategicEffect.TargetApproach);
                    if (approachValue >= 3)
                    {
                        synergy += 1;
                    }
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
            int skillLevel = playerState.PlayerSkills.GetLevelForSkill(req.SkillType);
            if (skillLevel >= req.RequiredLevel)
            {
                bonus += skillLevel - req.RequiredLevel + 1;
            }
        }

        return bonus;
    }

    private void EnsureCardTypeBalance(List<CardDefinition> viableCards, Dictionary<CardDefinition, CardViabilityScore> cardScores)
    {
        bool hasMomentumCard = viableCards.Any(c => c.EffectType == EffectTypes.Momentum);
        bool hasPressureCard = viableCards.Any(c => c.EffectType == EffectTypes.Pressure);

        if (!hasMomentumCard || !hasPressureCard)
        {
            // Add basic cards of the missing type
            List<CardDefinition> allCards = _cardRepository.GetAll();

            if (!hasMomentumCard)
            {
                CardDefinition basicMomentumCard = allCards.FirstOrDefault(c =>
                    c.EffectType == EffectTypes.Momentum && c.Tier == 1);

                if (basicMomentumCard != null)
                {
                    viableCards.Add(basicMomentumCard);
                    cardScores[basicMomentumCard] = new CardViabilityScore { IsPlayable = true, TotalScore = 20 };
                }
            }

            if (!hasPressureCard)
            {
                CardDefinition basicPressureCard = allCards.FirstOrDefault(c =>
                    c.EffectType == EffectTypes.Pressure && c.Tier == 1);

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
        HashSet<ApproachTags> usedApproaches = new HashSet<ApproachTags>();
        HashSet<FocusTags> usedFocuses = new HashSet<FocusTags>();

        // First pass: select best card, add its approach/focus to used sets
        if (viableCards.Any())
        {
            CardDefinition bestCard = viableCards
                .OrderBy(c => cardScores[c].TotalScore)
                .First();

            result.Add(bestCard);
            usedApproaches.Add(bestCard.Approach);
            usedFocuses.Add(bestCard.Focus);
        }

        // Ensure we have one momentum and one pressure card
        EnsureEffectTypeDiversity(result, viableCards, cardScores, usedApproaches, usedFocuses);

        // Add cards with unique approaches
        AddCardsWithUniqueApproaches(result, viableCards, cardScores, usedApproaches, usedFocuses);

        // Add cards with unique focuses
        AddCardsWithUniqueFocuses(result, viableCards, cardScores, usedApproaches, usedFocuses);

        // If we still need cards, add highest scoring remaining cards
        while (result.Count < 4 && viableCards.Any(c => !result.Contains(c)))
        {
            CardDefinition nextCard = viableCards
                .Where(c => !result.Contains(c))
                .OrderBy(c => cardScores[c].TotalScore)
                .First();

            result.Add(nextCard);
        }

        return result;
    }

    private void EnsureEffectTypeDiversity(
        List<CardDefinition> result,
        List<CardDefinition> viableCards,
        Dictionary<CardDefinition, CardViabilityScore> cardScores,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses)
    {
        bool hasMomentumCard = result.Any(c => c.EffectType == EffectTypes.Momentum);
        bool hasPressureCard = result.Any(c => c.EffectType == EffectTypes.Pressure);

        if (!hasMomentumCard)
        {
            CardDefinition bestMomentumCard = viableCards
                .Where(c => c.EffectType == EffectTypes.Momentum && !result.Contains(c))
                .OrderBy(c => cardScores[c].TotalScore)
                .FirstOrDefault();

            if (bestMomentumCard != null)
            {
                result.Add(bestMomentumCard);
                usedApproaches.Add(bestMomentumCard.Approach);
                usedFocuses.Add(bestMomentumCard.Focus);
            }
        }

        if (!hasPressureCard)
        {
            CardDefinition bestPressureCard = viableCards
                .Where(c => c.EffectType == EffectTypes.Pressure && !result.Contains(c))
                .OrderBy(c => cardScores[c].TotalScore)
                .FirstOrDefault();

            if (bestPressureCard != null)
            {
                result.Add(bestPressureCard);
                usedApproaches.Add(bestPressureCard.Approach);
                usedFocuses.Add(bestPressureCard.Focus);
            }
        }
    }

    private void AddCardsWithUniqueApproaches(
        List<CardDefinition> result,
        List<CardDefinition> viableCards,
        Dictionary<CardDefinition, CardViabilityScore> cardScores,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses)
    {
        while (result.Count < 3 && viableCards.Any(c => !result.Contains(c) && !usedApproaches.Contains(c.Approach)))
        {
            CardDefinition card = viableCards
                .Where(c => !result.Contains(c) && !usedApproaches.Contains(c.Approach))
                .OrderBy(c => cardScores[c].TotalScore)
                .First();

            result.Add(card);
            usedApproaches.Add(card.Approach);
            usedFocuses.Add(card.Focus);
        }
    }

    private void AddCardsWithUniqueFocuses(
        List<CardDefinition> result,
        List<CardDefinition> viableCards,
        Dictionary<CardDefinition, CardViabilityScore> cardScores,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses)
    {
        while (result.Count < 4 && viableCards.Any(c => !result.Contains(c) && !usedFocuses.Contains(c.Focus)))
        {
            CardDefinition card = viableCards
                .Where(c => !result.Contains(c) && !usedFocuses.Contains(c.Focus))
                .OrderBy(c => cardScores[c].TotalScore)
                .First();

            result.Add(card);
            usedApproaches.Add(card.Approach);
            usedFocuses.Add(card.Focus);
        }
    }

    private void AddOrIncrease(Dictionary<ApproachTags, int> dict, ApproachTags key, int amount)
    {
        if (dict.ContainsKey(key))
            dict[key] += amount;
        else
            dict[key] = amount;
    }
}

public class CardViabilityScore
{
    public int PositionalScore { get; set; } // Lower is better
    public int SituationalValue { get; set; } // Higher is better
    public int EnvironmentalSynergy { get; set; } // Higher is better
    public int SkillBonus { get; set; } // Higher is better
    public int TotalScore { get; set; } // Lower is better
    public bool IsPlayable { get; set; }
}