
/// <summary>
/// Complete location tag information for player display
/// </summary>
public class LocationTagInformation
{
    public string LocationId { get; set; }
    public string LocationName { get; set; }

    // All available tags sorted by type
    public List<TagDetailInfo> PlayerTags { get; set; }
    public List<TagDetailInfo> LocationReactionTags { get; set; }

    // Active tags
    public List<TagDetailInfo> ActiveTags { get; set; }

    public LocationTagInformation()
    {
        PlayerTags = new List<TagDetailInfo>();
        LocationReactionTags = new List<TagDetailInfo>();
        ActiveTags = new List<TagDetailInfo>();
    }
}
