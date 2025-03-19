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
        Dictionary<IChoice, int> choiceScores = CalculateChoiceScores(allChoices, state);
        List<FocusTags> blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        // STEP 1: Select initial choices with basic diversity
        List<IChoice> selectedChoices = new List<IChoice>();

        // Approach diversity - select one choice for each of the top 3 approaches
        List<ApproachTags> approachRanking = GetCharacterApproachRanking(state);
        for (int i = 0; i < Math.Min(3, approachRanking.Count); i++)
        {
            ApproachTags approach = approachRanking[i];

            // Find best choice for this approach
            IChoice? bestChoice = allChoices
                .Where(c => GetPrimaryApproach(c) == approach &&
                           !blockedFocuses.Contains(c.Focus) &&
                           !selectedChoices.Contains(c))
                .OrderByDescending(c => choiceScores[c])
                .FirstOrDefault();

            if (bestChoice != null)
                selectedChoices.Add(bestChoice);
        }

        // Effect type diversity - ensure at least one momentum and one pressure choice
        bool hasMomentum = selectedChoices.Any(c => c.EffectType == EffectTypes.Momentum);
        bool hasPressure = selectedChoices.Any(c => c.EffectType == EffectTypes.Pressure);

        if (!hasMomentum)
        {
            IChoice? momentumChoice = allChoices
                .Where(c => c.EffectType == EffectTypes.Momentum &&
                           !blockedFocuses.Contains(c.Focus) &&
                           !selectedChoices.Contains(c))
                .OrderByDescending(c => choiceScores[c])
                .FirstOrDefault();

            if (momentumChoice != null)
                selectedChoices.Add(momentumChoice);
        }

        if (!hasPressure)
        {
            IChoice? pressureChoice = allChoices
                .Where(c => c.EffectType == EffectTypes.Pressure &&
                           !blockedFocuses.Contains(c.Focus) &&
                           !selectedChoices.Contains(c))
                .OrderByDescending(c => choiceScores[c])
                .FirstOrDefault();

            if (pressureChoice != null)
                selectedChoices.Add(pressureChoice);
        }

        // Fill remaining slots with highest scoring choices
        while (selectedChoices.Count < handSize)
        {
            IChoice? nextChoice = allChoices
                .Where(c => !selectedChoices.Contains(c))
                .OrderByDescending(c => choiceScores[c])
                .FirstOrDefault();

            if (nextChoice != null)
                selectedChoices.Add(nextChoice);
            else
                break;
        }

        // STEP 2: Apply more aggressive diversity requirements
        selectedChoices = EnforceApproachDiversity(selectedChoices, allChoices, blockedFocuses, choiceScores);
        selectedChoices = EnforceFocusDiversity(selectedChoices, allChoices, blockedFocuses, choiceScores);
        selectedChoices = GuaranteeStrategicOptions(selectedChoices, blockedFocuses, allChoices, choiceScores);

        // STEP 3: Handle edge cases
        selectedChoices = HandleCriticalPressure(selectedChoices, state, GetPressureChoicesWithPositiveAlignment(allChoices, state), choiceScores);
        selectedChoices = HandleSuccessWithinReach(selectedChoices, state, allChoices, choiceScores);

        // Mark blocked choices
        foreach (IChoice choice in selectedChoices)
        {
            if (blockedFocuses.Contains(choice.Focus))
            {
                choice.SetBlocked();
            }
        }

        // STEP 4: Output Finalized Hand
        return FinalizeHand(selectedChoices, blockedFocuses, choiceScores);
    }

    /// <summary>
    /// Calculates scores for all choices based on the Wayfarer design document scoring formula
    /// </summary>
    private Dictionary<IChoice, int> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        Dictionary<IChoice, int> scores = new Dictionary<IChoice, int>();
        List<FocusTags> blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        foreach (IChoice choice in choices)
        {
            ApproachTags primaryApproach = GetPrimaryApproach(choice);

            // 1. Strategic Alignment Score (1-6 points)
            int strategicAlignmentScore;
            if (choice.EffectType == EffectTypes.Momentum)
            {
                if (IsMomentumIncreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 6;
                else if (IsMomentumDecreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 1;
                else
                    strategicAlignmentScore = 3; // Neutral
            }
            else // Pressure
            {
                if (IsPressureDecreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 5;
                else if (IsPressureIncreasingApproach(primaryApproach, state))
                    strategicAlignmentScore = 1;
                else
                    strategicAlignmentScore = 3; // Neutral
            }

            // 2. Character Proficiency Score (0-8 points)
            int approachValue = state.TagSystem.GetEncounterStateTagValue(primaryApproach);
            int characterProficiencyScore = Math.Min(8, approachValue * 2);

            // 3. Situational Score (2-3 points)
            int situationalScore;
            double pressureRatio = (double)state.Pressure / state.Location.MaxPressure;
            double momentumRatio = (double)state.Momentum / state.Location.StandardThreshold;

            if (pressureRatio >= 0.6 && choice.EffectType == EffectTypes.Pressure)
                situationalScore = 3;
            else if (momentumRatio <= 0.4 && choice.EffectType == EffectTypes.Momentum)
                situationalScore = 3;
            else
                situationalScore = 2;

            // 4. Focus Relevance Score (1-3 points)
            int focusRelevanceScore;

            switch (state.Location.EncounterType)
            {
                case EncounterTypes.Physical:
                    if (choice.Focus == FocusTags.Physical)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Environment)
                        focusRelevanceScore = 2;
                    else
                        focusRelevanceScore = 1;
                    break;

                case EncounterTypes.Social:
                    if (choice.Focus == FocusTags.Relationship)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Information)
                        focusRelevanceScore = 2;
                    else
                        focusRelevanceScore = 1;
                    break;

                case EncounterTypes.Intellectual:
                    if (choice.Focus == FocusTags.Information)
                        focusRelevanceScore = 3;
                    else if (choice.Focus == FocusTags.Relationship)
                        focusRelevanceScore = 2;
                    else
                        focusRelevanceScore = 1;
                    break;

                default:
                    focusRelevanceScore = 1;
                    break;
            }

            // 5. Narrative Tag Modifier (-15 or 0)
            int narrativeTagModifier = blockedFocuses.Contains(choice.Focus) ? -15 : 0;

            // 6. Location Preference Modifiers
            int locationPreferenceModifier = 0;

            // Strongly respect location preferences
            if (state.Location.MomentumReducingFocuses.Contains(choice.Focus))
            {
                locationPreferenceModifier -= 15;
            }

            if (state.Location.PressureReducingFocuses.Contains(choice.Focus))
            {
                locationPreferenceModifier += 8;
            }

            // Dangerous approaches become increasingly problematic as pressure builds
            if (state.Location.DangerousApproaches.Contains(primaryApproach))
            {
                locationPreferenceModifier -= 15;
            }

            if (state.Location.MomentumBoostApproaches.Contains(primaryApproach))
            {
                locationPreferenceModifier += 8;
            }

            // 7. Previous Choice Influence
            int previousChoiceModifier = 0;
            if (state.PreviousChoice != null)
            {
                // Continuity bonus for same approach
                if (GetPrimaryApproach(state.PreviousChoice) == primaryApproach)
                    previousChoiceModifier += 3;

                // Focus development bonus
                if (state.PreviousChoice.Focus == choice.Focus)
                    previousChoiceModifier += 2;

                // Effect type variety
                if (state.PreviousChoice.EffectType != choice.EffectType)
                    previousChoiceModifier += 1;

                // Penalize repetitive choice patterns
                bool sameApproach = primaryApproach == GetPrimaryApproach(state.PreviousChoice);
                bool sameFocus = choice.Focus == state.PreviousChoice.Focus;
                bool sameEffectType = choice.EffectType == state.PreviousChoice.EffectType;

                if (sameApproach && sameFocus && sameEffectType)
                {
                    previousChoiceModifier -= 25; // Severe penalty for identical choice pattern
                }
                else if (sameApproach && sameFocus)
                {
                    previousChoiceModifier -= 10; // Moderate penalty for similar choice pattern
                }
            }

            // 8. Approach Diversification
            int approachUsageCount = GetApproachUsageCount(state, primaryApproach);
            int approachDiversificationModifier = 0;

            if (approachUsageCount == 0)
                approachDiversificationModifier += 3; // Exploration bonus for unused approaches
            else if (approachUsageCount > 2)
                approachDiversificationModifier -= (approachUsageCount - 2); // Penalty for overspecialization

            // 9. Pressure Trend Context
            int pressureTrendModifier = 0;
            double pressureTrend = CalculatePressureTrend(state);
            if (choice.EffectType == EffectTypes.Pressure && pressureTrend > 0)
            {
                pressureTrendModifier += (int)(pressureTrend * 5);
            }

            // Calculate total score
            int totalScore = strategicAlignmentScore +
                            characterProficiencyScore +
                            situationalScore +
                            focusRelevanceScore +
                            narrativeTagModifier +
                            locationPreferenceModifier +
                            previousChoiceModifier +
                            approachDiversificationModifier +
                            pressureTrendModifier;

            scores[choice] = totalScore;
        }

        return scores;
    }

    private List<IChoice> EnforceFocusDiversity(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                          List<FocusTags> blockedFocuses, Dictionary<IChoice, int> choiceScores)
    {
        // Count focus distribution
        var focusCounts = selectedChoices.GroupBy(c => c.Focus)
                                      .ToDictionary(g => g.Key, g => g.Count());

        // STRICTER REQUIREMENT: No more than 2 choices with same focus
        foreach (var focusPair in focusCounts.Where(f => f.Value > 2))
        {
            FocusTags overrepresentedFocus = focusPair.Key;

            // Find choices with this focus
            var sameFocusChoices = selectedChoices.Where(c => c.Focus == overrepresentedFocus)
                                               .OrderBy(c => choiceScores[c])
                                               .ToList();

            // Find all possible focuses that are underrepresented
            var focusesNeeded = Enum.GetValues(typeof(FocusTags))
                                  .Cast<FocusTags>()
                                  .Where(f => !blockedFocuses.Contains(f) &&
                                            (!focusCounts.ContainsKey(f) || focusCounts[f] < 1))
                                  .ToList();

            if (!focusesNeeded.Any())
                continue;

            foreach (var neededFocus in focusesNeeded)
            {
                // Find best choice with needed focus
                var diverseFocusChoice = allChoices
                    .Where(c => c.Focus == neededFocus &&
                              !selectedChoices.Contains(c))
                    .OrderByDescending(c => choiceScores[c])
                    .FirstOrDefault();

                if (diverseFocusChoice != null && sameFocusChoices.Count > 1)
                {
                    // Replace one of the overrepresented choices
                    selectedChoices.Remove(sameFocusChoices.First());
                    selectedChoices.Add(diverseFocusChoice);
                    break;
                }
            }
        }

        // NEW REQUIREMENT: Ensure at least 3 different focuses are represented
        var uniqueFocuses = selectedChoices.Select(c => c.Focus).Distinct().Count();

        if (uniqueFocuses < 3)
        {
            // Find focuses not represented
            var representedFocuses = selectedChoices.Select(c => c.Focus).Distinct().ToList();
            var missingFocuses = Enum.GetValues(typeof(FocusTags))
                               .Cast<FocusTags>()
                               .Where(f => !blockedFocuses.Contains(f) && !representedFocuses.Contains(f))
                               .ToList();

            if (missingFocuses.Any())
            {
                // Find the focus with the most choices
                var mostRepresentedFocus = focusCounts
                    .OrderByDescending(f => f.Value)
                    .First().Key;

                var choicesToReplace = selectedChoices
                    .Where(c => c.Focus == mostRepresentedFocus)
                    .OrderBy(c => choiceScores[c])
                    .Take(3 - uniqueFocuses)
                    .ToList();

                foreach (var choiceToReplace in choicesToReplace)
                {
                    foreach (var missingFocus in missingFocuses)
                    {
                        var replacementChoice = allChoices
                            .Where(c => c.Focus == missingFocus &&
                                      !selectedChoices.Contains(c))
                            .OrderByDescending(c => choiceScores[c])
                            .FirstOrDefault();

                        if (replacementChoice != null)
                        {
                            selectedChoices.Remove(choiceToReplace);
                            selectedChoices.Add(replacementChoice);
                            missingFocuses.Remove(missingFocus);
                            break;
                        }
                    }

                    if (missingFocuses.Count == 0)
                        break;
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> EnforceApproachDiversity(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                             List<FocusTags> blockedFocuses, Dictionary<IChoice, int> choiceScores)
    {
        // Count approach distribution
        var approachCounts = selectedChoices.GroupBy(c => GetPrimaryApproach(c))
                                         .ToDictionary(g => g.Key, g => g.Count());

        // STRICTER REQUIREMENT: No more than 2 choices can use the same approach
        foreach (var approachPair in approachCounts.Where(a => a.Value > 2))
        {
            ApproachTags overrepresentedApproach = approachPair.Key;

            // Find choices with this approach
            var sameApproachChoices = selectedChoices.Where(c => GetPrimaryApproach(c) == overrepresentedApproach)
                                                .OrderBy(c => choiceScores[c])
                                                .ToList();

            // Get all approaches not yet represented or underrepresented
            var approachesNeeded = Enum.GetValues(typeof(ApproachTags))
                                    .Cast<ApproachTags>()
                                    .Where(a => IsApproachTag(a) &&
                                              (!approachCounts.ContainsKey(a) || approachCounts[a] < 1))
                                    .ToList();

            if (!approachesNeeded.Any())
                continue;

            foreach (var neededApproach in approachesNeeded)
            {
                // Find best choice with needed approach
                var diverseApproachChoice = allChoices
                    .Where(c => GetPrimaryApproach(c) == neededApproach &&
                              !selectedChoices.Contains(c) &&
                              !blockedFocuses.Contains(c.Focus))
                    .OrderByDescending(c => choiceScores[c])
                    .FirstOrDefault();

                if (diverseApproachChoice != null && sameApproachChoices.Count > 1)
                {
                    // Replace one of the overrepresented choices
                    selectedChoices.Remove(sameApproachChoices.First());
                    selectedChoices.Add(diverseApproachChoice);
                    break;
                }
            }
        }

        // NEW REQUIREMENT: Ensure at least 3 different approaches are represented
        var uniqueApproaches = selectedChoices.Select(c => GetPrimaryApproach(c)).Distinct().Count();

        if (uniqueApproaches < 3)
        {
            // Find approaches not represented
            var representedApproaches = selectedChoices.Select(c => GetPrimaryApproach(c)).Distinct().ToList();
            var missingApproaches = Enum.GetValues(typeof(ApproachTags))
                                  .Cast<ApproachTags>()
                                  .Where(a => IsApproachTag(a) && !representedApproaches.Contains(a))
                                  .ToList();

            if (missingApproaches.Any())
            {
                // Find the approach with the most choices (if there's a tie, take any)
                var mostRepresentedApproach = approachCounts
                    .OrderByDescending(a => a.Value)
                    .First().Key;

                var choicesToReplace = selectedChoices
                    .Where(c => GetPrimaryApproach(c) == mostRepresentedApproach)
                    .OrderBy(c => choiceScores[c])
                    .Take(3 - uniqueApproaches)
                    .ToList();

                foreach (var choiceToReplace in choicesToReplace)
                {
                    foreach (var missingApproach in missingApproaches)
                    {
                        var replacementChoice = allChoices
                            .Where(c => GetPrimaryApproach(c) == missingApproach &&
                                      !selectedChoices.Contains(c) &&
                                      !blockedFocuses.Contains(c.Focus))
                            .OrderByDescending(c => choiceScores[c])
                            .FirstOrDefault();

                        if (replacementChoice != null)
                        {
                            selectedChoices.Remove(choiceToReplace);
                            selectedChoices.Add(replacementChoice);
                            missingApproaches.Remove(missingApproach);
                            break;
                        }
                    }

                    if (missingApproaches.Count == 0)
                        break;
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> EnsureStrategicOptionDiversity(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                                       EncounterState state, List<FocusTags> blockedFocuses,
                                                       Dictionary<IChoice, int> choiceScores)
    {
        // In hostile encounters, ensure we have non-physical options
        if (state.Location.Hostility == EncounterInfo.HostilityLevels.Hostile &&
            state.Location.EncounterType == EncounterTypes.Physical)
        {
            // Check if we have strategic diversity
            bool hasPhysicalOption = selectedChoices.Any(c => c.Focus == FocusTags.Physical);
            bool hasRelationshipOption = selectedChoices.Any(c => c.Focus == FocusTags.Relationship);
            bool hasResourceOption = selectedChoices.Any(c => c.Focus == FocusTags.Resource);

            // If we have ONLY physical options, add a social/resource option
            if (hasPhysicalOption && !hasRelationshipOption && !hasResourceOption)
            {
                // Find social/resource choices
                var socialChoices = allChoices
                    .Where(c => (c.Focus == FocusTags.Relationship || c.Focus == FocusTags.Resource) &&
                              !selectedChoices.Contains(c) &&
                              !blockedFocuses.Contains(c.Focus))
                    .OrderByDescending(c => choiceScores[c])
                    .Take(1)
                    .ToList();

                if (socialChoices.Any())
                {
                    // Replace lowest scoring physical choice
                    var physicalChoices = selectedChoices
                        .Where(c => c.Focus == FocusTags.Physical)
                        .OrderBy(c => choiceScores[c])
                        .Take(1)
                        .ToList();

                    if (physicalChoices.Any())
                    {
                        selectedChoices.Remove(physicalChoices.First());
                        selectedChoices.Add(socialChoices.First());
                    }
                }
            }
        }

        return selectedChoices;
    }

    private int GetApproachUsageCount(EncounterState state, ApproachTags approach)
    {
        int approachValue = state.TagSystem.GetEncounterStateTagValue(approach);
        return (approachValue + 1) / 2; // Each 2 points counts as a use, rounded up
    }

    private int GetFocusUsageCount(EncounterState state, FocusTags focus)
    {
        return state.TagSystem.GetFocusTagValue(focus);
    }

    private double CalculatePressureTrend(EncounterState state)
    {
        // If no previous pressure data, assume neutral trend
        if (state.PreviousPressure == 0)
            return 0;

        // Calculate change relative to max pressure
        double normalizedChange = (double)(state.Pressure - state.PreviousPressure) / state.Location.MaxPressure;

        // Return value between -1 and 1, scaled for sensitivity
        return Math.Max(-1, Math.Min(1, normalizedChange * 4));
    }

    private IChoice SelectFirstChoice(List<IChoice> poolD1, List<IChoice> poolB1, List<IChoice> poolB2, EncounterState state)
    {
        // If previous choice exists and continuity would be valuable, select from same approach
        if (state.PreviousChoice != null && poolD1.Any() &&
            !IsApproachOverused(state, GetPrimaryApproach(state.PreviousChoice)))
        {
            return poolD1.First();
        }

        // Otherwise default to highest approach
        if (poolB1.Any())
        {
            return poolB1.First();
        }
        // If highest approach has no choices, select from second highest
        else if (poolB2.Any())
        {
            return poolB2.First();
        }

        return null;
    }

    private bool IsApproachOverused(EncounterState state, ApproachTags approach)
    {
        int approachValue = state.TagSystem.GetEncounterStateTagValue(approach);
        return approachValue >= 6; // Consider an approach overused at 6+ points
    }

    private IChoice SelectSecondChoice(IChoice firstChoice, List<IChoice> poolA1, List<IChoice> poolA2,
        List<IChoice> poolA3, List<IChoice> poolA4, List<IChoice> poolA5, List<IChoice> poolA6)
    {
        if (firstChoice == null)
            return null;

        // If First Choice builds momentum, select from pressure-reducing choices
        if (firstChoice.EffectType == EffectTypes.Momentum)
        {
            return GetFirstAvailableChoice(poolA2) ??
                   GetFirstAvailableChoice(poolA4) ??
                   GetFirstAvailableChoice(poolA6);
        }
        // If First Choice reduces pressure, select from momentum-building choices
        else
        {
            return GetFirstAvailableChoice(poolA1) ??
                   GetFirstAvailableChoice(poolA3) ??
                   GetFirstAvailableChoice(poolA5);
        }
    }

    private IChoice SelectThirdChoice(List<IChoice> selectedChoices, List<IChoice> allChoices,
        Dictionary<IChoice, int> choiceScores, List<IChoice> poolA1, List<IChoice> poolA2,
        List<IChoice> poolA3, List<IChoice> poolA4, List<IChoice> poolA5, List<IChoice> poolA6)
    {
        if (selectedChoices.Count < 2)
            return null;

        // Create a list of choices with approaches different from the first two
        List<ApproachTags> approachesToExclude = selectedChoices
            .Select(c => GetPrimaryApproach(c))
            .Distinct()
            .ToList();

        List<IChoice> diverseApproachChoices = allChoices
            .Where(c => !approachesToExclude.Contains(GetPrimaryApproach(c)) &&
                        !selectedChoices.Contains(c))
            .ToList();

        SortPoolByScore(diverseApproachChoices, choiceScores);

        if (diverseApproachChoices.Any())
        {
            return diverseApproachChoices.First();
        }

        // If no diverse approach choices, select highest from general pools
        return GetFirstAvailableChoice(poolA1) ??
               GetFirstAvailableChoice(poolA2) ??
               GetFirstAvailableChoice(poolA3) ??
               GetFirstAvailableChoice(poolA4) ??
               GetFirstAvailableChoice(poolA5) ??
               GetFirstAvailableChoice(poolA6);
    }

    private IChoice SelectFourthChoice(EncounterState state, List<IChoice> selectedChoices,
        List<FocusTags> blockedFocuses, List<IChoice> poolC1, List<IChoice> poolC2, List<IChoice> poolD2,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        int blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));

        // If we have a previous choice and no focus continuity yet, prioritize focus continuity
        if (state.PreviousChoice != null &&
            !selectedChoices.Any(c => c.Focus == state.PreviousChoice.Focus) &&
            poolD2.Any(c => !selectedChoices.Contains(c) && !blockedFocuses.Contains(c.Focus)))
        {
            List<IChoice> focusContinuityChoices = poolD2
                .Where(c => !selectedChoices.Contains(c) && !blockedFocuses.Contains(c.Focus))
                .ToList();

            SortPoolByScore(focusContinuityChoices, choiceScores);

            if (focusContinuityChoices.Any())
                return focusContinuityChoices.First();
        }

        // On odd turns OR if fewer than 2 choices are blocked, try to include a blocked choice
        if (state.CurrentTurn % 2 == 1 || blockedChoicesInHand < 2)
        {
            // If any active narrative tags, select highest-scoring choice from C2
            if (blockedFocuses.Any() && poolC2.Any())
            {
                IChoice? blockedChoiceNotSelected = poolC2
                    .Where(c => !selectedChoices.Contains(c))
                    .FirstOrDefault();

                if (blockedChoiceNotSelected != null)
                {
                    return blockedChoiceNotSelected;
                }
            }

            // Otherwise, select highest-scoring choice not yet selected
            List<IChoice> remainingChoices = allChoices
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            SortPoolByScore(remainingChoices, choiceScores);
            return remainingChoices.FirstOrDefault();
        }
        else // On even turns AND 2 choices already blocked
        {
            // Select highest-scoring choice from C1 not already selected
            List<IChoice> unblockedNotSelected = poolC1
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            SortPoolByScore(unblockedNotSelected, choiceScores);
            return unblockedNotSelected.FirstOrDefault();
        }
    }

    private List<IChoice> EnsureViableChoices(List<IChoice> selectedChoices, List<FocusTags> blockedFocuses,
        List<IChoice> poolC1, Dictionary<IChoice, int> choiceScores)
    {
        // Ensure Viable Choices Rule - no more than 2 blocked choices
        int blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));

        while (blockedChoicesInHand > 2)
        {
            // Get lowest scoring blocked choice
            List<IChoice> blockedChoicesInHandList = selectedChoices
                .Where(c => blockedFocuses.Contains(c.Focus))
                .ToList();

            SortPoolByScore(blockedChoicesInHandList, choiceScores, ascending: true);
            IChoice lowestScoringBlocked = blockedChoicesInHandList.First();

            // Remove it
            selectedChoices.Remove(lowestScoringBlocked);

            // Add highest scoring unblocked choice not in hand
            List<IChoice> unblockedNotInHand = poolC1
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            SortPoolByScore(unblockedNotInHand, choiceScores);
            IChoice? replacementChoice = unblockedNotInHand.FirstOrDefault();

            if (replacementChoice != null)
            {
                selectedChoices.Add(replacementChoice);
            }

            blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));
        }

        return selectedChoices;
    }

    private List<IChoice> GuaranteeStrategicOptions(List<IChoice> selectedChoices, List<FocusTags> blockedFocuses,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        // Guarantee Strategic Options Rule - ensure mix of momentum and pressure if possible
        List<IChoice> unblockedChoices = selectedChoices
            .Where(c => !blockedFocuses.Contains(c.Focus))
            .ToList();

        bool allUnblockedBuildMomentum = unblockedChoices.All(c => c.EffectType == EffectTypes.Momentum);
        bool allUnblockedReducePressure = unblockedChoices.All(c => c.EffectType == EffectTypes.Pressure);

        if (allUnblockedBuildMomentum)
        {
            // All unblocked choices build momentum, need pressure
            List<IChoice> momentumChoices = unblockedChoices
                .Where(c => c.EffectType == EffectTypes.Momentum)
                .ToList();

            SortPoolByScore(momentumChoices, choiceScores, ascending: true);
            IChoice? lowestMomentumChoice = momentumChoices.FirstOrDefault();

            if (lowestMomentumChoice != null)
            {
                selectedChoices.Remove(lowestMomentumChoice);

                // Add highest scoring pressure choice not in hand
                List<IChoice> pressureChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure &&
                                !selectedChoices.Contains(c) &&
                                !blockedFocuses.Contains(c.Focus))
                    .ToList();

                SortPoolByScore(pressureChoicesNotInHand, choiceScores);
                IChoice? replacementChoice = pressureChoicesNotInHand.FirstOrDefault();

                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }
        else if (allUnblockedReducePressure)
        {
            // All unblocked choices reduce pressure, need momentum
            List<IChoice> pressureChoices = unblockedChoices
                .Where(c => c.EffectType == EffectTypes.Pressure)
                .ToList();

            SortPoolByScore(pressureChoices, choiceScores, ascending: true);
            IChoice? lowestPressureChoice = pressureChoices.FirstOrDefault();

            if (lowestPressureChoice != null)
            {
                selectedChoices.Remove(lowestPressureChoice);

                // Add highest scoring momentum choice not in hand
                List<IChoice> momentumChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum &&
                                !selectedChoices.Contains(c) &&
                                !blockedFocuses.Contains(c.Focus))
                    .ToList();

                SortPoolByScore(momentumChoicesNotInHand, choiceScores);
                IChoice? replacementChoice = momentumChoicesNotInHand.FirstOrDefault();

                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> EnforceCharacterIdentity(List<IChoice> selectedChoices, ApproachTags highestApproach,
        List<IChoice> poolB1, Dictionary<IChoice, int> choiceScores)
    {
        // Character Identity Rule - ensure highest approach is represented
        bool hasHighestApproachChoice = selectedChoices.Any(c => GetPrimaryApproach(c) == highestApproach);

        if (!hasHighestApproachChoice)
        {
            // Remove lowest scoring choice
            SortPoolByScore(selectedChoices, choiceScores, ascending: true);
            IChoice? lowestScoringChoice = selectedChoices.FirstOrDefault();

            if (lowestScoringChoice != null)
            {
                selectedChoices.Remove(lowestScoringChoice);

                // Add highest scoring choice using character's highest approach
                List<IChoice> highestApproachChoices = poolB1
                    .Where(c => !selectedChoices.Contains(c))
                    .ToList();

                SortPoolByScore(highestApproachChoices, choiceScores);
                IChoice? replacementChoice = highestApproachChoices.FirstOrDefault();

                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> HandleCriticalPressure(List<IChoice> selectedChoices, EncounterState state,
        List<IChoice> poolA2, Dictionary<IChoice, int> choiceScores)
    {
        // Critical Pressure Rule
        double pressureRatio = (double)state.Pressure / state.Location.MaxPressure;

        if (pressureRatio >= 0.8)
        {
            bool hasPressureReducingChoice = selectedChoices.Any(c =>
                c.EffectType == EffectTypes.Pressure &&
                IsPressureDecreasingApproach(GetPrimaryApproach(c), state));

            if (!hasPressureReducingChoice && poolA2.Any())
            {
                // Remove lowest scoring choice
                SortPoolByScore(selectedChoices, choiceScores, ascending: true);
                IChoice? lowestScoringChoice = selectedChoices.FirstOrDefault();

                if (lowestScoringChoice != null)
                {
                    selectedChoices.Remove(lowestScoringChoice);

                    // Add highest scoring choice from A2 (pressure-reducing using favorable approach)
                    IChoice? replacementChoice = poolA2.FirstOrDefault();

                    if (replacementChoice != null)
                    {
                        selectedChoices.Add(replacementChoice);
                    }
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> HandleSuccessWithinReach(List<IChoice> selectedChoices, EncounterState state,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        // Success Within Reach Rule
        int successThreshold = state.Location.StandardThreshold;

        if ((state.Momentum + 6) >= successThreshold &&
            state.CurrentTurn >= (state.Location.TurnDuration - 2))
        {
            int momentumBuildingChoices = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);

            if (momentumBuildingChoices < 2)
            {
                // Remove lowest scoring pressure choice
                List<IChoice> pressureChoicesInHand = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure)
                    .ToList();

                if (pressureChoicesInHand.Any())
                {
                    SortPoolByScore(pressureChoicesInHand, choiceScores, ascending: true);
                    IChoice lowestPressureChoice = pressureChoicesInHand.First();
                    selectedChoices.Remove(lowestPressureChoice);

                    // Add highest momentum choice not in hand
                    List<IChoice> momentumChoicesNotInHand = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum &&
                                    !selectedChoices.Contains(c))
                        .ToList();

                    SortPoolByScore(momentumChoicesNotInHand, choiceScores);
                    IChoice? replacementChoice = momentumChoicesNotInHand.FirstOrDefault();

                    if (replacementChoice != null)
                    {
                        selectedChoices.Add(replacementChoice);
                    }
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> HandleRecentlyActivatedTags(List<IChoice> selectedChoices, EncounterState state,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        // If we have previous state to compare against
        if (state.PreviousApproachValues != null && state.PreviousApproachValues.Any())
        {
            // Check for recently activated narrative tags
            foreach (IEncounterTag tag in state.ActiveTags)
            {
                if (tag is NarrativeTag narrativeTag)
                {
                    // Check if this tag was active in the previous state
                    bool wasActive = narrativeTag.IsActive(BaseTagSystem.FromPreviousState(state.PreviousApproachValues, state.PreviousFocusValues));
                    bool isActive = narrativeTag.IsActive(state.TagSystem);

                    // If it's active now but wasn't active before, it was recently activated
                    if (isActive && !wasActive)
                    {
                        // Find approach that's most likely responsible for activation 
                        ApproachTags activatingApproach = FindActivatingApproach(narrativeTag, state);

                        // Check if we have a choice with this approach in the hand
                        bool hasChoiceWithActivatingApproach = selectedChoices.Any(c =>
                            GetPrimaryApproach(c) == activatingApproach);

                        if (!hasChoiceWithActivatingApproach)
                        {
                            // Find choices with the activating approach
                            List<IChoice> choicesWithActivatingApproach = allChoices
                                .Where(c => GetPrimaryApproach(c) == activatingApproach &&
                                           !selectedChoices.Contains(c))
                                .ToList();

                            SortPoolByScore(choicesWithActivatingApproach, choiceScores);

                            if (choicesWithActivatingApproach.Any())
                            {
                                // Remove lowest scoring choice
                                SortPoolByScore(selectedChoices, choiceScores, ascending: true);
                                IChoice? lowestScoringChoice = selectedChoices.FirstOrDefault();

                                if (lowestScoringChoice != null)
                                {
                                    selectedChoices.Remove(lowestScoringChoice);
                                    selectedChoices.Add(choicesWithActivatingApproach.First());
                                }

                                break; // Only add one choice for newly activated tags
                            }
                        }
                    }
                }
            }
        }

        return selectedChoices;
    }

    private ApproachTags FindActivatingApproach(NarrativeTag narrativeTag, EncounterState state)
    {
        // Check all approach tags to see which one changed enough to activate the narrative tag
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                // Get current and previous values
                int currentValue = state.TagSystem.GetEncounterStateTagValue(approach);
                int previousValue = state.PreviousApproachValues.ContainsKey(approach) ?
                    state.PreviousApproachValues[approach] : 0;

                // If this approach increased, it might be responsible
                if (currentValue > previousValue)
                {
                    // Create a test tag system with the approach at its previous value
                    BaseTagSystem testSystem = BaseTagSystem.FromPreviousState(
                        state.PreviousApproachValues,
                        state.PreviousFocusValues);

                    // Set the current approach back to its previous value
                    testSystem.SetEncounterStateTagValue(approach, previousValue);

                    // Check if the tag would be inactive with this approach at its previous value
                    bool wouldBeInactive = !narrativeTag.IsActive(testSystem);

                    // If reducing this approach would make the tag inactive, 
                    // this is likely the activating approach
                    if (wouldBeInactive)
                    {
                        return approach;
                    }
                }
            }
        }

        // If we can't find a clear activating approach, return the approach that changed the most
        ApproachTags mostChangedApproach = ApproachTags.None;
        int largestChange = 0;

        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                int currentValue = state.TagSystem.GetEncounterStateTagValue(approach);
                int previousValue = state.PreviousApproachValues.ContainsKey(approach) ?
                    state.PreviousApproachValues[approach] : 0;
                int change = currentValue - previousValue;

                if (change > largestChange)
                {
                    largestChange = change;
                    mostChangedApproach = approach;
                }
            }
        }

        return mostChangedApproach;
    }

    private List<IChoice> FinalizeHand(List<IChoice> selectedChoices, List<FocusTags> blockedFocuses,
        Dictionary<IChoice, int> scores)
    {
        // Sort choices: unblocked momentum first, unblocked pressure second, blocked last
        return selectedChoices
            .OrderBy(c => blockedFocuses.Contains(c.Focus)) // Unblocked first (false comes before true)
            .ThenBy(c => c.EffectType != EffectTypes.Momentum) // Momentum first
            .ThenByDescending(c => scores[c]) // Higher score first
            .ToList();
    }

    private List<ApproachTags> GetCharacterApproachRanking(EncounterState state)
    {
        Dictionary<ApproachTags, int> approachValues = new Dictionary<ApproachTags, int>
        {
            { ApproachTags.Dominance, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Dominance) },
            { ApproachTags.Rapport, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Rapport) },
            { ApproachTags.Analysis, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Analysis) },
            { ApproachTags.Precision, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Precision) },
            { ApproachTags.Evasion, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Evasion) }
        };

        return approachValues
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private List<IChoice> GetChoicesBySameApproach(List<IChoice> choices, IChoice referenceChoice)
    {
        ApproachTags referenceApproach = GetPrimaryApproach(referenceChoice);
        return choices.Where(c => GetPrimaryApproach(c) == referenceApproach).ToList();
    }

    private List<IChoice> GetChoicesBySameFocus(List<IChoice> choices, IChoice referenceChoice)
    {
        return choices.Where(c => c.Focus == referenceChoice.Focus).ToList();
    }

    private List<IChoice> GetMomentumChoicesWithPositiveAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   IsMomentumIncreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithPositiveAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   IsPressureDecreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNeutralAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   !IsMomentumIncreasingApproach(GetPrimaryApproach(c), state) &&
                   !IsMomentumDecreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNeutralAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   !IsPressureDecreasingApproach(GetPrimaryApproach(c), state) &&
                   !IsPressureIncreasingApproach(GetPrimaryApproach(c), state))
            .ToList();
    }

    private List<IChoice> GetMomentumChoicesWithNegativeAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Momentum &&
                   (IsMomentumDecreasingApproach(GetPrimaryApproach(c), state) ||
                    IsPressureIncreasingApproach(GetPrimaryApproach(c), state)))
            .ToList();
    }

    private List<IChoice> GetPressureChoicesWithNegativeAlignment(List<IChoice> choices, EncounterState state)
    {
        return choices
            .Where(c => c.EffectType == EffectTypes.Pressure &&
                   (IsPressureIncreasingApproach(GetPrimaryApproach(c), state) ||
                    IsMomentumDecreasingApproach(GetPrimaryApproach(c), state)))
            .ToList();
    }

    private List<IChoice> GetChoicesByApproach(List<IChoice> choices, ApproachTags approach)
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

    private ApproachTags GetPrimaryApproach(IChoice choice)
    {
        // Find the approach tag with the largest modification
        List<TagModification> approachMods = choice.TagModifications
            .Where(m => m.Type == TagModification.TagTypes.EncounterState)
            .Where(m => IsApproachTag((ApproachTags)m.Tag))
            .OrderByDescending(m => m.Delta)
            .ToList();

        if (approachMods.Any())
        {
            return (ApproachTags)approachMods.First().Tag;
        }

        // Default to Analysis if no approach is found (fallback)
        return ApproachTags.Analysis;
    }

    private bool IsApproachTag(ApproachTags tag)
    {
        return tag == ApproachTags.Dominance ||
               tag == ApproachTags.Rapport ||
               tag == ApproachTags.Analysis ||
               tag == ApproachTags.Precision ||
               tag == ApproachTags.Evasion;
    }

    private bool IsMomentumIncreasingApproach(ApproachTags approach, EncounterState state)
    {
        foreach (IEncounterTag tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.IncreaseMomentum &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsMomentumDecreasingApproach(ApproachTags approach, EncounterState state)
    {
        foreach (IEncounterTag tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.DecreaseMomentum &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsPressureDecreasingApproach(ApproachTags approach, EncounterState state)
    {
        foreach (IEncounterTag tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.DecreasePressure &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }

    private bool IsPressureIncreasingApproach(ApproachTags approach, EncounterState state)
    {
        foreach (IEncounterTag tag in state.ActiveTags)
        {
            if (tag is StrategicTag strategicTag &&
                strategicTag.EffectType == StrategicEffectTypes.IncreasePressure &&
                strategicTag.ScalingApproachTag == approach)
                return true;
        }
        return false;
    }
}