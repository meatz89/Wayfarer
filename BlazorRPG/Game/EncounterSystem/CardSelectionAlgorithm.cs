public class CardSelectionAlgorithm
{
    private readonly CardRepository _cardRepository;
    private readonly Random _random; // Used for weighted randomization

    public CardSelectionAlgorithm(CardRepository cardRepository)
    {
        _cardRepository = cardRepository;
        _random = new Random();
    }

    public List<CardDefinition> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get all available cards
        List<CardDefinition> allCards = _cardRepository.GetForEncounter(state);

        // Calculate positions based on player skills and encounter state
        Dictionary<ApproachTags, int> approachPositions = CalculateApproachPositions(state, playerState);
        Dictionary<FocusTags, int> focusPositions = CalculateFocusPositions(state, playerState);

        // Filter playable cards and calculate their distances
        List<CardDefinition> playableCards = new List<CardDefinition>();
        Dictionary<CardDefinition, int> cardDistances = new Dictionary<CardDefinition, int>();

        foreach (CardDefinition card in allCards)
        {
            // Get position values for this card's approach and focus
            int approachValue = approachPositions.GetValueOrDefault(card.Approach, 0);
            int focusValue = focusPositions.GetValueOrDefault(card.Focus, 0);

            // Check if card is playable
            bool isPlayable = true;
            if (card.Tier > 1) // Only tier 1 cards are always playable
            {
                if (approachValue < card.OptimalApproachPosition || focusValue < card.OptimalFocusPosition)
                    isPlayable = false;
            }

            if (!isPlayable)
                continue;

            playableCards.Add(card);

            // Calculate distance with a twist - introduce slight randomization
            int approachDistance = Math.Max(0, card.OptimalApproachPosition - approachValue);
            int focusDistance = Math.Max(0, card.OptimalFocusPosition - focusValue);

            // Base distance calculation
            int distance = approachDistance + focusDistance;

            // Apply tier modifier
            distance -= card.Tier * 2;

            // Apply turn-based variance to avoid same cards each turn
            int turnVariance = (state.CurrentTurn * 997) % 5; // Prime number creates pseudorandom sequence
            distance += (card.Id.GetHashCode() + turnVariance) % 3; // Small variance based on card ID

            cardDistances[card] = Math.Max(0, distance);
        }

        // Apply state-based and turn-based weighting
        ApplyDynamicWeighting(cardDistances, state);

        // Select cards using a dynamic method based on turn number
        List<CardDefinition> cardDefinitions = SelectDynamicHand(playableCards, cardDistances, state.CurrentTurn, 4);
        return cardDefinitions;
    }

    private Dictionary<ApproachTags, int> CalculateApproachPositions(EncounterState state, PlayerState playerState)
    {
        Dictionary<ApproachTags, int> positions = new Dictionary<ApproachTags, int>();

        // Add skill-based positions
        foreach (SkillApproachMapping mapping in SkillTagMappings.ApproachMappings)
        {
            int skillLevel = playerState.PlayerSkills.GetLevelForSkill(mapping.SkillType);
            int bonus = (int)(skillLevel * mapping.Multiplier);

            if (positions.ContainsKey(mapping.ApproachTag))
                positions[mapping.ApproachTag] += bonus;
            else
                positions[mapping.ApproachTag] = bonus;
        }

        // Add encounter state positions
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (approach != ApproachTags.None)
            {
                int stateValue = state.EncounterTagSystem.GetApproachTagValue(approach);
                if (positions.ContainsKey(approach))
                    positions[approach] += stateValue;
                else
                    positions[approach] = stateValue;
            }
        }

        return positions;
    }

    private Dictionary<FocusTags, int> CalculateFocusPositions(EncounterState state, PlayerState playerState)
    {
        Dictionary<FocusTags, int> positions = new Dictionary<FocusTags, int>();

        // Add skill-based positions
        foreach (SkillFocusMapping mapping in SkillTagMappings.FocusMappings)
        {
            int skillLevel = playerState.PlayerSkills.GetLevelForSkill(mapping.SkillType);
            int bonus = (int)(skillLevel * mapping.Multiplier);

            if (positions.ContainsKey(mapping.FocusTag))
                positions[mapping.FocusTag] += bonus;
            else
                positions[mapping.FocusTag] = bonus;
        }

        // Add encounter state positions
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            int stateValue = state.EncounterTagSystem.GetFocusTagValue(focus);
            if (positions.ContainsKey(focus))
                positions[focus] += stateValue;
            else
                positions[focus] = stateValue;
        }

        return positions;
    }

    private void ApplyDynamicWeighting(Dictionary<CardDefinition, int> distances, EncounterState state)
    {
        // State-based weighting (unchanged)
        if ((double)state.Pressure / state.EncounterInfo.MaxPressure > 0.6)
        {
            foreach (CardDefinition card in distances.Keys.Where(c => c.EffectType == EffectTypes.Pressure).ToList())
            {
                distances[card] = Math.Max(0, distances[card] - 4);
            }
        }

        if (state.CurrentTurn >= state.EncounterInfo.MaxTurns - 2 &&
            (double)state.Momentum / state.EncounterInfo.StandardThreshold < 0.7)
        {
            foreach (CardDefinition card in distances.Keys.Where(c => c.EffectType == EffectTypes.Momentum).ToList())
            {
                distances[card] = Math.Max(0, distances[card] - 4);
            }
        }

        // Dynamic weighting by turn number (creates variety between turns)
        int turnPhase = state.CurrentTurn % 4; // Creates a 4-turn cycle

        foreach (CardDefinition card in distances.Keys.ToList())
        {
            // Phase 0: Favor Analysis and Precision
            if (turnPhase == 0 && (card.Approach == ApproachTags.Analysis || card.Approach == ApproachTags.Precision))
                distances[card] = Math.Max(0, distances[card] - 3);

            // Phase 1: Favor Dominance and Rapport
            else if (turnPhase == 1 && (card.Approach == ApproachTags.Dominance || card.Approach == ApproachTags.Rapport))
                distances[card] = Math.Max(0, distances[card] - 3);

            // Phase 2: Favor Concealment and Information focus
            else if (turnPhase == 2 && (card.Approach == ApproachTags.Concealment || card.Focus == FocusTags.Information))
                distances[card] = Math.Max(0, distances[card] - 3);

            // Phase 3: Favor Resource and Relationship focus
            else if (turnPhase == 3 && (card.Focus == FocusTags.Resource || card.Focus == FocusTags.Relationship))
                distances[card] = Math.Max(0, distances[card] - 3);
        }
    }

    private List<CardDefinition> SelectDynamicHand(List<CardDefinition> playableCards, Dictionary<CardDefinition, int> distances, int turnNumber, int count)
    {
        List<CardDefinition> result = new List<CardDefinition>();
        HashSet<ApproachTags> usedApproaches = new HashSet<ApproachTags>();
        HashSet<FocusTags> usedFocuses = new HashSet<FocusTags>();

        // Group cards by approach and focus for easier selection
        Dictionary<ApproachTags, List<CardDefinition>> approachGroups = playableCards
            .GroupBy(c => c.Approach)
            .ToDictionary(g => g.Key, g => g.ToList());

        Dictionary<FocusTags, List<CardDefinition>> focusGroups = playableCards
            .GroupBy(c => c.Focus)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Use different selection strategies based on turn parity for variety
        if (turnNumber % 2 == 0)
        {
            // Even turns: Select using approach diversity first
            result = SelectByApproachDiversity(approachGroups, distances, usedApproaches, usedFocuses, count);
        }
        else
        {
            // Odd turns: Select using focus diversity first
            result = SelectByFocusDiversity(focusGroups, distances, usedApproaches, usedFocuses, count);
        }

        // If we still need more cards, select from remaining using a weighted random approach
        if (result.Count < count)
        {
            List<CardDefinition> remainingCards = playableCards
                .Where(c => !result.Contains(c))
                .OrderByDescending(c => c.Tier)
                .ToList();

            result.AddRange(SelectWeightedRandom(remainingCards, distances, usedApproaches, usedFocuses, count - result.Count));
        }

        return result;
    }

    private List<CardDefinition> SelectByApproachDiversity(
        Dictionary<ApproachTags, List<CardDefinition>> approachGroups,
        Dictionary<CardDefinition, int> distances,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses,
        int count)
    {
        List<CardDefinition> result = new List<CardDefinition>();

        // Select best card from each approach group
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)).Cast<ApproachTags>())
        {
            if (approach == ApproachTags.None || result.Count >= count)
                continue;

            if (approachGroups.TryGetValue(approach, out List<CardDefinition> groupCards))
            {
                // Select the best card from this approach group
                CardDefinition selected = SelectBestCardFromFocuses(groupCards, distances, usedFocuses);

                if (selected != null)
                {
                    result.Add(selected);
                    usedApproaches.Add(approach);
                    usedFocuses.Add(selected.Focus);
                }
            }
        }

        return result;
    }

    private List<CardDefinition> SelectByFocusDiversity(
        Dictionary<FocusTags, List<CardDefinition>> focusGroups,
        Dictionary<CardDefinition, int> distances,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses,
        int count)
    {
        List<CardDefinition> result = new List<CardDefinition>();

        // Select best card from each focus group
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)).Cast<FocusTags>())
        {
            if (result.Count >= count)
                break;

            if (focusGroups.TryGetValue(focus, out List<CardDefinition> groupCards))
            {
                // Select the best card from this focus group
                CardDefinition selected = SelectBestCardFromApproaches(groupCards, distances, usedApproaches);

                if (selected != null)
                {
                    result.Add(selected);
                    usedApproaches.Add(selected.Approach);
                    usedFocuses.Add(focus);
                }
            }
        }

        return result;
    }

    private CardDefinition SelectBestCardFromApproaches(
        List<CardDefinition> groupCards,
        Dictionary<CardDefinition, int> distances,
        HashSet<ApproachTags> usedTags)
    {
        // First try cards that don't conflict with existing selections
        List<CardDefinition> candidates = groupCards
            .Where(c => !usedTags.Contains(c.Approach))
            .OrderBy(c => distances[c])
            .ThenByDescending(c => c.Tier)
            .ToList();

        // If no perfect candidates, accept some overlap
        if (!candidates.Any())
        {
            candidates = groupCards
                .OrderBy(c => distances[c])
                .ThenByDescending(c => c.Tier)
                .ToList();
        }

        return candidates.FirstOrDefault();
    }

    private CardDefinition SelectBestCardFromFocuses(
        List<CardDefinition> groupCards,
        Dictionary<CardDefinition, int> distances,
        HashSet<FocusTags> usedTags)
    {
        // First try cards that don't conflict with existing selections
        List<CardDefinition> candidates = groupCards
            .Where(c => !usedTags.Contains(c.Focus))
            .OrderBy(c => distances[c])
            .ThenByDescending(c => c.Tier)
            .ToList();

        // If no perfect candidates, accept some overlap
        if (!candidates.Any())
        {
            candidates = groupCards
                .OrderBy(c => distances[c])
                .ThenByDescending(c => c.Tier)
                .ToList();
        }

        return candidates.FirstOrDefault();
    }

    private List<CardDefinition> SelectWeightedRandom(
        List<CardDefinition> cards,
        Dictionary<CardDefinition, int> distances,
        HashSet<ApproachTags> usedApproaches,
        HashSet<FocusTags> usedFocuses,
        int count)
    {
        if (!cards.Any() || count <= 0)
            return new List<CardDefinition>();

        List<CardDefinition> result = new List<CardDefinition>();

        // Create cards with weights
        List<(CardDefinition Card, int Weight)> weightedCards = new List<(CardDefinition, int)>();

        foreach (CardDefinition card in cards)
        {
            // Base weight - inverse of distance (closer = higher weight)
            int distance = distances[card];
            int weight = Math.Max(1, 20 - distance);

            // Bonus for higher tier cards
            weight += card.Tier * 5;

            // Bonus for unique approach/focus
            if (!usedApproaches.Contains(card.Approach))
                weight += 10;

            if (!usedFocuses.Contains(card.Focus))
                weight += 10;

            weightedCards.Add((card, weight));
        }

        // Select cards using weighted probability
        for (int i = 0; i < count; i++)
        {
            if (!weightedCards.Any())
                break;

            // Calculate total weight
            int totalWeight = weightedCards.Sum(w => w.Weight);

            // Select a random value within the total weight
            int randomValue = _random.Next(totalWeight);

            // Find the selected card
            int currentWeight = 0;
            CardDefinition selected = null;

            foreach ((CardDefinition card, int weight) in weightedCards)
            {
                currentWeight += weight;
                if (randomValue < currentWeight)
                {
                    selected = card;
                    break;
                }
            }

            // If no card was selected (shouldn't happen), pick the first one
            if (selected == null && weightedCards.Any())
                selected = weightedCards.First().Card;

            if (selected != null)
            {
                result.Add(selected);
                usedApproaches.Add(selected.Approach);
                usedFocuses.Add(selected.Focus);

                // Remove the selected card from consideration
                weightedCards.RemoveAll(w => w.Card == selected);
            }
        }

        return result;
    }
}