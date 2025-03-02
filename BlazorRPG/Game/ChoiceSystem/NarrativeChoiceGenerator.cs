/// <summary>
/// Generates choice sets based on the narrative context of the current encounter state
/// </summary>
public class NarrativeChoiceGenerator
{
    private readonly ChoiceRepository _repository;
    private readonly Dictionary<NarrativeTypes, List<Choice>> _narrativeChoicesCache = new Dictionary<NarrativeTypes, List<Choice>>();
    private readonly Dictionary<NarrativePhases, List<Choice>> _phaseChoicesCache = new Dictionary<NarrativePhases, List<Choice>>();
    public NarrativePhases NarrativePhase;

    public NarrativeChoiceGenerator(ChoiceRepository repository)
    {
        _repository = repository;
        InitializeNarrativeChoiceMappings();
    }

    /// <summary>
    /// Generate a set of choices based on the current encounter state
    /// </summary>
    public List<Choice> GenerateChoiceSet(EncounterState state)
    {
        // 1. Determine narrative context from current state
        NarrativeContext context = DetermineNarrativeContext(state);
        NarrativePhase = context.Phase;

        // 2. Get primary choices that define narrative direction
        List<Choice> selectedChoices = SelectPrimaryChoices(state, context);

        // 3. Complete choice set with complementary choices
        AddComplementaryChoices(selectedChoices, state, context);

        // 4. Ensure balance requirements
        EnsureMomentumPressureBalance(selectedChoices, state);
        EnsureTagDiversity(selectedChoices);

        return selectedChoices;
    }

    #region Context Determination

    /// <summary>
    /// Determine the narrative context based on the current state
    /// </summary>
    private NarrativeContext DetermineNarrativeContext(EncounterState state)
    {
        // Get dominant and secondary approaches and focuses
        (ApproachTypes dominantApproach, ApproachTypes secondaryApproach) = GetRankedApproaches(state);
        (FocusTypes dominantFocus, FocusTypes secondaryFocus) = GetRankedFocuses(state);

        // Determine narrative phase
        NarrativePhases phase = DetermineNarrativePhase(state);

        // Map to narrative types
        NarrativeTypes primaryNarrative = MapToNarrativeType(dominantApproach, dominantFocus, state);
        NarrativeTypes secondaryNarrative = DetermineSecondaryNarrative(state, primaryNarrative, secondaryApproach, secondaryFocus);

        return new NarrativeContext(
            primaryNarrative,
            secondaryNarrative,
            phase,
            dominantApproach,
            dominantFocus,
            secondaryApproach,
            secondaryFocus);
    }

    /// <summary>
    /// Get the top two approach tags by value
    /// </summary>
    private (ApproachTypes dominant, ApproachTypes secondary) GetRankedApproaches(EncounterState state)
    {
        var ranked = state.ApproachTypesDic
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetApproachPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();

        return (ranked.Count > 0 ? ranked[0] : ApproachTypes.Force,
                ranked.Count > 1 ? ranked[1] : ApproachTypes.Charm);
    }

    /// <summary>
    /// Get the top two focus tags by value
    /// </summary>
    private (FocusTypes dominant, FocusTypes secondary) GetRankedFocuses(EncounterState state)
    {
        var ranked = state.FocusTypesDic
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetFocusPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();

        return (ranked.Count > 0 ? ranked[0] : FocusTypes.Relationship,
                ranked.Count > 1 ? ranked[1] : FocusTypes.Information);
    }

