/// <summary>
/// Generates choice sets based on the current Encounter state
/// </summary>
public class ChoiceGenerator
{
    private readonly ChoiceRepository _repository;

    public ChoiceGenerator(ChoiceRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Generate a set of choices based on the current Encounter state
    /// </summary>
    public List<Choice> GenerateChoiceSet(EncounterState state)
    {
        // Determine momentum/pressure distribution based on M:P ratio
        int momentumChoicesCount = 3; // Default
        int pressureChoicesCount = 3; // Default

        // Adjust based on M:P ratio
        if (state.MomentumToPressureRatio > 2.0)
        {
            // More pressure choices when momentum is high
            momentumChoicesCount = 2;
            pressureChoicesCount = 4;
        }
        else if (state.MomentumToPressureRatio < 0.5)
        {
            // More momentum choices when pressure is high
            momentumChoicesCount = 4;
            pressureChoicesCount = 2;
        }

        // Get ranked approach tags (highest to lowest)
        List<ApproachTypes> rankedApproaches = GetRankedApproachTypess(state);

        // Get ranked focus tags (highest to lowest)
        List<FocusTypes> rankedFocuses = GetRankedFocusTypess(state);

        // Primary emphasis: Highest approach tag and highest focus tag
        ApproachTypes highestApproach = rankedApproaches.First();
        FocusTypes highestFocus = rankedFocuses.First();

        // Secondary emphasis: Tags with biggest gaps to highest values
        ApproachTypes secondaryApproach = GetTagWithBiggestGap(state.ApproachTypesDic, highestApproach);
        FocusTypes secondaryFocus = GetTagWithBiggestGap(state.FocusTypesDic, highestFocus);

        // Start building the choice set
        List<Choice> selectedChoices = new List<Choice>();

        // Add primary emphasis choices
        AddIfAvailable(selectedChoices, highestApproach, highestFocus, EffectTypes.Momentum);
        AddIfAvailable(selectedChoices, highestApproach, highestFocus, EffectTypes.Pressure);

        // Add secondary emphasis choices to fill remaining slots
        FillWithSecondaryChoices(
            selectedChoices,
            state,
            rankedApproaches,
            rankedFocuses,
            secondaryApproach,
            secondaryFocus,
            momentumChoicesCount,
            pressureChoicesCount);

        // Ensure diversity requirements are met
        EnsureTagDiversity(selectedChoices, state, rankedApproaches, rankedFocuses);

        // Final balance check
        EnsureMomentumPressureBalance(selectedChoices, momentumChoicesCount, pressureChoicesCount);

        return selectedChoices;
    }

    /// <summary>
    /// Get approach tags ranked by value (highest to lowest)
    /// </summary>
    private List<ApproachTypes> GetRankedApproachTypess(EncounterState state)
    {
        return state.ApproachTypesDic
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetApproachPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();
    }

    /// <summary>
    /// Get focus tags ranked by value (highest to lowest)
    /// </summary>
    private List<FocusTypes> GetRankedFocusTypess(EncounterState state)
    {
        return state.FocusTypesDic
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetFocusPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();
    }

    /// <summary>
    /// Get the priority value for an approach tag (used for tie-breaking)
    /// </summary>
    private int GetApproachPriority(ApproachTypes tag)
    {
        switch (tag)
        {
            case ApproachTypes.Force: return 1;
            case ApproachTypes.Charm: return 2;
            case ApproachTypes.Wit: return 3;
            case ApproachTypes.Finesse: return 4;
            case ApproachTypes.Stealth: return 5;
            default: return 99;
        }
    }

    /// <summary>
    /// Get the priority value for a focus tag (used for tie-breaking)
    /// </summary>
    private int GetFocusPriority(FocusTypes tag)
    {
        switch (tag)
        {
            case FocusTypes.Relationship: return 1;
            case FocusTypes.Information: return 2;
            case FocusTypes.Physical: return 3;
            case FocusTypes.Resource: return 4;
            case FocusTypes.Environment: return 5;
            default: return 99;
        }
    }

    /// <summary>
    /// Find the tag with the biggest gap to the highest value tag
    /// </summary>
    private T GetTagWithBiggestGap<T>(Dictionary<T, int> tagValues, T highestTag)
    {
        int highestValue = tagValues[highestTag];

        return tagValues
            .Where(pair => !pair.Key.Equals(highestTag)) // Exclude the highest tag
            .OrderByDescending(pair => highestValue - pair.Value) // Order by gap size (descending)
            .ThenBy(pair => pair.Value) // For equal gaps, prefer lower values
            .Select(pair => pair.Key)
            .FirstOrDefault();
    }

    /// <summary>
    /// Add a choice to the selected set if available
    /// </summary>
    private void AddIfAvailable(List<Choice> selectedChoices, ApproachTypes approach, FocusTypes focus, EffectTypes effect)
    {
        Choice choice = _repository.GetChoice(approach, focus, effect);
        if (choice != null && !selectedChoices.Contains(choice))
        {
            selectedChoices.Add(choice);
        }
    }

    /// <summary>
    /// Fill the remaining slots with secondary emphasis choices
    /// </summary>
    private void FillWithSecondaryChoices(
        List<Choice> selectedChoices,
        EncounterState state,
        List<ApproachTypes> rankedApproaches,
        List<FocusTypes> rankedFocuses,
        ApproachTypes secondaryApproach,
        FocusTypes secondaryFocus,
        int targetMomentumCount,
        int targetPressureCount)
    {
        // Count how many of each type we currently have
        int currentMomentumCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);
        int currentPressureCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);

