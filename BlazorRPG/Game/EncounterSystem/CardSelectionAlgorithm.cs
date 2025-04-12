/// <summary>
/// Deterministic card selection algorithm updated for the tiered card system
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
        // Get all choices that the player has unlocked AND can play based on requirements
        List<IChoice> availableChoices = _choiceRepository.GetAvailableChoices(state);

        // STEP 1: Calculate scores for all choices with contextual modifiers
        Dictionary<IChoice, int> choiceScores = CalculateChoiceScores(availableChoices, state);

        // Pool B: By Approach
        List<ApproachTags> approachRanking = GetCharacterApproachRanking(state);
        List<IChoice> poolB1 = GetChoicesByApproach(availableChoices, approachRanking[0]);
        List<IChoice> poolB2 = GetChoicesByApproach(availableChoices, approachRanking[1]);
        List<IChoice> poolB3 = GetChoicesByApproach(availableChoices, approachRanking[2]);
        List<IChoice> poolB4 = GetChoicesByApproach(availableChoices, approachRanking[3]);
        List<IChoice> poolB5 = GetChoicesByApproach(availableChoices, approachRanking[4]);

        // Pool C: By Narrative Tag Status
        List<FocusTags> blockedFocuses = GetBlockedFocuses(state.ActiveTags);
        List<IChoice> poolC1 = GetUnblockedChoices(availableChoices, blockedFocuses);
        List<IChoice> poolC2 = GetBlockedChoices(availableChoices, blockedFocuses);

        // Pool D: By Previous Choice Context (if any)
        List<IChoice> poolD1 = state.PreviousChoice != null ? GetChoicesBySameApproach(availableChoices, state.PreviousChoice) : new List<IChoice>();
        List<IChoice> poolD2 = state.PreviousChoice != null ? GetChoicesBySameFocus(availableChoices, state.PreviousChoice) : new List<IChoice>();

        // NEW Pool E: By Tier
        List<IChoice> poolE1 = GetChoicesByTier(availableChoices, CardTiers.Master); // Tier 5
        List<IChoice> poolE2 = GetChoicesByTier(availableChoices, CardTiers.Expert); // Tier 4
        List<IChoice> poolE3 = GetChoicesByTier(availableChoices, CardTiers.Adept);  // Tier 3
        List<IChoice> poolE4 = GetChoicesByTier(availableChoices, CardTiers.Trained); // Tier 2
        List<IChoice> poolE5 = GetChoicesByTier(availableChoices, CardTiers.Novice);  // Tier 1

        // Sort all pools by score
        SortPoolByScore(poolB1, choiceScores);
        SortPoolByScore(poolB2, choiceScores);
        SortPoolByScore(poolB3, choiceScores);
        SortPoolByScore(poolB4, choiceScores);
        SortPoolByScore(poolB5, choiceScores);
        SortPoolByScore(poolC1, choiceScores);
        SortPoolByScore(poolC2, choiceScores);
        SortPoolByScore(poolD1, choiceScores);
        SortPoolByScore(poolD2, choiceScores);
        SortPoolByScore(poolE1, choiceScores);
        SortPoolByScore(poolE2, choiceScores);
        SortPoolByScore(poolE3, choiceScores);
        SortPoolByScore(poolE4, choiceScores);
        SortPoolByScore(poolE5, choiceScores);

        // STEP 3: Select Initial Choices
        List<IChoice> selectedChoices = new List<IChoice>();

        // First Choice: Highest Tier Strategic Advantage
        IChoice firstChoice = SelectHighestTierChoice(poolE1, poolE2, poolE3, poolE4, poolE5);
        if (firstChoice != null)
        {
            selectedChoices.Add(firstChoice);
        }

        // Third Choice: Progression Enabler
        IChoice thirdChoice = SelectProgressionChoice(selectedChoices, availableChoices, state, choiceScores);
        if (thirdChoice != null)
        {
            selectedChoices.Add(thirdChoice);
        }

        // Fourth Choice: Approach/Focus Diversity
        IChoice fourthChoice = SelectDiversityChoice(selectedChoices, availableChoices, choiceScores);
        if (fourthChoice != null)
        {
            selectedChoices.Add(fourthChoice);
        }

        // STEP 4: Apply Diversity Improvements
        selectedChoices = EnsureViableChoices(selectedChoices, blockedFocuses, poolC1, choiceScores);
        selectedChoices = GuaranteeStrategicOptions(selectedChoices, blockedFocuses, availableChoices, choiceScores);
        selectedChoices = EnforceCharacterIdentity(selectedChoices, approachRanking[0], poolB1, choiceScores);
        selectedChoices = EnforceFocusDiversity(selectedChoices, availableChoices, blockedFocuses, choiceScores);
        selectedChoices = EnforceApproachDiversity(selectedChoices, availableChoices, blockedFocuses, choiceScores);
        selectedChoices = EnsureStrategicOptionDiversity(selectedChoices, availableChoices, state, blockedFocuses, choiceScores);
        selectedChoices = EnsureTacticalDistinctiveness(selectedChoices, availableChoices, choiceScores);
        selectedChoices = EnforcePressureAwareChoiceDistribution(selectedChoices, availableChoices, state, choiceScores);

        // STEP 5: Handle Edge Cases
        selectedChoices = HandleSuccessWithinReach(selectedChoices, state, availableChoices, choiceScores);

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
    /// and updated with tier bonus scoring
    /// </summary>
    private Dictionary<IChoice, int> CalculateChoiceScores(List<IChoice> choices, EncounterState state)
    {
        Dictionary<IChoice, int> scores = new Dictionary<IChoice, int>();
        List<FocusTags> blockedFocuses = GetBlockedFocuses(state.ActiveTags);

        // Identify requirements that the player is close to meeting
        Dictionary<ApproachTags, int> approachRequirementTarget = new Dictionary<ApproachTags, int>();
        Dictionary<FocusTags, int> focusRequirementTarget = new Dictionary<FocusTags, int>();

        // Get the player's full collection to identify locked cards
        List<IChoice> allPlayerCards = _choiceRepository.GetAllChoices();
        foreach (IChoice card in allPlayerCards)
        {
            // Check if this is a card that can't be played yet
            if (card.Requirement != null && card.Requirement.Type != RequirementInfo.RequirementTypes.None)
            {
                if (card.Requirement.Type == RequirementInfo.RequirementTypes.Approach)
                {
                    ApproachTags approach = card.Requirement.ApproachTag;
                    int requiredValue = card.Requirement.Value;
                    int currentValue = state.TagSystem.GetEncounterStateTagValue(approach);

                    // If we're 1-2 points away from unlocking this card
                    if (currentValue < requiredValue && currentValue >= requiredValue - 2)
                    {
                        if (!approachRequirementTarget.ContainsKey(approach) ||
                            approachRequirementTarget[approach] < requiredValue)
                        {
                            approachRequirementTarget[approach] = requiredValue;
                        }
                    }
                }
                else if (card.Requirement.Type == RequirementInfo.RequirementTypes.Focus)
                {
                    FocusTags focus = card.Requirement.FocusTag;
                    int requiredValue = card.Requirement.Value;
                    int currentValue = state.TagSystem.GetFocusTagValue(focus);

                    // If we're 1-2 points away from unlocking this card
                    if (currentValue < requiredValue && currentValue >= requiredValue - 2)
                    {
                        if (!focusRequirementTarget.ContainsKey(focus) ||
                            focusRequirementTarget[focus] < requiredValue)
                        {
                            focusRequirementTarget[focus] = requiredValue;
                        }
                    }
                }
            }
        }

        foreach (IChoice choice in choices)
        {
            ApproachTags primaryApproach = GetPrimaryApproach(choice);

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

            switch (state.Location.Type)
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

            // 6. NEW: Tier Bonus (0-20 points)
            int tierBonus = ((int)choice.Tier - 1) * 5; // 0, 5, 10, 15, 20 points for tiers 1-5

            // 7. NEW: Progression Value (0-8 points)
            int progressionValue = 0;

            // Check if this choice builds an approach we need for a requirement
            foreach (KeyValuePair<ApproachTags, int> approachTarget in approachRequirementTarget)
            {
                if (GetTagModificationValue(choice, approachTarget.Key) > 0)
                {
                    // Higher bonus the closer we are to meeting the requirement
                    int currentValue = state.TagSystem.GetEncounterStateTagValue(approachTarget.Key);
                    int pointsAway = approachTarget.Value - currentValue;

                    if (pointsAway <= 1)
                        progressionValue = 8; // Just 1 point away
                    else if (pointsAway <= 2)
                        progressionValue = 5; // 2 points away
                }
            }

            // Check if this choice builds a focus we need for a requirement
            foreach (KeyValuePair<FocusTags, int> focusTarget in focusRequirementTarget)
            {
                if (choice.Focus == focusTarget.Key)
                {
                    // Higher bonus the closer we are to meeting the requirement
                    int currentValue = state.TagSystem.GetFocusTagValue(focusTarget.Key);
                    int pointsAway = focusTarget.Value - currentValue;

                    if (pointsAway <= 1)
                        progressionValue = Math.Max(progressionValue, 8); // Just 1 point away
                    else if (pointsAway <= 2)
                        progressionValue = Math.Max(progressionValue, 5); // 2 points away
                }
            }

            // Calculate total score
            int totalScore = characterProficiencyScore +
                            situationalScore +
                            focusRelevanceScore +
                            narrativeTagModifier +
                            tierBonus +
                            progressionValue;

            scores[choice] = totalScore;
        }

        return scores;
    }

    /// <summary>
    /// Gets the modification value for a specific approach tag
    /// </summary>
    private int GetTagModificationValue(IChoice choice, ApproachTags approach)
    {
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState && (ApproachTags)mod.Tag == approach)
            {
                return mod.Delta;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets choices of a specific tier
    /// </summary>
    private List<IChoice> GetChoicesByTier(List<IChoice> choices, CardTiers tier)
    {
        return choices.Where(c => c.Tier == tier).ToList();
    }

    /// <summary>
    /// Selects the highest tier choice available, prioritizing strategic advantage
    /// </summary>
    private IChoice SelectHighestTierChoice(List<IChoice> tier5, List<IChoice> tier4,
                                         List<IChoice> tier3, List<IChoice> tier2, List<IChoice> tier1)
    {
        // Try to select from each tier, starting with highest
        if (tier5.Any()) return tier5.First();
        if (tier4.Any()) return tier4.First();
        if (tier3.Any()) return tier3.First();
        if (tier2.Any()) return tier2.First();
        if (tier1.Any()) return tier1.First();

        return null;
    }

    /// <summary>
    /// Selects a choice that helps build toward requirements for locked cards
    /// </summary>
    private IChoice SelectProgressionChoice(List<IChoice> selectedChoices, List<IChoice> availableChoices,
                                         EncounterState state, Dictionary<IChoice, int> choiceScores)
    {
        // Get all choices that aren't already selected
        List<IChoice> remainingChoices = availableChoices
            .Where(c => !selectedChoices.Contains(c))
            .ToList();

        // Extract progression-oriented scores from choice scores
        var progressionScores = remainingChoices
            .Select(c => new { Choice = c, ProgressionScore = GetProgressionScore(c, state) })
            .Where(c => c.ProgressionScore > 0)
            .OrderByDescending(c => c.ProgressionScore)
            .ThenByDescending(c => choiceScores[c.Choice])
            .ToList();

        if (progressionScores.Any())
        {
            return progressionScores.First().Choice;
        }

        // If no progression choices, try diversity
        return SelectDiversityChoice(selectedChoices, availableChoices, choiceScores);
    }

    /// <summary>
    /// Calculates a progression score for a choice based on how it helps reach requirements
    /// </summary>
    private int GetProgressionScore(IChoice choice, EncounterState state)
    {
        int score = 0;
        ApproachTags primaryApproach = GetPrimaryApproach(choice);

        // Get all player cards
        List<IChoice> allPlayerCards = _choiceRepository.GetAllChoices();

        // Check if this choice helps with approach requirements
        foreach (IChoice card in allPlayerCards)
        {
            if (card.Requirement != null &&
                card.Requirement.Type == RequirementInfo.RequirementTypes.Approach &&
                card.Requirement.ApproachTag == primaryApproach)
            {
                int currentValue = state.TagSystem.GetEncounterStateTagValue(primaryApproach);
                int requiredValue = card.Requirement.Value;

                if (currentValue < requiredValue)
                {
                    int pointsAway = requiredValue - currentValue;
                    if (pointsAway <= 2)
                        score += (3 - pointsAway) * 3; // More points for being closer
                }
            }
        }

        // Check if this choice helps with focus requirements
        foreach (IChoice card in allPlayerCards)
        {
            if (card.Requirement != null &&
                card.Requirement.Type == RequirementInfo.RequirementTypes.Focus &&
                card.Requirement.FocusTag == choice.Focus)
            {
                int currentValue = state.TagSystem.GetFocusTagValue(choice.Focus);
                int requiredValue = card.Requirement.Value;

                if (currentValue < requiredValue)
                {
                    int pointsAway = requiredValue - currentValue;
                    if (pointsAway <= 2)
                        score += (3 - pointsAway) * 3; // More points for being closer
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Selects a choice that provides approach/focus diversity
    /// </summary>
    private IChoice SelectDiversityChoice(List<IChoice> selectedChoices, List<IChoice> availableChoices,
                                       Dictionary<IChoice, int> choiceScores)
    {
        // Get approaches and focuses already in the selection
        List<ApproachTags> approachesInSelection = selectedChoices
            .Select(c => GetPrimaryApproach(c))
            .Distinct()
            .ToList();

        List<FocusTags> focusesInSelection = selectedChoices
            .Select(c => c.Focus)
            .Distinct()
            .ToList();

        // Find choices with different approach AND focus
        List<IChoice> diverseChoices = availableChoices
            .Where(c => !selectedChoices.Contains(c) &&
                      !approachesInSelection.Contains(GetPrimaryApproach(c)) &&
                      !focusesInSelection.Contains(c.Focus))
            .OrderByDescending(c => choiceScores[c])
            .ToList();

        if (diverseChoices.Any())
            return diverseChoices.First();

        // If no completely diverse choice, prioritize approach diversity
        diverseChoices = availableChoices
            .Where(c => !selectedChoices.Contains(c) &&
                      !approachesInSelection.Contains(GetPrimaryApproach(c)))
            .OrderByDescending(c => choiceScores[c])
            .ToList();

        if (diverseChoices.Any())
            return diverseChoices.First();

        // Otherwise prioritize focus diversity
        diverseChoices = availableChoices
            .Where(c => !selectedChoices.Contains(c) &&
                      !focusesInSelection.Contains(c.Focus))
            .OrderByDescending(c => choiceScores[c])
            .ToList();

        if (diverseChoices.Any())
            return diverseChoices.First();

        // If all else fails, pick the highest scoring remaining choice
        return availableChoices
            .Where(c => !selectedChoices.Contains(c))
            .OrderByDescending(c => choiceScores[c])
            .FirstOrDefault();
    }

    /// <summary>
    /// IMPROVED: Ensures sufficient focus diversity among chosen cards
    /// </summary>
    private List<IChoice> EnforceFocusDiversity(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                              List<FocusTags> blockedFocuses, Dictionary<IChoice, int> choiceScores)
    {
        // Count focus distribution
        Dictionary<FocusTags, int> focusCounts = selectedChoices.GroupBy(c => c.Focus)
                                      .ToDictionary(g => g.Key, g => g.Count());

        // STRICTER REQUIREMENT: No more than 2 choices with same focus
        foreach (KeyValuePair<FocusTags, int> focusPair in focusCounts.Where(f => f.Value > 2))
        {
            FocusTags overrepresentedFocus = focusPair.Key;

            // Find choices with this focus
            List<IChoice> sameFocusChoices = selectedChoices.Where(c => c.Focus == overrepresentedFocus)
                                               .OrderBy(c => choiceScores[c])
                                               .ToList();

            // Find all possible focuses that are underrepresented
            List<FocusTags> focusesNeeded = Enum.GetValues(typeof(FocusTags))
                                  .Cast<FocusTags>()
                                  .Where(f => !blockedFocuses.Contains(f) &&
                                            (!focusCounts.ContainsKey(f) || focusCounts[f] < 1))
                                  .ToList();

            if (!focusesNeeded.Any())
                continue;

            foreach (FocusTags neededFocus in focusesNeeded)
            {
                // Find best choice with needed focus
                IChoice? diverseFocusChoice = allChoices
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
        int uniqueFocuses = selectedChoices.Select(c => c.Focus).Distinct().Count();

        if (uniqueFocuses < 3)
        {
            // Find focuses not represented
            List<FocusTags> representedFocuses = selectedChoices.Select(c => c.Focus).Distinct().ToList();
            List<FocusTags> missingFocuses = Enum.GetValues(typeof(FocusTags))
                               .Cast<FocusTags>()
                               .Where(f => !blockedFocuses.Contains(f) && !representedFocuses.Contains(f))
                               .ToList();

            if (missingFocuses.Any())
            {
                // Find the focus with the most choices (if there's a tie, take any)
                FocusTags mostRepresentedFocus = focusCounts
                    .OrderByDescending(f => f.Value)
                    .First().Key;

                List<IChoice> choicesToReplace = selectedChoices
                    .Where(c => c.Focus == mostRepresentedFocus)
                    .OrderBy(c => choiceScores[c])
                    .Take(3 - uniqueFocuses)
                    .ToList();

                foreach (IChoice? choiceToReplace in choicesToReplace)
                {
                    foreach (FocusTags missingFocus in missingFocuses.ToList())
                    {
                        IChoice? replacementChoice = allChoices
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

    /// <summary>
    /// IMPROVED: Ensures approach diversity among chosen cards
    /// </summary>
    private List<IChoice> EnforceApproachDiversity(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                                 List<FocusTags> blockedFocuses, Dictionary<IChoice, int> choiceScores)
    {
        // Count approach distribution
        Dictionary<ApproachTags, int> approachCounts = selectedChoices.GroupBy(c => GetPrimaryApproach(c))
                                         .ToDictionary(g => g.Key, g => g.Count());

        // STRICTER REQUIREMENT: No more than 2 choices can use the same approach
        foreach (KeyValuePair<ApproachTags, int> approachPair in approachCounts.Where(a => a.Value > 2))
        {
            ApproachTags overrepresentedApproach = approachPair.Key;

            // Find choices with this approach
            List<IChoice> sameApproachChoices = selectedChoices.Where(c => GetPrimaryApproach(c) == overrepresentedApproach)
                                                .OrderBy(c => choiceScores[c])
                                                .ToList();

            // Get all approaches not yet represented or underrepresented
            List<ApproachTags> approachesNeeded = Enum.GetValues(typeof(ApproachTags))
                                    .Cast<ApproachTags>()
                                    .Where(a => IsApproachTag(a) &&
                                              (!approachCounts.ContainsKey(a) || approachCounts[a] < 1))
                                    .ToList();

            if (!approachesNeeded.Any())
                continue;

            foreach (ApproachTags neededApproach in approachesNeeded)
            {
                // Find best choice with needed approach
                IChoice? diverseApproachChoice = allChoices
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
        int uniqueApproaches = selectedChoices.Select(c => GetPrimaryApproach(c)).Distinct().Count();

        if (uniqueApproaches < 3)
        {
            // Find approaches not represented
            List<ApproachTags> representedApproaches = selectedChoices.Select(c => GetPrimaryApproach(c)).Distinct().ToList();
            List<ApproachTags> missingApproaches = Enum.GetValues(typeof(ApproachTags))
                                  .Cast<ApproachTags>()
                                  .Where(a => IsApproachTag(a) && !representedApproaches.Contains(a))
                                  .ToList();

            if (missingApproaches.Any())
            {
                // Find the approach with the most choices (if there's a tie, take any)
                ApproachTags mostRepresentedApproach = approachCounts
                    .OrderByDescending(a => a.Value)
                    .First().Key;

                List<IChoice> choicesToReplace = selectedChoices
                    .Where(c => GetPrimaryApproach(c) == mostRepresentedApproach)
                    .OrderBy(c => choiceScores[c])
                    .Take(3 - uniqueApproaches)
                    .ToList();

                foreach (IChoice? choiceToReplace in choicesToReplace)
                {
                    foreach (ApproachTags missingApproach in missingApproaches.ToList())
                    {
                        IChoice? replacementChoice = allChoices
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

    /// <summary>
    /// NEW: Ensures choices don't feel like variations of the same tactic
    /// </summary>
    private List<IChoice> EnsureTacticalDistinctiveness(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                                     Dictionary<IChoice, int> choiceScores)
    {
        // Check for approach+focus combinations that appear multiple times
        var tacticalCombos = selectedChoices
            .GroupBy(c => new { Approach = GetPrimaryApproach(c), Focus = c.Focus })
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var combo in tacticalCombos)
        {
            // We have multiple choices with the same approach+focus combo
            List<IChoice> similarChoices = combo.OrderBy(c => choiceScores[c]).ToList();

            // Keep the highest scoring one
            IChoice choiceToKeep = similarChoices.Last();
            similarChoices.Remove(choiceToKeep);

            // Replace the others
            foreach (IChoice? similarChoice in similarChoices)
            {
                // Find choices with different approach+focus combinations
                List<IChoice> tacticallyDistinctChoices = allChoices
                    .Where(c => GetPrimaryApproach(c) != combo.Key.Approach &&
                              c.Focus != combo.Key.Focus &&
                              !selectedChoices.Contains(c))
                    .OrderByDescending(c => choiceScores[c])
                    .Take(1)
                    .ToList();

                if (tacticallyDistinctChoices.Any())
                {
                    selectedChoices.Remove(similarChoice);
                    selectedChoices.Add(tacticallyDistinctChoices.First());
                }
            }
        }

        // Also check for same approach+effectType combinations
        var approachEffectCombos = selectedChoices
            .GroupBy(c => new { Approach = GetPrimaryApproach(c), EffectType = c.EffectType })
            .Where(g => g.Count() > 2) // Allow at most 2 of same approach+effect
            .ToList();

        foreach (var combo in approachEffectCombos)
        {
            // We have too many of the same approach+effect combo
            List<IChoice> similarChoices = combo.OrderBy(c => choiceScores[c]).ToList();

            while (similarChoices.Count > 2)
            {
                IChoice lowestChoice = similarChoices.First();
                similarChoices.Remove(lowestChoice);

                // Find a tactically different choice
                IChoice? differentChoice = allChoices
                    .Where(c => GetPrimaryApproach(c) != combo.Key.Approach &&
                              !selectedChoices.Contains(c))
                    .OrderByDescending(c => choiceScores[c])
                    .FirstOrDefault();

                if (differentChoice != null)
                {
                    selectedChoices.Remove(lowestChoice);
                    selectedChoices.Add(differentChoice);
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
            state.Location.Type == EncounterTypes.Physical)
        {
            // Check if we have strategic diversity
            bool hasPhysicalOption = selectedChoices.Any(c => c.Focus == FocusTags.Physical);
            bool hasRelationshipOption = selectedChoices.Any(c => c.Focus == FocusTags.Relationship);
            bool hasResourceOption = selectedChoices.Any(c => c.Focus == FocusTags.Resource);

            // If we have ONLY physical options, add a social/resource option
            if (hasPhysicalOption && !hasRelationshipOption && !hasResourceOption)
            {
                // Find social/resource choices
                List<IChoice> socialChoices = allChoices
                    .Where(c => (c.Focus == FocusTags.Relationship || c.Focus == FocusTags.Resource) &&
                              !selectedChoices.Contains(c) &&
                              !blockedFocuses.Contains(c.Focus))
                    .OrderByDescending(c => choiceScores[c])
                    .Take(1)
                    .ToList();

                if (socialChoices.Any())
                {
                    // Replace lowest scoring physical choice
                    List<IChoice> physicalChoices = selectedChoices
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

    /// <summary>
    /// Ensures pressure reduction choices only appear when there's meaningful pressure to reduce
    /// </summary>
    private List<IChoice> EnforcePressureAwareChoiceDistribution(List<IChoice> selectedChoices, List<IChoice> allChoices,
                                                               EncounterState state, Dictionary<IChoice, int> choiceScores)
    {
        // Determine current pressure context
        double pressureRatio = (double)state.Pressure / state.Location.MaxPressure;

        // Different selection strategies based on pressure levels
        if (pressureRatio < 0.2) // Low pressure (0-20% of max)
        {
            // At low pressure, focus primarily on momentum-building choices
            int pressureChoiceCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);

            // Keep at most 1 pressure choice at low pressure
            if (pressureChoiceCount > 1)
            {
                // Get all pressure choices, ordered by score
                List<IChoice> pressureChoices = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure)
                    .OrderBy(c => choiceScores[c])
                    .ToList();

                // Keep only the highest-scoring pressure choice
                for (int i = 0; i < pressureChoices.Count - 1; i++)
                {
                    // Find momentum choices not currently in the selection
                    IChoice? replacementChoice = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum &&
                                  !selectedChoices.Contains(c))
                        .OrderByDescending(c => choiceScores[c])
                        .FirstOrDefault();

                    if (replacementChoice != null)
                    {
                        selectedChoices.Remove(pressureChoices[i]);
                        selectedChoices.Add(replacementChoice);
                    }
                }
            }
        }
        else if (pressureRatio >= 0.5) // High pressure (50%+ of max)
        {
            // At high pressure, ensure enough pressure-reducing choices
            int pressureChoiceCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);

            // Aim for at least 2 pressure choices at high pressure
            if (pressureChoiceCount < 2)
            {
                // Find how many pressure choices to add
                int pressureChoicesToAdd = 2 - pressureChoiceCount;

                // Find lowest-scoring momentum choices 
                List<IChoice> momentumChoices = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum)
                    .OrderBy(c => choiceScores[c])
                    .Take(pressureChoicesToAdd)
                    .ToList();

                // Find pressure choices not in selection
                List<IChoice> pressureChoicesNotSelected = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure &&
                              !selectedChoices.Contains(c))
                    .OrderByDescending(c => choiceScores[c])
                    .Take(pressureChoicesToAdd)
                    .ToList();

                // Replace momentum choices with pressure choices
                for (int i = 0; i < Math.Min(momentumChoices.Count, pressureChoicesNotSelected.Count); i++)
                {
                    selectedChoices.Remove(momentumChoices[i]);
                    selectedChoices.Add(pressureChoicesNotSelected[i]);
                }
            }
        }
        else // Medium pressure (20-50% of max)
        {
            // At medium pressure, aim for balanced distribution
            int pressureChoiceCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);

            // Aim for 1-2 pressure choices at medium pressure
            if (pressureChoiceCount < 1)
            {
                // Add a pressure choice
                IChoice? momentumChoice = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Momentum)
                    .OrderBy(c => choiceScores[c])
                    .FirstOrDefault();

                IChoice? pressureChoice = allChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure &&
                              !selectedChoices.Contains(c))
                    .OrderByDescending(c => choiceScores[c])
                    .FirstOrDefault();

                if (momentumChoice != null && pressureChoice != null)
                {
                    selectedChoices.Remove(momentumChoice);
                    selectedChoices.Add(pressureChoice);
                }
            }
            else if (pressureChoiceCount > 2)
            {
                // Too many pressure choices, replace some with momentum choices
                List<IChoice> pressureChoices = selectedChoices
                    .Where(c => c.EffectType == EffectTypes.Pressure)
                    .OrderBy(c => choiceScores[c])
                    .ToList();

                for (int i = 0; i < pressureChoices.Count - 2; i++)
                {
                    IChoice? momentumChoice = allChoices
                        .Where(c => c.EffectType == EffectTypes.Momentum &&
                                  !selectedChoices.Contains(c))
                        .OrderByDescending(c => choiceScores[c])
                        .FirstOrDefault();

                    if (momentumChoice != null)
                    {
                        selectedChoices.Remove(pressureChoices[i]);
                        selectedChoices.Add(momentumChoice);
                    }
                }
            }
        }

        return selectedChoices;
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
            { ApproachTags.Concealment, state.TagSystem.GetEncounterStateTagValue(ApproachTags.Concealment) }
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
               tag == ApproachTags.Concealment;
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

}