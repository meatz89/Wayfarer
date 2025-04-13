public class CardSelectionAlgorithm
{
    private readonly ChoiceCardRepository _choiceRepository;

    public CardSelectionAlgorithm(ChoiceCardRepository choiceRepository)
    {
        _choiceRepository = choiceRepository;
    }

    public List<ChoiceCard> SelectChoices(EncounterState state, PlayerState playerState, int handSize = 4)
    {
        // Get all available choices
        List<ChoiceCard> availableChoices = _choiceRepository.GetAvailableChoices(state);

        // Calculate distance for each card from current position
        Dictionary<ChoiceCard, int> cardDistances = CalculateCardDistances(availableChoices, state.EncounterTagSystem);

        // Sort cards by distance (closest first)
        List<ChoiceCard> sortedChoices = availableChoices
            .OrderBy(c => cardDistances[c])
            .ToList();

        // Apply tiebreakers for cards with the same distance
        List<ChoiceCard> selectedChoices = ApplyTiebreakers(sortedChoices, cardDistances, state);

        // Take the top 4 cards
        return selectedChoices.Take(handSize).ToList();
    }

    private Dictionary<ChoiceCard, int> CalculateCardDistances(List<ChoiceCard> choices, EncounterTagSystem tagSystem)
    {
        Dictionary<ChoiceCard, int> distances = new Dictionary<ChoiceCard, int>();

        foreach (ChoiceCard choice in choices)
        {
            // Cast to ChoiceCard to access optimal position properties
            ChoiceCard card = choice as ChoiceCard;
            if (card == null) continue;

            // Get current position values
            int currentApproachValue = tagSystem.GetEncounterStateTagValue(card.Approach);
            int currentFocusValue = tagSystem.GetFocusTagValue(card.Focus);

            // Calculate Manhattan distance
            int distance = 
                //Math.Abs(currentApproachValue - card.OptimalApproachValue) +
                          Math.Abs(currentFocusValue - card.OptimalFocusValue);

            distances[choice] = distance;
        }

        return distances;
    }

    private List<ChoiceCard> ApplyTiebreakers(List<ChoiceCard> sortedChoices,
                                         Dictionary<ChoiceCard, int> distances,
                                         EncounterState state)
    {
        // Group cards by distance
        var groupedByDistance = sortedChoices
            .GroupBy(c => distances[c])
            .OrderBy(g => g.Key)
            .ToList();

        List<ChoiceCard> finalSelection = new List<ChoiceCard>();

        // Process each distance group
        foreach (var group in groupedByDistance)
        {
            // If adding all cards in this group would exceed 4, apply tiebreakers
            if (finalSelection.Count + group.Count() > 4)
            {
                var remainingSlots = 4 - finalSelection.Count;
                var cardsToAdd = ApplyTiebreakersToGroup(group.ToList(), remainingSlots, finalSelection, state);
                finalSelection.AddRange(cardsToAdd);
                break;
            }
            else
            {
                // Add all cards in this distance group
                finalSelection.AddRange(group);
            }

            // If we have 4 cards, stop processing
            if (finalSelection.Count >= 4)
                break;
        }

        return finalSelection;
    }

    private List<ChoiceCard> ApplyTiebreakersToGroup(List<ChoiceCard> cards, int slots,
                                                List<ChoiceCard> alreadySelected,
                                                EncounterState state)
    {
        List<ChoiceCard> result = new List<ChoiceCard>();

        // Ensure at least one momentum-building and one pressure-reducing card
        bool needsMomentumCard = !alreadySelected.Any(c => c.EffectType == EffectTypes.Momentum);
        bool needsPressureCard = !alreadySelected.Any(c => c.EffectType == EffectTypes.Pressure);

        if (needsMomentumCard)
        {
            ChoiceCard momentumCard = cards
                .Where(c => c.EffectType == EffectTypes.Momentum)
                .OrderByDescending(c => c.Tier)
                .FirstOrDefault();

            if (momentumCard != null)
            {
                result.Add(momentumCard);
                cards.Remove(momentumCard);
                slots--;
            }
        }

        if (needsPressureCard && slots > 0)
        {
            ChoiceCard pressureCard = cards
                .Where(c => c.EffectType == EffectTypes.Pressure)
                .OrderByDescending(c => c.Tier)
                .FirstOrDefault();

            if (pressureCard != null)
            {
                result.Add(pressureCard);
                cards.Remove(pressureCard);
                slots--;
            }
        }

        // Higher tier cards take precedence
        if (slots > 0)
        {
            var groupedByTier = cards
                .GroupBy(c => c.Tier)
                .OrderByDescending(g => g.Key)
                .ToList();

            foreach (var tierGroup in groupedByTier)
            {
                // Prioritize approach diversity within each tier
                Dictionary<ApproachTags, bool> approachesUsed = new Dictionary<ApproachTags, bool>();

                // Mark approaches already selected
                foreach (ChoiceCard c in alreadySelected.Concat(result))
                {
                    ApproachTags approach = c.Approach;
                    approachesUsed[approach] = true;
                }

                // First add cards with unused approaches
                foreach (ChoiceCard card in tierGroup.OrderBy(c => c.GetHashCode())) // Deterministic ordering
                {
                    ApproachTags approach = card.Approach;

                    if (!approachesUsed.ContainsKey(approach) || !approachesUsed[approach])
                    {
                        result.Add(card);
                        approachesUsed[approach] = true;
                        slots--;

                        if (slots <= 0) break;
                    }
                }

                // If we still have slots, add remaining cards in this tier
                if (slots > 0)
                {
                    foreach (ChoiceCard card in tierGroup.Where(c => !result.Contains(c))
                                                .OrderBy(c => c.GetHashCode()))
                    {
                        result.Add(card);
                        slots--;

                        if (slots <= 0) break;
                    }
                }

                if (slots <= 0) break;
            }
        }

        return result;
    }
}