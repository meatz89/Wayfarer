/// <summary>
/// Implements the deterministic choice generation algorithm from Wayfarer design
/// </summary>
public class CardSelectionAlgorithm
{
    private readonly ChoiceRepository _choiceRepository;

    public CardSelectionAlgorithm(ChoiceRepository choiceRepository)
    {
        _choiceRepository = choiceRepository;
    }

    /// <summary>
    /// Select a hand of choices for the player based on the current encounter state
    /// </summary>
    public List<IChoice> SelectChoices(EncounterState state, int handSize = 4)
    {
        List<IChoice> allChoices = new List<IChoice>(_choiceRepository.GetAllStandardChoices());

        // STEP 1: Calculate scores for all choices
        Dictionary<IChoice, int> choiceScores = CalculateChoiceScores(allChoices, state);

        // STEP 2: Categorize choices into pools
        // Pool A: By Effect Type and Strategic Alignment
        var poolA1 = GetMomentumChoicesWithPositiveAlignment(allChoices, choiceScores, state);
        var poolA2 = GetPressureChoicesWithPositiveAlignment(allChoices, choiceScores, state);
        var poolA3 = GetMomentumChoicesWithNeutralAlignment(allChoices, choiceScores, state);
        var poolA4 = GetPressureChoicesWithNeutralAlignment(allChoices, choiceScores, state);
        var poolA5 = GetMomentumChoicesWithNegativeAlignment(allChoices, choiceScores, state);
        var poolA6 = GetPressureChoicesWithNegativeAlignment(allChoices, choiceScores, state);

        // Pool B: By Approach
        var characterApproachValues = GetCharacterApproachValues(state);
        var poolB1 = GetChoicesByApproach(allChoices, choiceScores, characterApproachValues[0].Item1);
        var poolB2 = GetChoicesByApproach(allChoices, choiceScores, characterApproachValues[1].Item1);
        var poolB3 = GetChoicesByApproach(allChoices, choiceScores, characterApproachValues[2].Item1);
        var poolB4 = GetChoicesByApproach(allChoices, choiceScores, characterApproachValues[3].Item1);
        var poolB5 = GetChoicesByApproach(allChoices, choiceScores, characterApproachValues[4].Item1);

        // Pool C: By Narrative Tag Status
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);
        var poolC1 = GetUnblockedChoices(allChoices, blockedFocuses);
        var poolC2 = GetBlockedChoices(allChoices, blockedFocuses);

        // STEP 3: Select Initial Choices
        List<IChoice> selectedChoices = new List<IChoice>();

        // First Choice: Character Strength
        IChoice firstChoice = null;
        if (poolB1.Any())
        {
            firstChoice = GetHighestScoringChoice(poolB1, choiceScores);
        }
        else if (poolB2.Any())
        {
            firstChoice = GetHighestScoringChoice(poolB2, choiceScores);
        }

        if (firstChoice != null)
        {
            selectedChoices.Add(firstChoice);
        }

        // Second Choice: Strategic Advantage
        IChoice secondChoice = null;
        if (firstChoice != null)
        {
            if (firstChoice.EffectType == EffectTypes.Momentum)
            {
                // If first choice builds momentum, add a pressure-reducing choice
                secondChoice = GetHighestScoringChoice(poolA2, choiceScores);
            }
            else
            {
                // If first choice reduces pressure, add a momentum-building choice
                secondChoice = GetHighestScoringChoice(poolA1, choiceScores);
            }

            // If the primary list is empty, try subsequent lists in sequence
            if (secondChoice == null)
            {
                secondChoice = GetHighestScoringChoice(poolA3, choiceScores) ??
                               GetHighestScoringChoice(poolA4, choiceScores) ??
                               GetHighestScoringChoice(poolA5, choiceScores) ??
                               GetHighestScoringChoice(poolA6, choiceScores);
            }
        }

        if (secondChoice != null)
        {
            selectedChoices.Add(secondChoice);
        }

        // Third Choice: Approach Diversity
        IChoice thirdChoice = null;
        if (selectedChoices.Count >= 2)
        {
            // Create a list of choices with approaches different from the first two
            var approachesToExclude = selectedChoices.Select(c => c.GetPrimaryApproach()).Distinct().ToList();
            var diverseApproachChoices = allChoices.Where(c => !approachesToExclude.Contains(c.GetPrimaryApproach())).ToList();

            if (diverseApproachChoices.Any())
            {
                thirdChoice = GetHighestScoringChoice(diverseApproachChoices, choiceScores);
            }
            else
            {
                // If no diverse approach choices, select highest from general pools
                thirdChoice = GetHighestScoringChoice(poolA1, choiceScores) ??
                              GetHighestScoringChoice(poolA2, choiceScores) ??
                              GetHighestScoringChoice(poolA3, choiceScores) ??
                              GetHighestScoringChoice(poolA4, choiceScores) ??
                              GetHighestScoringChoice(poolA5, choiceScores) ??
                              GetHighestScoringChoice(poolA6, choiceScores);
            }
        }

