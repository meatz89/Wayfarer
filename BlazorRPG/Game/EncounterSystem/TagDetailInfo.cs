
/// <summary>
/// Comprehensive information about a tag for display to the player
/// </summary>
public class TagDetailInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsLocationReaction { get; set; }
    public bool IsActive { get; set; }

    // For player tags
    public SignatureElementTypes? SourceElement { get; set; }
    public int? ThresholdValue { get; set; }

    // Effect information
    public TagEffectInfo Effect { get; set; }

    // Trigger information for location reaction tags
    public List<TagTriggerInfo> ActivationTriggers { get; set; }
    public List<TagTriggerInfo> RemovalTriggers { get; set; }

    public TagDetailInfo()
    {
        ActivationTriggers = new List<TagTriggerInfo>();
        RemovalTriggers = new List<TagTriggerInfo>();
    }

    /// <summary>
    /// Create a comprehensive info object from a tag
    /// </summary>
    public TagDetailInfo(EncounterTag tag)
    {
        Id = tag.Id;
        Name = tag.Name;
        Description = tag.Description;
        IsLocationReaction = tag.IsLocationReaction;
        IsActive = tag.IsActive;

        // Add threshold info for player tags
        if (!tag.IsLocationReaction)
        {
            SourceElement = tag.SourceElement;
            ThresholdValue = tag.ThresholdValue;
        }

        // Add effect info
        Effect = new TagEffectInfo(tag.Effect);

        // Add trigger info for location reaction tags
        ActivationTriggers = new List<TagTriggerInfo>();
        RemovalTriggers = new List<TagTriggerInfo>();

        if (tag.IsLocationReaction)
        {
            foreach (var trigger in tag.ActivationTriggers)
            {
                ActivationTriggers.Add(new TagTriggerInfo(trigger));
            }

            foreach (var trigger in tag.RemovalTriggers)
            {
                RemovalTriggers.Add(new TagTriggerInfo(trigger));
            }
        }
    }

    /// <summary>
    /// Get a human-readable description of how to activate this tag
    /// </summary>
    public string GetActivationDescription()
    {
        if (!IsLocationReaction && ThresholdValue.HasValue && SourceElement.HasValue)
        {
            return $"Activated when {SourceElement.Value} reaches level {ThresholdValue.Value}";
        }
        else if (IsLocationReaction && ActivationTriggers.Any())
        {
            return "Activated by: " + string.Join(" or ",
                ActivationTriggers.Select(t => t.GetHumanReadableCondition()));
        }

        return "No activation information available";
    }

    /// <summary>
    /// Get a human-readable description of how to remove this tag
    /// </summary>
    public string GetRemovalDescription()
    {
        if (!IsLocationReaction && ThresholdValue.HasValue && SourceElement.HasValue)
        {
            return $"Deactivated when {SourceElement.Value} falls below level {ThresholdValue.Value}";
        }
        else if (IsLocationReaction && RemovalTriggers.Any())
        {
            return "Removed by: " + string.Join(" or ",
                RemovalTriggers.Select(t => t.GetHumanReadableCondition()));
        }

        return "No removal information available";
    }
}
