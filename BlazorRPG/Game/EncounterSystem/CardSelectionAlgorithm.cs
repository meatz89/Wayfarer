using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Deterministic card selection algorithm that follows the Wayfarer design document
/// </summary>
public class CardSelectionAlgorithm
{
    private readonly ChoiceRepository _choiceRepository;

    public CardSelectionAlgorithm(ChoiceRepository choiceRepository)
    {
        _choiceRepository = choiceRepository;
    }

    /// <summary>
    /// Select a hand of choices for the player following the deterministic algorithm
    /// </summary>
    /// <param name="state">Current encounter state</param>
    /// <param name="handSize">Number of choices to select (default 4)</param>
    /// <returns>List of selected choices</returns>
    public List<IChoice> SelectChoices(EncounterState state, int handSize = 4)
    {
        List<IChoice> allChoices = new List<IChoice>(_choiceRepository.GetAllStandardChoices());

        // STEP 1: Calculate scores for all choices
        Dictionary<IChoice, int> choiceScores = CalculateChoiceScores(allChoices, state);

        // STEP 2: Categorize choices into pools
        var poolA1 = GetMomentumChoicesWithPositiveAlignment(allChoices, choiceScores, state);
        var poolA2 = GetPressureChoicesWithPositiveAlignment(allChoices, choiceScores, state);
        var poolA3 = GetMomentumChoicesWithNeutralAlignment(allChoices, choiceScores, state);
        var poolA4 = GetPressureChoicesWithNeutralAlignment(allChoices, choiceScores, state);
        var poolA5 = GetMomentumChoicesWithNegativeAlignment(allChoices, choiceScores, state);
        var poolA6 = GetPressureChoicesWithNegativeAlignment(allChoices, choiceScores, state);

        var characterApproachValues = GetCharacterApproachValues(state);
        var poolB1 = GetChoicesByApproach(allChoices, characterApproachValues[0].Item1);
        var poolB2 = GetChoicesByApproach(allChoices, characterApproachValues[1].Item1);
        var poolB3 = GetChoicesByApproach(allChoices, characterApproachValues[2].Item1);
        var poolB4 = GetChoicesByApproach(allChoices, characterApproachValues[3].Item1);
        var poolB5 = GetChoicesByApproach(allChoices, characterApproachValues[4].Item1);

        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);
        var poolC1 = GetUnblockedChoices(allChoices, blockedFocuses);
        var poolC2 = GetBlockedChoices(allChoices, blockedFocuses);

        // Sort all pools by score
        SortPoolByScore(poolA1, choiceScores);
        SortPoolByScore(poolA2, choiceScores);
        SortPoolByScore(poolA3, choiceScores);
        SortPoolByScore(poolA4, choiceScores);
        SortPoolByScore(poolA5, choiceScores);
        SortPoolByScore(poolA6, choiceScores);
        SortPoolByScore(poolB1, choiceScores);
        SortPoolByScore(poolB2, choiceScores);
        SortPoolByScore(poolB3, choiceScores);
        SortPoolByScore(poolB4, choiceScores);
        SortPoolByScore(poolB5, choiceScores);
        SortPoolByScore(poolC1, choiceScores);
        SortPoolByScore(poolC2, choiceScores);

        // STEP 3: Select Initial Choices
        List<IChoice> selectedChoices = new List<IChoice>();

        // First Choice: Character Strength
        IChoice firstChoice = null;
        if (poolB1.Any())
        {
            firstChoice = poolB1.First();
        }
        else if (poolB2.Any())
        {
            firstChoice = poolB2.First();
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
                secondChoice = GetFirstAvailableChoice(poolA2);
            }
            else
            {
                // If first choice reduces pressure, add a momentum-building choice
                secondChoice = GetFirstAvailableChoice(poolA1);
            }

