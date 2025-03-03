
/// <summary>
/// Extended builder for location strategic properties
/// </summary>
public class LocationStrategicBuilder
{
    private string _id;
    private string _name;
    private List<SignatureElementTypes> _favoredElements;
    private List<SignatureElementTypes> _disfavoredElements;
    private List<string> _availableTagIds;
    private List<string> _locationReactionTagIds;

    public LocationStrategicBuilder()
    {
        _id = string.Empty;
        _name = string.Empty;
        _favoredElements = new List<SignatureElementTypes>();
        _disfavoredElements = new List<SignatureElementTypes>();
        _availableTagIds = new List<string>();
        _locationReactionTagIds = new List<string>();
    }

    public LocationStrategicBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public LocationStrategicBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public LocationStrategicBuilder WithFavoredElement(SignatureElementTypes element)
    {
        _favoredElements.Add(element);
        return this;
    }

    public LocationStrategicBuilder WithDisfavoredElement(SignatureElementTypes element)
    {
        _disfavoredElements.Add(element);
        return this;
    }

    public LocationStrategicBuilder WithAvailableTag(string tagId)
    {
        _availableTagIds.Add(tagId);
        return this;
    }

    public LocationStrategicBuilder WithLocationReactionTag(string tagId)
    {
        _locationReactionTagIds.Add(tagId);
        return this;
    }

    public LocationStrategicProperties Build()
    {
        return new LocationStrategicProperties(
            _id,
            _name,
            _favoredElements,
            _disfavoredElements,
            _availableTagIds,
            _locationReactionTagIds
        );
    }
}
