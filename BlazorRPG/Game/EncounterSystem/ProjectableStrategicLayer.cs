/// <summary>
/// Extended strategic layer that supports choice projection
/// </summary>
public class ProjectableStrategicLayer : StrategicLayer
{
    public ProjectableStrategicLayer(LocationStrategicProperties locationProperties, EncounterTagRepository tagRepository)
        : base(locationProperties, tagRepository)
    {
    }

    /// <summary>
    /// Projects the outcome of a choice without changing state
    /// </summary>
    public ChoiceProjection ProjectChoice(Choice choice, EncounterState state)
    {
        ChoiceProjection projection = new ChoiceProjection();

        // Capture current state
        projection.CurrentMomentum = state.Momentum;
        projection.CurrentPressure = state.Pressure;
        projection.CurrentSignature = new StrategicSignature(GetSignature());

        // Deep clone the state data to avoid modifying the actual state
        StrategicSignature clonedSignature = new StrategicSignature(GetSignature());
        List<EncounterTag> clonedTags = CloneActiveTags();

        // Map the choice's approach to a signature element
        SignatureElementTypes elementType = StrategicSignature.ApproachToElement(choice.ApproachType);

        // Calculate projected signature
        clonedSignature.IncrementElement(elementType);
        projection.ProjectedSignature = clonedSignature;

        // Calculate base outcome
        ChoiceOutcome baseOutcome = ProjectBaseOutcome(choice, elementType);
        projection.BaseMomentumChange = baseOutcome.Momentum;
        projection.BasePressureChange = baseOutcome.Pressure;

        // Project tag changes
        ProjectTagChanges(choice, state, clonedSignature, clonedTags, projection);

        // Apply effects from active tags
        ChoiceOutcome modifiedOutcome = ProjectTagEffects(choice, baseOutcome, clonedTags, projection);

        // Calculate final changes
        projection.MomentumChange = modifiedOutcome.Momentum;
        projection.PressureChange = modifiedOutcome.Pressure + 1; // Include end-of-turn pressure

        // Calculate projected values
        projection.ProjectedMomentum = state.Momentum + projection.MomentumChange;
        projection.ProjectedPressure = state.Pressure + projection.PressureChange;

        return projection;
    }

    /// <summary>
    /// Projects the base outcome of a choice without applying tag effects
    /// </summary>
    private ChoiceOutcome ProjectBaseOutcome(Choice choice, SignatureElementTypes elementType)
    {
        return CalculateBaseOutcome(choice, elementType);
    }

    /// <summary>
    /// Projects which tags would be activated or deactivated
    /// </summary>
    private void ProjectTagChanges(Choice choice, EncounterState state, StrategicSignature signature,
                                 List<EncounterTag> activeTags, ChoiceProjection projection)
    {
        // Check each tag for activation/deactivation
        foreach (EncounterTag tag in GetAllAvailableTags())
        {
            EncounterTag clonedTag = new EncounterTag(tag);

            // Handle threshold-based player tags
            if (!clonedTag.IsLocationReaction)
            {
                bool shouldBeActive = clonedTag.ShouldBeActive(signature);
                bool isCurrentlyActive = IsTagActive(tag.Id);

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
                bool isCurrentlyActive = IsTagActive(tag.Id);

                // Project cumulative triggers (simplified for projection)
                if (!isCurrentlyActive && clonedTag.ShouldBeActivated(choice, state, signature))
                {
                    projection.TagsActivated.Add(clonedTag);
                }
                else if (isCurrentlyActive && clonedTag.ShouldBeRemoved(choice, state, signature))
                {
                    projection.TagsDeactivated.Add(clonedTag);
                }
            }
        }

        // Update active tags list for effect calculation
        foreach (var tag in projection.TagsActivated)
        {
            tag.IsActive = true;
            activeTags.Add(tag);
        }

        foreach (var tag in projection.TagsDeactivated)
        {
            var tagToRemove = activeTags.FirstOrDefault(t => t.Id == tag.Id);
            if (tagToRemove != null)
            {
                activeTags.Remove(tagToRemove);
            }
        }
    }

    /// <summary>
    /// Projects the effects of active tags on the choice outcome
    /// </summary>
    private ChoiceOutcome ProjectTagEffects(Choice choice, ChoiceOutcome baseOutcome,
                                          List<EncounterTag> activeTags, ChoiceProjection projection)
    {
        ChoiceOutcome modifiedOutcome = new ChoiceOutcome(baseOutcome.Momentum, baseOutcome.Pressure);

        foreach (EncounterTag tag in activeTags)
        {
            ChoiceOutcome before = new ChoiceOutcome(modifiedOutcome.Momentum, modifiedOutcome.Pressure);
            modifiedOutcome = tag.ProcessEffect(choice, modifiedOutcome);

            // Track the tag's effect
            int momentumEffect = modifiedOutcome.Momentum - before.Momentum;
            int pressureEffect = modifiedOutcome.Pressure - before.Pressure;

            if (momentumEffect != 0)
            {
                projection.TagMomentumEffects[tag.Name] = momentumEffect;
            }

            if (pressureEffect != 0)
            {
                projection.TagPressureEffects[tag.Name] = pressureEffect;
            }
        }

        return modifiedOutcome;
    }

    /// <summary>
    /// Clone the current set of active tags
    /// </summary>
    private List<EncounterTag> CloneActiveTags()
    {
        List<EncounterTag> clonedTags = new List<EncounterTag>();
        foreach (var tag in GetActiveTags())
        {
            clonedTags.Add(new EncounterTag(tag));
        }
        return clonedTags;
    }
}