            // If the primary list is empty, try subsequent lists
            if (secondChoice == null)
            {
                secondChoice = GetFirstAvailableChoice(poolA3) ??
                               GetFirstAvailableChoice(poolA4) ??
                               GetFirstAvailableChoice(poolA5) ??
                               GetFirstAvailableChoice(poolA6);
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
            var approachesToExclude = selectedChoices.Select(c => GetPrimaryApproach(c)).Distinct().ToList();
            var diverseApproachChoices = allChoices.Where(c => !approachesToExclude.Contains(GetPrimaryApproach(c))).ToList();
            SortPoolByScore(diverseApproachChoices, choiceScores);

            if (diverseApproachChoices.Any())
            {
                thirdChoice = diverseApproachChoices.First();
            }
            else
            {
                // If no diverse approach choices, select highest from general pools
                thirdChoice = GetFirstAvailableChoice(poolA1) ??
                              GetFirstAvailableChoice(poolA2) ??
                              GetFirstAvailableChoice(poolA3) ??
                              GetFirstAvailableChoice(poolA4) ??
                              GetFirstAvailableChoice(poolA5) ??
                              GetFirstAvailableChoice(poolA6);
            }
        }

        if (thirdChoice != null)
        {
            selectedChoices.Add(thirdChoice);
        }

