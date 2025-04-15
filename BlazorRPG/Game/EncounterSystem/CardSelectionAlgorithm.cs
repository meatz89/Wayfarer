public class CardSelectionAlgorithm
{
    private readonly CardRepository _choiceRepository;

    public CardSelectionAlgorithm(CardRepository choiceRepository)
    {
        _choiceRepository = choiceRepository;
    }

    public List<ChoiceCard> SelectChoices(EncounterState state, PlayerState playerState, int handSize = 4)
    {
        // Get all available choices
        List<ChoiceCard> allCards = _choiceRepository.GetAvailableChoices(state);
        List<ChoiceCard> playableCards = FilterPlayableCards(state, allCards);
        
        // Calculate distances
        Dictionary<ChoiceCard, int> cardDistances = CalculateCardDistances(playableCards, state.ActiveTags, state.EncounterTagSystem, state);

        // Apply state-based weighting
        ApplyStateBasedWeighting(cardDistances, state);

        // Apply tier-based weighting to prioritize higher tier cards of same focus type
        ApplyTierBasedWeighting(cardDistances, playableCards);

        // Create turn-based card pools to ensure natural rotation
        List<ChoiceCard> primaryPool = new List<ChoiceCard>();
        List<ChoiceCard> secondaryPool = new List<ChoiceCard>();

        // Divide cards between pools based on turn number
        foreach (ChoiceCard card in playableCards)
        {
            // Alternate which attributes we prioritize each turn
            if (state.CurrentTurn % 2 == 0)
            {
                // Even turns: prioritize by approach
                if (card.Approach == GetPriorityApproach(state, state.CurrentTurn))
                    primaryPool.Add(card);
                else
                    secondaryPool.Add(card);
            }
            else
            {
                // Odd turns: prioritize by focus
                if (card.Focus == GetPriorityFocus(state, state.CurrentTurn))
                    primaryPool.Add(card);
                else
                    secondaryPool.Add(card);
            }
        }

        // Sort each pool by distance and ensure higher tier cards are prioritized
        List<ChoiceCard> sortedPrimaryPool = SortPoolByDistanceAndTier(primaryPool, cardDistances);
        List<ChoiceCard> sortedSecondaryPool = SortPoolByDistanceAndTier(secondaryPool, cardDistances);

        // Select from pools with diversity constraints
        List<ChoiceCard> result = new List<ChoiceCard>();

        // First 2-3 cards from primary pool (with diversity)
        result.AddRange(SelectDiverseCards(sortedPrimaryPool, Math.Min(3, sortedPrimaryPool.Count), new List<ChoiceCard>()));

        // Remaining cards from secondary pool (with diversity)
        int remainingSlots = handSize - result.Count;
        if (remainingSlots > 0 && sortedSecondaryPool.Any())
        {
            result.AddRange(SelectDiverseCards(sortedSecondaryPool, remainingSlots, result));
        }

        // If we still don't have enough cards, add from primary pool
        remainingSlots = handSize - result.Count;
        if (remainingSlots > 0)
        {
            foreach (ChoiceCard card in sortedPrimaryPool)
            {
                if (!result.Contains(card))
                {
                    result.Add(card);
                    remainingSlots--;

                    if (remainingSlots <= 0) break;
                }
            }
        }

        // Final pass: ensure tier prioritization within each focus group
        result = EnforceTierPrioritization(result, playableCards);

        return result;
    }

    private static List<ChoiceCard> FilterPlayableCards(EncounterState state, List<ChoiceCard> allCards)
    {
        List<ChoiceCard> availableChoices = new List<ChoiceCard>();
        foreach (ChoiceCard choiceCard in allCards)
        {

            // Get current position values
            int currentFocusValue = state.EncounterTagSystem.GetFocusTagValue(choiceCard.Focus);

            // Apply narrative tag influence on optimal focus value
            int optimalFocusValue = choiceCard.OptimalFocusValue;
            List<NarrativeTag> narrativeTags = state.ActiveTags.Where(tag => tag is NarrativeTag).Select(t => (NarrativeTag)t).ToList();
            foreach (NarrativeTag tag in narrativeTags)
            {
                if (tag.AffectedFocus == choiceCard.Focus)
                {
                    optimalFocusValue += tag.RequirementChange;
                }
            }

            bool canNotBePlayed = choiceCard.Tier != CardTiers.Novice && currentFocusValue < optimalFocusValue; // Card can not be picked yet
            if (!canNotBePlayed) availableChoices.Add(choiceCard);
        }

        return availableChoices;
    }

    // NEW: Sort pool by both distance and tier to ensure higher tier cards come first
    private List<ChoiceCard> SortPoolByDistanceAndTier(List<ChoiceCard> pool, Dictionary<ChoiceCard, int> distances)
    {
        // First group by focus tag
        var focusGroups = pool.GroupBy(c => c.Focus).ToList();
        List<ChoiceCard> sortedPool = new List<ChoiceCard>();

        // For each focus group, ensure higher tiers come before lower tiers at same distance
        foreach (var focusGroup in focusGroups)
        {
            // First sort by distance
            var sortedGroup = focusGroup.OrderBy(c => distances[c]).ToList();

            // Then for cards with same distance, ensure tier ordering
            int currentDistance = -1;
            List<ChoiceCard> sameDistanceCards = new List<ChoiceCard>();

            foreach (var card in sortedGroup)
            {
                if (distances[card] != currentDistance)
                {
                    // Process any cards at the previous distance
                    if (sameDistanceCards.Any())
                    {
                        // Sort by tier and add to result
                        sortedPool.AddRange(sameDistanceCards.OrderByDescending(c => c.Tier));
                        sameDistanceCards.Clear();
                    }

                    currentDistance = distances[card];
                }

                sameDistanceCards.Add(card);
            }

            // Handle the last group
            if (sameDistanceCards.Any())
            {
                sortedPool.AddRange(sameDistanceCards.OrderByDescending(c => c.Tier));
            }
        }

        return sortedPool;
    }

    // Apply additional weighting based on card tier
    private void ApplyTierBasedWeighting(Dictionary<ChoiceCard, int> distances, List<ChoiceCard> cards)
    {
        // Group cards by focus
        var focusGroups = cards.GroupBy(c => c.Focus).ToList();

        foreach (var focusGroup in focusGroups)
        {
            // Find highest tier card for this focus
            int highestTier = focusGroup.Max(c => (int)c.Tier);

            // Apply distance reduction proportional to tier
            foreach (var card in focusGroup)
            {
                // Higher tier cards get more distance reduction
                int tierBonus = (int)card.Tier * 2;
                distances[card] = Math.Max(0, distances[card] - tierBonus);
            }
        }
    }

    // Ensure higher tier cards are prioritized within focus groups
    private List<ChoiceCard> EnforceTierPrioritization(List<ChoiceCard> selectedCards, List<ChoiceCard> allCards)
    {
        // Group selected cards by focus
        var focusGroups = selectedCards.GroupBy(c => c.Focus).ToList();
        List<ChoiceCard> result = new List<ChoiceCard>();

        foreach (var focusGroup in focusGroups)
        {
            // Check if we have a suboptimal selection
            if (focusGroup.Count() > 1)
            {
                // Find the highest tier card in this focus group
                ChoiceCard highestTierCard = focusGroup.OrderByDescending(c => c.Tier).First();
                result.Add(highestTierCard);

                // Only add one card per focus group initially
                continue;
            }

            // If only one card in the focus group, just add it
            result.AddRange(focusGroup);
        }

        // Fill remaining slots with diverse approaches
        int remainingSlots = 4 - result.Count;
        if (remainingSlots > 0)
        {
            // Get unused approaches
            var usedApproaches = result.Select(c => c.Approach).ToHashSet();
            var remainingCards = selectedCards.Where(c => !result.Contains(c))
                                            .OrderByDescending(c => c.Tier)
                                            .ToList();

            // First add cards with unused approaches
            foreach (var card in remainingCards)
            {
                if (!usedApproaches.Contains(card.Approach))
                {
                    result.Add(card);
                    usedApproaches.Add(card.Approach);
                    remainingSlots--;

                    if (remainingSlots <= 0) break;
                }
            }

            // Then add any remaining highest tier cards
            if (remainingSlots > 0)
            {
                result.AddRange(remainingCards.Take(remainingSlots));
            }
        }

        return result;
    }

    private Dictionary<ChoiceCard, int> CalculateCardDistances(List<ChoiceCard> choices, List<IEncounterTag> activeTags, EncounterTagSystem tagSystem, EncounterState state)
    {
        // Extract narrative tags
        List<NarrativeTag> narrativeTags = activeTags.Where(tag => tag is NarrativeTag).Select(t => (NarrativeTag)t).ToList();
        Dictionary<ChoiceCard, int> distances = new Dictionary<ChoiceCard, int>();

        // Get encounter state variables that will influence distance
        double pressureRatio = (double)state.Pressure / state.EncounterInfo.MaxPressure;
        double momentumRatio = (double)state.Momentum / state.EncounterInfo.StandardThreshold;
        int turnNumber = state.CurrentTurn;

        foreach (ChoiceCard card in choices)
        {
            // Get current position values
            int currentFocusValue = tagSystem.GetFocusTagValue(card.Focus);

            // Apply narrative tag influence on optimal focus value
            int optimalFocusValue = card.OptimalFocusValue;
            foreach (NarrativeTag tag in narrativeTags)
            {
                if (tag.AffectedFocus == card.Focus)
                {
                    optimalFocusValue += tag.RequirementChange;
                }
            }

            bool canNotBePlayed = card.Tier != CardTiers.Novice && currentFocusValue < optimalFocusValue; // Card can not be picked yet
            if(canNotBePlayed) { continue; }

            // Base distance calculation with narrative tag influence
            int baseDistance = Math.Abs(currentFocusValue - optimalFocusValue);
            int dynamicDistance = baseDistance;

            dynamicDistance = CalculateDynamicDistance(tagSystem, pressureRatio, momentumRatio, turnNumber, card, dynamicDistance);
            distances[card] = Math.Max(0, dynamicDistance); // Never negative
        }

        return distances;
    }

    private static int CalculateDynamicDistance(EncounterTagSystem tagSystem, double pressureRatio, double momentumRatio, int turnNumber, ChoiceCard card, int dynamicDistance)
    {
        // Dynamic modifiers that change between turns
        // Adjust distance based on current pressure/momentum
        if (card.EffectType == EffectTypes.Momentum && momentumRatio < 0.5)
            dynamicDistance -= 2; // Favor momentum cards when momentum is low
        else if (card.EffectType == EffectTypes.Pressure && pressureRatio > 0.5)
            dynamicDistance -= 2; // Favor pressure cards when pressure is high

        // Adjust based on turn number (creates natural progression)
        if (card.Tier > CardTiers.Novice && turnNumber > 2)
            dynamicDistance -= (turnNumber / 2); // Higher tier cards become more attractive as encounter progresses

        // Add small oscillation based on turn number (creates natural variety)
        dynamicDistance += (turnNumber % 2 == 0) ? 1 : -1;

        // Approach affinity impact 
        ApproachTags approach = card.Approach;
        int approachValue = tagSystem.GetEncounterStateTagValue(approach);
        if (approachValue > 3)
            dynamicDistance -= 1; // Favor approaches the player is building

        // Apply tier-based distance reduction
        dynamicDistance -= (int)card.Tier; // Higher tier cards are closer
        return dynamicDistance;
    }

    private ApproachTags GetPriorityApproach(EncounterState state, int turnNumber)
    {
        // Same implementation as before
        var approaches = Enum.GetValues(typeof(ApproachTags))
                            .Cast<ApproachTags>()
                            .Where(a => a != ApproachTags.None)
                            .ToList();

        return approaches[turnNumber % approaches.Count];
    }

    private FocusTags GetPriorityFocus(EncounterState state, int turnNumber)
    {
        // Same implementation as before
        var focuses = Enum.GetValues(typeof(FocusTags))
                         .Cast<FocusTags>()
                         .ToList();

        return focuses[(turnNumber / 2) % focuses.Count];
    }

    private List<ChoiceCard> SelectDiverseCards(List<ChoiceCard> pool, int count, List<ChoiceCard> existingSelection)
    {
        // Modified to prioritize higher tier cards within each focus group
        List<ChoiceCard> result = new List<ChoiceCard>();
        HashSet<ApproachTags> usedApproaches = new HashSet<ApproachTags>();
        HashSet<FocusTags> usedFocuses = new HashSet<FocusTags>();

        // Track what's already selected
        foreach (ChoiceCard card in existingSelection)
        {
            usedApproaches.Add(card.Approach);
            usedFocuses.Add(card.Focus);
        }

        // Group cards by focus first
        var focusGroups = pool.GroupBy(c => c.Focus)
                             .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.Tier).ToList());

        // First pass: add highest tier card from each unused focus
        foreach (var focusGroup in focusGroups)
        {
            if (!usedFocuses.Contains(focusGroup.Key) && focusGroup.Value.Any())
            {
                // Take highest tier card from this focus
                ChoiceCard highestTierCard = focusGroup.Value.First();
                result.Add(highestTierCard);
                usedFocuses.Add(focusGroup.Key);
                usedApproaches.Add(highestTierCard.Approach);

                if (result.Count >= count) break;
            }
        }

        // Second pass: add highest tier card from each unused approach
        if (result.Count < count)
        {
            // Group remaining cards by approach
            var approachGroups = pool.Where(c => !result.Contains(c))
                                    .GroupBy(c => c.Approach)
                                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.Tier).ToList());

            foreach (var approachGroup in approachGroups)
            {
                if (!usedApproaches.Contains(approachGroup.Key) && approachGroup.Value.Any())
                {
                    // Take highest tier card from this approach
                    ChoiceCard highestTierCard = approachGroup.Value.First();
                    result.Add(highestTierCard);
                    usedApproaches.Add(approachGroup.Key);
                    usedFocuses.Add(highestTierCard.Focus);

                    if (result.Count >= count) break;
                }
            }
        }

        // Final pass: add remaining highest tier cards
        if (result.Count < count)
        {
            // Get all remaining cards sorted by tier
            var remainingCards = pool.Where(c => !result.Contains(c))
                                    .OrderByDescending(c => c.Tier)
                                    .ToList();

            result.AddRange(remainingCards.Take(count - result.Count));
        }

        return result;
    }

    private void ApplyStateBasedWeighting(Dictionary<ChoiceCard, int> distances, EncounterState state)
    {
        // Same implementation as before
        if ((double)state.Pressure / state.EncounterInfo.MaxPressure > 0.7)
        {
            foreach (ChoiceCard card in distances.Keys.Where(c => c.EffectType == EffectTypes.Pressure).ToList())
            {
                distances[card] = Math.Max(0, distances[card] - 5);
            }
        }

        if (state.CurrentTurn >= state.EncounterInfo.MaxTurns - 2 &&
            (double)state.Momentum / state.EncounterInfo.StandardThreshold < 0.8)
        {
            foreach (ChoiceCard card in distances.Keys.Where(c => c.EffectType == EffectTypes.Momentum).ToList())
            {
                distances[card] = Math.Max(0, distances[card] - 5);
            }
        }
    }
}