    /// <summary>
    /// Get the priority for an approach (used for tie-breaking)
    /// </summary>
    private int GetApproachPriority(ApproachTypes approach)
    {
        switch (approach)
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
    /// Get the priority for a focus (used for tie-breaking)
    /// </summary>
    private int GetFocusPriority(FocusTypes focus)
    {
        switch (focus)
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
    /// Determine the current narrative phase based on momentum and pressure
    /// </summary>
    private NarrativePhases DetermineNarrativePhase(EncounterState state)
    {
        float mRatio = state.Momentum / 10.0f;
        float pRatio = state.Pressure / 10.0f;
        float mpBalance = mRatio - pRatio; // Direct momentum-to-pressure comparison

        if (mRatio < 0.2f && pRatio < 0.2f)
            return NarrativePhases.Introduction;

        if (mRatio > 0.6f && pRatio < 0.4f) // Strong momentum, fading pressure
            return NarrativePhases.Resolution;

        if (mpBalance > 0.2f && mRatio > 0.6f) // Momentum significantly outweighs pressure
            return NarrativePhases.Breakthrough;

        if (pRatio >= 0.6f && mRatio < 0.5f)
            return NarrativePhases.Escalation;

        if (mpBalance < -0.2f && pRatio > 0.6f) // Pressure remains dominant
            return NarrativePhases.Escalation;

        if (pRatio >= 0.8f && mRatio >= 0.7f)
            return NarrativePhases.Crisis;

        return NarrativePhases.Complication; // Default fallback
    }

    /// <summary>
    /// Map approach and focus to a narrative type
    /// </summary>
    private NarrativeTypes MapToNarrativeType(ApproachTypes approach, FocusTypes focus, EncounterState state)
    {
        // Different combinations of approach/focus map to narrative types
        if (approach == ApproachTypes.Force)
        {
            if (focus == FocusTypes.Physical || focus == FocusTypes.Environment)
                return NarrativeTypes.Confrontation;
            else if (focus == FocusTypes.Resource)
                return NarrativeTypes.Negotiation;
            else
                return NarrativeTypes.Maneuvering;
        }
        else if (approach == ApproachTypes.Charm)
        {
            if (focus == FocusTypes.Relationship)
                return NarrativeTypes.Persuasion;
            else if (focus == FocusTypes.Resource)
                return NarrativeTypes.Negotiation;
            else
                return NarrativeTypes.Deception;
        }
        else if (approach == ApproachTypes.Wit)
        {
            if (focus == FocusTypes.Information)
                return NarrativeTypes.Investigation;
            else if (focus == FocusTypes.Environment)
                return NarrativeTypes.Adaptation;
            else
                return NarrativeTypes.Examination;
        }
        else if (approach == ApproachTypes.Finesse)
        {
            if (focus == FocusTypes.Physical)
                return NarrativeTypes.Maneuvering;
            else if (focus == FocusTypes.Environment)
                return NarrativeTypes.Adaptation;
            else
                return NarrativeTypes.Examination;
        }
        else if (approach == ApproachTypes.Stealth)
        {
            if (focus == FocusTypes.Information || focus == FocusTypes.Relationship)
                return NarrativeTypes.Deception;
            else
                return NarrativeTypes.Maneuvering;
        }

        // Default fallback
        return NarrativeTypes.Confrontation;
    }

    /// <summary>
    /// Determine the secondary narrative that complements the primary one
    /// </summary>
    private NarrativeTypes DetermineSecondaryNarrative(EncounterState state, NarrativeTypes primaryNarrative,
                                                      ApproachTypes secondaryApproach, FocusTypes secondaryFocus)
    {
        // Create a complementary narrative using secondary tags
        NarrativeTypes secondaryNarrativeFromTags = MapToNarrativeType(secondaryApproach, secondaryFocus, state);

        // If it's the same as primary, choose something different
        if (secondaryNarrativeFromTags == primaryNarrative)
        {
            // Use phase to determine a complementary narrative
            switch (DetermineNarrativePhase(state))
            {
                case NarrativePhases.Introduction:
                    return primaryNarrative == NarrativeTypes.Investigation ? NarrativeTypes.Persuasion : NarrativeTypes.Investigation;

                case NarrativePhases.Complication:
                    return primaryNarrative == NarrativeTypes.Confrontation ? NarrativeTypes.Deception : NarrativeTypes.Confrontation;

                case NarrativePhases.Escalation:
                    return primaryNarrative == NarrativeTypes.Confrontation ? NarrativeTypes.Maneuvering : NarrativeTypes.Confrontation;

                case NarrativePhases.Breakthrough:
                    return primaryNarrative == NarrativeTypes.Examination ? NarrativeTypes.Adaptation : NarrativeTypes.Examination;

                case NarrativePhases.Resolution:
                    return primaryNarrative == NarrativeTypes.Negotiation ? NarrativeTypes.Persuasion : NarrativeTypes.Negotiation;

                case NarrativePhases.Crisis:
                    return primaryNarrative == NarrativeTypes.Confrontation ? NarrativeTypes.Maneuvering : NarrativeTypes.Confrontation;

                default:
                    // Fallback to a different narrative
                    return primaryNarrative == NarrativeTypes.Confrontation ? NarrativeTypes.Persuasion : NarrativeTypes.Confrontation;
            }
        }

        return secondaryNarrativeFromTags;
    }

    #endregion

    #region Choice Selection

    /// <summary>
    /// Select primary choices based on the narrative context
    /// </summary>
    private List<Choice> SelectPrimaryChoices(EncounterState state, NarrativeContext context)
    {
        List<Choice> primaryChoices = new List<Choice>();

        // Get choices that match the primary narrative
        List<Choice> narrativeChoices = GetChoicesForNarrative(context.PrimaryNarrative);

        // Calculate scores for each choice
        Dictionary<Choice, float> choiceScores = new Dictionary<Choice, float>();
        foreach (Choice choice in narrativeChoices)
        {
            float score = CalculateContextRelevance(choice, context, state);
            choiceScores[choice] = score;
        }

        // Select top choices for primary narrative
        List<Choice> candidateChoices = choiceScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .Select(kvp => kvp.Key)
            .ToList();

        primaryChoices.AddRange(candidateChoices);

        return primaryChoices;
    }

    /// <summary>
    /// Add complementary choices to complete the choice set
    /// </summary>
    private void AddComplementaryChoices(List<Choice> selectedChoices, EncounterState state, NarrativeContext context)
    {
        // Get choices that match the secondary narrative
        List<Choice> secondaryNarrativeChoices = GetChoicesForNarrative(context.SecondaryNarrative);

        // Calculate scores for each choice
        Dictionary<Choice, float> choiceScores = new Dictionary<Choice, float>();
        foreach (Choice choice in secondaryNarrativeChoices)
        {
            if (!selectedChoices.Contains(choice))
            {
                float score = CalculateContextRelevance(choice, context, state);
                score *= 0.8f; // Slightly lower priority than primary choices
                choiceScores[choice] = score;
            }
        }

        // Add top secondary narrative choices
        int secondaryChoicesToAdd = Math.Min(2, 6 - selectedChoices.Count);
        List<Choice> secondaryChoices = choiceScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(secondaryChoicesToAdd)
            .Select(kvp => kvp.Key)
            .ToList();

        selectedChoices.AddRange(secondaryChoices);

        // Add phase-appropriate choice if needed
        if (selectedChoices.Count < 6)
        {
            List<Choice> phaseChoices = GetChoicesForPhase(context.Phase);

            // Filter out already selected choices
            phaseChoices = phaseChoices
                .Where(c => !selectedChoices.Contains(c))
                .ToList();

            // Score phase choices
            Dictionary<Choice, float> phaseScores = new Dictionary<Choice, float>();
            foreach (Choice choice in phaseChoices)
            {
                float score = CalculatePhaseRelevance(choice, context.Phase, state);
                phaseScores[choice] = score;
            }

            // Add top phase choice
            Choice phaseChoice = phaseScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (phaseChoice != null)
                selectedChoices.Add(phaseChoice);
        }

        // If we still need more choices, add based on secondary tags
        while (selectedChoices.Count < 6)
        {
            Choice additionalChoice = SelectChoiceForSecondaryTags(selectedChoices, state, context);
            if (additionalChoice != null)
            {
                selectedChoices.Add(additionalChoice);
            }
            else
            {
                // Fallback: add any choice not already selected
                Choice fallbackChoice = _repository.GetAllChoices()
                    .Where(c => !selectedChoices.Contains(c))
                    .OrderBy(c => Guid.NewGuid()) // Pseudo-random but deterministic based on state
                    .FirstOrDefault();

                if (fallbackChoice != null)
                    selectedChoices.Add(fallbackChoice);
                else
                    break; // No more choices available
            }
        }
    }

    /// <summary>
    /// Select a choice based on secondary tag values
    /// </summary>
    private Choice SelectChoiceForSecondaryTags(List<Choice> selectedChoices, EncounterState state, NarrativeContext context)
    {
        // Get secondary tag values
        ApproachTypes secondaryApproach = context.SecondaryApproach;
        FocusTypes secondaryFocus = context.SecondaryFocus;

        // Try to find choices using these tags that aren't already selected
        List<Choice> candidates = _repository.GetAllChoices()
            .Where(c => (c.ApproachType == secondaryApproach || c.FocusType == secondaryFocus) &&
                        !selectedChoices.Contains(c))
            .ToList();

        if (candidates.Count == 0)
            return null;

        // Score candidates
        Dictionary<Choice, float> scores = new Dictionary<Choice, float>();
        foreach (Choice choice in candidates)
        {
            float score = 0f;

            // Base score from tag match
            if (choice.ApproachType == secondaryApproach)
                score += 10f;
            if (choice.FocusType == secondaryFocus)
                score += 10f;

            // Consider momentum/pressure ratio
            if (choice.EffectType == EffectTypes.Momentum && state.Pressure > state.Momentum)
                score += 5f;
            if (choice.EffectType == EffectTypes.Pressure && state.Momentum > state.Pressure * 1.5f)
                score += 5f;

            // Add small variations based on tag values
            score += state.GetTagValue(choice.ApproachType) * 0.2f;
            score += state.GetTagValue(choice.FocusType) * 0.2f;

            // Add a bit of uniqueness based on specific tag values
            score += (state.GetTagValue(choice.ApproachType) % 3) * 0.1f;
            score += (state.GetTagValue(choice.FocusType) % 4) * 0.1f;

            scores[choice] = score;
        }

        // Return the highest-scoring candidate
        return scores
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();
    }

    /// <summary>
    /// Calculate how relevant a choice is to the current narrative context
    /// </summary>
    private float CalculateContextRelevance(Choice choice, NarrativeContext context, EncounterState state)
    {
        float score = 0f;

        // Base relevance from matching approach/focus
        if (choice.ApproachType == context.DominantApproach)
            score += 10f;
        if (choice.FocusType == context.DominantFocus)
            score += 10f;

        // Secondary matches have less impact
        if (choice.ApproachType == context.SecondaryApproach)
            score += 5f;
        if (choice.FocusType == context.SecondaryFocus)
            score += 5f;

        // Adjust by actual tag values (creates sensitivity to small tag changes)
        score *= (0.5f + (state.GetTagValue(choice.ApproachType) / 10.0f));
        score *= (0.5f + (state.GetTagValue(choice.FocusType) / 10.0f));

        // Phase relevance
        if (IsChoiceRelevantToPhase(choice, context.Phase))
            score *= 1.2f;

        // Add momentum/pressure considerations
        if (choice.EffectType == EffectTypes.Momentum && state.Pressure > state.Momentum)
            score *= 1.15f;
        if (choice.EffectType == EffectTypes.Pressure && state.Momentum > state.Pressure * 1.5f)
            score *= 1.15f;

        // Apply a unique modifier based on specific tag values
        // This ensures small tag changes affect ordering
        score += (state.GetTagValue(choice.ApproachType) % 3) * 0.2f;
        score += (state.GetTagValue(choice.FocusType) % 4) * 0.2f;

        // Add slight variation based on momentum and pressure values
        score += (state.Momentum % 5) * 0.05f;
        score += (state.Pressure % 5) * 0.05f;

        return score;
    }

    /// <summary>
    /// Calculate how relevant a choice is to the current narrative phase
    /// </summary>
    private float CalculatePhaseRelevance(Choice choice, NarrativePhases phase, EncounterState state)
    {
        float score = 0f;

        // Base score based on appropriate effect type for this phase
        switch (phase)
        {
            case NarrativePhases.Introduction:
                // Balanced in early phase
                score += 10f;
                break;

            case NarrativePhases.Complication:
                // Slight preference for momentum
                if (choice.EffectType == EffectTypes.Momentum)
                    score += 12f;
                else
                    score += 8f;
                break;

            case NarrativePhases.Escalation:
                // Strong preference for momentum in high pressure
                if (choice.EffectType == EffectTypes.Momentum)
                    score += 15f;
                else
                    score += 5f;
                break;

            case NarrativePhases.Breakthrough:
                // Very strong preference for momentum
                if (choice.EffectType == EffectTypes.Momentum)
                    score += 18f;
                else
                    score += 2f;
                break;

            case NarrativePhases.Resolution:
                // Balanced with slight preference for pressure
                if (choice.EffectType == EffectTypes.Pressure)
                    score += 12f;
                else
                    score += 8f;
                break;

            case NarrativePhases.Crisis:
                // Strong preference for pressure in crisis
                if (choice.EffectType == EffectTypes.Pressure)
                    score += 15f;
                else
                    score += 5f;
                break;
        }

        // Adjust by tag values
        score *= (0.7f + (state.GetTagValue(choice.ApproachType) / 10.0f));
        score *= (0.7f + (state.GetTagValue(choice.FocusType) / 10.0f));

        // Add small variations for state sensitivity
        score += (state.Momentum % 3) * 0.1f;
        score += (state.Pressure % 4) * 0.1f;

        return score;
    }

    /// <summary>
    /// Check if a choice is particularly relevant to the current narrative phase
    /// </summary>
    private bool IsChoiceRelevantToPhase(Choice choice, NarrativePhases phase)
    {
        switch (phase)
        {
            case NarrativePhases.Introduction:
                // Information gathering and relationship approaches work well early
                return choice.FocusType == FocusTypes.Information ||
                       choice.FocusType == FocusTypes.Relationship;

            case NarrativePhases.Complication:
                // Environmental and resource focuses work well in complications
                return choice.FocusType == FocusTypes.Environment ||
                       choice.FocusType == FocusTypes.Resource;

            case NarrativePhases.Escalation:
                // Force and physical approaches are relevant in escalation
                return choice.ApproachType == ApproachTypes.Force ||
                       choice.FocusType == FocusTypes.Physical;

            case NarrativePhases.Breakthrough:
                // Wit and charm are useful for breakthroughs
                return choice.ApproachType == ApproachTypes.Wit ||
                       choice.ApproachType == ApproachTypes.Charm;

            case NarrativePhases.Resolution:
                // Relationship and resource focuses work well in resolution
                return choice.FocusType == FocusTypes.Relationship ||
                       choice.FocusType == FocusTypes.Resource;

            case NarrativePhases.Crisis:
                // Force, stealth, and physical are crisis-appropriate
                return choice.ApproachType == ApproachTypes.Force ||
                       choice.ApproachType == ApproachTypes.Stealth ||
                       choice.FocusType == FocusTypes.Physical;

            default:
                return false;
        }
    }

    #endregion

    #region Narrative-Choice Mappings

    /// <summary>
    /// Initialize mappings between narrative types and choices
    /// </summary>
    private void InitializeNarrativeChoiceMappings()
    {
        // Map choices to narrative types
        MapChoicesToNarrativeType(NarrativeTypes.Confrontation, new List<ApproachTypes> { ApproachTypes.Force },
                                  new List<FocusTypes> { FocusTypes.Physical, FocusTypes.Environment });

        MapChoicesToNarrativeType(NarrativeTypes.Persuasion, new List<ApproachTypes> { ApproachTypes.Charm },
                                  new List<FocusTypes> { FocusTypes.Relationship });

        MapChoicesToNarrativeType(NarrativeTypes.Investigation, new List<ApproachTypes> { ApproachTypes.Wit, ApproachTypes.Finesse },
                                  new List<FocusTypes> { FocusTypes.Information });

        MapChoicesToNarrativeType(NarrativeTypes.Maneuvering, new List<ApproachTypes> { ApproachTypes.Finesse, ApproachTypes.Stealth },
                                  new List<FocusTypes> { FocusTypes.Physical, FocusTypes.Environment });

        MapChoicesToNarrativeType(NarrativeTypes.Deception, new List<ApproachTypes> { ApproachTypes.Stealth, ApproachTypes.Charm },
                                  new List<FocusTypes> { FocusTypes.Relationship, FocusTypes.Information });

        MapChoicesToNarrativeType(NarrativeTypes.Negotiation, new List<ApproachTypes> { ApproachTypes.Charm, ApproachTypes.Force },
                                  new List<FocusTypes> { FocusTypes.Resource });

        MapChoicesToNarrativeType(NarrativeTypes.Examination, new List<ApproachTypes> { ApproachTypes.Wit, ApproachTypes.Finesse },
                                  new List<FocusTypes> { FocusTypes.Information, FocusTypes.Environment });

        MapChoicesToNarrativeType(NarrativeTypes.Adaptation, new List<ApproachTypes> { ApproachTypes.Finesse, ApproachTypes.Wit },
                                  new List<FocusTypes> { FocusTypes.Environment });

        // Map choices to narrative phases
        MapChoicesToNarrativePhase(NarrativePhases.Introduction, new List<ApproachTypes> { ApproachTypes.Charm, ApproachTypes.Wit },
                                  new List<FocusTypes> { FocusTypes.Information, FocusTypes.Relationship });

        MapChoicesToNarrativePhase(NarrativePhases.Complication, new List<ApproachTypes> { ApproachTypes.Wit, ApproachTypes.Finesse },
                                  new List<FocusTypes> { FocusTypes.Environment, FocusTypes.Resource });

        MapChoicesToNarrativePhase(NarrativePhases.Escalation, new List<ApproachTypes> { ApproachTypes.Force, ApproachTypes.Stealth },
                                  new List<FocusTypes> { FocusTypes.Physical, FocusTypes.Environment });

        MapChoicesToNarrativePhase(NarrativePhases.Breakthrough, new List<ApproachTypes> { ApproachTypes.Wit, ApproachTypes.Charm },
                                  new List<FocusTypes> { FocusTypes.Information, FocusTypes.Resource });

        MapChoicesToNarrativePhase(NarrativePhases.Resolution, new List<ApproachTypes> { ApproachTypes.Charm, ApproachTypes.Finesse },
                                  new List<FocusTypes> { FocusTypes.Relationship, FocusTypes.Resource });

        MapChoicesToNarrativePhase(NarrativePhases.Crisis, new List<ApproachTypes> { ApproachTypes.Force, ApproachTypes.Stealth },
                                  new List<FocusTypes> { FocusTypes.Physical, FocusTypes.Environment });
    }

    /// <summary>
    /// Map choices to a narrative type based on approaches and focuses
    /// </summary>
    private void MapChoicesToNarrativeType(NarrativeTypes narrativeType, List<ApproachTypes> approaches, List<FocusTypes> focuses)
    {
        List<Choice> matchingChoices = new List<Choice>();

        foreach (Choice choice in _repository.GetAllChoices())
        {
            if (approaches.Contains(choice.ApproachType) && focuses.Contains(choice.FocusType))
            {
                matchingChoices.Add(choice);
            }
            else if (approaches.Contains(choice.ApproachType) && choice.EffectType == EffectTypes.Pressure)
            {
                // Also include pressure choices for these approaches
                matchingChoices.Add(choice);
            }
            else if (focuses.Contains(choice.FocusType) && choice.EffectType == EffectTypes.Momentum)
            {
                // Also include momentum choices for these focuses
                matchingChoices.Add(choice);
            }
        }

        _narrativeChoicesCache[narrativeType] = matchingChoices;
    }

    /// <summary>
    /// Map choices to a narrative phase based on approaches and focuses
    /// </summary>
    private void MapChoicesToNarrativePhase(NarrativePhases phase, List<ApproachTypes> approaches, List<FocusTypes> focuses)
    {
        List<Choice> matchingChoices = new List<Choice>();

        foreach (Choice choice in _repository.GetAllChoices())
        {
            if (approaches.Contains(choice.ApproachType) && focuses.Contains(choice.FocusType))
            {
                matchingChoices.Add(choice);
            }
        }

        // For breakthrough and escalation, emphasize momentum choices
        if (phase == NarrativePhases.Breakthrough || phase == NarrativePhases.Escalation)
        {
            matchingChoices = matchingChoices
                .Where(c => c.EffectType == EffectTypes.Momentum)
                .ToList();

            // Add a few more momentum choices if needed
            if (matchingChoices.Count < 5)
            {
                matchingChoices.AddRange(
                    _repository.GetChoicesByEffect(EffectTypes.Momentum)
                    .Where(c => !matchingChoices.Contains(c))
                    .Take(5 - matchingChoices.Count)
                );
            }
        }

        // For crisis, emphasize pressure choices
        if (phase == NarrativePhases.Crisis)
        {
            matchingChoices = matchingChoices
                .Where(c => c.EffectType == EffectTypes.Pressure)
                .ToList();

            // Add a few more pressure choices if needed
            if (matchingChoices.Count < 5)
            {
                matchingChoices.AddRange(
                    _repository.GetChoicesByEffect(EffectTypes.Pressure)
                    .Where(c => !matchingChoices.Contains(c))
                    .Take(5 - matchingChoices.Count)
                );
            }
        }

        _phaseChoicesCache[phase] = matchingChoices;
    }

    /// <summary>
    /// Get choices associated with a specific narrative type
    /// </summary>
    private List<Choice> GetChoicesForNarrative(NarrativeTypes narrativeType)
    {
        return _narrativeChoicesCache.ContainsKey(narrativeType) ?
            _narrativeChoicesCache[narrativeType] :
            new List<Choice>();
    }

    /// <summary>
    /// Get choices associated with a specific narrative phase
    /// </summary>
    private List<Choice> GetChoicesForPhase(NarrativePhases phase)
    {
        return _phaseChoicesCache.ContainsKey(phase) ?
            _phaseChoicesCache[phase] :
            new List<Choice>();
    }

    #endregion

    #region Balance Enforcement

    /// <summary>
    /// Ensure momentum/pressure balance in the choice set
    /// </summary>
    private void EnsureMomentumPressureBalance(List<Choice> choices, EncounterState state)
    {
        // Determine target balance based on momentum/pressure ratio
        int targetMomentumCount = 3; // Default balanced distribution
        int targetPressureCount = 3;

        // Adjust based on M:P ratio
        float ratio = (float)state.MomentumToPressureRatio;
        if (ratio > 2.0f)
        {
            // More pressure choices when momentum is high
            targetMomentumCount = 2;
            targetPressureCount = 4;
        }
        else if (ratio < 0.5f)
        {
            // More momentum choices when pressure is high
            targetMomentumCount = 4;
            targetPressureCount = 2;
        }

        // Count current distribution
        int currentMomentumCount = choices.Count(c => c.EffectType == EffectTypes.Momentum);
        int currentPressureCount = choices.Count(c => c.EffectType == EffectTypes.Pressure);

        // Adjust if needed (try to replace choices rather than add/remove)
        while (currentMomentumCount > targetMomentumCount && currentPressureCount < targetPressureCount)
        {
            // Find a momentum choice to replace
            Choice momentumChoice = choices.First(c => c.EffectType == EffectTypes.Momentum);
            int index = choices.IndexOf(momentumChoice);

            // Try to replace with corresponding pressure choice
            Choice pressureChoice = _repository.GetChoice(momentumChoice.ApproachType, momentumChoice.FocusType, EffectTypes.Pressure);

            if (pressureChoice != null && !choices.Contains(pressureChoice))
            {
                choices[index] = pressureChoice;
                currentMomentumCount--;
                currentPressureCount++;
            }
            else
            {
                // Try any other non-selected pressure choice
                pressureChoice = _repository.GetChoicesByEffect(EffectTypes.Pressure)
                    .FirstOrDefault(c => !choices.Contains(c));

                if (pressureChoice != null)
                {
                    choices[index] = pressureChoice;
                    currentMomentumCount--;
                    currentPressureCount++;
                }
                else
                {
                    // Can't find a suitable replacement, break out
                    break;
                }
            }
        }

        // Handle the opposite imbalance
        while (currentPressureCount > targetPressureCount && currentMomentumCount < targetMomentumCount)
        {
            // Find a pressure choice to replace
            Choice pressureChoice = choices.First(c => c.EffectType == EffectTypes.Pressure);
            int index = choices.IndexOf(pressureChoice);

            // Try to replace with corresponding momentum choice
            Choice momentumChoice = _repository.GetChoice(pressureChoice.ApproachType, pressureChoice.FocusType, EffectTypes.Momentum);

            if (momentumChoice != null && !choices.Contains(momentumChoice))
            {
                choices[index] = momentumChoice;
                currentPressureCount--;
                currentMomentumCount++;
            }
            else
            {
                // Try any other non-selected momentum choice
                momentumChoice = _repository.GetChoicesByEffect(EffectTypes.Momentum)
                    .FirstOrDefault(c => !choices.Contains(c));

                if (momentumChoice != null)
                {
                    choices[index] = momentumChoice;
                    currentPressureCount--;
                    currentMomentumCount++;
                }
                else
                {
                    // Can't find a suitable replacement, break out
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Ensure tag diversity in the choice set
    /// </summary>
    private void EnsureTagDiversity(List<Choice> choices)
    {
        // Check approach diversity
        List<ApproachTypes> approachTypes = choices.Select(c => c.ApproachType).Distinct().ToList();

        if (approachTypes.Count < 3)
        {
            // Need to add more approach diversity
            // First find which approaches are missing
            List<ApproachTypes> missingApproaches = Enum.GetValues(typeof(ApproachTypes))
                .Cast<ApproachTypes>()
                .Where(a => !approachTypes.Contains(a))
                .ToList();

            // For each missing approach, try to add a choice with that approach
            foreach (ApproachTypes approach in missingApproaches)
            {
                if (approachTypes.Count >= 3)
                    break;

                // Find a choice with this approach that's not already selected
                Choice newChoice = _repository.GetChoicesByApproach(approach)
                    .FirstOrDefault(c => !choices.Contains(c));

                if (newChoice != null)
                {
                    // Remove a choice with an over-represented approach
                    Dictionary<ApproachTypes, int> approachCounts = choices
                        .GroupBy(c => c.ApproachType)
                        .ToDictionary(g => g.Key, g => g.Count());

                    ApproachTypes mostUsedApproach = approachCounts
                        .OrderByDescending(kvp => kvp.Value)
                        .First()
                        .Key;

                    // Find a choice with the most used approach to replace
                    Choice toReplace = choices
                        .First(c => c.ApproachType == mostUsedApproach);

                    int index = choices.IndexOf(toReplace);
                    choices[index] = newChoice;

                    // Update approach types list
                    approachTypes = choices.Select(c => c.ApproachType).Distinct().ToList();
                }
            }
        }

        // Check focus diversity
        List<FocusTypes> focusTypes = choices.Select(c => c.FocusType).Distinct().ToList();

        if (focusTypes.Count < 3)
        {
            // Need to add more focus diversity
            // First find which focuses are missing
            List<FocusTypes> missingFocuses = Enum.GetValues(typeof(FocusTypes))
                .Cast<FocusTypes>()
                .Where(f => !focusTypes.Contains(f))
                .ToList();

            // For each missing focus, try to add a choice with that focus
            foreach (FocusTypes focus in missingFocuses)
            {
                if (focusTypes.Count >= 3)
                    break;

                // Find a choice with this focus that's not already selected
                Choice newChoice = _repository.GetChoicesByFocus(focus)
                    .FirstOrDefault(c => !choices.Contains(c));

                if (newChoice != null)
                {
                    // Remove a choice with an over-represented focus
                    Dictionary<FocusTypes, int> focusCounts = choices
                        .GroupBy(c => c.FocusType)
                        .ToDictionary(g => g.Key, g => g.Count());

                    FocusTypes mostUsedFocus = focusCounts
                        .OrderByDescending(kvp => kvp.Value)
                        .First()
                        .Key;

                    // Find a choice with the most used focus to replace
                    Choice toReplace = choices
                        .First(c => c.FocusType == mostUsedFocus);

                    int index = choices.IndexOf(toReplace);
                    choices[index] = newChoice;

                    // Update focus types list
                    focusTypes = choices.Select(c => c.FocusType).Distinct().ToList();
                }
            }
        }
    }

    #endregion
}
