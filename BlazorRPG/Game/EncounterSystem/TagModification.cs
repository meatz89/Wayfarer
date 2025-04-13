/// <summary>
/// Represents a tag modification to be applied when a choice is made
/// </summary>
public class TagModification
{
    internal static TagModification None()
    {
        return new TagModification(TagTypes.None, null);
    }

    public enum TagTypes { Approach, Focus, None }

    public TagTypes EncounterTagType { get; }
    public object TagName { get; }

    private TagModification(TagTypes type, object tag)
    {
        EncounterTagType = type;
        TagName = tag;
    }

    public static TagModification IncreaseApproach(ApproachTags tag)
    {
        return new TagModification(TagTypes.Approach, tag);
    }

    public static TagModification IncreaseFocus(FocusTags tag)
    {
        return new TagModification(TagTypes.Focus, tag);
    }

    public override string ToString()
    {
        return $"{EncounterTagType} (to {TagName})";
    }

}
