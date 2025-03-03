
/// <summary>
/// Represents an encounter tag with persistent effects
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
    }

    /// <summary>
    /// Check if this tag should be active based on element value
    /// </summary>
    public bool ShouldBeActive(StrategicSignature signature)
    {
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
        }

        return modifiedOutcome;
    }
}
