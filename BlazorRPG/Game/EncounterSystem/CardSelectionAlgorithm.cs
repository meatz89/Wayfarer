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

        // STEP 1: Calculate scores for all choices with contextual modifiers
        Dictionary<IChoice, int> choiceScores = CalculateChoiceScores(allChoices, state);

        // STEP 2: Categorize choices into pools
        // Pool A: By Effect Type and Strategic Alignment
        var poolA1 = GetMomentumChoicesWithPositiveAlignment(allChoices, state);
        var poolA2 = GetPressureChoicesWithPositiveAlignment(allChoices, state);
        var poolA3 = GetMomentumChoicesWithNeutralAlignment(allChoices, state);
        var poolA4 = GetPressureChoicesWithNeutralAlignment(allChoices, state);
        var poolA5 = GetMomentumChoicesWithNegativeAlignment(allChoices, state);
        var poolA6 = GetPressureChoicesWithNegativeAlignment(allChoices, state);

        // Pool B: By Approach
        var approachRanking = GetCharacterApproachRanking(state);
        var poolB1 = GetChoicesByApproach(allChoices, approachRanking[0]);
        var poolB2 = GetChoicesByApproach(allChoices, approachRanking[1]);
        var poolB3 = GetChoicesByApproach(allChoices, approachRanking[2]);
        var poolB4 = GetChoicesByApproach(allChoices, approachRanking[3]);
        var poolB5 = GetChoicesByApproach(allChoices, approachRanking[4]);

        // Pool C: By Narrative Tag Status
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);
        var poolC1 = GetUnblockedChoices(allChoices, blockedFocuses);
        var poolC2 = GetBlockedChoices(allChoices, blockedFocuses);

        // Pool D: By Previous Choice Context (if any)
        var poolD1 = state.PreviousChoice != null ? GetChoicesBySameApproach(allChoices, state.PreviousChoice) : new List<IChoice>();
        var poolD2 = state.PreviousChoice != null ? GetChoicesBySameFocus(allChoices, state.PreviousChoice) : new List<IChoice>();

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
        SortPoolByScore(poolD1, choiceScores);
        SortPoolByScore(poolD2, choiceScores);

        // STEP 3: Select Initial Choices
        List<IChoice> selectedChoices = new List<IChoice>();

        // First Choice: Continuity or Character Strength
        IChoice firstChoice = SelectFirstChoice(poolD1, poolB1, poolB2, state);
        if (firstChoice != null)
        {
            selectedChoices.Add(firstChoice);
        }

        // Second Choice: Strategic Advantage
        IChoice secondChoice = SelectSecondChoice(firstChoice, poolA1, poolA2, poolA3, poolA4, poolA5, poolA6);
        if (secondChoice != null)
        {
            selectedChoices.Add(secondChoice);
        }

        // Third Choice: Approach Diversity
        IChoice thirdChoice = SelectThirdChoice(selectedChoices, allChoices, choiceScores, poolA1, poolA2, poolA3, poolA4, poolA5, poolA6);
        if (thirdChoice != null)
        {
            selectedChoices.Add(thirdChoice);
        }

        // Fourth Choice: Focus Diversity or Narrative Tag Impact
        IChoice fourthChoice = SelectFourthChoice(state, selectedChoices, blockedFocuses, poolC1, poolC2, poolD2, allChoices, choiceScores);
        if (fourthChoice != null)
        {
            selectedChoices.Add(fourthChoice);
        }

        // STEP 4: Validate Hand Composition
        selectedChoices = EnsureViableChoices(selectedChoices, blockedFocuses, poolC1, choiceScores);
        selectedChoices = GuaranteeStrategicOptions(selectedChoices, blockedFocuses, allChoices, choiceScores);
        selectedChoices = EnforceCharacterIdentity(selectedChoices, approachRanking[0], poolB1, choiceScores);

        // STEP 5: Handle Edge Cases
        selectedChoices = HandleCriticalPressure(selectedChoices, state, poolA2, choiceScores);
        selectedChoices = HandleSuccessWithinReach(selectedChoices, state, allChoices, choiceScores);
        selectedChoices = HandleRecentlyActivatedTags(selectedChoices, state, allChoices, choiceScores);

        // Mark blocked choices
        foreach (IChoice choice in selectedChoices)
        {
            if (blockedFocuses.Contains(choice.Focus))
            {
                choice.SetBlocked();
            }
        }

        // STEP 6: Output Finalized Hand
        return FinalizeHand(selectedChoices, blockedFocuses, choiceScores);
    }

    /// <summary>
    /// Calculates scores for all choices based on the Wayfarer design document scoring formula
    /// </summary>
    private Dictionary<IChoice, int> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        Dictionary<IChoice, int> scores = new Dictionary<IChoice, int>();
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        foreach (IChoice choice in choices)
        {
            EncounterStateTags primaryApproach = GetPrimaryApproach(choice);

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

            // 6. Contextual Modifiers
            int contextualModifier = 0;

            // Previous Choice Influence
            if (state.PreviousChoice != null)
            {
                // Continuity bonus for same approach
                if (GetPrimaryApproach(state.PreviousChoice) == primaryApproach)
                    contextualModifier += 3;

                // Focus development bonus
                if (state.PreviousChoice.Focus == choice.Focus)
                    contextualModifier += 2;

                // Effect type variety
                if (state.PreviousChoice.EffectType != choice.EffectType)
                    contextualModifier += 1;
            }

            // Approach Diversification
            int approachUsageCount = GetApproachUsageCount(state, primaryApproach);
            if (approachUsageCount == 0)
                contextualModifier += 3; // Exploration bonus for unused approaches
            else if (approachUsageCount > 2)
                contextualModifier -= (approachUsageCount - 2); // Penalty for overspecialization

            // Pressure Trend
            double pressureTrend = CalculatePressureTrend(state);
            if (choice.EffectType == EffectTypes.Pressure && pressureTrend > 0)
                contextualModifier += (int)(pressureTrend * 3); // Rising pressure increases value of pressure-reducing choices

            // Focus Diversity
            int focusUsageCount = GetFocusUsageCount(state, choice.Focus);
            if (focusUsageCount == 0)
                contextualModifier += 2; // Bonus for unused focuses

            // Strategic Tag Utilization 
            if (choice.EffectType == EffectTypes.Momentum &&
                IsMomentumIncreasingApproach(primaryApproach, state) &&
                approachValue >= 3)
            {
                contextualModifier += 2; // Bonus for leveraging high momentum-increasing approach
            }

            if (choice.EffectType == EffectTypes.Pressure &&
                IsPressureDecreasingApproach(primaryApproach, state) &&
                approachValue >= 3)
            {
                contextualModifier += 2; // Bonus for leveraging high pressure-decreasing approach
            }

            // Calculate total score
            int totalScore = strategicAlignmentScore + characterProficiencyScore +
                            situationalScore + focusRelevanceScore + narrativeTagModifier + contextualModifier;

            scores[choice] = totalScore;
        }

        return scores;
    }

    private int GetApproachUsageCount(EncounterState state, EncounterStateTags approach)
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

    private bool IsApproachOverused(EncounterState state, EncounterStateTags approach)
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
        var approachesToExclude = selectedChoices
            .Select(c => GetPrimaryApproach(c))
            .Distinct()
            .ToList();

        var diverseApproachChoices = allChoices
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
            var focusContinuityChoices = poolD2
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
                var blockedChoiceNotSelected = poolC2
                    .Where(c => !selectedChoices.Contains(c))
                    .FirstOrDefault();

                if (blockedChoiceNotSelected != null)
                {
                    return blockedChoiceNotSelected;
                }
            }

            // Otherwise, select highest-scoring choice not yet selected
            var remainingChoices = allChoices
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            SortPoolByScore(remainingChoices, choiceScores);
            return remainingChoices.FirstOrDefault();
        }
        else // On even turns AND 2 choices already blocked
        {
            // Select highest-scoring choice from C1 not already selected
            var unblockedNotSelected = poolC1
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
            var blockedChoicesInHandList = selectedChoices
                .Where(c => blockedFocuses.Contains(c.Focus))
                .ToList();

            SortPoolByScore(blockedChoicesInHandList, choiceScores, ascending: true);
            var lowestScoringBlocked = blockedChoicesInHandList.First();

            // Remove it
            selectedChoices.Remove(lowestScoringBlocked);

            // Add highest scoring unblocked choice not in hand
            var unblockedNotInHand = poolC1
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            SortPoolByScore(unblockedNotInHand, choiceScores);
            var replacementChoice = unblockedNotInHand.FirstOrDefault();

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
        var unblockedChoices = selectedChoices
            .Where(c => !blockedFocuses.Contains(c.Focus))
            .ToList();

        bool allUnblockedBuildMomentum = unblockedChoices.All(c => c.EffectType == EffectTypes.Momentum);
        bool allUnblockedReducePressure = unblockedChoices.All(c => c.EffectType == EffectTypes.Pressure);

        if (allUnblockedBuildMomentum)
        {
            // All unblocked choices build momentum, need pressure
            var momentumChoices = unblockedChoices
                .Where(c => c.EffectType == EffectTypes.Momentum)
                .ToList();

            SortPoolByScore(momentumChoices, choiceScores, ascending: true);
            var lowestMomentumChoice = momentumChoices.FirstOrDefault();

            if (lowestMomentumChoice != null)
            {
                selectedChoices.Remove(lowestMomentumChoice);

                // Add highest scoring pressure choice not in hand
                var pressureChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure &&
                                !selectedChoices.Contains(c) &&
                                !blockedFocuses.Contains(c.Focus))
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
            var pressureChoices = unblockedChoices
                .Where(c => c.EffectType == EffectTypes.Pressure)
                .ToList();

            SortPoolByScore(pressureChoices, choiceScores, ascending: true);
            var lowestPressureChoice = pressureChoices.FirstOrDefault();

            if (lowestPressureChoice != null)
            {
                selectedChoices.Remove(lowestPressureChoice);

                // Add highest scoring momentum choice not in hand
                var momentumChoicesNotInHand = allChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum &&
                                !selectedChoices.Contains(c) &&
                                !blockedFocuses.Contains(c.Focus))
                    .ToList();

                SortPoolByScore(momentumChoicesNotInHand, choiceScores);
                var replacementChoice = momentumChoicesNotInHand.FirstOrDefault();

                if (replacementChoice != null)
                {
                    selectedChoices.Add(replacementChoice);
                }
            }
        }

        return selectedChoices;
    }

    private List<IChoice> EnforceCharacterIdentity(List<IChoice> selectedChoices, EncounterStateTags highestApproach,
        List<IChoice> poolB1, Dictionary<IChoice, int> choiceScores)
    {
        // Character Identity Rule - ensure highest approach is represented
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
                var highestApproachChoices = poolB1
                    .Where(c => !selectedChoices.Contains(c))
                    .ToList();

                SortPoolByScore(highestApproachChoices, choiceScores);
                var replacementChoice = highestApproachChoices.FirstOrDefault();

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
                var lowestScoringChoice = selectedChoices.FirstOrDefault();

                if (lowestScoringChoice != null)
                {
                    selectedChoices.Remove(lowestScoringChoice);

                    // Add highest scoring choice from A2 (pressure-reducing using favorable approach)
                    var replacementChoice = poolA2.FirstOrDefault();

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
                var pressureChoicesInHand = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure)
                    .ToList();

                if (pressureChoicesInHand.Any())
                {
                    SortPoolByScore(pressureChoicesInHand, choiceScores, ascending: true);
                    var lowestPressureChoice = pressureChoicesInHand.First();
                    selectedChoices.Remove(lowestPressureChoice);

                    // Add highest momentum choice not in hand
                    var momentumChoicesNotInHand = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum &&
                                    !selectedChoices.Contains(c))
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

        return selectedChoices;
    }

    private List<IChoice> HandleRecentlyActivatedTags(List<IChoice> selectedChoices, EncounterState state,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        // If we have previous state to compare against
        if (state.PreviousApproachValues != null && state.PreviousApproachValues.Any())
        {
            // Check for recently activated narrative tags
            foreach (var tag in state.ActiveTags)
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
                        EncounterStateTags activatingApproach = FindActivatingApproach(narrativeTag, state);

                        // Check if we have a choice with this approach in the hand
                        bool hasChoiceWithActivatingApproach = selectedChoices.Any(c =>
                            GetPrimaryApproach(c) == activatingApproach);

                        if (!hasChoiceWithActivatingApproach)
                        {
                            // Find choices with the activating approach
                            var choicesWithActivatingApproach = allChoices
                                .Where(c => GetPrimaryApproach(c) == activatingApproach &&
                                           !selectedChoices.Contains(c))
                                .ToList();

                            SortPoolByScore(choicesWithActivatingApproach, choiceScores);

                            if (choicesWithActivatingApproach.Any())
                            {
                                // Remove lowest scoring choice
                                SortPoolByScore(selectedChoices, choiceScores, ascending: true);
                                var lowestScoringChoice = selectedChoices.FirstOrDefault();

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

    private EncounterStateTags FindActivatingApproach(NarrativeTag narrativeTag, EncounterState state)
    {
        // Check all approach tags to see which one changed enough to activate the narrative tag
        foreach (EncounterStateTags approach in Enum.GetValues(typeof(EncounterStateTags)))
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
        EncounterStateTags mostChangedApproach = EncounterStateTags.None;
        int largestChange = 0;

        foreach (EncounterStateTags approach in Enum.GetValues(typeof(EncounterStateTags)))
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
        Dictionary<IChoice, int> choiceScores)
    {
        // Sort choices: unblocked momentum first, unblocked pressure second, blocked last
        return selectedChoices
            .OrderBy(c => blockedFocuses.Contains(c.Focus)) // Unblocked first (false comes before true)
            .ThenBy(c => c.EffectType != EffectTypes.Momentum) // Momentum first
            .ThenByDescending(c => choiceScores[c]) // Higher score first
            .ToList();
    }

    private List<EncounterStateTags> GetCharacterApproachRanking(EncounterState state)
    {
        var approachValues = new Dictionary<EncounterStateTags, int>
        {
            { EncounterStateTags.Dominance, state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Dominance) },
            { EncounterStateTags.Rapport, state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Rapport) },
            { EncounterStateTags.Analysis, state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Analysis) },
            { EncounterStateTags.Precision, state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Precision) },
            { EncounterStateTags.Concealment, state.TagSystem.GetEncounterStateTagValue(EncounterStateTags.Concealment) }
        };

        return approachValues
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private List<IChoice> GetChoicesBySameApproach(List<IChoice> choices, IChoice referenceChoice)
    {
        EncounterStateTags referenceApproach = GetPrimaryApproach(referenceChoice);
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
}