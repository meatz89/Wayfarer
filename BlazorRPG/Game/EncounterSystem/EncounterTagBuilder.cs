/// <summary>
/// Fluent builder class for creating encounter tag definitions
/// </summary>
public class EncounterTagBuilder
{
    private string _id;
    private string _name;
    private string _description;
    private SignatureElementTypes _sourceElement;
    private int _thresholdValue;
    private TagEffect _effect;
    private bool _isLocationReaction;
    private List<TagTrigger> _activationTriggers;
    private List<TagTrigger> _removalTriggers;

    public EncounterTagBuilder()
    {
        _effect = new TagEffect();
        _activationTriggers = new List<TagTrigger>();
        _removalTriggers = new List<TagTrigger>();
        _isLocationReaction = false;
    }

    /// <summary>
    /// Sets the tag ID using a constant from TagRegistry
    /// </summary>
    public EncounterTagBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the tag display name
    /// </summary>
    public EncounterTagBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the tag description
    /// </summary>
    public EncounterTagBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the source signature element
    /// </summary>
    public EncounterTagBuilder WithSourceElement(SignatureElementTypes element)
    {
        _sourceElement = element;
        return this;
    }

    /// <summary>
    /// Sets the threshold value (0 for location reaction tags)
    /// </summary>
    public EncounterTagBuilder WithThreshold(int threshold)
    {
        _thresholdValue = threshold;
        return this;
    }

    /// <summary>
    /// Marks the tag as a location reaction tag
    /// </summary>
    public EncounterTagBuilder AsLocationReaction()
    {
        _isLocationReaction = true;
        _thresholdValue = 0; // Location reaction tags always have threshold 0
        return this;
    }

    #region Effect Builders

    /// <summary>
    /// Sets the tag as a negative effect
    /// </summary>
    public EncounterTagBuilder AsNegative()
    {
        _effect.IsNegative = true;
        return this;
    }

    /// <summary>
    /// Adds momentum for affected choices
    /// </summary>
    public EncounterTagBuilder AddMomentum(int value)
    {
        _effect.MomentumModifier = value;
        return this;
    }

    /// <summary>
    /// Reduces pressure for affected choices
    /// </summary>
    public EncounterTagBuilder ReducePressure(int value)
    {
        _effect.PressureModifier = -value;
        return this;
    }

    /// <summary>
    /// Increases pressure for affected choices
    /// </summary>
    public EncounterTagBuilder AddPressure(int value)
    {
        _effect.PressureModifier = value;
        return this;
    }

    /// <summary>
    /// Sets the tag to prevent all pressure from affected choices
    /// </summary>
    public EncounterTagBuilder ZeroPressure()
    {
        _effect.ZeroPressure = true;
        return this;
    }

    /// <summary>
    /// Sets the tag to double momentum from affected choices
    /// </summary>
    public EncounterTagBuilder DoubleMomentum()
    {
        _effect.DoubleMomentum = true;
        return this;
    }

    /// <summary>
    /// Sets the tag to block momentum gain from affected choices
    /// </summary>
    public EncounterTagBuilder BlockMomentum()
    {
        _effect.BlockMomentum = true;
        return this;
    }

    /// <summary>
    /// Sets the tag to double pressure from affected choices
    /// </summary>
    public EncounterTagBuilder DoublePressure()
    {
        _effect.DoublePressure = true;
        return this;
    }

    /// <summary>
    /// Sets a special effect type
    /// </summary>
    public EncounterTagBuilder WithSpecialEffect(TagEffectType specialEffectId)
    {
        _effect.EffectType = specialEffectId;
        return this;
    }

    /// <summary>
    /// Limits the effect to a specific focus type
    /// </summary>
    public EncounterTagBuilder AffectFocus(FocusTypes focusType)
    {
        _effect.AffectedFocus = focusType;
        return this;
    }

    /// <summary>
    /// Limits the effect to a specific approach type
    /// </summary>
    public EncounterTagBuilder AffectApproach(ApproachTypes approachType)
    {
        _effect.AffectedApproach = approachType;
        return this;
    }

    #endregion

    #region Trigger Builders

    /// <summary>
    /// Adds an activation trigger based on approach type
    /// </summary>
    public EncounterTagBuilder ActivateOnApproach(string triggerId, string description, ApproachTypes approach)
    {
        _activationTriggers.Add(new TagTrigger(triggerId, description, approach));
        return this;
    }

    /// <summary>
    /// Adds an activation trigger based on focus type
    /// </summary>
    public EncounterTagBuilder ActivateOnFocus(string triggerId, string description, FocusTypes focus)
    {
        _activationTriggers.Add(new TagTrigger(triggerId, description, null, focus));
        return this;
    }

    /// <summary>
    /// Adds an activation trigger based on signature element threshold
    /// </summary>
    public EncounterTagBuilder ActivateOnSignature(string triggerId, string description, int minValue, SignatureElementTypes element, bool isCumulative = false)
    {
        _activationTriggers.Add(new TagTrigger(triggerId, description, null, null, minValue, element, isCumulative));
        return this;
    }

    /// <summary>
    /// Adds a removal trigger based on approach type
    /// </summary>
    public EncounterTagBuilder RemoveOnApproach(string triggerId, string description, ApproachTypes approach)
    {
        _removalTriggers.Add(new TagTrigger(triggerId, description, approach));
        return this;
    }

    /// <summary>
    /// Adds a removal trigger based on focus type
    /// </summary>
    public EncounterTagBuilder RemoveOnFocus(string triggerId, string description, FocusTypes focus)
    {
        _removalTriggers.Add(new TagTrigger(triggerId, description, null, focus));
        return this;
    }

    /// <summary>
    /// Adds a removal trigger based on signature element threshold
    /// </summary>
    public EncounterTagBuilder RemoveOnSignature(string triggerId, string description, int minValue, SignatureElementTypes element)
    {
        _removalTriggers.Add(new TagTrigger(triggerId, description, null, null, minValue, element));
        return this;
    }

    #endregion

    /// <summary>
    /// Builds and returns the configured EncounterTag
    /// </summary>
    public EncounterTag Build()
    {
        // Validate tag is properly configured
        if (string.IsNullOrEmpty(_id))
            throw new InvalidOperationException("Tag ID must be specified");

        if (string.IsNullOrEmpty(_name))
            throw new InvalidOperationException("Tag name must be specified");

        if (string.IsNullOrEmpty(_description))
            throw new InvalidOperationException("Tag description must be specified");

        // Create the encounter tag
        EncounterTag tag = new EncounterTag(
            _id,
            _name,
            _description,
            _sourceElement,
            _thresholdValue,
            _effect
        );

        // Set location reaction flag if needed
        tag.IsLocationReaction = _isLocationReaction;

        // Add all activation triggers
        foreach (var trigger in _activationTriggers)
        {
            tag.ActivationTriggers.Add(trigger);
        }

        // Add all removal triggers
        foreach (var trigger in _removalTriggers)
        {
            tag.RemovalTriggers.Add(trigger);
        }

        return tag;
    }
}