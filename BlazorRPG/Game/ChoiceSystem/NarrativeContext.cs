/// <summary>
/// Represents the narrative context derived from the current encounter state
/// </summary>
public class NarrativeContext
{
    public NarrativeTypes PrimaryNarrative { get; }
    public NarrativeTypes SecondaryNarrative { get; }
    public NarrativePhases Phase { get; }
    public ApproachTypes DominantApproach { get; }
    public FocusTypes DominantFocus { get; }
    public ApproachTypes SecondaryApproach { get; }
    public FocusTypes SecondaryFocus { get; }

    public NarrativeContext(
        NarrativeTypes primaryNarrative,
        NarrativeTypes secondaryNarrative,
        NarrativePhases phase,
        ApproachTypes dominantApproach,
        FocusTypes dominantFocus,
        ApproachTypes secondaryApproach,
        FocusTypes secondaryFocus)
    {
        PrimaryNarrative = primaryNarrative;
        SecondaryNarrative = secondaryNarrative;
        Phase = phase;
        DominantApproach = dominantApproach;
        DominantFocus = dominantFocus;
        SecondaryApproach = secondaryApproach;
        SecondaryFocus = secondaryFocus;
    }
}
