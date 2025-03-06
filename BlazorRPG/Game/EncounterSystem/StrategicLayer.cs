/// <summary>
/// Strategic layer to handle encounter mechanics with unified tag effects
/// </summary>
public class StrategicLayer
{
    protected readonly StrategicSignature _signature;
    protected readonly List<EncounterTag> _availableTags;
    protected readonly List<EncounterTag> _activeTags;
    protected readonly LocationStrategicProperties _locationProperties;
    protected readonly EncounterTagRepository _tagRepository;
    protected readonly Dictionary<string, int> _cumulativeTriggers;

    public StrategicLayer(LocationStrategicProperties locationProperties, EncounterTagRepository tagRepository)
    {
        _signature = new StrategicSignature();
        _locationProperties = locationProperties;
        _tagRepository = tagRepository;
        _cumulativeTriggers = new Dictionary<string, int>();

        // Load available player tags from repository using IDs
        _availableTags = new List<EncounterTag>();
        foreach (string tagId in locationProperties.AvailableTagIds)
        {
            EncounterTag tag = tagRepository.GetTag(tagId);
            if (tag != null)
            {
                _availableTags.Add(tag);
            }
        }

        // Add location reaction tags
        foreach (string tagId in locationProperties.LocationReactionTagIds)
        {
            EncounterTag tag = tagRepository.GetTag(tagId);
            if (tag != null && tag.IsLocationReaction)
            {
                _availableTags.Add(tag);
            }
        }

        _activeTags = new List<EncounterTag>();
    }

    /// <summary>
    /// Process a choice from the strategic perspective by first projecting then applying changes
    /// </summary>
    public ChoiceOutcome ProcessChoice(Choice choice, EncounterState state)
    {
        // First create a projection of the choice's effects
        ChoiceProjection projection = ProjectChoice(choice, state);

        // Apply the projection to the actual state
        ApplyProjection(projection);

        // Return the outcome for the caller to use
        return new ChoiceOutcome(projection.MomentumChange, projection.PressureChange);
    }

    /// <summary>
    /// Projects the outcome of a choice without changing state
    /// </summary>
    public virtual ChoiceProjection ProjectChoice(Choice choice, EncounterState state)
    {
        ChoiceProjection projection = new ChoiceProjection();

        // Capture current state
        projection.CurrentMomentum = state.Momentum;
        projection.CurrentPressure = state.Pressure;
        projection.CurrentSignature = new StrategicSignature(_signature);

        // Deep clone the state data to avoid modifying the actual state
        StrategicSignature clonedSignature = new StrategicSignature(_signature);
        List<EncounterTag> clonedTags = CloneActiveTags();

        // Map the choice's approach to a signature element
        SignatureElementTypes elementType = StrategicSignature.ApproachToElement(choice.ApproachType);

        // Calculate projected signature
        clonedSignature.IncrementElement(elementType);
        projection.ProjectedSignature = clonedSignature;

        // Calculate base outcome
        ChoiceOutcome baseOutcome = CalculateBaseOutcome(choice, elementType);
        projection.BaseMomentumChange = baseOutcome.Momentum;
        projection.BasePressureChange = baseOutcome.Pressure;

        // Project tag changes
        ProjectTagChanges(choice, state, clonedSignature, clonedTags, projection);

        // Set initial momentum/pressure changes from base outcome
        projection.MomentumChange = baseOutcome.Momentum;
        projection.PressureChange = baseOutcome.Pressure + 1; // Include end-of-turn pressure

        // Calculate initial projected values
        projection.ProjectedMomentum = state.Momentum + projection.MomentumChange;
        projection.ProjectedPressure = state.Pressure + projection.PressureChange;

        // Apply tag effects - now unified in a single system
        ApplyTagEffects(choice, clonedTags, projection);

        return projection;
    }

    /// <summary>
    /// Apply a projected choice outcome to the actual state
    /// </summary>
    public void ApplyProjection(ChoiceProjection projection)
    {
        // Apply signature changes
        _signature.SetFromSignature(projection.ProjectedSignature);

        // Apply tag activations/deactivations - using the actual tag objects
        foreach (var projectedTag in projection.TagsActivated)
        {
            var actualTag = _availableTags.FirstOrDefault(t => t.Id == projectedTag.Id);
            if (actualTag != null && !actualTag.IsActive)
            {
                actualTag.IsActive = true;
                _activeTags.Add(actualTag);
            }
        }

        foreach (var projectedTag in projection.TagsDeactivated)
        {
            var actualTag = _activeTags.FirstOrDefault(t => t.Id == projectedTag.Id);
            if (actualTag != null)
            {
                actualTag.IsActive = false;
                _activeTags.Remove(actualTag);
            }
        }

        // Update cumulative triggers if needed
        foreach (var tagTriggerPair in projection.CumulativeTriggerChanges)
        {
            _cumulativeTriggers[tagTriggerPair.Key] = tagTriggerPair.Value;
        }
    }