        if (thirdChoice != null)
        {
            selectedChoices.Add(thirdChoice);
        }

        // Fourth Choice: Focus Diversity or Narrative Tag Impact
        IChoice fourthChoice = null;
        int blockedChoicesInHandNumber = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));

        if (state.CurrentTurn % 2 == 1 || blockedChoicesInHandNumber < 2)
        {
            // On odd turns OR if fewer than 2 choices are blocked, try to include a blocked choice
            if (blockedFocuses.Any() && poolC2.Any())
            {
                fourthChoice = GetHighestScoringChoice(poolC2, choiceScores);
            }
            else
            {
                // Otherwise, just get highest scoring not yet selected
                fourthChoice = GetHighestScoringChoice(
                    allChoices.Where(c => !selectedChoices.Contains(c)).ToList(),
                    choiceScores);
            }
        }
        else
        {
            // On even turns AND 2 choices already blocked, get unblocked
            fourthChoice = GetHighestScoringChoice(
                poolC1.Where(c => !selectedChoices.Contains(c)).ToList(),
                choiceScores);
        }

        if (fourthChoice != null)
        {
            selectedChoices.Add(fourthChoice);
        }

        // STEP 4: Validate Hand Composition
        // Ensure Viable Choices Rule - no more than 2 blocked choices
        var blockedChoicesCount = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));
        if (blockedChoicesCount > 2)
        {
            // Get lowest scoring blocked choice
            var blockedChoicesInHand = selectedChoices.Where(c => blockedFocuses.Contains(c.Focus)).ToList();
            var lowestScoringBlocked = GetLowestScoringChoice(blockedChoicesInHand, choiceScores);

            // Remove it
            selectedChoices.Remove(lowestScoringBlocked);

            // Add highest scoring unblocked choice not in hand
            var unblockedNotInHand = poolC1.Where(c => !selectedChoices.Contains(c)).ToList();
            var replacementChoice = GetHighestScoringChoice(unblockedNotInHand, choiceScores);
            if (replacementChoice != null)
            {
                selectedChoices.Add(replacementChoice);
            }
        }

        // Guarantee Strategic Options Rule - ensure mix of momentum and pressure if possible
        var unblockedChoices = selectedChoices.Where(c => !blockedFocuses.Contains(c.Focus)).ToList();
        var allUnblockedBuildMomentum = unblockedChoices.All(c => c.EffectType == EffectTypes.Momentum);
        var allUnblockedReducePressure = unblockedChoices.All(c => c.EffectType == EffectTypes.Pressure);

        if (allUnblockedBuildMomentum)
        {
            // All unblocked choices build momentum, need pressure
            var lowestMomentumChoice = GetLowestScoringChoice(
                unblockedChoices.Where(c => c.EffectType == EffectTypes.Momentum).ToList(),
                choiceScores);

            if (lowestMomentumChoice != null)
            {
                selectedChoices.Remove(lowestMomentumChoice);

                // Add highest scoring pressure choice not in hand
                var pressureChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure && !selectedChoices.Contains(c))
                    .ToList();

                var replacementChoice = GetHighestScoringChoice(pressureChoicesNotInHand, choiceScores);
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }
        else if (allUnblockedReducePressure)
        {
            // All unblocked choices reduce pressure, need momentum
            var lowestPressureChoice = GetLowestScoringChoice(
                unblockedChoices.Where(c => c.EffectType == EffectTypes.Pressure).ToList(),
                choiceScores);

            if (lowestPressureChoice != null)
            {
                selectedChoices.Remove(lowestPressureChoice);

                // Add highest scoring momentum choice not in hand
                var momentumChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum && !selectedChoices.Contains(c))
                    .ToList();

                var replacementChoice = GetHighestScoringChoice(momentumChoicesNotInHand, choiceScores);
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        // Character Identity Rule - ensure highest approach is represented
        var highestApproach = characterApproachValues[0].Item1;
        bool hasHighestApproachChoice = selectedChoices.Any(c => c.GetPrimaryApproach() == highestApproach);

        if (!hasHighestApproachChoice)
        {
            // Remove lowest scoring choice
            var lowestScoringChoice = GetLowestScoringChoice(selectedChoices, choiceScores);
            selectedChoices.Remove(lowestScoringChoice);

            // Add highest scoring choice using character's highest approach
            var highestApproachChoices = poolB1.Where(c => !selectedChoices.Contains(c)).ToList();
            var replacementChoice = GetHighestScoringChoice(highestApproachChoices, choiceScores);

            if (replacementChoice != null)
            {
                selectedChoices.Add(replacementChoice);
            }
        }

        // STEP 5: Handle Edge Cases
        // Critical Pressure Rule
        double pressureRatio = (double)state.Pressure / EncounterState.MaxPressure;
        if (pressureRatio >= 0.8)
        {
            bool hasPressureReducingChoice = selectedChoices.Any(c =>
                c.EffectType == EffectTypes.Pressure &&
                IsApproachFavorableForPressure(c.GetPrimaryApproach(), state));

            if (!hasPressureReducingChoice)
            {
                // Remove lowest scoring choice
                var lowestScoringChoice = GetLowestScoringChoice(selectedChoices, choiceScores);
                selectedChoices.Remove(lowestScoringChoice);

                // Add highest scoring choice from A2 (pressure-reducing, favorable approach)
                var replacementChoice = GetHighestScoringChoice(poolA2, choiceScores);
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        // Success Within Reach Rule
        int successThreshold = state.Location.StandardThreshold;
        if ((state.Momentum + 6) >= successThreshold &&
            state.CurrentTurn >= (state.Location.TurnDuration - 2))
        {
            int momentumBuildingChoices = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);
            if (momentumBuildingChoices < 2)
            {
                // Remove lowest scoring pressure choice
                var pressureChoicesInHand = selectedChoices.Where(c => c.EffectType == EffectTypes.Pressure).ToList();
                if (pressureChoicesInHand.Any())
                {
                    var lowestPressureChoice = GetLowestScoringChoice(pressureChoicesInHand, choiceScores);
                    selectedChoices.Remove(lowestPressureChoice);

                    // Add highest momentum choice not in hand
                    var momentumChoicesNotInHand = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum && !selectedChoices.Contains(c))
                        .ToList();

                    var replacementChoice = GetHighestScoringChoice(momentumChoicesNotInHand, choiceScores);
                    if (replacementChoice != null)
                    {
                        selectedChoices.Add(replacementChoice);
                    }
                }
            }
        }

        // STEP 6: Output Finalized Hand
        // Sort final choices by type: unblocked momentum first, unblocked pressure second, blocked last
        return selectedChoices
            .OrderBy(c => blockedFocuses.Contains(c.Focus)) // Unblocked first (false comes before true)
            .ThenBy(c => c.EffectType != EffectTypes.Momentum) // Momentum first
            .ThenByDescending(c => choiceScores[c]) // Higher score first
            .ToList();
    }

    // Calculate scores for all choices
    private Dictionary<IChoice, int> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        Dictionary<IChoice, int> scores = new Dictionary<IChoice, int>();
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        foreach (IChoice choice in choices)
        {
            // 1. Strategic Alignment Score (1-6 points)
            int strategicAlignmentScore = 3; // Default neutral

            if (choice.EffectType == EffectTypes.Momentum)
            {
                if (IsMomentumIncreasingApproach(choice.GetPrimaryApproach(), state))
                    strategicAlignmentScore = 6;
                else if (IsMomentumDecreasingApproach(choice.GetPrimaryApproach(), state))
                    strategicAlignmentScore = 1;
            }
            else // Pressure
            {
                if (IsPressureDecreasingApproach(choice.GetPrimaryApproach(), state))
                    strategicAlignmentScore = 5;
                else if (IsPressureIncreasingApproach(choice.GetPrimaryApproach(), state))
                    strategicAlignmentScore = 1;
            }

            // 2. Character Proficiency Score (0-8 points)
            int characterProficiencyScore = Math.Min(8, state.TagSystem.GetEncounterStateTagValue(choice.GetPrimaryApproach()) * 2);

            // 3. Situational Score (2-3 points)
            int situationalScore = 2; // Default
            double pressureRatio = (double)state.Pressure / EncounterState.MaxPressure;
            double momentumRatio = (double)state.Momentum / state.Location.StandardThreshold;

            if (pressureRatio >= 0.6 && choice.EffectType == EffectTypes.Pressure)
                situationalScore = 3;
            else if (momentumRatio <= 0.4 && choice.EffectType == EffectTypes.Momentum)
                situationalScore = 3;

            // 4. Focus Relevance Score (1-3 points)
            int focusRelevanceScore = 1; // Default

            switch (state.Location.EncounterType)
            {
                case EncounterTypes.Physical:
                    if (choice.Focus == FocusTags.Physical)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Environment)
                        focusRelevanceScore = 2;
                    break;

                case EncounterTypes.Social:
                    if (choice.Focus == FocusTags.Relationship)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Information)
                        focusRelevanceScore = 2;
                    break;

                case EncounterTypes.Intellectual:
                    if (choice.Focus == FocusTags.Information)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Relationship)
                        focusRelevanceScore = 2;
                    break;
            }

            // 5. Narrative Tag Modifier (-15 or 0)
            int narrativeTagModifier = 0;
            if (blockedFocuses.Contains(choice.Focus))
                narrativeTagModifier = -15;

            // Calculate total score
            int totalScore = strategicAlignmentScore + characterProficiencyScore +
                           situationalScore + focusRelevanceScore + narrativeTagModifier;

            scores[choice] = totalScore;
        }

        return scores;
    }

    // Helper methods for strategic alignment
    private bool IsMomentumIncreasingApproach(EncounterStateTags approach, EncounterState state)
    {
        foreach (var tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.IncreaseMomentum &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsMomentumDecreasingApproach(EncounterStateTags approach, EncounterState state)
    {
        foreach (var tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.DecreaseMomentum &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsPressureDecreasingApproach(EncounterStateTags approach, EncounterState state)
    {
        foreach (var tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.DecreasePressure &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsPressureIncreasingApproach(EncounterStateTags approach, EncounterState state)
    {
        foreach (var tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.IncreasePressure &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsApproachFavorableForPressure(EncounterStateTags approach, EncounterState state)
    {
        return IsPressureDecreasingApproach(approach, state);
    }

    // Helper methods for categorizing choices
    private List<IChoice> GetMomentumChoicesWithPositiveAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   IsMomentumIncreasingApproach(c.GetPrimaryApproach(), state))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithPositiveAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   IsPressureDecreasingApproach(c.GetPrimaryApproach(), state))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNeutralAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   !IsMomentumIncreasingApproach(c.GetPrimaryApproach(), state) &&
                   !IsMomentumDecreasingApproach(c.GetPrimaryApproach(), state))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNeutralAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   !IsPressureDecreasingApproach(c.GetPrimaryApproach(), state) &&
                   !IsPressureIncreasingApproach(c.GetPrimaryApproach(), state))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNegativeAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   (IsMomentumDecreasingApproach(c.GetPrimaryApproach(), state) ||
                    IsPressureIncreasingApproach(c.GetPrimaryApproach(), state)))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNegativeAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   (IsPressureIncreasingApproach(c.GetPrimaryApproach(), state) ||
                    IsMomentumDecreasingApproach(c.GetPrimaryApproach(), state)))
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    // Get character's approaches sorted by value (highest first)
    private List<Tuple<EncounterStateTags, int>> GetCharacterApproachValues(EncounterState state)
    {
        var approaches = new List<Tuple<EncounterStateTags, int>>
        {
            new Tuple<EncounterStateTags, int>(EncounterStateTags.Dominance,
                state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Dominance)),
            new Tuple<EncounterStateTags, int>(EncounterStateTags.Rapport,
                state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Rapport)),
            new Tuple<EncounterStateTags, int>(EncounterStateTags.Analysis,
                state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Analysis)),
            new Tuple<EncounterStateTags, int>(EncounterStateTags.Precision,
                state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Precision)),
            new Tuple<EncounterStateTags, int>(EncounterStateTags.Concealment,
                state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Concealment))
        };

        return approaches.OrderByDescending(a => a.Item2).ToList();
    }

    private List<IChoice> GetChoicesByApproach(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterStateTags approach)
    {
        return choices
            .Where(c => c.GetPrimaryApproach() == approach)
            .OrderByDescending(c => scores[c])
            .ToList();
    }

    // Helper methods for narrative tags
    private List<FocusTags> GetBlockedFocuses(List<IEncounterTag> activeTags)
    {
        List<FocusTags> blockedFocuses = new List<FocusTags>();

        foreach (IEncounterTag tag in activeTags)
        {
            if (tag is NarrativeTag narrativeTag)
            {
                blockedFocuses.Add(narrativeTag.BlockedFocus);
            }
        }

        return blockedFocuses;
    }

    private List<IChoice> GetUnblockedChoices(List<IChoice> choices, List<FocusTags> blockedFocuses)
    {
        return choices.Where(c => !blockedFocuses.Contains(c.Focus)).ToList();
    }

    private List<IChoice> GetBlockedChoices(List<IChoice> choices, List<FocusTags> blockedFocuses)
    {
        return choices.Where(c => blockedFocuses.Contains(c.Focus)).ToList();
    }

    // Helper methods for selection
    private IChoice GetHighestScoringChoice(List<IChoice> choices, Dictionary<IChoice, int> scores)
    {
        if (!choices.Any())
            return null;

        return choices.OrderByDescending(c => scores[c]).First();
    }

    private IChoice GetLowestScoringChoice(List<IChoice> choices, Dictionary<IChoice, int> scores)
    {
        if (!choices.Any())
            return null;

        return choices.OrderBy(c => scores[c]).First();
    }
}