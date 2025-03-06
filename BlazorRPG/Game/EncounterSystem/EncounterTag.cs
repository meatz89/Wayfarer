
/// <summary>
/// Extends EncounterTag with trigger conditions and whether it's a location reaction
/// </summary>
public class EncounterTag
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public SignatureElementTypes SourceElement { get; }
    public int ThresholdValue { get; }
    public bool IsActive { get; set; }
    public TagEffect Effect { get; }
    public bool IsLocationReaction { get; set; }
    public List<TagTrigger> ActivationTriggers { get; }
    public List<TagTrigger> RemovalTriggers { get; }

    public EncounterTag(string id, string name, string description, SignatureElementTypes sourceElement,
                       int thresholdValue, TagEffect effect)
    {
        Id = id;
        Name = name;
        Description = description;
        SourceElement = sourceElement;
        ThresholdValue = thresholdValue;
        IsActive = false;
        Effect = effect;
        IsLocationReaction = false;
        ActivationTriggers = new List<TagTrigger>();
        RemovalTriggers = new List<TagTrigger>();
    }

    /// <summary>
    /// Create a deep copy of another tag
    /// </summary>
    public EncounterTag(EncounterTag other)
    {
        Id = other.Id;
        Name = other.Name;
        Description = other.Description;
        SourceElement = other.SourceElement;
        ThresholdValue = other.ThresholdValue;
        IsLocationReaction = other.IsLocationReaction;
        IsActive = other.IsActive;

        // Deep copy the tag effect
        Effect = new TagEffect
        {
            AffectedApproach = other.Effect.AffectedApproach,
            AffectedFocus = other.Effect.AffectedFocus,
            BlockMomentum = other.Effect.BlockMomentum,
            DoubleMomentum = other.Effect.DoubleMomentum,
            DoublePressure = other.Effect.DoublePressure,
            IsNegative = other.Effect.IsNegative,
            IsSpecialEffect = other.Effect.IsSpecialEffect,
            MomentumModifier = other.Effect.MomentumModifier,
            PressureModifier = other.Effect.PressureModifier,
            SpecialEffectId = other.Effect.SpecialEffectId,
            ZeroPressure = other.Effect.ZeroPressure
        };

        // Deep copy the activation triggers
        ActivationTriggers = new List<TagTrigger>();
        foreach (TagTrigger trigger in other.ActivationTriggers)
        {
            ActivationTriggers.Add(new TagTrigger(
                trigger.TriggerId,
                trigger.Description,
                trigger.TriggerApproach,
                trigger.TriggerFocus,
                trigger.MinSignatureValue,
                trigger.SignatureElement,
                trigger.IsCumulative
            ));
        }

        // Deep copy the removal triggers
        RemovalTriggers = new List<TagTrigger>();
        foreach (TagTrigger trigger in other.RemovalTriggers)
        {
            RemovalTriggers.Add(new TagTrigger(
                trigger.TriggerId,
                trigger.Description,
                trigger.TriggerApproach,
                trigger.TriggerFocus,
                trigger.MinSignatureValue,
                trigger.SignatureElement,
                trigger.IsCumulative
            ));
        }
    }

    /// <summary>
    /// Check if this tag should be active based on element value
    /// </summary>
    public bool ShouldBeActive(StrategicSignature signature)
    {
        // If it's a location reaction, it doesn't use the threshold mechanic
        if (IsLocationReaction)
            return IsActive;

        // Regular player tag uses threshold
        return signature.GetElementValue(SourceElement) >= ThresholdValue;
    }

    /// <summary>
    /// Apply this tag's effect to the given choice outcome
    /// </summary>
    public ChoiceOutcome ProcessEffect(Choice choice, ChoiceOutcome outcome)
    {
        if (!IsActive)
        {
            return outcome;
        }

        ChoiceOutcome modifiedOutcome = new ChoiceOutcome(outcome.Momentum, outcome.Pressure);

        // Check if this tag applies to the choice
        bool tagApplies = true;

        if (Effect.AffectedFocus.HasValue && choice.FocusType != Effect.AffectedFocus.Value)
        {
            tagApplies = false;
        }

        if (Effect.AffectedApproach.HasValue && choice.ApproachType != Effect.AffectedApproach.Value)
        {
            tagApplies = false;
        }

        if (tagApplies)
        {
            // Apply momentum modifier
            modifiedOutcome.Momentum += Effect.MomentumModifier;

            // Apply pressure modifier
            modifiedOutcome.Pressure += Effect.PressureModifier;

            // Apply special effects
            if (Effect.ZeroPressure)
            {
                modifiedOutcome.Pressure = 0;
            }

            if (Effect.DoubleMomentum)
            {
                modifiedOutcome.Momentum *= 2;
            }

            // Apply negative effects
            if (Effect.IsNegative)
            {
                if (Effect.BlockMomentum)
                {
                    modifiedOutcome.Momentum = 0;
                }

                if (Effect.DoublePressure)
                {
                    modifiedOutcome.Pressure *= 2;
                }
            }
        }

        return modifiedOutcome;
    }

    /// <summary>
    /// Check if this tag should be activated by the given choice and state
    /// </summary>
    public bool ShouldBeActivated(Choice choice, EncounterState state, StrategicSignature signature)
    {
        if (!IsLocationReaction || IsActive)
            return false;

        foreach (TagTrigger trigger in ActivationTriggers)
        {
            if (trigger.IsTriggered(choice, state, signature))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if this tag should be removed by the given choice and state
    /// </summary>
    public bool ShouldBeRemoved(Choice choice, EncounterState state, StrategicSignature signature)
    {
        if (!IsLocationReaction || !IsActive)
            return false;

        foreach (TagTrigger trigger in RemovalTriggers)
        {
            if (trigger.IsTriggered(choice, state, signature))
            {
                return true;
            }
        }

        return false;
    }
}
