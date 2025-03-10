namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Handles selection of choices for the player's hand each turn
    /// </summary>
    public class CardSelectionAlgorithm
    {
        private readonly ChoiceRepository _choiceRepository;

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
            List<ApproachTypes> blockedApproaches = GetBlockedApproaches(state.ActiveTags);
            availableChoices = availableChoices
                .Where(choice => !blockedApproaches.Contains(choice.Approach))
                .ToList();

            // If all approaches are blocked, add emergency choices
            if (availableChoices.Count == 0)
            {
                foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)).Cast<ApproachTypes>())
                {
                    EmergencyChoice emergencyChoice = _choiceRepository.GetEmergencyChoice(approach);
                    if (emergencyChoice != null)
                        result.Add(emergencyChoice);
                }

                return result.Take(handSize).ToList();
            }

            // 2. Calculate scores for each choice
            List<ChoiceScore> choiceScores = CalculateChoiceScores(availableChoices, state);

            // 3. Select strategic diverse hand

            // A. One momentum choice from highest-scoring approach
            ChoiceScore? highestScoringMomentum = choiceScores
            .Where(cs => cs.Choice.EffectType == EffectTypes.Momentum)
            .OrderByDescending(cs => cs.Score)
            .FirstOrDefault();

            if (highestScoringMomentum != null)
            {
                result.Add(highestScoringMomentum.Choice);
                availableChoices.Remove(highestScoringMomentum.Choice);
            }

            // B. One momentum choice from a complementary approach
            ChoiceScore? complementaryMomentum = choiceScores
            .Where(cs => cs.Choice.EffectType == EffectTypes.Momentum &&
                      cs.Choice.Approach != highestScoringMomentum?.Choice.Approach)
            .OrderByDescending(cs => cs.Score)
            .FirstOrDefault();

            if (complementaryMomentum != null)
            {
                result.Add(complementaryMomentum.Choice);
                availableChoices.Remove(complementaryMomentum.Choice);
            }

            // C. One pressure choice for tag building
            ChoiceScore? pressureChoice = choiceScores
            .Where(cs => cs.Choice.EffectType == EffectTypes.Pressure)
            .OrderByDescending(cs => cs.Score)
            .FirstOrDefault();

            if (pressureChoice != null)
            {
                result.Add(pressureChoice.Choice);
                availableChoices.Remove(pressureChoice.Choice);
            }

            // D. Special or situational choice
            IReadOnlyList<SpecialChoice> specialChoices = _choiceRepository.GetSpecialChoicesForLocation(
            state.CurrentLocation.Name, state.TagSystem);

            if (specialChoices.Count > 0)
            {
                result.Add(specialChoices.First());
            }
            else
            {
                // If no special choices, add another momentum choice
                IChoice? additionalChoice = choiceScores
                .Where(cs => !result.Contains(cs.Choice))
                .OrderByDescending(cs => cs.Score)
                .FirstOrDefault()?.Choice;

                if (additionalChoice != null)
                    result.Add(additionalChoice);
            }

            // 4. Ensure hand diversity and fill to handSize
            if (result.Count < handSize)
            {
                IOrderedEnumerable<ChoiceScore> remainingChoices = choiceScores
                .Where(cs => !result.Contains(cs.Choice))
                .OrderByDescending(cs => cs.Score);

                foreach (ChoiceScore? choiceScore in remainingChoices)
                {
                    // Check for diversity - no more than 2 of the same approach
                    if (result.Count(c => c.Approach == choiceScore.Choice.Approach) < 2)
                    {
                        result.Add(choiceScore.Choice);
                        if (result.Count >= handSize)
                            break;
                    }
                }
            }

            // 5. Apply strategic tag effects (handled by EncounterState when choices are used)

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
                if (state.CurrentLocation.FavoredApproaches.Contains(choice.Approach))
                    score += 3;
                if (state.CurrentLocation.DisfavoredApproaches.Contains(choice.Approach))
                    score -= 2;
                if (state.CurrentLocation.FavoredFocuses.Contains(choice.Focus))
                    score += 3;
                if (state.CurrentLocation.DisfavoredFocuses.Contains(choice.Focus))
                    score -= 2;

                // Tag matching bonuses
                switch (choice.Approach)
                {
                    case ApproachTypes.Force:
                        score += state.TagSystem.GetApproachTagValue(ApproachTags.Dominance);
                        break;
                    case ApproachTypes.Charm:
                        score += state.TagSystem.GetApproachTagValue(ApproachTags.Rapport);
                        break;
                    case ApproachTypes.Wit:
                        score += state.TagSystem.GetApproachTagValue(ApproachTags.Analysis);
                        break;
                    case ApproachTypes.Finesse:
                        score += state.TagSystem.GetApproachTagValue(ApproachTags.Precision);
                        break;
                    case ApproachTypes.Stealth:
                        score += state.TagSystem.GetApproachTagValue(ApproachTags.Concealment);
                        break;
                }

                // Focus matching bonus
                score += state.TagSystem.GetFocusTagValue(choice.Focus);

                scores.Add(new ChoiceScore(choice, score));
            }

            return scores;
        }

        // Get the list of approaches blocked by narrative tags
        private List<ApproachTypes> GetBlockedApproaches(List<IEncounterTag> activeTags)
        {
            List<ApproachTypes> blockedApproaches = new List<ApproachTypes>();

            foreach (IEncounterTag tag in activeTags)
            {
                if (tag is NarrativeTag narrativeTag && narrativeTag.BlockedApproach.HasValue)
                {
                    blockedApproaches.Add(narrativeTag.BlockedApproach.Value);
                }
            }

            return blockedApproaches;
        }

        // Helper class for scoring choices
        private class ChoiceScore
        {
            public IChoice Choice { get; }
            public int Score { get; }

            public ChoiceScore(IChoice choice, int score)
            {
                Choice = choice;
                Score = score;
            }
        }
    }
}
