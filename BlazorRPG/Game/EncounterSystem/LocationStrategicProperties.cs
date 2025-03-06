/// <summary>
/// Extended location strategic properties to include reactive tags
/// </summary>
public class LocationStrategicProperties
{
    public string LocationId { get; }
    public string LocationName { get; }
    public List<SignatureElementTypes> FavoredElements { get; }
    public List<SignatureElementTypes> DisfavoredElements { get; }
    public List<string> AvailableTagIds { get; }
    public List<string> LocationReactionTagIds { get; }

    public LocationStrategicProperties(string id, string name,
                                      List<SignatureElementTypes> favored,
                                      List<SignatureElementTypes> disfavored,
                                      List<string> availableTagIds,
                                      List<string> locationReactionTagIds)
    {
        LocationId = id;
        LocationName = name;
        FavoredElements = favored;
        DisfavoredElements = disfavored;
        AvailableTagIds = availableTagIds;
        LocationReactionTagIds = locationReactionTagIds;
    }
}