        // Add secondary approach + primary focus
        if (currentMomentumCount < targetMomentumCount)
        {
            AddIfAvailable(selectedChoices, secondaryApproach, rankedFocuses.First(), EffectTypes.Momentum);
            currentMomentumCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);
        }

        if (currentPressureCount < targetPressureCount)
        {
            AddIfAvailable(selectedChoices, secondaryApproach, rankedFocuses.First(), EffectTypes.Pressure);
            currentPressureCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);
        }

        // Add primary approach + secondary focus
        if (currentMomentumCount < targetMomentumCount)
        {
            AddIfAvailable(selectedChoices, rankedApproaches.First(), secondaryFocus, EffectTypes.Momentum);
            currentMomentumCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);
        }

        if (currentPressureCount < targetPressureCount)
        {
            AddIfAvailable(selectedChoices, rankedApproaches.First(), secondaryFocus, EffectTypes.Pressure);
            currentPressureCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);
        }

        // Continue adding choices until we have enough
        foreach (ApproachTypes approach in rankedApproaches)
        {
            foreach (FocusTypes focus in rankedFocuses)
            {
                // Add momentum choices if needed
                if (currentMomentumCount < targetMomentumCount)
                {
                    AddIfAvailable(selectedChoices, approach, focus, EffectTypes.Momentum);
                    currentMomentumCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Momentum);
                }

                // Add pressure choices if needed
                if (currentPressureCount < targetPressureCount)
                {
                    AddIfAvailable(selectedChoices, approach, focus, EffectTypes.Pressure);
                    currentPressureCount = selectedChoices.Count(c => c.EffectType == EffectTypes.Pressure);
                }

                // Check if we have enough choices
                if (currentMomentumCount >= targetMomentumCount && currentPressureCount >= targetPressureCount)
                {
                    break;
                }
            }

            // Check if we have enough choices
            if (currentMomentumCount >= targetMomentumCount && currentPressureCount >= targetPressureCount)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Ensure the choice set meets tag diversity requirements
    /// </summary>
    private void EnsureTagDiversity(
        List<Choice> selectedChoices,
        EncounterState state,
        List<ApproachTypes> rankedApproaches,
        List<FocusTypes> rankedFocuses)
    {
        // Check approach tag diversity
        List<ApproachTypes> ApproachTypess = selectedChoices.Select(c => c.ApproachType).Distinct().ToList();
        if (ApproachTypess.Count < 3)
        {
            // Need to add more approach diversity
            foreach (ApproachTypes approach in rankedApproaches)
            {
                if (!ApproachTypess.Contains(approach))
                {
                    // Try to add a choice with this approach
                    foreach (FocusTypes focus in rankedFocuses)
                    {
                        // Try momentum choice first
                        Choice momentumChoice = _repository.GetChoice(approach, focus, EffectTypes.Momentum);
                        if (momentumChoice != null && !selectedChoices.Contains(momentumChoice))
                        {
                            // Replace a choice that already has a well-represented approach
                            ReplaceChoiceForDiversity(selectedChoices, momentumChoice, EffectTypes.Momentum);
                            ApproachTypess = selectedChoices.Select(c => c.ApproachType).Distinct().ToList();
                            break;
                        }

                        // Try pressure choice if momentum didn't work
                        Choice pressureChoice = _repository.GetChoice(approach, focus, EffectTypes.Pressure);
                        if (pressureChoice != null && !selectedChoices.Contains(pressureChoice))
                        {
                            // Replace a choice that already has a well-represented approach
                            ReplaceChoiceForDiversity(selectedChoices, pressureChoice, EffectTypes.Pressure);
                            ApproachTypess = selectedChoices.Select(c => c.ApproachType).Distinct().ToList();
                            break;
                        }
                    }
                }

                // Check if we now have enough diversity
                if (ApproachTypess.Count >= 3)
                {
                    break;
                }
            }
        }

        // Check focus tag diversity
        List<FocusTypes> FocusTypess = selectedChoices.Select(c => c.FocusType).Distinct().ToList();
        if (FocusTypess.Count < 3)
        {
            // Need to add more focus diversity
            foreach (FocusTypes focus in rankedFocuses)
            {
                if (!FocusTypess.Contains(focus))
                {
                    // Try to add a choice with this focus
                    foreach (ApproachTypes approach in rankedApproaches)
                    {
                        // Try momentum choice first
                        Choice momentumChoice = _repository.GetChoice(approach, focus, EffectTypes.Momentum);
                        if (momentumChoice != null && !selectedChoices.Contains(momentumChoice))
                        {
                            // Replace a choice that already has a well-represented focus
                            ReplaceChoiceForDiversity(selectedChoices, momentumChoice, EffectTypes.Momentum, false);
                            FocusTypess = selectedChoices.Select(c => c.FocusType).Distinct().ToList();
                            break;
                        }

                        // Try pressure choice if momentum didn't work
                        Choice pressureChoice = _repository.GetChoice(approach, focus, EffectTypes.Pressure);
                        if (pressureChoice != null && !selectedChoices.Contains(pressureChoice))
                        {
                            // Replace a choice that already has a well-represented focus
                            ReplaceChoiceForDiversity(selectedChoices, pressureChoice, EffectTypes.Pressure, false);
                            FocusTypess = selectedChoices.Select(c => c.FocusType).Distinct().ToList();
                            break;
                        }
                    }
                }

                // Check if we now have enough diversity
                if (FocusTypess.Count >= 3)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Replace a choice to improve tag diversity while maintaining momentum/pressure balance
    /// </summary>
    private void ReplaceChoiceForDiversity(
        List<Choice> choices,
        Choice newChoice,
        EffectTypes effectType,
        bool replaceByApproach = true)
    {
        // Find candidates for replacement (same effect type)
        List<Choice> candidates = choices.Where(c => c.EffectType == effectType).ToList();

        if (replaceByApproach)
        {
            // Count occurrences of each approach tag
            Dictionary<ApproachTypes, int> approachCounts = candidates.GroupBy(c => c.ApproachType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find the most common approach tag
            ApproachTypes mostCommonApproach = approachCounts.OrderByDescending(pair => pair.Value).First().Key;

            // Replace a choice with that approach
            Choice toReplace = candidates.First(c => c.ApproachType == mostCommonApproach);
            int index = choices.IndexOf(toReplace);
            if (index >= 0)
            {
                choices[index] = newChoice;
            }
        }
        else
        {
            // Count occurrences of each focus tag
            Dictionary<FocusTypes, int> focusCounts = candidates.GroupBy(c => c.FocusType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find the most common focus tag
            FocusTypes mostCommonFocus = focusCounts.OrderByDescending(pair => pair.Value).First().Key;

            // Replace a choice with that focus
            Choice toReplace = candidates.First(c => c.FocusType == mostCommonFocus);
            int index = choices.IndexOf(toReplace);
            if (index >= 0)
            {
                choices[index] = newChoice;
            }
        }
    }

    /// <summary>
    /// Ensure we have the correct momentum/pressure balance
    /// </summary>
    private void EnsureMomentumPressureBalance(
        List<Choice> choices,
        int targetMomentumCount,
        int targetPressureCount)
    {
        int currentMomentumCount = choices.Count(c => c.EffectType == EffectTypes.Momentum);
        int currentPressureCount = choices.Count(c => c.EffectType == EffectTypes.Pressure);

        // If we have too many momentum choices
        while (currentMomentumCount > targetMomentumCount && currentPressureCount < targetPressureCount)
        {
            // Find a momentum choice to replace
            Choice momentumChoice = choices.First(c => c.EffectType == EffectTypes.Momentum);

            // Try to replace it with the corresponding pressure choice
            Choice pressureChoice = _repository.GetChoice(momentumChoice.ApproachType, momentumChoice.FocusType, EffectTypes.Pressure);

            if (pressureChoice != null && !choices.Contains(pressureChoice))
            {
                int index = choices.IndexOf(momentumChoice);
                choices[index] = pressureChoice;

                currentMomentumCount--;
                currentPressureCount++;
            }
            else
            {
                // Can't replace this one, try another
                choices.Remove(momentumChoice);
                choices.Add(momentumChoice);
            }
        }

        // If we have too many pressure choices
        while (currentPressureCount > targetPressureCount && currentMomentumCount < targetMomentumCount)
        {
            // Find a pressure choice to replace
            Choice pressureChoice = choices.First(c => c.EffectType == EffectTypes.Pressure);

            // Try to replace it with the corresponding momentum choice
            Choice momentumChoice = _repository.GetChoice(pressureChoice.ApproachType, pressureChoice.FocusType, EffectTypes.Momentum);

            if (momentumChoice != null && !choices.Contains(momentumChoice))
            {
                int index = choices.IndexOf(pressureChoice);
                choices[index] = momentumChoice;

                currentPressureCount--;
                currentMomentumCount++;
            }
            else
            {
                // Can't replace this one, try another
                choices.Remove(pressureChoice);
                choices.Add(pressureChoice);
            }
        }

        // Make sure we have exactly 6 choices
        while (choices.Count > 6)
        {
            choices.RemoveAt(choices.Count - 1);
        }
    }
}
