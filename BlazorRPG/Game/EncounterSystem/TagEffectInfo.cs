
using System.Text;
/// <summary>
/// Information about a tag's effect for display to the player
/// </summary>
public class TagEffectInfo
{
    public FocusTypes? AffectedFocus { get; set; }
    public ApproachTypes? AffectedApproach { get; set; }
    public int MomentumModifier { get; set; }
    public int PressureModifier { get; set; }
    public bool ZeroPressure { get; set; }
    public bool DoubleMomentum { get; set; }
    public bool IsSpecialEffect { get; set; }
    public string SpecialEffectDescription { get; set; }
    public bool IsNegative { get; set; }
    public bool BlockMomentum { get; set; }
    public bool DoublePressure { get; set; }

    /// <summary>
    /// Create a display-friendly info object from a tag effect
    /// </summary>
    public TagEffectInfo(TagEffect effect)
    {
        AffectedFocus = effect.AffectedFocus;
        AffectedApproach = effect.AffectedApproach;
        MomentumModifier = effect.MomentumModifier;
        PressureModifier = effect.PressureModifier;
        ZeroPressure = effect.ZeroPressure;
        DoubleMomentum = effect.DoubleMomentum;
        IsNegative = effect.IsNegative;
        BlockMomentum = effect.BlockMomentum;
        DoublePressure = effect.DoublePressure;
    }

    /// <summary>
    /// Generate a human-readable description of the effect
    /// </summary>
    public string GetHumanReadableDescription()
    {
        StringBuilder description = new StringBuilder();

        // Determine the target of the effect
        string targetDescription = "all choices";
        if (AffectedApproach.HasValue)
        {
            targetDescription = $"{AffectedApproach.Value} choices";
        }
        else if (AffectedFocus.HasValue)
        {
            targetDescription = $"{AffectedFocus.Value}-related choices";
        }

        // Describe momentum effects
        if (BlockMomentum)
        {
            description.AppendLine($"• {targetDescription} generate no momentum");
        }
        else if (DoubleMomentum)
        {
            description.AppendLine($"• Double momentum from {targetDescription}");
        }
        else if (MomentumModifier != 0)
        {
            string sign = MomentumModifier > 0 ? "+" : "";
            description.AppendLine($"• {sign}{MomentumModifier} momentum for {targetDescription}");
        }

        // Describe pressure effects
        if (ZeroPressure)
        {
            description.AppendLine($"• {targetDescription} generate no pressure");
        }
        else if (DoublePressure)
        {
            description.AppendLine($"• Double pressure from {targetDescription}");
        }
        else if (PressureModifier != 0)
        {
            string sign = PressureModifier > 0 ? "+" : "";
            description.AppendLine($"• {sign}{PressureModifier} pressure for {targetDescription}");
        }

        // Describe special effects
        if (IsSpecialEffect && !string.IsNullOrEmpty(SpecialEffectDescription))
        {
            description.AppendLine($"• {SpecialEffectDescription}");
        }

        return description.ToString().TrimEnd();
    }
}
