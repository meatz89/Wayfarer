
using System.Text;
/// <summary>
/// Information about a tag trigger for display to the player
/// </summary>
public class TagTriggerInfo
{
    public string TriggerId { get; set; }
    public string Description { get; set; }
    public ApproachTypes? TriggerApproach { get; set; }
    public FocusTypes? TriggerFocus { get; set; }
    public int? MinSignatureValue { get; set; }
    public SignatureElementTypes? SignatureElement { get; set; }

    /// <summary>
    /// Create a display-friendly info object from a tag trigger
    /// </summary>
    public TagTriggerInfo(TagTrigger trigger)
    {
        TriggerId = trigger.TriggerId;
        Description = trigger.Description;
        TriggerApproach = trigger.TriggerApproach;
        TriggerFocus = trigger.TriggerFocus;
        MinSignatureValue = trigger.MinSignatureValue;
        SignatureElement = trigger.SignatureElement;
    }

    /// <summary>
    /// Get a human-readable description of the trigger condition
    /// </summary>
    public string GetHumanReadableCondition()
    {
        StringBuilder condition = new StringBuilder();

        // Approach trigger
        if (TriggerApproach.HasValue)
        {
            condition.Append($"Using {TriggerApproach.Value} approach");
        }

        // Focus trigger
        if (TriggerFocus.HasValue)
        {
            if (condition.Length > 0)
            {
                condition.Append(" with ");
            }
            condition.Append($"{TriggerFocus.Value} focus");
        }

        // Signature value trigger
        if (MinSignatureValue.HasValue && SignatureElement.HasValue)
        {
            if (condition.Length > 0)
            {
                condition.Append(" or ");
            }
            condition.Append($"reaching {SignatureElement.Value} level {MinSignatureValue.Value}");
        }

        return condition.ToString();
    }
}
