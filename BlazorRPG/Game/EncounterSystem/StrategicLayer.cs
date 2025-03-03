
/// <summary>
/// Manages the strategic layer for encounters
/// </summary>
public class StrategicLayer
{
    private readonly StrategicSignature _signature;
    private readonly List<EncounterTag> _availableTags;
    private readonly List<EncounterTag> _activeTags;
    private readonly LocationStrategicProperties _locationProperties;
    private readonly EncounterTagRepository _tagRepository;

    public StrategicLayer(LocationStrategicProperties locationProperties, EncounterTagRepository tagRepository)
    {
        _signature = new StrategicSignature();
        _locationProperties = locationProperties;
        _tagRepository = tagRepository;

        // Load available tags from repository using IDs
        _availableTags = new List<EncounterTag>();
        foreach (string tagId in locationProperties.AvailableTagIds)
        {
            EncounterTag tag = tagRepository.GetTag(tagId);
            if (tag != null)
            {
                _availableTags.Add(tag);
            }
        }

        _activeTags = new List<EncounterTag>();
    }

    /// <summary>
    /// Process a choice from the strategic perspective
    /// </summary>
    public ChoiceOutcome ProcessChoice(Choice choice, EncounterState state)
    {
        // Map the choice's approach to a signature element
        SignatureElementTypes elementType = StrategicSignature.ApproachToElement(choice.ApproachType);

        // Increment the corresponding signature element
        _signature.IncrementElement(elementType);

        // Calculate base outcome
        ChoiceOutcome outcome = CalculateBaseOutcome(choice, elementType);

        // Update tag states
        UpdateTagStates();

        // Apply effects from active tags
        ChoiceOutcome modifiedOutcome = ApplyTagEffects(choice, outcome);

        return modifiedOutcome;
    }

    /// <summary>
    /// Calculate the base outcome for a choice
    /// </summary>
    private ChoiceOutcome CalculateBaseOutcome(Choice choice, SignatureElementTypes elementType)
    {
        int momentum = 0;
        int pressure = 0;

        // Base effect from choice type
        if (choice.EffectType == EffectTypes.Momentum)
        {
            momentum += 1;
        }
        else if (choice.EffectType == EffectTypes.Pressure)
        {
            pressure += 1;
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
    /// Apply effects from all active tags
    /// </summary>
    private ChoiceOutcome ApplyTagEffects(Choice choice, ChoiceOutcome baseOutcome)
    {
        ChoiceOutcome modifiedOutcome = new ChoiceOutcome(baseOutcome.Momentum, baseOutcome.Pressure);

        foreach (EncounterTag tag in _activeTags)
        {
            modifiedOutcome = tag.ProcessEffect(choice, modifiedOutcome);
        }

        return modifiedOutcome;
    }

    /// <summary>
    /// Update which tags are active based on current signature values
    /// </summary>
    private void UpdateTagStates()
    {
        List<EncounterTag> tagsToActivate = new List<EncounterTag>();
        List<EncounterTag> tagsToDeactivate = new List<EncounterTag>();

        foreach (EncounterTag tag in _availableTags)
        {
            bool shouldBeActive = tag.ShouldBeActive(_signature);

            if (shouldBeActive && !tag.IsActive)
            {
                tagsToActivate.Add(tag);
            }
            else if (!shouldBeActive && tag.IsActive)
            {
                tagsToDeactivate.Add(tag);
            }
        }

        // Apply changes after iteration to avoid collection modification during enumeration
        foreach (EncounterTag tag in tagsToActivate)
        {
            tag.IsActive = true;
            _activeTags.Add(tag);
        }

        foreach (EncounterTag tag in tagsToDeactivate)
        {
            tag.IsActive = false;
            _activeTags.Remove(tag);
        }
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
