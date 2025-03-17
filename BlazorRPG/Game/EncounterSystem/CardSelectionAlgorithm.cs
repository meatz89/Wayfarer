public class CardSelectionAlgorithm
{
    private readonly ChoiceRepository _choiceRepository;
    private readonly List<IChoice> _recentlyUsedChoices = new List<IChoice>(); // Track recent choices
    private readonly Random _random = new Random();

    public CardSelectionAlgorithm(ChoiceRepository choiceRepository)
    {
        _choiceRepository = choiceRepository;
    }

    // Select a hand of choices for the player
    public List<IChoice> SelectChoices(EncounterState state, int handSize = 4)
    {
        List<IChoice> result = new List<IChoice>();
        List<IChoice> availableChoices = new List<IChoice>(_choiceRepository.GetStandardChoices());

        // 1. Apply narrative tag filters
        List<FocusTags> blockedApproaches = GetBlockedChoices(state.ActiveTags);
        availableChoices = availableChoices
            .Where(choice => !blockedApproaches.Contains(choice.Focus))
            .ToList();

        // Remove recently used choices to enforce diversity
        availableChoices = availableChoices
            .Where(choice => !_recentlyUsedChoices.Contains(choice))
            .ToList();

        // If all approaches are blocked or too few choices remain, add emergency choices
        if (availableChoices.Count < handSize)
        {
            // Clear the recent choices list to avoid getting stuck
            _recentlyUsedChoices.Clear();

            // Re-populate available choices without the recency filter
            availableChoices = _choiceRepository.GetStandardChoices()
                .Where(choice => !blockedApproaches.Contains(choice.Focus))
                .ToList();
        }

        // 2. Calculate scores for each choice
        List<ChoiceScore> choiceScores = CalculateChoiceScores(availableChoices, state);

        // 3. Select strategic diverse hand with randomization

        // 3.A Get momentum choices for selection
        List<ChoiceScore> momentumChoices = choiceScores
            .Where(cs => cs.Choice.EffectType == EffectTypes.Momentum)
            .ToList();

        // Get a random selection from top-scoring momentum choices
        if (momentumChoices.Count > 0)
        {
            // Sort by score descending
            momentumChoices = momentumChoices.OrderByDescending(cs => cs.Score).ToList();

            // Take top 3 scoring choices (or all if fewer than 3)
            int topCount = Math.Min(3, momentumChoices.Count);
            int randomIndex = _random.Next(topCount);

            // Add a random choice from the top performers
            result.Add(momentumChoices[randomIndex].Choice);
            momentumChoices.RemoveAt(randomIndex);
            availableChoices.Remove(result[0]);
        }

        // 3.B Add a momentum choice from a different approach
        if (momentumChoices.Count > 0 && result.Count > 0)
        {
            // Filter for different approaches
            List<ChoiceScore> differentApproachChoices = momentumChoices
                .Where(cs => !result.Any(c => c.Approach == cs.Choice.Approach))
                .ToList();

            if (differentApproachChoices.Count > 0)
            {
                // Randomize selection from top scoring different approaches
                differentApproachChoices = differentApproachChoices.OrderByDescending(cs => cs.Score).ToList();
                int topCount = Math.Min(3, differentApproachChoices.Count);
                int randomIndex = _random.Next(topCount);

                result.Add(differentApproachChoices[randomIndex].Choice);
                availableChoices.Remove(result[1]);
            }
        }

        // 3.C Add a pressure choice with randomization
        List<ChoiceScore> pressureChoices = choiceScores
            .Where(cs => cs.Choice.EffectType == EffectTypes.Pressure &&
                    !result.Any(c => c.Approach == cs.Choice.Approach && c.Focus == cs.Choice.Focus))
            .OrderByDescending(cs => cs.Score)
            .ToList();

        if (pressureChoices.Count > 0)
        {
            int topCount = Math.Min(3, pressureChoices.Count);
            int randomIndex = _random.Next(topCount);

            result.Add(pressureChoices[randomIndex].Choice);
            availableChoices.Remove(result[2]);
        }

        // 3.D Special choice or another diverse option
        //IReadOnlyList<SpecialChoice> specialChoices = _choiceRepository.GetSpecialChoicesForLocation(
        //    state.Location.Name, state.TagSystem);

        //if (specialChoices.Count > 0)
        //{
        //    // Randomly select from available special choices
        //    int randomIndex = _random.Next(specialChoices.Count);
        //    result.Add(specialChoices[randomIndex]);
        //}
        //else
        //{

        // Find a choice with approach and focus not yet in the hand
        List<ChoiceScore> diverseChoices = choiceScores
            .Where(cs => !result.Contains(cs.Choice) &&
                    !result.Any(c => c.Approach == cs.Choice.Approach && c.Focus == cs.Choice.Focus))
            .OrderByDescending(cs => cs.Score)
            .ToList();

        if (diverseChoices.Count > 0)
        {
            int topCount = Math.Min(3, diverseChoices.Count);
            int randomIndex = _random.Next(topCount);

            result.Add(diverseChoices[randomIndex].Choice);
        }

        //}

        // 4. Fill the hand if needed
        while (result.Count < handSize && choiceScores.Any(cs => !result.Contains(cs.Choice)))
        {
            // Find remaining choices not in hand
            List<ChoiceScore> remainingChoices = choiceScores
                .Where(cs => !result.Contains(cs.Choice))
                .OrderByDescending(cs => cs.Score)
                .ToList();

            // Get a random choice from top scorers
            if (remainingChoices.Count > 0)
            {
                int topCount = Math.Min(3, remainingChoices.Count);
                int randomIndex = _random.Next(topCount);

                result.Add(remainingChoices[randomIndex].Choice);
            }
        }

        // 5. Update recently used choices
        _recentlyUsedChoices.AddRange(result);

        // Maintain limited history (last 8 choices)
        while (_recentlyUsedChoices.Count > 8)
        {
            _recentlyUsedChoices.RemoveAt(0);
        }

        return result;
    }

    // Calculate scores for each choice based on location preferences and current tags
    private List<ChoiceScore> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        List<ChoiceScore> scores = new List<ChoiceScore>();

        foreach (IChoice choice in choices)
        {
            int score = 10; // Base score

            // Location preference bonuses
            if (state.Location.FavoredApproaches.Contains(choice.Approach))
                score += 3;
            if (state.Location.DisfavoredApproaches.Contains(choice.Approach))
                score -= 2;

            // Tag matching bonuses
            switch (choice.Approach)
            {
                case ApproachTags.Dominance:
                    score += state.TagSystem.GetEncounterStateTagValue(ApproachTags.Dominance);
                    break;
                case ApproachTags.Rapport:
                    score += state.TagSystem.GetEncounterStateTagValue(ApproachTags.Rapport);
                    break;
                case ApproachTags.Analysis:
                    score += state.TagSystem.GetEncounterStateTagValue(ApproachTags.Analysis);
                    break;
                case ApproachTags.Precision:
                    score += state.TagSystem.GetEncounterStateTagValue(ApproachTags.Precision);
                    break;
                case ApproachTags.Concealment:
                    score += state.TagSystem.GetEncounterStateTagValue(ApproachTags.Concealment);
                    break;
            }

            // Focus matching bonus
            score += state.TagSystem.GetFocusTagValue(choice.Focus);

            scores.Add(new ChoiceScore(choice, score));
        }

        return scores;
    }

    // Get the list of approaches blocked by narrative tags
    private List<FocusTags> GetBlockedChoices(List<IEncounterTag> activeTags)
    {
        List<FocusTags> blockedApproaches = new List<FocusTags>();

        foreach (IEncounterTag tag in activeTags)
        {
            if (tag is NarrativeTag narrativeTag && narrativeTag.BlockedFocus != null)
            {
                blockedApproaches.Add(narrativeTag.BlockedFocus);
            }
        }

        return blockedApproaches;
    }
}
