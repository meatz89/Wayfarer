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
        // Pool A: By Effect Type and Strategic Alignment
        var poolA1 = GetMomentumChoicesWithPositiveAlignment(allChoices, state);  // Momentum-building choices using approaches that increase momentum
        var poolA2 = GetPressureChoicesWithPositiveAlignment(allChoices, state);  // Pressure-reducing choices using approaches that decrease pressure
        var poolA3 = GetMomentumChoicesWithNeutralAlignment(allChoices, state);   // Momentum-building choices using neutral approaches
        var poolA4 = GetPressureChoicesWithNeutralAlignment(allChoices, state);   // Pressure-reducing choices using neutral approaches
        var poolA5 = GetMomentumChoicesWithNegativeAlignment(allChoices, state);  // Momentum-building choices using approaches that decrease momentum or increase pressure
        var poolA6 = GetPressureChoicesWithNegativeAlignment(allChoices, state);  // Pressure-reducing choices using approaches that decrease momentum or increase pressure

        // Pool B: By Approach
        var approachRanking = GetCharacterApproachRanking(state);
        var poolB1 = GetChoicesByApproach(allChoices, approachRanking[0]);  // Choices using character's highest approach tag
        var poolB2 = GetChoicesByApproach(allChoices, approachRanking[1]);  // Choices using character's second highest approach tag
        var poolB3 = GetChoicesByApproach(allChoices, approachRanking[2]);  // Choices using character's third highest approach tag
        var poolB4 = GetChoicesByApproach(allChoices, approachRanking[3]);  // Choices using character's fourth highest approach tag
        var poolB5 = GetChoicesByApproach(allChoices, approachRanking[4]);  // Choices using character's fifth highest approach tag

        // Pool C: By Narrative Tag Status
        var blockedFocuses = GetBlockedFocuses(state.ActiveTags);
        var poolC1 = GetUnblockedChoices(allChoices, blockedFocuses);  // Choices not blocked by narrative tags
        var poolC2 = GetBlockedChoices(allChoices, blockedFocuses);    // Choices blocked by narrative tags

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
        IChoice firstChoice = SelectFirstChoice(poolB1, poolB2);
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
        IChoice fourthChoice = SelectFourthChoice(state, selectedChoices, blockedFocuses, poolC1, poolC2, allChoices, choiceScores);
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

            // Calculate total score
            int totalScore = strategicAlignmentScore + characterProficiencyScore +
                             situationalScore + focusRelevanceScore + narrativeTagModifier;

            scores[choice] = totalScore;
        }

        return scores;
    }

    private IChoice SelectFirstChoice(List<IChoice> poolB1, List<IChoice> poolB2)
    {
        // First Choice: Character Strength - Select highest-scoring choice from list B1
        if (poolB1.Any())
        {
            return poolB1.First();
        }
        // If B1 is empty, select from B2
        else if (poolB2.Any())
        {
            return poolB2.First();
        }

        return null;
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
        List<FocusTags> blockedFocuses, List<IChoice> poolC1, List<IChoice> poolC2,
        List<IChoice> allChoices, Dictionary<IChoice, int> choiceScores)
    {
        int blockedChoicesInHand = selectedChoices.Count(c => blockedFocuses.Contains(c.Focus));

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