        // Fourth Choice: Focus Diversity or Narrative Tag Impact
        IChoice fourthChoice = null;
        int blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));

        if (state.CurrentTurn % 2 == 1 || blockedChoicesInHand < 2)
        {
            // On odd turns OR if fewer than 2 choices are blocked, try to include a blocked choice
            if (blockedFocuses.Any() && poolC2.Any())
            {
                fourthChoice = poolC2.First();
            }
            else
            {
                // Otherwise, just get highest scoring not yet selected
                var remainingChoices = allChoices.Where(c => !selectedChoices.Contains(c)).ToList();
                SortPoolByScore(remainingChoices, choiceScores);
                fourthChoice = remainingChoices.FirstOrDefault();
            }
        }
        else
        {
            // On even turns AND 2 choices already blocked, get unblocked
            var unblockedNotSelected = poolC1.Where(c => !selectedChoices.Contains(c)).ToList();
            SortPoolByScore(unblockedNotSelected, choiceScores);
            fourthChoice = unblockedNotSelected.FirstOrDefault();
        }

        if (fourthChoice != null)
        {
            selectedChoices.Add(fourthChoice);
        }

        // STEP 4: Validate Hand Composition
        // Ensure Viable Choices Rule - no more than 2 blocked choices
        blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));
        if (blockedChoicesInHand > 2)
        {
            // Get lowest scoring blocked choice
            var blockedChoicesInHandList = selectedChoices.Where(c => blockedFocuses.Contains(c.Focus)).ToList();
            SortPoolByScore(blockedChoicesInHandList, choiceScores, ascending: true);
            var lowestScoringBlocked = blockedChoicesInHandList.First();

            // Remove it
            selectedChoices.Remove(lowestScoringBlocked);

            // Add highest scoring unblocked choice not in hand
            var unblockedNotInHand = poolC1.Where(c => !selectedChoices.Contains(c)).ToList();
            SortPoolByScore(unblockedNotInHand, choiceScores);
            var replacementChoice = unblockedNotInHand.FirstOrDefault();
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
            var momentumChoices = unblockedChoices.Where(c => c.EffectType == EffectTypes.Momentum).ToList();
            SortPoolByScore(momentumChoices, choiceScores, ascending: true);
            var lowestMomentumChoice = momentumChoices.FirstOrDefault();

            if (lowestMomentumChoice != null)
            {
                selectedChoices.Remove(lowestMomentumChoice);

                // Add highest scoring pressure choice not in hand
                var pressureChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure && !selectedChoices.Contains(c))
                    .ToList();
                SortPoolByScore(pressureChoicesNotInHand, choiceScores);

                var replacementChoice = pressureChoicesNotInHand.FirstOrDefault();
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }
        else if (allUnblockedReducePressure)
        {
            // All unblocked choices reduce pressure, need momentum
            var pressureChoices = unblockedChoices.Where(c => c.EffectType == EffectTypes.Pressure).ToList();
            SortPoolByScore(pressureChoices, choiceScores, ascending: true);
            var lowestPressureChoice = pressureChoices.FirstOrDefault();

            if (lowestPressureChoice != null)
            {
                selectedChoices.Remove(lowestPressureChoice);

                // Add highest scoring momentum choice not in hand
                var momentumChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum && !selectedChoices.Contains(c))
                    .ToList();
                SortPoolByScore(momentumChoicesNotInHand, choiceScores);

                var replacementChoice = momentumChoicesNotInHand.FirstOrDefault();
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        // Character Identity Rule - ensure highest approach is represented
        var highestApproach = characterApproachValues[0].Item1;
        bool hasHighestApproachChoice = selectedChoices.Any(c => GetPrimaryApproach(c) == highestApproach);

        if (!hasHighestApproachChoice)
        {
            // Remove lowest scoring choice
            SortPoolByScore(selectedChoices, choiceScores, ascending: true);
            var lowestScoringChoice = selectedChoices.FirstOrDefault();
            if (lowestScoringChoice != null)
            {
                selectedChoices.Remove(lowestScoringChoice);

                // Add highest scoring choice using character's highest approach
                var highestApproachChoices = poolB1.Where(c => !selectedChoices.Contains(c)).ToList();
                SortPoolByScore(highestApproachChoices, choiceScores);

                var replacementChoice = highestApproachChoices.FirstOrDefault();
                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        // STEP 5: Handle Edge Cases
        // Critical Pressure Rule
        double pressureRatio = (double)state.Pressure / EncounterState.MaxPressure;
        if (pressureRatio >= 0.8)
        {
            bool hasPressureReducingChoice = selectedChoices.Any(c =>
                c.EffectType == EffectTypes.Pressure &&
                IsApproachFavorableForPressure(GetPrimaryApproach(c), state));

            if (!hasPressureReducingChoice)
            {
                // Remove lowest scoring choice
                SortPoolByScore(selectedChoices, choiceScores, ascending: true);
                var lowestScoringChoice = selectedChoices.FirstOrDefault();
                if (lowestScoringChoice != null)
                {
                    selectedChoices.Remove(lowestScoringChoice);

                    // Add highest scoring choice from A2 (pressure-reducing, favorable approach)
                    var replacementChoice = poolA2.FirstOrDefault();
                    if (replacementChoice != null)
                    {
                        selectedChoices.Add(replacementChoice);
                    }
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
                    SortPoolByScore(pressureChoicesInHand, choiceScores, ascending: true);
                    var lowestPressureChoice = pressureChoicesInHand.First();
                    selectedChoices.Remove(lowestPressureChoice);

                    // Add highest momentum choice not in hand
                    var momentumChoicesNotInHand = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum && !selectedChoices.Contains(c))
                        .ToList();
                    SortPoolByScore(momentumChoicesNotInHand, choiceScores);

                    var replacementChoice = momentumChoicesNotInHand.FirstOrDefault();
                    if (replacementChoice != null)
                    {
                        selectedChoices.Add(replacementChoice);
                    }
                }
            }
        }

        List<IChoice> blockedChoices = selectedChoices.Where(c => blockedFocuses.Contains(c.Focus)).ToList();
        foreach (IChoice blockedChoice in blockedChoices)
        {
            var choice = selectedChoices.FirstOrDefault(c => c.Equals(blockedChoice));
            choice.SetBlocked();
        }

        // STEP 6: Output Finalized Hand
        // Sort by: unblocked momentum first, unblocked pressure second, blocked last
        return selectedChoices
            .OrderBy(c => blockedFocuses.Contains(c.Focus)) // Unblocked first (false comes before true)
            .ThenBy(c => c.EffectType != EffectTypes.Momentum) // Momentum first
            .ThenByDescending(c => choiceScores[c]) // Higher score first
            .ToList();
    }


    /// <summary>
    /// Calculates scores for all choices based on the project knowledge formula
    /// </summary>
    private Dictionary<IChoice, int> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        Dictionary<IChoice, int> scores = new Dictionary<IChoice, int>();
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        foreach (IChoice choice in choices)
        {
            // 1. Strategic Alignment Score (1-6 points)
            int strategicAlignmentScore = 3; // Default neutral
            var primaryApproach = GetPrimaryApproach(choice);

            if (choice.EffectType == EffectTypes.Momentum)
            {
                if (IsMomentumIncreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 6;
                else if (IsMomentumDecreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 1;
            }
            else // Pressure
            {
                if (IsPressureDecreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 5;
                else if (IsPressureIncreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 1;
            }

            // 2. Character Proficiency Score (0-8 points)
            int characterProficiencyScore = Math.Min(8, state.TagSystem.GetEncounterStateTagValue(primaryApproach) * 2);

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

    private List<IChoice> GetMomentumChoicesWithPositiveAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   IsMomentumIncreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithPositiveAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   IsPressureDecreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNeutralAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   !IsMomentumIncreasingApproach(GetPrimaryApproach(c), state) &&
                   !IsMomentumDecreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNeutralAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   !IsPressureDecreasingApproach(GetPrimaryApproach(c), state) &&
                   !IsPressureIncreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNegativeAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   (IsMomentumDecreasingApproach(GetPrimaryApproach(c), state) ||
                    IsPressureIncreasingApproach(GetPrimaryApproach(c), state)))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNegativeAlignment(
        List<IChoice> choices, Dictionary<IChoice, int> scores, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   (IsPressureIncreasingApproach(GetPrimaryApproach(c), state) ||
                    IsMomentumDecreasingApproach(GetPrimaryApproach(c), state)))
            .ToList();
    }

    private List<IChoice> GetChoicesByApproach(List<IChoice> choices, EncounterStateTags approach)
    {
        return choices.Where(c => GetPrimaryApproach(c) == approach).ToList();
    }

    private List<IChoice> GetUnblockedChoices(List<IChoice> choices, List<FocusTags> blockedFocuses)
    {
        return choices.Where(c => !blockedFocuses.Contains(c.Focus)).ToList();
    }

    private List<IChoice> GetBlockedChoices(List<IChoice> choices, List<FocusTags> blockedFocuses)
    {
        return choices.Where(c => blockedFocuses.Contains(c.Focus)).ToList();
    }

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

    private void SortPoolByScore(List<IChoice> pool, Dictionary<IChoice, int> scores, bool ascending = false)
    {
        if (ascending)
            pool.Sort((x, y) => scores[x].CompareTo(scores[y]));
        else
            pool.Sort((x, y) => scores[y].CompareTo(scores[x]));
    }

    private IChoice GetFirstAvailableChoice(List<IChoice> pool)
    {
        return pool.Any() ? pool.First() : null;
    }

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

    private EncounterStateTags GetPrimaryApproach(IChoice choice)
    {
        // Find the approach tag with the largest modification
        var approachMods = choice.TagModifications
            .Where(m => m.Type == TagModification.TagTypes.EncounterState)
            .Where(m => IsApproachTag((EncounterStateTags)m.Tag))
            .OrderByDescending(m => m.Delta)
            .ToList();

        if (approachMods.Any())
        {
            return (EncounterStateTags)approachMods.First().Tag;
        }

        // Default to Analysis if no approach is found (fallback)
        return EncounterStateTags.Analysis;
    }

    private bool IsApproachTag(EncounterStateTags tag)
    {
        return tag == EncounterStateTags.Dominance ||
               tag == EncounterStateTags.Rapport ||
               tag == EncounterStateTags.Analysis ||
               tag == EncounterStateTags.Precision ||
               tag == EncounterStateTags.Concealment;
    }

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
}