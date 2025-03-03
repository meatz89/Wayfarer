
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extended strategic layer to handle location reaction tags
/// </summary>
public class StrategicLayer
{
    private readonly StrategicSignature _signature;
    private readonly List<EncounterTag> _availableTags;
    private readonly List<EncounterTag> _activeTags;
    private readonly LocationStrategicProperties _locationProperties;
    private readonly EncounterTagRepository _tagRepository;
    private readonly Dictionary<string, int> _cumulativeTriggers;

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

        // Update tag states - both threshold-based and trigger-based
        UpdateTagStates(choice, state);

        // Apply effects from active tags
        ChoiceOutcome modifiedOutcome = ApplyTagEffects(choice, outcome);

        return modifiedOutcome;
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
    /// Update which tags are active based on current signature values and triggers
    /// </summary>
    private void UpdateTagStates(Choice choice, EncounterState state)
    {
        List<EncounterTag> tagsToActivate = new List<EncounterTag>();
        List<EncounterTag> tagsToDeactivate = new List<EncounterTag>();

        // Check each tag for activation/deactivation
        foreach (EncounterTag tag in _availableTags)
        {
            // Handle threshold-based player tags
            if (!tag.IsLocationReaction)
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
            // Handle trigger-based location reaction tags
            else
            {
                // Process cumulative triggers
                foreach (TagTrigger trigger in tag.ActivationTriggers)
                {
                    if (trigger.IsCumulative && trigger.IsTriggered(choice, state, _signature))
                    {
                        string triggerKey = $"{tag.Id}_{trigger.TriggerId}";
                        if (!_cumulativeTriggers.ContainsKey(triggerKey))
                        {
                            _cumulativeTriggers[triggerKey] = 0;
                        }
                        _cumulativeTriggers[triggerKey]++;

                        // Check if we've hit the threshold
                        if (trigger.MinSignatureValue.HasValue &&
                            _cumulativeTriggers[triggerKey] >= trigger.MinSignatureValue.Value)
                        {
                            // Tag should be activated
                            if (!tag.IsActive)
                            {
                                tagsToActivate.Add(tag);
                            }
                        }
                    }
                }

                // Process regular triggers
                if (!tag.IsActive && tag.ShouldBeActivated(choice, state, _signature))
                {
                    tagsToActivate.Add(tag);
                }
                else if (tag.IsActive && tag.ShouldBeRemoved(choice, state, _signature))
                {
                    tagsToDeactivate.Add(tag);
                }
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
