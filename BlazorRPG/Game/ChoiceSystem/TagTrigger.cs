/// <summary>
/// Tag trigger condition - what player action will trigger the location's negative reaction
/// </summary>
public class TagTrigger
{
    public string TriggerId { get; }
    public string Description { get; }
    public ApproachTypes? TriggerApproach { get; }
    public FocusTypes? TriggerFocus { get; }
    public int? MinSignatureValue { get; }
    public SignatureElementTypes? SignatureElement { get; }
    public bool IsCumulative { get; }

    public TagTrigger(string id, string description,
                     ApproachTypes? triggerApproach = null,
                     FocusTypes? triggerFocus = null,
                     int? minSignatureValue = null,
                     SignatureElementTypes? signatureElement = null,
                     bool isCumulative = false)
    {
        TriggerId = id;
        Description = description;
        TriggerApproach = triggerApproach;
        TriggerFocus = triggerFocus;
        MinSignatureValue = minSignatureValue;
        SignatureElement = signatureElement;
        IsCumulative = isCumulative;
    }

    /// <summary>
    /// Check if this trigger is activated by the given choice and state
    /// </summary>
    public bool IsTriggered(Choice choice, EncounterState state, StrategicSignature signature)
    {
        // Check approach trigger
        if (TriggerApproach.HasValue && choice.ApproachType != TriggerApproach.Value)
        {
            return false;
        }

        // Check focus trigger
        if (TriggerFocus.HasValue && choice.FocusType != TriggerFocus.Value)
        {
            return false;
        }

        // Check signature value
        if (MinSignatureValue.HasValue && SignatureElement.HasValue)
        {
            int currentValue = signature.GetElementValue(SignatureElement.Value);
            if (currentValue < MinSignatureValue.Value)
            {
                return false;
            }
        }

        // All conditions passed
        return true;
    }
}