    /// <summary>
    /// Calculate the base outcome for a choice
    /// </summary>
    protected ChoiceOutcome CalculateBaseOutcome(Choice choice, SignatureElementTypes elementType)
    {
        int momentum = 0;
        int pressure = 0;

        // Base effect from choice type
        if (choice.EffectType == EffectTypes.Momentum)
        {
            momentum += 2; // Standard momentum gain
        }
        else if (choice.EffectType == EffectTypes.Pressure)
        {
            pressure -= 1; // Standard pressure reduction
        }

        // Check location favored/disfavored elements
        if (_locationProperties.FavoredElements.Contains(elementType))
        {
            momentum += 1; // Bonus momentum for favored element
        }

        if (_locationProperties.DisfavoredElements.Contains(elementType))
        {
            pressure += 1; // Extra pressure for disfavored element
        }

        return new ChoiceOutcome(momentum, pressure);
    }

    /// <summary>
    /// Projects which tags would be activated or deactivated
    /// </summary>
    protected virtual void ProjectTagChanges(Choice choice, EncounterState state, StrategicSignature projectedSignature,
                                     List<EncounterTag> clonedTags, ChoiceProjection projection)
    {
        // Check each tag for activation/deactivation
        foreach (EncounterTag tag in _availableTags)
        {
            EncounterTag clonedTag = new EncounterTag(tag);
            bool isCurrentlyActive = IsTagActive(tag.Id);

            // Handle threshold-based player tags
            if (!clonedTag.IsLocationReaction)
            {
                bool shouldBeActive = clonedTag.ShouldBeActive(projectedSignature);

                if (shouldBeActive && !isCurrentlyActive)
                {
                    projection.TagsActivated.Add(clonedTag);
                }
                else if (!shouldBeActive && isCurrentlyActive)
                {
                    projection.TagsDeactivated.Add(clonedTag);
                }
            }
            // Handle trigger-based location reaction tags
            else
            {
                // Project cumulative triggers
                Dictionary<string, int> projectedCumulativeTriggers = new Dictionary<string, int>(_cumulativeTriggers);

                foreach (TagTrigger trigger in clonedTag.ActivationTriggers)
                {
                    if (trigger.IsCumulative && trigger.IsTriggered(choice, state, projectedSignature))
                    {
                        string triggerKey = $"{clonedTag.Id}_{trigger.TriggerId}";
                        if (!projectedCumulativeTriggers.ContainsKey(triggerKey))
                        {
                            projectedCumulativeTriggers[triggerKey] = 0;
                        }
                        projectedCumulativeTriggers[triggerKey]++;

                        // Record the change for later application
                        projection.CumulativeTriggerChanges[triggerKey] = projectedCumulativeTriggers[triggerKey];

                        // Check if we've hit the threshold
                        if (trigger.MinSignatureValue.HasValue &&
                            projectedCumulativeTriggers[triggerKey] >= trigger.MinSignatureValue.Value)
                        {
                            // Tag should be activated
                            if (!isCurrentlyActive)
                            {
                                projection.TagsActivated.Add(clonedTag);
                            }
                        }
                    }
                }

                // Project regular triggers
                if (!isCurrentlyActive && clonedTag.ShouldBeActivated(choice, state, projectedSignature))
                {
                    projection.TagsActivated.Add(clonedTag);
                }
                else if (isCurrentlyActive && clonedTag.ShouldBeRemoved(choice, state, projectedSignature))
                {
                    projection.TagsDeactivated.Add(clonedTag);
                }
            }
        }

        // Update cloned tags list for effect calculation
        foreach (var tag in projection.TagsActivated)
        {
            tag.IsActive = true;
            clonedTags.Add(tag);
        }

        foreach (var tag in projection.TagsDeactivated)
        {
            var tagToRemove = clonedTags.FirstOrDefault(t => t.Id == tag.Id);
            if (tagToRemove != null)
            {
                clonedTags.Remove(tagToRemove);
            }
        }
    }

    /// <summary>
    /// Apply all tag effects to the choice projection
    /// </summary>
    protected void ApplyTagEffects(Choice choice, List<EncounterTag> activeTags, ChoiceProjection projection)
    {
        foreach (EncounterTag tag in activeTags)
        {
            if (tag.IsActive)
            {
                // Apply effect using the unified tag effect system
                tag.Effect.ApplyToProjection(choice, projection, tag.Name);
            }
        }
    }

    /// <summary>
    /// Clone the current set of active tags
    /// </summary>
    protected List<EncounterTag> CloneActiveTags()
    {
        List<EncounterTag> clonedTags = new List<EncounterTag>();
        foreach (var tag in _activeTags)
        {
            clonedTags.Add(new EncounterTag(tag));
        }
        return clonedTags;
    }

    /// <summary>
    /// Get all available tags for this location
    /// </summary>
    public List<EncounterTag> GetAllAvailableTags()
    {
        return _availableTags;
    }

    /// <summary>
    /// Check if a specific tag is active
    /// </summary>
    public bool IsTagActive(string tagId)
    {
        return _activeTags.Any(t => t.Id == tagId);
    }

    /// <summary>
    /// Get current active tags
    /// </summary>
    public List<EncounterTag> GetActiveTags()
    {
        return _activeTags;
    }

    /// <summary>
    /// Get current signature values
    /// </summary>
    public StrategicSignature GetSignature()
    {
        return _signature;
    }
}