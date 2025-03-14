
/// <summary>
/// Represents a tag modification to be applied when a choice is made
/// </summary>
public class TagModification
{
    public enum TagTypes { EncounterState, Approach, Focus }

    public TagTypes Type { get; }
    public object Tag { get; }
    public int Delta { get; }

    private TagModification(TagTypes type, object tag, int delta)
    {
        Type = type;
        Tag = tag;
        Delta = delta;
    }

    public static TagModification ForEncounterState(EncounterStateTags tag, int delta)
    {
        return new TagModification(TagTypes.EncounterState, tag, delta);
    }

    public static TagModification ForApproach(ApproachTags tag, int delta)
    {
        return new TagModification(TagTypes.Approach, tag, delta);
    }

    public static TagModification ForFocus(FocusTags tag, int delta)
    {
        return new TagModification(TagTypes.Focus, tag, delta);
    }

    public override string ToString()
    {
        return $"{Type} ({Delta} to {Tag})";
    }

}
